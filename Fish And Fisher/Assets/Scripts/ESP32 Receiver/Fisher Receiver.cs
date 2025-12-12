using UnityEngine;
using System;
using System.IO.Ports;
using System.Threading;
using System.Globalization;

public class FisherReceiver : MonoBehaviour
{
    [Header("Serial Settings")]
    public string portName = "COM13";
    public int baudRate = 115200;
    public float reconnectInterval = 2f;

    [Header("Encoder Settings")]
    public float countsPerRevolution = 80f;  // Encoder CPR (counts per revolution)

    [Header("Received Data")]
    public Quaternion rawQuaternion = Quaternion.identity;
    public Quaternion relativeQuaternion = Quaternion.identity;
    public Vector3 eulerAngles = Vector3.zero;
    public long encoderCount = 0;
    public bool buttonPressed = false;

    [Header("Encoder Speed")]
    public float encoderSpeed = 0f;      // deg/s
    public float encoderRPM = 0f;        // revolutions per minute

    [Header("Status")]
    public bool isConnected = false;
    public bool isCalibrated = false;
    public int dataReceivedCount = 0;
    public string connectionStatus = "Not connected";

    [Header("距离数据发送设置")]
    [Tooltip("是否启用距离数据发送")]
    public bool enableProximitySending = true;

    [Tooltip("发送间隔（秒）")]
    public float proximitySendInterval = 0.1f;  // 100ms

    SerialPort serial;
    Thread readThread;
    volatile bool running = false;
    volatile string latestLine = "";
    
    Quaternion qOffset = Quaternion.identity;
    float lastReconnectAttempt = 0f;

    // For speed calculation
    long lastEncoderCount = 0;
    float lastSpeedTime = 0f;
    bool hasLastCount = false;

    // 距离数据发送计时器
    float lastProximitySendTime = 0f;

    void Start()
    {
        TryConnect();
        lastSpeedTime = Time.time;
    }

    void TryConnect()
    {
        CloseSerial();

        try
        {
            serial = new SerialPort(portName, baudRate);
            serial.ReadTimeout = 50;
            serial.DtrEnable = true;
            serial.RtsEnable = true;
            serial.Open();

            running = true;
            isConnected = true;
            connectionStatus = "Connected";
            
            readThread = new Thread(ReadSerial);
            readThread.Start();
            
            Debug.Log($"Fisher Receiver: Connected to {portName}");
        }
        catch (Exception ex)
        {
            isConnected = false;
            connectionStatus = $"Failed: {ex.Message}";
            Debug.LogWarning($"Fisher Receiver: Connection failed - {ex.Message}");
        }
    }

    void CloseSerial()
    {
        running = false;
        
        if (readThread != null && readThread.IsAlive)
        {
            readThread.Join(100);
        }
        
        if (serial != null && serial.IsOpen)
        {
            try { serial.Close(); } catch { }
        }
        
        serial = null;
        isConnected = false;
    }

    void ReadSerial()
    {
        while (running)
        {
            try
            {
                if (serial == null || !serial.IsOpen)
                {
                    running = false;
                    break;
                }

                string line = serial.ReadLine();
                if (line.StartsWith("FISHER:"))
                {
                    latestLine = line;
                }
            }
            catch (TimeoutException)
            {
                // Expected when no data available
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Fisher Receiver: Read error - {ex.Message}");
                running = false;
                break;
            }
        }
    }

    void Update()
    {
        // Check for disconnection and attempt reconnection
        if (!isConnected || !running || serial == null || !serial.IsOpen)
        {
            isConnected = false;
            connectionStatus = "Disconnected - attempting reconnect...";
            
            if (Time.time - lastReconnectAttempt > reconnectInterval)
            {
                lastReconnectAttempt = Time.time;
                Debug.Log("Fisher Receiver: Attempting to reconnect...");
                TryConnect();
            }
            return;
        }

        // Parse on main thread
        string line = latestLine;
        if (string.IsNullOrEmpty(line)) return;
        latestLine = "";

        try
        {
            string[] parts = line.Substring(7).Split(',');
            if (parts.Length < 6) return;

            float qw = float.Parse(parts[0], CultureInfo.InvariantCulture);
            float qx = float.Parse(parts[1], CultureInfo.InvariantCulture);
            float qy = float.Parse(parts[2], CultureInfo.InvariantCulture);
            float qz = float.Parse(parts[3], CultureInfo.InvariantCulture);
            encoderCount = long.Parse(parts[4]);
            buttonPressed = parts[5].Trim() == "1";

            rawQuaternion = new Quaternion(qx, qy, qz, qw);

            if (!isCalibrated)
            {
                qOffset = Quaternion.Inverse(rawQuaternion);
                isCalibrated = true;
                Debug.Log("Fisher Receiver: Calibrated");
            }

            relativeQuaternion = qOffset * rawQuaternion;
            eulerAngles = NormalizeEuler(relativeQuaternion.eulerAngles);

            // Calculate encoder speed
            CalculateEncoderSpeed();

            dataReceivedCount++;
            connectionStatus = "Receiving data";
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Fisher Receiver: Parse error - {ex.Message}");
        }

        if (Input.GetKeyDown(KeyCode.Space))
            Recalibrate();

        // 发送距离数据
        SendProximityData();
    }

    /// <summary>
    /// 发送距离数据到ESP32
    /// </summary>
    void SendProximityData()
    {
        if (!enableProximitySending) return;
        if (!isConnected || serial == null || !serial.IsOpen) return;
        if (Time.time - lastProximitySendTime < proximitySendInterval) return;

        // 获取距离数据
        float proximityValue = 0f;
        if (FishAndFisher.DistanceTracker.Instance != null)
        {
            proximityValue = FishAndFisher.DistanceTracker.Instance.GetProximityValue();
        }

        // 发送数据（带标识符）
        string message = $"PROXIMITY:{proximityValue:F2}\n";
        try
        {
            serial.Write(message);
            lastProximitySendTime = Time.time;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Fisher Receiver: 发送距离数据失败 - {ex.Message}");
        }
    }

    void CalculateEncoderSpeed()
    {
        float currentTime = Time.time;
        float deltaTime = currentTime - lastSpeedTime;

        if (deltaTime > 0.01f)  // Minimum 10ms between calculations
        {
            if (hasLastCount)
            {
                long deltaCount = encoderCount - lastEncoderCount;
                
                // Calculate speed: (counts / CPR) * 360 = degrees
                // degrees / deltaTime = deg/s
                float degreesPerCount = 360f / countsPerRevolution;
                encoderSpeed = (deltaCount * degreesPerCount) / deltaTime;
                
                // Calculate RPM: (deg/s / 360) * 60 = RPM
                encoderRPM = (encoderSpeed / 360f) * 60f;
            }

            lastEncoderCount = encoderCount;
            lastSpeedTime = currentTime;
            hasLastCount = true;
        }
    }

    Vector3 NormalizeEuler(Vector3 euler)
    {
        return new Vector3(
            euler.x > 180 ? euler.x - 360 : euler.x,
            euler.y > 180 ? euler.y - 360 : euler.y,
            euler.z > 180 ? euler.z - 360 : euler.z
        );
    }

    public void Recalibrate()
    {
        qOffset = Quaternion.Inverse(rawQuaternion);
        Debug.Log("Fisher Receiver: Recalibrated");
    }

    // Reset encoder count to zero
    public void ResetEncoderCount()
    {
        lastEncoderCount = encoderCount;
        hasLastCount = false;
        encoderSpeed = 0f;
        encoderRPM = 0f;
        Debug.Log("Fisher Receiver: Encoder reset");
    }

    void OnApplicationQuit()
    {
        CloseSerial();
    }

    void OnDestroy()
    {
        CloseSerial();
    }
}

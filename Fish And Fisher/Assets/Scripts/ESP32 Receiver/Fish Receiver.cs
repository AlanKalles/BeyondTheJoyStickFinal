using UnityEngine;
using System;
using System.IO.Ports;
using System.Threading;
using System.Globalization;

public class FishReceiver : MonoBehaviour
{
    [Header("Serial Settings")]
    public string portName = "COM11";
    public int baudRate = 115200;
    public float reconnectInterval = 2f;  // Seconds between reconnection attempts

    [Header("Received Data")]
    public Quaternion rawQuaternion = Quaternion.identity;
    public Quaternion relativeQuaternion = Quaternion.identity;
    public Vector3 eulerAngles = Vector3.zero;
    public float gyroMagnitude = 0f;

    [Header("Status")]
    public bool isConnected = false;
    public bool isCalibrated = false;
    public int dataReceivedCount = 0;
    public string connectionStatus = "Not connected";

    SerialPort serial;
    Thread readThread;
    volatile bool running = false;
    volatile string latestLine = "";
    
    Quaternion qOffset = Quaternion.identity;
    float lastReconnectAttempt = 0f;

    void Start()
    {
        TryConnect();
    }

    void TryConnect()
    {
        // Close existing connection if any
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
            
            Debug.Log($"Fish Receiver: Connected to {portName}");
        }
        catch (Exception ex)
        {
            isConnected = false;
            connectionStatus = $"Failed: {ex.Message}";
            Debug.LogWarning($"Fish Receiver: Connection failed - {ex.Message}");
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
                if (line.StartsWith("FISH:"))
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
                Debug.LogWarning($"Fish Receiver: Read error - {ex.Message}");
                running = false;  // Signal to attempt reconnection
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
                Debug.Log("Fish Receiver: Attempting to reconnect...");
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
            string[] parts = line.Substring(5).Split(',');
            if (parts.Length < 5) return;

            float qw = float.Parse(parts[0], CultureInfo.InvariantCulture);
            float qx = float.Parse(parts[1], CultureInfo.InvariantCulture);
            float qy = float.Parse(parts[2], CultureInfo.InvariantCulture);
            float qz = float.Parse(parts[3], CultureInfo.InvariantCulture);
            gyroMagnitude = float.Parse(parts[4], CultureInfo.InvariantCulture);

            rawQuaternion = new Quaternion(qx, qy, qz, qw);

            if (!isCalibrated)
            {
                qOffset = Quaternion.Inverse(rawQuaternion);
                isCalibrated = true;
                Debug.Log("Fish Receiver: Calibrated");
            }

            relativeQuaternion = qOffset * rawQuaternion;
            eulerAngles = NormalizeEuler(relativeQuaternion.eulerAngles);
            dataReceivedCount++;
            connectionStatus = "Receiving data";
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Fish Receiver: Parse error - {ex.Message}");
        }

        if (Input.GetKeyDown(KeyCode.Space))
            Recalibrate();
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
        Debug.Log("Fish Receiver: Recalibrated");
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

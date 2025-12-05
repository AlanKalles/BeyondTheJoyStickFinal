using UnityEngine;
using System;
using System.IO.Ports;
using System.Threading;
using System.Globalization;

public class FishReceiver : MonoBehaviour
{
    [Header("Serial Settings")]
    public string portName = "/dev/cu.usbmodem3C8427C31A6C2";
    public int baudRate = 115200;

    [Header("Received Data")]
    public Quaternion rawQuaternion = Quaternion.identity;
    public Quaternion relativeQuaternion = Quaternion.identity;
    public Vector3 eulerAngles = Vector3.zero;
    public float gyroMagnitude = 0f;

    [Header("Calibration")]
    public bool isCalibrated = false;
    public int dataReceivedCount = 0;

    SerialPort serial;
    Thread readThread;
    bool running = false;

    // Raw data from thread (volatile for thread safety)
    volatile string latestLine = "";
    
    // Reference quaternion for calibration
    Quaternion qOffset = Quaternion.identity;

    void Start()
    {
        serial = new SerialPort(portName, baudRate);
        serial.ReadTimeout = 50;
        serial.DtrEnable = true;
        serial.RtsEnable = true;

        try
        {
            serial.Open();
            running = true;
            readThread = new Thread(ReadSerial);
            readThread.Start();
            Debug.Log($"Fish Receiver: Serial started on {portName}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Fish Receiver: Failed to open serial port - {ex.Message}");
        }
    }

    void ReadSerial()
    {
        while (running)
        {
            try
            {
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
            }
        }
    }

    void Update()
    {
        // Parse on main thread
        string line = latestLine;
        if (string.IsNullOrEmpty(line)) return;

        // Clear so we don't re-parse same line
        latestLine = "";

        try
        {
            // Parse: FISH:qw,qx,qy,qz,gyroMag
            string[] parts = line.Substring(5).Split(',');
            if (parts.Length < 5) return;

            float qw = float.Parse(parts[0], CultureInfo.InvariantCulture);
            float qx = float.Parse(parts[1], CultureInfo.InvariantCulture);
            float qy = float.Parse(parts[2], CultureInfo.InvariantCulture);
            float qz = float.Parse(parts[3], CultureInfo.InvariantCulture);
            gyroMagnitude = float.Parse(parts[4], CultureInfo.InvariantCulture);

            // Create quaternion (Unity uses x,y,z,w order)
            rawQuaternion = new Quaternion(qx, qy, qz, qw);

            // Calibrate on first data
            if (!isCalibrated)
            {
                qOffset = Quaternion.Inverse(rawQuaternion);
                isCalibrated = true;
                Debug.Log("Fish Receiver: Calibrated");
            }

            // Apply calibration offset
            relativeQuaternion = qOffset * rawQuaternion;
            
            // Convert to euler angles (-180 to 180)
            eulerAngles = NormalizeEuler(relativeQuaternion.eulerAngles);

            dataReceivedCount++;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Fish Receiver: Parse error - {ex.Message}");
        }

        // Press Space to recalibrate
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Recalibrate();
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
        Debug.Log("Fish Receiver: Recalibrated");
    }

    void OnApplicationQuit()
    {
        running = false;
        Thread.Sleep(50);
        if (serial != null && serial.IsOpen)
            serial.Close();
    }
}

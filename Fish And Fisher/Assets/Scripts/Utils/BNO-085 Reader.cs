using UnityEngine;
using System.IO.Ports;
using System.Globalization;

public class BNO085QuaternionReader : MonoBehaviour
{
    [Header("Serial Settings")]
    
    public string portName = "/dev/cu.usbmodem3C8427C2EE1C2"; // 🧷 改成你自己电脑上看到的那个端口名
    public int baudRate = 115200;

    private SerialPort serial;
    public Quaternion CurrentRotation { get; private set; } = Quaternion.identity;

    void Start()
    {
        try
        {
            serial = new SerialPort(portName, baudRate);
            serial.ReadTimeout = 50;
            serial.DtrEnable = true;
            serial.Open();
            Debug.Log("Serial port opened: " + portName);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to open serial port: " + e.Message);
        }
    }

    void Update()
    {
        if (serial == null || !serial.IsOpen)
            return;

        try
        {
            // 一行数据形如：qw,qx,qy,qz
            string line = serial.ReadLine();
            string[] parts = line.Split(',');

            if (parts.Length == 4)
            {
                // 为了避免小数点格式问题，用 InvariantCulture
                float qw = float.Parse(parts[0], CultureInfo.InvariantCulture);
                float qx = float.Parse(parts[1], CultureInfo.InvariantCulture);
                float qy = float.Parse(parts[2], CultureInfo.InvariantCulture);
                float qz = float.Parse(parts[3], CultureInfo.InvariantCulture);

                // 先直接用同样的坐标系
                Quaternion q = new Quaternion(qx, qy, qz, qw);

                // 可能需要做坐标系转换（后面说）
                CurrentRotation = q;

                // 应用到当前物体
                transform.localRotation = CurrentRotation;
            }
        }
        catch (System.TimeoutException)
        {
            // 没读到数据就算了，不报错
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Serial read error: " + e.Message);
        }
    }

    void OnDestroy()
    {
        if (serial != null && serial.IsOpen)
        {
            serial.Close();
            serial.Dispose();
        }
    }
}

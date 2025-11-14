using System;
using System.Globalization;
using System.IO.Ports;
using UnityEngine;

namespace FishAndFisher.Fish
{
    /// <summary>
    /// 直接用 BNO-085 控制鱼的朝向 / 视角方向的脚本
    /// 1. 串口读取四元数 qw,qx,qy,qz
    /// 2. 转成欧拉角，只取需要的轴控制鱼
    /// </summary>
    public class ImuFishController : MonoBehaviour
    {
        [Header("串口设置")]
        [Tooltip("Mac 上类似 /dev/cu.usbmodemXXXX 或 /dev/cu.SLAB_USBtoUART")]
        public string portName = "/dev/cu.usbmodem1101";
        public int baudRate = 115200;
        [Tooltip("串口读取超时时间，IMU 更新慢时记得调大")]
        public int readTimeoutMs = 120;

        [Header("控制目标")]
        [Tooltip("要被 IMU 控的目标（不填就用当前物体）")]
        public Transform fishTarget;

        [Header("轴向映射")]
        [Tooltip("是否用 IMU 的 yaw 控制水平转向（左右转头）")]
        public bool useYawForHorizontal = true;
        [Tooltip("是否用 pitch 控制上下俯仰（可选）")]
        public bool usePitchForVertical = false;

        [Tooltip("最大倾斜角（度）映射到 1：比如 30 度 = 最大输入")]
        public float maxTiltAngle = 30f;

        [Tooltip("是否反转左右")]
        public bool invertHorizontal = false;
        [Tooltip("是否反转上下")]
        public bool invertVertical = false;

        [Header("直接应用到鱼的欧拉角")]
        [Tooltip("是否直接用 yaw 设置鱼的 Y 轴朝向")]
        public bool applyYawToFishRotation = true;
        [Tooltip("是否直接用 pitch 设置鱼的 X 轴旋转")]
        public bool applyPitchToFishRotation = false;

        [Header("调试")]
        public bool debugLog = false;
        public Quaternion imuRotation = Quaternion.identity;
        public Vector3 imuEuler; // 方便在 Inspector 里看

        private SerialPort serial;
        private FishMovement fishMovement;

        [Header("驱动鱼移动（可选）")]
        [Tooltip("是否把 IMU 的 yaw/pitch 当成摇杆输入喂给 FishMovement")]
        public bool driveFishMovement = true;
        [Tooltip("即使 pitch 很小也保持一个基础前进量，营造持续前进的鱼游感")]
        public bool forceForwardMotion = true;
        [Range(0f, 1f)]
        [Tooltip("当 forceForwardMotion=true 时，至少保持多少纵向输入")]
        public float minimumForwardInput = 0.3f;

        private void Awake()
        {
            if (fishTarget == null)
                fishTarget = transform;

            fishMovement = fishTarget.GetComponent<FishMovement>();

            if (driveFishMovement && fishMovement == null)
            {
                Debug.LogWarning("[ImuFishController] 启用了 driveFishMovement 但 fishTarget 上没有 FishMovement 组件。");
            }
        }

        private void OnEnable()
        {
            OpenSerial();
        }

        private void OnDisable()
        {
            CloseSerial();
        }

        private void OnDestroy()
        {
            CloseSerial();
        }

        private void Update()
        {
            ReadImu();
            ApplyToFish();
        }

        #region 串口相关

        private void OpenSerial()
        {
            try
            {
                serial = new SerialPort(portName, baudRate);
                serial.ReadTimeout = readTimeoutMs;
                serial.NewLine = "\n";
                serial.Open();
                Debug.Log("[ImuFishController] Serial opened: " + portName);
            }
            catch (Exception e)
            {
                Debug.LogError("[ImuFishController] Failed to open serial port: " + e.Message);
            }
        }

        private void CloseSerial()
        {
            if (serial != null)
            {
                try
                {
                    if (serial.IsOpen)
                        serial.Close();
                }
                catch { /* 忽略关闭异常 */ }

                serial = null;
            }
        }

        /// <summary>
        /// 读取一行串口数据："qw,qx,qy,qz"
        /// </summary>
        private void ReadImu()
        {
            if (serial == null || !serial.IsOpen) return;

            try
            {
                if (serial.BytesToRead == 0)
                    return;

                string line = serial.ReadLine();   // 例如："0.99,-0.01,0.05,0.00"
                string[] parts = line.Split(',');
                if (parts.Length != 4) return;

                float qw = float.Parse(parts[0], CultureInfo.InvariantCulture);
                float qx = float.Parse(parts[1], CultureInfo.InvariantCulture);
                float qy = float.Parse(parts[2], CultureInfo.InvariantCulture);
                float qz = float.Parse(parts[3], CultureInfo.InvariantCulture);

                // 如果方向反了，可以在这里试试给某些分量取反
                imuRotation = new Quaternion(qx, qy, qz, qw);
                imuEuler = imuRotation.eulerAngles;

                if (debugLog)
                    Debug.Log($"[ImuFishController] IMU Euler: {imuEuler}");
            }
            catch (TimeoutException)
            {
                // IMU 没有及时发数据时会进这里，属正常情况，静默忽略
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ImuFishController] Serial read error: " + e.Message);
            }
        }

        #endregion

        #region 应用到鱼

        private void ApplyToFish()
        {
            if (fishTarget == null) return;

            // 把欧拉角转到 [-180,180]，方便理解
            float yaw   = Mathf.DeltaAngle(0f, imuEuler.y);
            float pitch = Mathf.DeltaAngle(0f, imuEuler.x);

            if (invertHorizontal) yaw   = -yaw;
            if (invertVertical)   pitch = -pitch;

            // -------- 1. 像摇杆一样：映射成 [-1,1] 的输入（以后你要喂给移动系统可以用）
            float horizontal = 0f;
            float vertical   = 0f;

            if (useYawForHorizontal)
                horizontal = Mathf.Clamp(yaw / maxTiltAngle, -1f, 1f);

            if (usePitchForVertical)
                vertical = Mathf.Clamp(-pitch / maxTiltAngle, -1f, 1f);

            // -------- 1.1 把 IMU 当作移动输入喂给 FishMovement
            if (driveFishMovement && fishMovement != null)
            {
                Vector2 imuInput = new Vector2(horizontal, vertical);

                if (forceForwardMotion)
                {
                    float forward = Mathf.Clamp01(Mathf.Abs(imuInput.y));
                    forward = Mathf.Max(forward, minimumForwardInput);
                    imuInput.y = forward;
                }

                fishMovement.SetMoveInput(imuInput);
            }

            // -------- 2. 直接用 yaw/pitch 控制鱼的朝向（最直观）
            Vector3 fishEuler = fishTarget.eulerAngles;

            if (applyYawToFishRotation)
            {
                // 只改 Y 轴（水平朝向），不动别的
                fishEuler.y = yaw;
            }

            if (applyPitchToFishRotation)
            {
                // 可选：让鱼上下摆一点（看需要）
                fishEuler.x = pitch;
            }

            fishTarget.rotation = Quaternion.Euler(fishEuler);

            // 你现在的第三人称相机如果是跟着 FishPlayer 的，
            // 那么视角就会跟着鱼一起转。
        }

        #endregion
    }
}

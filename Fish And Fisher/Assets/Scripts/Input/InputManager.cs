using UnityEngine;

namespace FishAndFisher.Input
{
    /// <summary>
    /// 输入管理器 - 跨场景单例，统一管理所有输入源
    /// 支持键盘、ESP32 等多种输入设备的自动检测和切换
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        #region 单例模式

        private static InputManager instance;

        /// <summary>
        /// 全局访问点
        /// </summary>
        public static InputManager Instance
        {
            get
            {
                // 如果实例不存在，尝试在场景中查找
                if (instance == null)
                {
                    instance = FindFirstObjectByType<InputManager>();

                    // 如果场景中没有，自动创建一个
                    if (instance == null)
                    {
                        GameObject managerObject = new GameObject("InputManager");
                        instance = managerObject.AddComponent<InputManager>();
                        Debug.Log("[InputManager] 自动创建输入管理器实例");
                    }
                }
                return instance;
            }
        }

        #endregion

        #region 字段

        [Header("当前输入设备")]
        [SerializeField]
        [Tooltip("当前使用的输入提供者类型")]
        private string currentProviderName = "未初始化";

        [Header("ESP32 设置")]
        [Tooltip("是否优先使用ESP32输入（如果可用）")]
        [SerializeField] private bool preferESP32Input = true;

        [Tooltip("ESP32连接检测间隔（秒）")]
        [SerializeField] private float esp32CheckInterval = 1f;

        private IInputProvider currentProvider;
        private KeyboardInputProvider keyboardProvider;
        private ESP32InputProvider esp32Provider; // 预留 ESP32 提供者

        // ESP32 Receiver 引用（场景中的MonoBehaviour）
        private FishReceiver fishReceiver;
        private FisherReceiver fisherReceiver;
        private float lastESP32CheckTime = 0f;

        #endregion

        #region Unity 生命周期

        private void Awake()
        {
            // 单例检查
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject); // 跨场景保持
                Debug.Log("[InputManager] 输入管理器已初始化（跨场景持久化）");
            }
            else if (instance != this)
            {
                Debug.LogWarning("[InputManager] 检测到重复实例，销毁多余对象");
                Destroy(gameObject);
                return;
            }

            InitializeInputProviders();
        }

        private void Update()
        {
            // 定期检测ESP32连接状态
            if (preferESP32Input && Time.time - lastESP32CheckTime > esp32CheckInterval)
            {
                lastESP32CheckTime = Time.time;
                TryFindESP32Receivers();
                UpdateProviderName();
            }
        }

        private void OnDestroy()
        {
            // 清理当前输入提供者
            if (currentProvider != null)
            {
                currentProvider.Cleanup();
                Debug.Log("[InputManager] 已清理输入提供者");
            }

            // 清理单例引用
            if (instance == this)
            {
                instance = null;
            }
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化所有输入提供者
        /// </summary>
        private void InitializeInputProviders()
        {
            Debug.Log("[InputManager] 开始初始化输入提供者...");

            // 初始化键盘输入作为备用
            UseKeyboardInput();

            // 尝试查找ESP32 Receiver
            if (preferESP32Input)
            {
                TryFindESP32Receivers();
            }

            UpdateProviderName();
            Debug.Log($"[InputManager] 输入初始化完成，当前设备: {currentProviderName}");
        }

        /// <summary>
        /// 尝试在场景中查找ESP32 Receiver
        /// </summary>
        private void TryFindESP32Receivers()
        {
            // 查找FishReceiver
            if (fishReceiver == null)
            {
                fishReceiver = FindFirstObjectByType<FishReceiver>();
                if (fishReceiver != null)
                {
                    Debug.Log("[InputManager] 找到 FishReceiver");
                }
            }

            // 查找FisherReceiver
            if (fisherReceiver == null)
            {
                fisherReceiver = FindFirstObjectByType<FisherReceiver>();
                if (fisherReceiver != null)
                {
                    Debug.Log("[InputManager] 找到 FisherReceiver");
                }
            }
        }

        /// <summary>
        /// 更新输入提供者名称显示
        /// </summary>
        private void UpdateProviderName()
        {
            bool fishESP32Connected = fishReceiver != null && fishReceiver.isConnected;
            bool fisherESP32Connected = fisherReceiver != null && fisherReceiver.isConnected;

            if (fishESP32Connected && fisherESP32Connected)
            {
                currentProviderName = "ESP32 (鱼+渔夫)";
            }
            else if (fishESP32Connected)
            {
                currentProviderName = "ESP32 (鱼) + 键盘 (渔夫)";
            }
            else if (fisherESP32Connected)
            {
                currentProviderName = "键盘 (鱼) + ESP32 (渔夫)";
            }
            else
            {
                currentProviderName = "键盘";
            }
        }

        /// <summary>
        /// 切换到键盘输入
        /// </summary>
        private void UseKeyboardInput()
        {
            // 清理旧的提供者
            if (currentProvider != null && currentProvider != keyboardProvider)
            {
                currentProvider.Cleanup();
            }

            // 创建或重用键盘提供者
            if (keyboardProvider == null)
            {
                keyboardProvider = new KeyboardInputProvider();
                keyboardProvider.Initialize();
            }

            currentProvider = keyboardProvider;
            Debug.Log("[InputManager] 已切换到键盘输入");
        }

        #endregion

        #region 公共输入接口

        /// <summary>
        /// 获取移动输入（用于鱼的移动控制）
        /// 优先使用ESP32 FishReceiver的数据
        /// </summary>
        public Vector2 GetMovement()
        {
            // 优先检测ESP32 FishReceiver
            if (preferESP32Input && fishReceiver != null && fishReceiver.isConnected)
            {
                // 从FishReceiver的欧拉角获取移动方向
                // 使用pitch(X)和roll(Z)作为移动输入
                Vector3 euler = fishReceiver.eulerAngles;

                // 将倾斜角度映射到-1到1的范围（假设最大倾斜45度）
                float maxTiltAngle = 45f;
                float x = Mathf.Clamp(euler.z / maxTiltAngle, -1f, 1f);  // Roll -> 左右
                float y = Mathf.Clamp(-euler.x / maxTiltAngle, -1f, 1f); // Pitch -> 前后（取反）

                return new Vector2(x, y);
            }

            // 回退到键盘输入
            if (currentProvider == null || !currentProvider.IsConnected)
            {
                return Vector2.zero;
            }
            return currentProvider.GetMovement();
        }

        /// <summary>
        /// 获取加速/跳跃按钮状态（用于鱼的加速）
        /// 优先使用ESP32 FishReceiver的陀螺仪震动检测
        /// </summary>
        public bool GetJumpPressed()
        {
            // 优先检测ESP32 FishReceiver
            if (preferESP32Input && fishReceiver != null && fishReceiver.isConnected)
            {
                // 使用gyroMagnitude检测震动（阈值可调）
                float shakeThreshold = 2.0f;
                return fishReceiver.gyroMagnitude > shakeThreshold;
            }

            // 回退到键盘输入
            if (currentProvider == null || !currentProvider.IsConnected)
            {
                return false;
            }
            return currentProvider.GetJumpPressed();
        }

        /// <summary>
        /// 获取瞄准/观察位置（用于渔夫准心控制）
        /// 优先使用ESP32 FisherReceiver的数据
        /// 返回屏幕坐标（与键盘模式一致）
        /// </summary>
        public Vector2 GetLookPosition()
        {
            // 优先检测ESP32 FisherReceiver
            if (preferESP32Input && fisherReceiver != null && fisherReceiver.isConnected)
            {
                // 从FisherReceiver的欧拉角获取瞄准方向
                Vector3 euler = fisherReceiver.eulerAngles;

                // 将倾斜角度映射到归一化值 (-1 到 1)
                float maxTiltAngle = 45f;
                float normalizedX = Mathf.Clamp(euler.y / maxTiltAngle, -1f, 1f);  // Yaw -> 左右
                float normalizedY = Mathf.Clamp(-euler.x / maxTiltAngle, -1f, 1f); // Pitch -> 上下（取反）

                // 转换为屏幕坐标（与键盘模式的鼠标坐标格式一致）
                // FisherCrosshairController 期望的是屏幕坐标，用于 ScreenPointToRay
                float screenX = (normalizedX + 1f) * 0.5f * Screen.width;
                float screenY = (normalizedY + 1f) * 0.5f * Screen.height;

                return new Vector2(screenX, screenY);
            }

            // 回退到键盘输入
            if (currentProvider == null || !currentProvider.IsConnected)
            {
                return Vector2.zero;
            }
            return currentProvider.GetLookPosition();
        }

        /// <summary>
        /// 获取攻击按钮状态（用于渔夫挥竿）
        /// 优先使用ESP32 FisherReceiver的按钮状态
        /// </summary>
        public bool GetAttackPressed()
        {
            // 优先检测ESP32 FisherReceiver
            if (preferESP32Input && fisherReceiver != null && fisherReceiver.isConnected)
            {
                return fisherReceiver.buttonPressed;
            }

            // 回退到键盘输入
            if (currentProvider == null || !currentProvider.IsConnected)
            {
                return false;
            }
            return currentProvider.GetAttackPressed();
        }

        /// <summary>
        /// 获取当前输入设备名称
        /// </summary>
        public string GetCurrentDeviceName()
        {
            return currentProviderName;
        }

        /// <summary>
        /// 检查当前输入设备是否已连接
        /// </summary>
        public bool IsInputConnected()
        {
            return currentProvider != null && currentProvider.IsConnected;
        }

        #endregion

        #region 手动切换（可选功能）

        /// <summary>
        /// 手动强制切换到键盘输入（禁用ESP32优先）
        /// </summary>
        public void ForceKeyboardInput()
        {
            preferESP32Input = false;
            Debug.Log("[InputManager] 手动切换到键盘输入，已禁用ESP32优先");
            UpdateProviderName();
        }

        /// <summary>
        /// 启用ESP32优先输入
        /// </summary>
        public void EnableESP32Input()
        {
            preferESP32Input = true;
            TryFindESP32Receivers();
            UpdateProviderName();
            Debug.Log("[InputManager] 已启用ESP32优先输入");
        }

        /// <summary>
        /// 检查Fish ESP32是否已连接
        /// </summary>
        public bool IsFishESP32Connected()
        {
            return fishReceiver != null && fishReceiver.isConnected;
        }

        /// <summary>
        /// 检查Fisher ESP32是否已连接
        /// </summary>
        public bool IsFisherESP32Connected()
        {
            return fisherReceiver != null && fisherReceiver.isConnected;
        }

        /// <summary>
        /// 获取FishReceiver引用（供外部直接访问ESP32数据）
        /// </summary>
        public FishReceiver GetFishReceiver()
        {
            return fishReceiver;
        }

        /// <summary>
        /// 获取FisherReceiver引用（供外部直接访问ESP32数据）
        /// </summary>
        public FisherReceiver GetFisherReceiver()
        {
            return fisherReceiver;
        }

        #endregion
    }
}

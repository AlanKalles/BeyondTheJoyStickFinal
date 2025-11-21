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

        private IInputProvider currentProvider;
        private KeyboardInputProvider keyboardProvider;
        private ESP32InputProvider esp32Provider; // 预留 ESP32 提供者

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

            // TODO: 未来在这里检测 ESP32
            // if (TryDetectESP32(out esp32Provider))
            // {
            //     currentProvider = esp32Provider;
            //     currentProviderName = "ESP32";
            //     Debug.Log("[InputManager] 检测到 ESP32，使用 ESP32 输入");
            // }
            // else
            // {
            //     // 回退到键盘输入
            //     UseKeyboardInput();
            // }

            // 当前阶段：默认使用键盘输入
            UseKeyboardInput();

            Debug.Log($"[InputManager] 输入初始化完成，当前设备: {currentProviderName}");
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
            currentProviderName = keyboardProvider.GetDeviceName();
            Debug.Log("[InputManager] 已切换到键盘输入");
        }

        // TODO: 未来实现 ESP32 检测和切换方法
        // private bool TryDetectESP32(out ESP32InputProvider provider)
        // {
        //     provider = new ESP32InputProvider();
        //     if (provider.TryConnect())
        //     {
        //         provider.Initialize();
        //         return true;
        //     }
        //     return false;
        // }

        #endregion

        #region 公共输入接口

        /// <summary>
        /// 获取移动输入（用于鱼的移动控制）
        /// </summary>
        public Vector2 GetMovement()
        {
            if (currentProvider == null || !currentProvider.IsConnected)
            {
                return Vector2.zero;
            }
            return currentProvider.GetMovement();
        }

        /// <summary>
        /// 获取加速/跳跃按钮状态（用于鱼的加速）
        /// </summary>
        public bool GetJumpPressed()
        {
            if (currentProvider == null || !currentProvider.IsConnected)
            {
                return false;
            }
            return currentProvider.GetJumpPressed();
        }

        /// <summary>
        /// 获取瞄准/观察位置（用于渔夫准心控制）
        /// </summary>
        public Vector2 GetLookPosition()
        {
            if (currentProvider == null || !currentProvider.IsConnected)
            {
                return Vector2.zero;
            }
            return currentProvider.GetLookPosition();
        }

        /// <summary>
        /// 获取攻击按钮状态（用于渔夫挥竿）
        /// </summary>
        public bool GetAttackPressed()
        {
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
        /// 手动强制切换到键盘输入
        /// </summary>
        public void ForceKeyboardInput()
        {
            Debug.Log("[InputManager] 手动切换到键盘输入");
            UseKeyboardInput();
        }

        // TODO: 未来添加手动切换到 ESP32 的方法
        // public void ForceESP32Input()
        // {
        //     if (esp32Provider != null && esp32Provider.IsConnected)
        //     {
        //         currentProvider = esp32Provider;
        //         currentProviderName = "ESP32";
        //         Debug.Log("[InputManager] 手动切换到 ESP32 输入");
        //     }
        //     else
        //     {
        //         Debug.LogWarning("[InputManager] ESP32 未连接，无法切换");
        //     }
        // }

        #endregion
    }
}

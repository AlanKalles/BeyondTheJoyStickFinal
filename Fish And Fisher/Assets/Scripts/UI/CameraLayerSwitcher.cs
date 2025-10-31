using UnityEngine;
using UnityEngine.UI;

namespace FishAndFisher
{
    /// <summary>
    /// 相机图层切换器 - 在两个LayerMask之间切换相机的cullingMask
    /// </summary>
    public class CameraLayerSwitcher : MonoBehaviour
    {
        [Header("相机引用")]
        [Tooltip("需要切换cullingMask的相机")]
        public Camera targetCamera;

        [Header("图层设置")]
        [Tooltip("图层模式A（例如：显示鱼的视角）")]
        public LayerMask layerMaskA;

        [Tooltip("图层模式B（例如：显示渔夫的视角）")]
        public LayerMask layerMaskB;

        [Header("按钮设置")]
        [Tooltip("切换按钮（可选，如果为空则只能通过代码调用）")]
        [SerializeField] private Button switchButton;

        [Header("当前状态")]
        [Tooltip("当前使用的是哪个LayerMask")]
        [SerializeField] private bool isUsingLayerMaskA = true;

        private void Awake()
        {
            // 验证相机引用
            if (targetCamera == null)
            {
                Debug.LogWarning("[CameraLayerSwitcher] 相机未设置，尝试使用主相机");
                targetCamera = Camera.main;
            }

            if (targetCamera == null)
            {
                Debug.LogError("[CameraLayerSwitcher] 无法找到相机！请在Inspector中设置。");
                return;
            }

            // 绑定按钮事件
            if (switchButton != null)
            {
                switchButton.onClick.AddListener(SwitchLayer);
            }
        }

        private void Start()
        {
            // 初始化相机图层为LayerMaskA
            if (targetCamera != null)
            {
                ApplyLayerMask(isUsingLayerMaskA);
            }
        }

        /// <summary>
        /// 切换图层（在LayerMaskA和LayerMaskB之间切换）
        /// </summary>
        public void SwitchLayer()
        {
            if (targetCamera == null)
            {
                Debug.LogError("[CameraLayerSwitcher] 相机不存在，无法切换图层！");
                return;
            }

            // 切换状态
            isUsingLayerMaskA = !isUsingLayerMaskA;

            // 应用新的图层
            ApplyLayerMask(isUsingLayerMaskA);

            Debug.Log($"[CameraLayerSwitcher] 已切换到 {(isUsingLayerMaskA ? "LayerMaskA" : "LayerMaskB")}");
        }

        /// <summary>
        /// 应用指定的图层遮罩
        /// </summary>
        private void ApplyLayerMask(bool useLayerMaskA)
        {
            if (targetCamera == null) return;

            if (useLayerMaskA)
            {
                targetCamera.cullingMask = layerMaskA.value;
            }
            else
            {
                targetCamera.cullingMask = layerMaskB.value;
            }
        }

        /// <summary>
        /// 设置为LayerMaskA
        /// </summary>
        public void SetToLayerMaskA()
        {
            if (targetCamera == null) return;

            isUsingLayerMaskA = true;
            ApplyLayerMask(true);

            Debug.Log("[CameraLayerSwitcher] 已设置为 LayerMaskA");
        }

        /// <summary>
        /// 设置为LayerMaskB
        /// </summary>
        public void SetToLayerMaskB()
        {
            if (targetCamera == null) return;

            isUsingLayerMaskA = false;
            ApplyLayerMask(false);

            Debug.Log("[CameraLayerSwitcher] 已设置为 LayerMaskB");
        }

        /// <summary>
        /// 获取当前使用的是哪个LayerMask
        /// </summary>
        public bool IsUsingLayerMaskA()
        {
            return isUsingLayerMaskA;
        }

        /// <summary>
        /// 通过输入键切换（Update中调用）
        /// </summary>
        private void Update()
        {
            // 可选：通过键盘快捷键切换（例如：Tab键）
            // 取消注释下面的代码启用键盘切换
            /*
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                SwitchLayer();
            }
            */
        }

        /// <summary>
        /// 在编辑器中重置组件
        /// </summary>
        private void Reset()
        {
            // 自动找到主相机
            targetCamera = Camera.main;

            // 设置默认值
            isUsingLayerMaskA = true;

            // 设置默认LayerMask（显示所有层）
            layerMaskA = -1; // Everything
            layerMaskB = -1; // Everything
        }

        /// <summary>
        /// 在Inspector中显示调试信息
        /// </summary>
        private void OnValidate()
        {
            // 在编辑器中验证设置
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }
    }
}

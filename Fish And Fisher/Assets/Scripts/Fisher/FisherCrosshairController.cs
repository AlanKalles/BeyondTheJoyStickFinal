using UnityEngine;
using UnityEngine.InputSystem;

namespace FishAndFisher.Fisher
{
    /// <summary>
    /// 渔夫准心控制器 - 管理逻辑准心和视觉准心的双Transform系统
    /// </summary>
    public class FisherCrosshairController : MonoBehaviour
    {
        [Header("准心引用")]
        [Tooltip("逻辑准心Transform - 在鱼平面上移动，用于碰撞检测")]
        [SerializeField] private Transform logicCrosshair;

        [Tooltip("视觉准心Transform - 显示在倾斜平面上的Quad")]
        [SerializeField] private Transform visualCrosshair;

        [Header("平面设置")]
        [Tooltip("鱼所在平面的Y坐标（逻辑准心的固定高度）")]
        [SerializeField] private float fishPlaneY = 0f;

        [Tooltip("视觉准心Quad平面的Y坐标")]
        [SerializeField] private float visualPlaneY = 5f;

        [Header("移动设置")]
        [Tooltip("准心移动范围限制（矩形边界，与鱼活动范围对等）")]
        [SerializeField] private Vector2 boundarySize = new Vector2(50f, 50f);

        [Tooltip("准心移动速度")]
        [SerializeField] private float moveSpeed = 5f;

        [Tooltip("准心移动平滑度")]
        [SerializeField] private float smoothSpeed = 10f;

        [Header("相机设置")]
        [Tooltip("用于射线检测的相机")]
        [SerializeField] private Camera targetCamera;

        // 输入系统
        private InputSystem_Actions inputActions;
        private Vector2 mousePosition;

        // 目标位置（逻辑准心的目标XZ坐标）
        private Vector3 targetLogicPosition;

        private void Awake()
        {
            // 初始化输入系统
            inputActions = new InputSystem_Actions();

            // 如果没有指定相机，使用主相机
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            // 验证必需的引用
            if (logicCrosshair == null)
            {
                Debug.LogError("[FisherCrosshairController] 逻辑准心Transform未设置！");
            }

            if (visualCrosshair == null)
            {
                Debug.LogError("[FisherCrosshairController] 视觉准心Transform未设置！");
            }
        }

        private void OnEnable()
        {
            inputActions.Enable();

            // 订阅Look输入（使用鼠标位置）
            inputActions.Player.Look.performed += OnLookPerformed;
            inputActions.Player.Look.canceled += OnLookCanceled;
        }

        private void OnDisable()
        {
            inputActions.Player.Look.performed -= OnLookPerformed;
            inputActions.Player.Look.canceled -= OnLookCanceled;

            inputActions.Disable();
        }

        private void Start()
        {
            // 初始化准心位置（世界坐标原点）
            if (logicCrosshair != null)
            {
                targetLogicPosition = new Vector3(0, fishPlaneY, 0);
                logicCrosshair.position = targetLogicPosition;
            }

            // 同步视觉准心
            SyncVisualCrosshair();
        }

        private void Update()
        {
            // 更新鼠标位置到准心逻辑位置
            UpdateCrosshairPosition();

            // 平滑移动逻辑准心
            MoveLogicCrosshair();

            // 同步视觉准心位置
            SyncVisualCrosshair();
        }

        /// <summary>
        /// 鼠标Look输入回调
        /// </summary>
        private void OnLookPerformed(InputAction.CallbackContext context)
        {
            mousePosition = context.ReadValue<Vector2>();
        }

        /// <summary>
        /// 鼠标Look取消回调
        /// </summary>
        private void OnLookCanceled(InputAction.CallbackContext context)
        {
            mousePosition = Vector2.zero;
        }

        /// <summary>
        /// 通过鼠标位置更新准心目标位置
        /// </summary>
        private void UpdateCrosshairPosition()
        {
            if (targetCamera == null) return;

            // 从相机发射射线到鼠标位置
            Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);

            // 创建鱼平面（Y = fishPlaneY的水平面）
            Plane fishPlane = new Plane(Vector3.up, new Vector3(0, fishPlaneY, 0));

            // 检测射线与平面的交点
            if (fishPlane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);

                // 限制在矩形边界内（与鱼活动范围对等）
                hitPoint.x = Mathf.Clamp(hitPoint.x, -boundarySize.x / 2f, boundarySize.x / 2f);
                hitPoint.z = Mathf.Clamp(hitPoint.z, -boundarySize.y / 2f, boundarySize.y / 2f);
                hitPoint.y = fishPlaneY; // 确保Y坐标固定

                // 更新目标位置
                targetLogicPosition = hitPoint;
            }
        }

        /// <summary>
        /// 平滑移动逻辑准心到目标位置
        /// </summary>
        private void MoveLogicCrosshair()
        {
            if (logicCrosshair == null) return;

            // 使用Lerp实现平滑移动
            logicCrosshair.position = Vector3.Lerp(
                logicCrosshair.position,
                targetLogicPosition,
                smoothSpeed * Time.deltaTime
            );
        }

        /// <summary>
        /// 同步视觉准心位置到逻辑准心的XZ坐标
        /// </summary>
        private void SyncVisualCrosshair()
        {
            if (visualCrosshair == null || logicCrosshair == null) return;

            // 视觉准心使用逻辑准心的XZ坐标，但使用不同的Y坐标
            Vector3 syncPosition = new Vector3(
                logicCrosshair.position.x,
                visualPlaneY,
                logicCrosshair.position.z
            );

            visualCrosshair.position = syncPosition;
        }

        /// <summary>
        /// 获取逻辑准心的当前位置（用于其他系统访问）
        /// </summary>
        public Vector3 GetLogicCrosshairPosition()
        {
            return logicCrosshair != null ? logicCrosshair.position : Vector3.zero;
        }

        /// <summary>
        /// 获取视觉准心的当前位置
        /// </summary>
        public Vector3 GetVisualCrosshairPosition()
        {
            return visualCrosshair != null ? visualCrosshair.position : Vector3.zero;
        }

        // 可视化调试信息
        private void OnDrawGizmos()
        {
            // 绘制移动范围矩形边界（与鱼活动范围对等）
            Gizmos.color = Color.yellow;
            Vector3 boundaryCenter = new Vector3(0, fishPlaneY, 0);
            Vector3 boundaryBoxSize = new Vector3(boundarySize.x, 0.1f, boundarySize.y);
            Gizmos.DrawWireCube(boundaryCenter, boundaryBoxSize);

            if (!Application.isPlaying) return;

            // 绘制逻辑准心位置
            if (logicCrosshair != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(logicCrosshair.position, 0.3f);
                Gizmos.DrawLine(logicCrosshair.position, logicCrosshair.position + Vector3.up * 0.5f);
            }

            // 绘制视觉准心位置
            if (visualCrosshair != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(visualCrosshair.position, 0.3f);
            }

            // 绘制连接线
            if (logicCrosshair != null && visualCrosshair != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(logicCrosshair.position, visualCrosshair.position);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using FishAndFisher.Input; // 引入新的输入管理命名空间

namespace FishAndFisher.Fisher
{
    /// <summary>
    /// 渔夫准心控制器 - 管理逻辑准心和视觉准心的双Transform系统
    /// 使用 InputManager 支持键盘和 ESP32 输入
    /// </summary>
    public class FisherCrosshairController : MonoBehaviour
    {
        [Header("准心引用")]
        [Tooltip("逻辑准心Transform - 在鱼平面上移动，用于碰撞检测")]
        [SerializeField] private Transform logicCrosshair;

        [Tooltip("视觉准心Transform - 显示在倾斜平面上的Quad")]
        [SerializeField] private Transform visualCrosshair;

        [Header("平面设置")]
        [Tooltip("是否使用全局游戏平面高度（GameBoundary.GamePlaneY）")]
        [SerializeField] private bool useGlobalPlaneHeight = true;

        [Tooltip("鱼所在平面的Y坐标（逻辑准心的固定高度，仅在不使用全局平面时有效）")]
        [SerializeField] private float fishPlaneY = 0f;

        [Tooltip("视觉准心Quad平面的Y坐标")]
        [SerializeField] private float visualPlaneY = 5f;

        // 运行时的实际平面高度
        private float actualFishPlaneY;

        [Header("移动设置")]
        [Tooltip("使用全局边界（GameBoundary）")]
        [SerializeField] private bool useGlobalBoundary = true;

        [Tooltip("准心移动速度")]
        [SerializeField] private float moveSpeed = 5f;

        [Tooltip("准心移动平滑度")]
        [SerializeField] private float smoothSpeed = 10f;

        [Header("相机设置")]
        [Tooltip("用于射线检测的相机")]
        [SerializeField] private Camera targetCamera;

        // 目标位置（逻辑准心的目标XZ坐标）
        private Vector3 targetLogicPosition;

        private void Awake()
        {
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

        private void Start()
        {
            // 确定实际平面高度
            UpdateActualFishPlaneY();

            // 初始化准心位置（世界坐标原点）
            if (logicCrosshair != null)
            {
                targetLogicPosition = new Vector3(0, actualFishPlaneY, 0);
                logicCrosshair.position = targetLogicPosition;
            }

            // 同步视觉准心
            SyncVisualCrosshair();
        }

        /// <summary>
        /// 更新实际平面高度（根据配置选择全局或手动设置）
        /// </summary>
        private void UpdateActualFishPlaneY()
        {
            if (useGlobalPlaneHeight && GameBoundary.Instance != null)
            {
                actualFishPlaneY = GameBoundary.Instance.GamePlaneY;

                // 如果手动设置的值与全局值不一致，发出警告
                if (Mathf.Abs(fishPlaneY - actualFishPlaneY) > 0.01f)
                {
                    Debug.LogWarning($"[FisherCrosshairController] 使用全局平面高度 {actualFishPlaneY}，忽略手动设置的 fishPlaneY={fishPlaneY}");
                }
            }
            else
            {
                actualFishPlaneY = fishPlaneY;

                if (useGlobalPlaneHeight && GameBoundary.Instance == null)
                {
                    Debug.LogWarning("[FisherCrosshairController] 未找到 GameBoundary，回退使用手动设置的 fishPlaneY");
                }
            }
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
        /// 通过 InputManager 获取鼠标位置并更新准心目标位置
        /// </summary>
        private void UpdateCrosshairPosition()
        {
            if (targetCamera == null) return;

            // 从 InputManager 获取鼠标位置
            Vector2 mouseScreenPosition = InputManager.Instance.GetLookPosition();

            // 从相机发射射线到鼠标位置
            Ray ray = targetCamera.ScreenPointToRay(mouseScreenPosition);

            // 创建鱼平面（Y = actualFishPlaneY的水平面）
            Plane fishPlane = new Plane(Vector3.up, new Vector3(0, actualFishPlaneY, 0));

            // 检测射线与平面的交点
            if (fishPlane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);

                // 限制在边界内
                if (useGlobalBoundary && GameBoundary.Instance != null)
                {
                    // 使用全局边界
                    hitPoint = GameBoundary.Instance.ClampPointToBounds(hitPoint);
                }

                hitPoint.y = actualFishPlaneY; // 确保Y坐标固定

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

using UnityEngine;

namespace FishAndFisher.Fisher
{
    /// <summary>
    /// 钓鱼线渲染控制器
    /// 持续显示从鱼竿尖端到鱼漂的连线
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class FishingLineController : MonoBehaviour
    {
        [Header("线条端点")]
        [Tooltip("线条起点（通常是鱼竿尖端）")]
        [SerializeField] private Transform startTransform;

        [Tooltip("线条终点（通常是鱼漂/视觉准心位置）")]
        [SerializeField] private Transform endTransform;

        private LineRenderer lineRenderer;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();

            // 初始化LineRenderer设置
            lineRenderer.positionCount = 2;
        }

        private void Start()
        {
            // 启动时启用线条显示
            lineRenderer.enabled = true;
        }

        private void Update()
        {
            if (startTransform == null || endTransform == null)
            {
                lineRenderer.enabled = false;
                return;
            }

            // 确保线条显示
            if (!lineRenderer.enabled)
            {
                lineRenderer.enabled = true;
            }

            // 实时更新两端位置
            lineRenderer.SetPosition(0, startTransform.position);
            lineRenderer.SetPosition(1, endTransform.position);
        }

        /// <summary>
        /// 发射钓鱼线（保留接口以保持兼容性，但现在为空实现）
        /// </summary>
        /// <param name="cooldownTime">冷却时间（不再使用）</param>
        public void FireLine(float cooldownTime)
        {
            // 线条现在持续显示，此方法保留以保持向后兼容
        }

        /// <summary>
        /// 当钓到鱼时的处理（保留接口以保持兼容性）
        /// </summary>
        public void OnFishCaught()
        {
            // 可以在这里添加钓到鱼时的特殊效果
            // 目前保持线条显示
        }

        /// <summary>
        /// 设置线条起点
        /// </summary>
        public void SetStartTransform(Transform start)
        {
            startTransform = start;
        }

        /// <summary>
        /// 设置线条终点
        /// </summary>
        public void SetEndTransform(Transform end)
        {
            endTransform = end;
        }

        /// <summary>
        /// 隐藏线条
        /// </summary>
        public void HideLine()
        {
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }
        }

        /// <summary>
        /// 显示线条
        /// </summary>
        public void ShowLine()
        {
            if (lineRenderer != null)
            {
                lineRenderer.enabled = true;
            }
        }
    }
}

using UnityEngine;

namespace FishAndFisher.Fisher
{
    /// <summary>
    /// 钓鱼线渲染控制器 - Phase1专用
    /// 负责在渔夫攻击时显示从鱼竿到钩子的线条动画
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class FishingLineController : MonoBehaviour
    {
        [Header("线条端点")]
        [Tooltip("线条起点（通常是鱼竿尖端）")]
        [SerializeField] private Transform startTransform;

        [Tooltip("线条终点（通常是逻辑准心位置）")]
        [SerializeField] private Transform endTransform;

        [Header("动画设置")]
        [Tooltip("线条延伸到终点所需时间")]
        [SerializeField] private float extendDuration = 0.1f;

        private LineRenderer lineRenderer;
        private bool isAnimating = false;
        private float animationTimer = 0f;
        private AnimationState currentState = AnimationState.Idle;

        // 发射时记录的固定终点位置
        private Vector3 cachedEndPosition;

        // 线条总显示时间（由外部传入的冷却时间决定）
        private float totalDisplayDuration = 1f;

        private enum AnimationState
        {
            Idle,       // 空闲状态（不显示）
            Extending,  // 延伸中
            Holding,    // 停留在终点
        }

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();

            // 初始化LineRenderer设置
            lineRenderer.positionCount = 2;
            lineRenderer.enabled = false;
        }

        private void Update()
        {
            if (!isAnimating)
                return;

            animationTimer += Time.deltaTime;

            switch (currentState)
            {
                case AnimationState.Extending:
                    UpdateExtending();
                    break;

                case AnimationState.Holding:
                    UpdateHolding();
                    break;
            }
        }

        /// <summary>
        /// 发射钓鱼线（由FisherController调用）
        /// </summary>
        /// <param name="cooldownTime">冷却时间，决定线条总显示时长</param>
        public void FireLine(float cooldownTime)
        {
            if (startTransform == null || endTransform == null)
            {
                Debug.LogWarning("FishingLineController: 起点或终点Transform未设置！");
                return;
            }

            // 记录发射时的终点位置（之后不再随endTransform移动）
            cachedEndPosition = endTransform.position;

            // 设置线条显示时间为冷却时间
            totalDisplayDuration = cooldownTime;

            // 开始延伸动画
            currentState = AnimationState.Extending;
            animationTimer = 0f;
            isAnimating = true;
            lineRenderer.enabled = true;
        }

        /// <summary>
        /// 当钓到鱼时立即隐藏线条（由FisherController调用）
        /// </summary>
        public void OnFishCaught()
        {
            HideLine();
        }

        /// <summary>
        /// 更新延伸动画
        /// </summary>
        private void UpdateExtending()
        {
            float progress = Mathf.Clamp01(animationTimer / extendDuration);

            Vector3 startPos = startTransform.position;

            // 起点始终跟随startTransform
            lineRenderer.SetPosition(0, startPos);

            // 终点使用缓存的固定位置，根据进度从起点向目标点延伸
            Vector3 currentEndPos = Vector3.Lerp(startPos, cachedEndPosition, progress);
            lineRenderer.SetPosition(1, currentEndPos);

            // 延伸完成后进入停留状态
            if (progress >= 1f)
            {
                currentState = AnimationState.Holding;
                // 不重置计时器，继续计算剩余显示时间
            }
        }

        /// <summary>
        /// 更新停留状态
        /// </summary>
        private void UpdateHolding()
        {
            // 起点跟随startTransform，终点使用缓存的固定位置
            lineRenderer.SetPosition(0, startTransform.position);
            lineRenderer.SetPosition(1, cachedEndPosition);

            // 当总计时器超过显示时间时隐藏（显示时间 = 冷却时间）
            if (animationTimer >= totalDisplayDuration)
            {
                HideLine();
            }
        }

        /// <summary>
        /// 隐藏线条
        /// </summary>
        private void HideLine()
        {
            lineRenderer.enabled = false;
            isAnimating = false;
            currentState = AnimationState.Idle;
            animationTimer = 0f;
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

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (extendDuration <= 0f)
                extendDuration = 0.1f;
        }
#endif
    }
}

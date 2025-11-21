using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace FishAndFisher.Phase2
{
    /// <summary>
    /// Phase2全屏转场UI控制器
    /// 实现白色背景+中间图片的淡入淡出效果
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class Phase2TransitionUI : MonoBehaviour
    {
        [Header("转场设置")]
        [Tooltip("淡入持续时间（秒）")]
        [SerializeField] private float fadeInDuration = 0.75f;

        [Tooltip("淡出持续时间（秒）")]
        [SerializeField] private float fadeOutDuration = 0.75f;

        [Header("UI组件")]
        [Tooltip("白色背景Panel的Image组件")]
        [SerializeField] private Image whiteBackground;

        [Tooltip("中间图片的Image组件（可选）")]
        [SerializeField] private Image centerImage;

        // CanvasGroup组件（用于控制整体透明度）
        private CanvasGroup canvasGroup;

        // 是否正在播放转场动画
        private bool isTransitioning = false;

        private void Awake()
        {
            // 获取CanvasGroup组件
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
                // 新创建的CanvasGroup默认alpha为1，手动设置为0
                canvasGroup.alpha = 0f;
            }

            // 不再强制设置alpha，尊重Inspector中配置的值
            // Main场景：在Inspector中设置alpha=0（初始透明，等待淡入）
            // Phase2场景：在Inspector中设置alpha=1（初始不透明，直接淡出）

            // 根据当前alpha值决定是否阻挡射线
            canvasGroup.blocksRaycasts = (canvasGroup.alpha > 0.5f);

            // 确保GameObject初始为激活状态
            gameObject.SetActive(true);

            Debug.Log($"[Phase2TransitionUI] Awake完成，当前alpha={canvasGroup.alpha}");
        }

        /// <summary>
        /// 播放完整的转场动画（淡入 → 回调 → 淡出）
        /// </summary>
        /// <param name="onMidTransition">转场中间时的回调（通常用于场景切换）</param>
        public void PlayTransition(System.Action onMidTransition = null)
        {
            if (isTransitioning)
            {
                Debug.LogWarning("[Phase2TransitionUI] 转场动画已在播放中，忽略重复调用");
                return;
            }

            StartCoroutine(TransitionSequence(onMidTransition));
        }

        /// <summary>
        /// 转场动画序列
        /// </summary>
        private IEnumerator TransitionSequence(System.Action callback)
        {
            isTransitioning = true;

            Debug.Log("[Phase2TransitionUI] 开始转场动画");

            // 阶段1：淡入（0 → 1）
            Debug.Log("[Phase2TransitionUI] 淡入开始");
            yield return StartCoroutine(FadeIn());
            Debug.Log("[Phase2TransitionUI] 淡入完成");

            // 阶段2：执行回调（场景切换）
            if (callback != null)
            {
                Debug.Log("[Phase2TransitionUI] 执行中间回调（场景切换）");
                callback.Invoke();
            }

            // 短暂等待确保场景已切换
            yield return new WaitForSeconds(0.1f);

            // 阶段3：淡出（1 → 0）
            Debug.Log("[Phase2TransitionUI] 淡出开始");
            yield return StartCoroutine(FadeOut());
            Debug.Log("[Phase2TransitionUI] 淡出完成");

            isTransitioning = false;

            Debug.Log("[Phase2TransitionUI] 转场动画完成");
        }

        /// <summary>
        /// 淡入动画（Alpha: 0 → 1）
        /// </summary>
        private IEnumerator FadeIn()
        {
            canvasGroup.blocksRaycasts = true;

            float elapsedTime = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeInDuration;

                // 使用平滑插值
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, t);

                yield return null;
            }

            // 确保最终值为1
            canvasGroup.alpha = 1f;
        }

        /// <summary>
        /// 淡出动画（Alpha: 1 → 0）
        /// </summary>
        private IEnumerator FadeOut()
        {
            float elapsedTime = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeOutDuration;

                // 使用平滑插值
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);

                yield return null;
            }

            // 确保最终值为0
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// 立即显示转场UI（用于测试）
        /// </summary>
        public void ShowImmediate()
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }

        /// <summary>
        /// 立即隐藏转场UI（用于测试）
        /// </summary>
        public void HideImmediate()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// 仅播放淡入动画
        /// </summary>
        public void FadeInOnly(System.Action onComplete = null)
        {
            StartCoroutine(FadeInSequence(onComplete));
        }

        private IEnumerator FadeInSequence(System.Action callback)
        {
            yield return StartCoroutine(FadeIn());
            callback?.Invoke();
        }

        /// <summary>
        /// 仅播放淡出动画
        /// </summary>
        public void FadeOutOnly(System.Action onComplete = null)
        {
            StartCoroutine(FadeOutSequence(onComplete));
        }

        private IEnumerator FadeOutSequence(System.Action callback)
        {
            yield return StartCoroutine(FadeOut());
            callback?.Invoke();
        }

#if UNITY_EDITOR
        /// <summary>
        /// 编辑器中的验证
        /// </summary>
        private void OnValidate()
        {
            // 确保持续时间为正值
            if (fadeInDuration <= 0f)
                fadeInDuration = 0.75f;

            if (fadeOutDuration <= 0f)
                fadeOutDuration = 0.75f;

            // 检查白色背景
            if (whiteBackground == null)
            {
                // 尝试在子对象中找到名为 "WhiteBackground" 的Image
                Transform bg = transform.Find("WhiteBackground");
                if (bg != null)
                    whiteBackground = bg.GetComponent<Image>();
            }

            // 检查中间图片
            if (centerImage == null)
            {
                // 尝试在子对象中找到名为 "CenterImage" 的Image
                Transform center = transform.Find("CenterImage");
                if (center != null)
                    centerImage = center.GetComponent<Image>();
            }
        }
#endif
    }
}

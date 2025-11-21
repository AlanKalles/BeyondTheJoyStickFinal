using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace FishAndFisher
{
    /// <summary>
    /// 抓到鱼提示UI - 显示"抓到鱼!"提示，并在1.5秒后自动触发转场
    /// </summary>
    public class FishCaughtNotificationUI : MonoBehaviour
    {
        [Header("UI组件")]
        [Tooltip("提示文本")]
        [SerializeField] private TextMeshProUGUI notificationText;

        [Tooltip("倒计时文本（已弃用，保留以兼容旧场景）")]
        [SerializeField] private TextMeshProUGUI countdownText;

        [Tooltip("开始争斗按钮（已弃用，保留以兼容旧场景）")]
        [SerializeField] private Button startButton;

        [Tooltip("按钮文本（已弃用，保留以兼容旧场景）")]
        [SerializeField] private TextMeshProUGUI buttonText;

        [Header("动画设置")]
        [Tooltip("弹出动画持续时间")]
        [SerializeField] private float popupDuration = 0.5f;

        [Tooltip("弹出动画缩放倍数")]
        [SerializeField] private float popupScale = 1.2f;

        [Tooltip("显示持续时间（自动触发转场前的等待时间）")]
        [SerializeField] private float displayDuration = 1.5f;

        [Header("文本设置")]
        [Tooltip("提示文本内容")]
        [SerializeField] private string notificationMessage = "抓到鱼了！";

        [Tooltip("按钮文本内容（已弃用）")]
        [SerializeField] private string buttonMessage = "开始争斗！";

        // Canvas Group用于淡入淡出
        private CanvasGroup canvasGroup;

        // 是否正在显示倒计时（已弃用，保留以兼容）
        private bool isShowingCountdown = false;

        // 动画协程引用
        private Coroutine fadeCoroutine;
        private Coroutine scaleCoroutine;
        private Coroutine autoTransitionCoroutine;

        // 自动转场回调事件
        public event System.Action OnAutoTransition;

        private void Awake()
        {
            // 获取或添加CanvasGroup组件
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // 自动查找组件
            if (notificationText == null)
            {
                notificationText = transform.Find("NotificationText")?.GetComponent<TextMeshProUGUI>();
            }

            if (countdownText == null)
            {
                countdownText = transform.Find("CountdownText")?.GetComponent<TextMeshProUGUI>();
            }

            if (startButton == null)
            {
                startButton = GetComponentInChildren<Button>();
            }

            if (buttonText == null && startButton != null)
            {
                buttonText = startButton.GetComponentInChildren<TextMeshProUGUI>();
            }

            // 设置按钮点击事件（保留以兼容旧场景）
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartButtonClicked);
                // 新设计中隐藏按钮
                startButton.gameObject.SetActive(false);
            }

            // 初始化文本
            if (notificationText != null)
            {
                notificationText.text = notificationMessage;
            }

            if (buttonText != null)
            {
                buttonText.text = buttonMessage;
            }

            // 初始隐藏倒计时文本
            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(false);
            }

            // 初始化隐藏
            Hide();
        }

        /// <summary>
        /// 显示提示UI并在displayDuration秒后自动触发转场
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            isShowingCountdown = false;

            // 隐藏旧的UI元素
            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(false);
            }

            if (startButton != null)
            {
                startButton.gameObject.SetActive(false);
            }

            // 停止之前的动画
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
            if (autoTransitionCoroutine != null) StopCoroutine(autoTransitionCoroutine);

            // 淡入动画
            fadeCoroutine = StartCoroutine(FadeIn(0.3f));

            // 弹出动画
            scaleCoroutine = StartCoroutine(ScalePopup(popupDuration));

            // 启动自动转场计时器
            autoTransitionCoroutine = StartCoroutine(AutoTransitionSequence());

            Debug.Log($"[FishCaughtNotificationUI] 提示UI已显示，将在 {displayDuration} 秒后自动触发转场");
        }

        /// <summary>
        /// 自动转场序列协程
        /// </summary>
        private IEnumerator AutoTransitionSequence()
        {
            // 等待指定时间
            yield return new WaitForSeconds(displayDuration);

            // 触发转场回调
            Debug.Log("[FishCaughtNotificationUI] 自动触发转场");
            OnAutoTransition?.Invoke();

            // 淡出隐藏
            Hide();
        }

        /// <summary>
        /// 隐藏提示UI
        /// </summary>
        public void Hide()
        {
            // 停止之前的动画
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);

            // 淡出动画
            fadeCoroutine = StartCoroutine(FadeOut(0.3f));

            Debug.Log("[FishCaughtNotificationUI] 提示UI已隐藏");
        }

        /// <summary>
        /// 更新倒计时显示
        /// </summary>
        public void UpdateCountdown(int seconds)
        {
            if (!isShowingCountdown)
            {
                // 第一次显示倒计时，隐藏按钮
                isShowingCountdown = true;

                if (startButton != null)
                {
                    startButton.gameObject.SetActive(false);
                }

                if (countdownText != null)
                {
                    countdownText.gameObject.SetActive(true);
                }
            }

            // 更新倒计时文本
            if (countdownText != null)
            {
                countdownText.text = seconds.ToString();

                // 脉冲动画
                StartCoroutine(CountdownPulse());
            }
        }

        /// <summary>
        /// 开始按钮点击事件（已弃用，保留以兼容旧场景）
        /// </summary>
        private void OnStartButtonClicked()
        {
            // 已弃用：新设计使用自动转场，不需要手动点击按钮
            Debug.Log("[FishCaughtNotificationUI] 开始按钮已点击（已弃用，使用自动转场）");
        }

        private void OnDestroy()
        {
            // 停止所有协程
            StopAllCoroutines();

            // 移除按钮监听
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(OnStartButtonClicked);
            }

            // 清空事件
            OnAutoTransition = null;
        }

        /// <summary>
        /// 淡入协程
        /// </summary>
        private IEnumerator FadeIn(float duration)
        {
            canvasGroup.alpha = 0f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        /// <summary>
        /// 淡出协程
        /// </summary>
        private IEnumerator FadeOut(float duration)
        {
            float startAlpha = canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 弹出缩放协程
        /// </summary>
        private IEnumerator ScalePopup(float duration)
        {
            transform.localScale = Vector3.zero;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // OutBack缓动曲线模拟
                float overshoot = 1.70158f;
                t = t - 1f;
                float scale = t * t * ((overshoot + 1f) * t + overshoot) + 1f;

                transform.localScale = Vector3.one * scale;
                yield return null;
            }

            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// 倒计时脉冲动画协程
        /// </summary>
        private IEnumerator CountdownPulse()
        {
            if (countdownText == null) yield break;

            countdownText.transform.localScale = Vector3.one * 1.5f;
            float elapsed = 0f;
            float duration = 0.3f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float scale = Mathf.Lerp(1.5f, 1f, elapsed / duration);
                countdownText.transform.localScale = Vector3.one * scale;
                yield return null;
            }

            countdownText.transform.localScale = Vector3.one;
        }
    }
}

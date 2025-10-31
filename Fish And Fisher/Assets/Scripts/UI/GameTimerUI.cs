using UnityEngine;
using TMPro;

namespace FishAndFisher
{
    /// <summary>
    /// 游戏计时器UI - 显示倒计时
    /// </summary>
    public class GameTimerUI : MonoBehaviour
    {
        [Header("UI引用")]
        [Tooltip("显示时间的TextMeshPro组件")]
        [SerializeField] private TextMeshProUGUI timerText;

        [Header("颜色设置")]
        [Tooltip("正常时间的颜色")]
        [SerializeField] private Color normalColor = Color.white;

        [Tooltip("警告时间的颜色（时间不足时）")]
        [SerializeField] private Color warningColor = Color.yellow;

        [Tooltip("危险时间的颜色（时间紧急时）")]
        [SerializeField] private Color dangerColor = Color.red;

        [Header("警告阈值")]
        [Tooltip("警告时间阈值（秒）")]
        [SerializeField] private float warningThreshold = 30f;

        [Tooltip("危险时间阈值（秒）")]
        [SerializeField] private float dangerThreshold = 10f;

        [Header("动画设置")]
        [Tooltip("是否启用脉冲动画")]
        [SerializeField] private bool enablePulseAnimation = true;

        [Tooltip("脉冲动画速度")]
        [SerializeField] private float pulseSpeed = 2f;

        [Tooltip("脉冲缩放范围")]
        [SerializeField] private float pulseScale = 1.2f;

        private bool isPulsing = false;
        private float pulseTimer = 0f;
        private Vector3 originalScale;

        private void Awake()
        {
            // 验证引用
            if (timerText == null)
            {
                Debug.LogError("[GameTimerUI] TextMeshProUGUI组件未设置！请在Inspector中分配。");
            }

            // 保存原始缩放
            if (timerText != null)
            {
                originalScale = timerText.transform.localScale;
            }
        }

        private void Update()
        {
            // 更新脉冲动画
            if (isPulsing && enablePulseAnimation)
            {
                UpdatePulseAnimation();
            }
        }

        /// <summary>
        /// 更新计时器显示
        /// </summary>
        public void UpdateTimer(float remainingTime)
        {
            if (timerText == null) return;

            // 格式化时间（MM:SS）
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            // 根据剩余时间更新颜色
            UpdateTimerColor(remainingTime);

            // 在危险时间启用脉冲动画
            isPulsing = remainingTime <= dangerThreshold && remainingTime > 0f;
        }

        /// <summary>
        /// 更新计时器颜色
        /// </summary>
        private void UpdateTimerColor(float remainingTime)
        {
            if (timerText == null) return;

            Color targetColor = normalColor;

            if (remainingTime <= dangerThreshold)
            {
                targetColor = dangerColor;
            }
            else if (remainingTime <= warningThreshold)
            {
                targetColor = warningColor;
            }

            timerText.color = targetColor;
        }

        /// <summary>
        /// 更新脉冲动画
        /// </summary>
        private void UpdatePulseAnimation()
        {
            if (timerText == null) return;

            pulseTimer += Time.deltaTime * pulseSpeed;

            // 使用Sin曲线实现脉冲效果
            float scale = 1f + Mathf.Sin(pulseTimer) * (pulseScale - 1f);
            timerText.transform.localScale = originalScale * scale;
        }

        /// <summary>
        /// 重置计时器显示
        /// </summary>
        public void ResetTimer()
        {
            if (timerText == null) return;

            timerText.text = "00:00";
            timerText.color = normalColor;
            timerText.transform.localScale = originalScale;
            isPulsing = false;
            pulseTimer = 0f;
        }

        /// <summary>
        /// 显示计时器
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 隐藏计时器
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}

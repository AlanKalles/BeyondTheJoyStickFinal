using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FishAndFisher
{
    /// <summary>
    /// Phase2进度条UI - 可视化显示Phase2争斗进度
    /// </summary>
    public class Phase2ProgressBarUI : MonoBehaviour
    {
        [Header("UI组件")]
        [Tooltip("进度条Slider组件")]
        [SerializeField] private Slider progressSlider;

        [Tooltip("进度条填充图像（用于颜色渐变）")]
        [SerializeField] private Image fillImage;

        [Tooltip("进度百分比文本")]
        [SerializeField] private TextMeshProUGUI percentageText;

        [Header("颜色设置")]
        [Tooltip("左侧颜色（鱼）")]
        [SerializeField] private Color fishColor = new Color(0.2f, 0.5f, 1f); // 蓝色

        [Tooltip("中间颜色")]
        [SerializeField] private Color neutralColor = Color.white;

        [Tooltip("右侧颜色（渔夫）")]
        [SerializeField] private Color fisherColor = new Color(1f, 0.6f, 0.2f); // 橙色

        [Header("进度条数据引用")]
        [Tooltip("Phase2进度条逻辑")]
        [SerializeField] private Phase2.Phase2ProgressBar phase2ProgressBar;

        [Header("动画设置")]
        [Tooltip("进度条平滑移动速度")]
        [SerializeField] private float smoothSpeed = 5f;

        // 当前显示的进度值（用于平滑插值）
        private float displayProgress = 50f;

        private void Awake()
        {
            // 自动查找组件（如果未设置）
            if (progressSlider == null)
            {
                progressSlider = GetComponentInChildren<Slider>();
            }

            if (fillImage == null && progressSlider != null)
            {
                fillImage = progressSlider.fillRect?.GetComponent<Image>();
            }

            if (phase2ProgressBar == null)
            {
                phase2ProgressBar = FindFirstObjectByType<Phase2.Phase2ProgressBar>();
            }

            // 验证引用
            if (progressSlider == null)
            {
                Debug.LogError("[Phase2ProgressBarUI] Slider组件未找到！");
            }

            if (phase2ProgressBar == null)
            {
                Debug.LogError("[Phase2ProgressBarUI] Phase2ProgressBar逻辑未找到！");
            }

            // 初始化隐藏
            Hide();
        }

        private void Update()
        {
            if (phase2ProgressBar == null || !gameObject.activeSelf)
            {
                return;
            }

            // 获取目标进度
            float targetProgress = phase2ProgressBar.CurrentProgress;

            // 平滑插值到目标进度
            displayProgress = Mathf.Lerp(displayProgress, targetProgress, Time.deltaTime * smoothSpeed);

            // 更新UI
            UpdateProgressBar();
        }

        /// <summary>
        /// 显示进度条UI
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);

            // 重置到中间位置
            if (phase2ProgressBar != null)
            {
                displayProgress = phase2ProgressBar.CurrentProgress;
            }

            Debug.Log("[Phase2ProgressBarUI] 进度条UI已显示");
        }

        /// <summary>
        /// 隐藏进度条UI
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            Debug.Log("[Phase2ProgressBarUI] 进度条UI已隐藏");
        }

        /// <summary>
        /// 更新进度条显示
        /// </summary>
        private void UpdateProgressBar()
        {
            if (progressSlider == null || phase2ProgressBar == null)
            {
                return;
            }

            // 设置Slider范围和值
            progressSlider.minValue = 0f;
            progressSlider.maxValue = phase2ProgressBar.MaxProgress;
            progressSlider.value = displayProgress;

            // 计算进度百分比（0-1）
            float percentage = displayProgress / phase2ProgressBar.MaxProgress;

            // 更新颜色渐变
            UpdateFillColor(percentage);

            // 更新百分比文本
            UpdatePercentageText(percentage);
        }

        /// <summary>
        /// 更新填充颜色（渐变）
        /// </summary>
        private void UpdateFillColor(float percentage)
        {
            if (fillImage == null)
            {
                return;
            }

            // 三段渐变：0-0.5（鱼色到中性色），0.5-1（中性色到渔夫色）
            Color targetColor;

            if (percentage < 0.5f)
            {
                // 左半段：鱼色 → 中性色
                float t = percentage * 2f; // 0-0.5 映射到 0-1
                targetColor = Color.Lerp(fishColor, neutralColor, t);
            }
            else
            {
                // 右半段：中性色 → 渔夫色
                float t = (percentage - 0.5f) * 2f; // 0.5-1 映射到 0-1
                targetColor = Color.Lerp(neutralColor, fisherColor, t);
            }

            fillImage.color = targetColor;
        }

        /// <summary>
        /// 更新百分比文本
        /// </summary>
        private void UpdatePercentageText(float percentage)
        {
            if (percentageText == null)
            {
                return;
            }

            // 显示百分比和提示文字
            string statusText = "";

            if (percentage < 0.25f)
            {
                statusText = "Fish is winning!";
            }
            else if (percentage < 0.45f)
            {
                statusText = "Fish in advantage!";
            }
            else if (percentage < 0.55f)
            {
                statusText = "Fair";
            }
            else if (percentage < 0.75f)
            {
                statusText = "Fisher in advantage!";
            }
            else
            {
                statusText = "Fisher is winning!";
            }

            percentageText.text = $"{(percentage * 100f):F0}% - {statusText}";
        }

        /// <summary>
        /// 设置进度条逻辑引用（运行时设置）
        /// </summary>
        public void SetProgressBar(Phase2.Phase2ProgressBar progressBar)
        {
            phase2ProgressBar = progressBar;
        }
    }
}

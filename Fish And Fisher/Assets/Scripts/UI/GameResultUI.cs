using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FishAndFisher
{
    /// <summary>
    /// 游戏结果UI - 显示胜负结果面板
    /// </summary>
    public class GameResultUI : MonoBehaviour
    {
        [Header("UI引用")]
        [Tooltip("结果面板Panel")]
        [SerializeField] private GameObject resultPanel;

        [Tooltip("胜利者文本")]
        [SerializeField] private TextMeshProUGUI winnerText;

        [Tooltip("结果描述文本")]
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("按钮")]
        [Tooltip("重新开始按钮")]
        [SerializeField] private Button restartButton;

        [Tooltip("退出按钮")]
        [SerializeField] private Button quitButton;

        [Header("文本设置")]
        [Tooltip("鱼胜利时的标题")]
        [SerializeField] private string fishWinTitle = "FISH WINS!";

        [Tooltip("鱼胜利时的描述")]
        [SerializeField] private string fishWinDescription = "The fish successfully escaped!";

        [Tooltip("渔夫胜利时的标题")]
        [SerializeField] private string fisherWinTitle = "FISHER WINS!";

        [Tooltip("渔夫胜利时的描述")]
        [SerializeField] private string fisherWinDescription = "The fisher caught the fish!";

        [Header("颜色设置")]
        [Tooltip("鱼胜利颜色")]
        [SerializeField] private Color fishWinColor = new Color(0.2f, 0.6f, 0.9f); // 蓝色

        [Tooltip("渔夫胜利颜色")]
        [SerializeField] private Color fisherWinColor = new Color(0.9f, 0.6f, 0.2f); // 橙色

        [Header("动画设置")]
        [Tooltip("面板显示动画时长")]
        [SerializeField] private float showAnimationDuration = 0.5f;

        [Tooltip("是否启用缩放动画")]
        [SerializeField] private bool enableScaleAnimation = true;

        private CanvasGroup canvasGroup;
        private bool isAnimating = false;
        private float animationTimer = 0f;

        private void Awake()
        {
            // 验证引用
            if (resultPanel == null)
            {
                Debug.LogError("[GameResultUI] 结果面板Panel未设置！");
                return;
            }

            // 获取或添加CanvasGroup
            canvasGroup = resultPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = resultPanel.AddComponent<CanvasGroup>();
            }

            // 绑定按钮事件
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartButtonClicked);
            }
            else
            {
                Debug.LogWarning("[GameResultUI] 重新开始按钮未设置！");
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitButtonClicked);
            }
            else
            {
                Debug.LogWarning("[GameResultUI] 退出按钮未设置！");
            }

            // 初始隐藏
            Hide();
        }

        private void Update()
        {
            // 更新显示动画
            if (isAnimating)
            {
                UpdateShowAnimation();
            }
        }

        /// <summary>
        /// 显示结果面板
        /// </summary>
        public void ShowResult(GameResult result)
        {
            if (resultPanel == null) return;

            // 设置文本内容
            switch (result)
            {
                case GameResult.FishWins:
                    SetResultContent(fishWinTitle, fishWinDescription, fishWinColor);
                    break;
                case GameResult.FisherWins:
                    SetResultContent(fisherWinTitle, fisherWinDescription, fisherWinColor);
                    break;
            }

            // 显示面板
            Show();

            Debug.Log($"[GameResultUI] 显示结果: {result}");
        }

        /// <summary>
        /// 设置结果内容
        /// </summary>
        private void SetResultContent(string title, string description, Color color)
        {
            if (winnerText != null)
            {
                winnerText.text = title;
                winnerText.color = color;
            }

            if (descriptionText != null)
            {
                descriptionText.text = description;
            }
        }

        /// <summary>
        /// 显示面板
        /// </summary>
        public void Show()
        {
            if (resultPanel == null) return;

            resultPanel.SetActive(true);

            // 启动动画
            if (enableScaleAnimation)
            {
                isAnimating = true;
                animationTimer = 0f;
                resultPanel.transform.localScale = Vector3.zero;
                canvasGroup.alpha = 0f;
            }
            else
            {
                resultPanel.transform.localScale = Vector3.one;
                canvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        public void Hide()
        {
            if (resultPanel == null) return;

            resultPanel.SetActive(false);
            isAnimating = false;
        }

        /// <summary>
        /// 更新显示动画
        /// </summary>
        private void UpdateShowAnimation()
        {
            animationTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(animationTimer / showAnimationDuration);

            // 使用缓动函数（EaseOutBack）
            float scale = EaseOutBack(progress);

            resultPanel.transform.localScale = Vector3.one * scale;
            canvasGroup.alpha = progress;

            // 动画完成
            if (progress >= 1f)
            {
                isAnimating = false;
            }
        }

        /// <summary>
        /// 缓动函数 - EaseOutBack
        /// </summary>
        private float EaseOutBack(float t)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1f;

            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        /// <summary>
        /// 重新开始按钮点击
        /// </summary>
        private void OnRestartButtonClicked()
        {
            Debug.Log("[GameResultUI] 点击重新开始");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
            else
            {
                Debug.LogError("[GameResultUI] GameManager不存在！");
            }
        }

        /// <summary>
        /// 退出按钮点击
        /// </summary>
        private void OnQuitButtonClicked()
        {
            Debug.Log("[GameResultUI] 点击退出");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
            else
            {
                Debug.LogError("[GameResultUI] GameManager不存在！");
            }
        }

        /// <summary>
        /// 在编辑器中重置组件
        /// </summary>
        private void Reset()
        {
            // 设置默认值
            fishWinTitle = "FISH WINS!";
            fishWinDescription = "The fish successfully escaped!";
            fisherWinTitle = "FISHER WINS!";
            fisherWinDescription = "The fisher caught the fish!";
            fishWinColor = new Color(0.2f, 0.6f, 0.9f);
            fisherWinColor = new Color(0.9f, 0.6f, 0.2f);
            showAnimationDuration = 0.5f;
            enableScaleAnimation = true;
        }
    }
}

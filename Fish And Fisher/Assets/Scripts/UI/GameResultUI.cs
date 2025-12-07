using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using FishAndFisher.Phase2;

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

        [Header("结果图片设置")]
        [Tooltip("结果图片的Image组件")]
        [SerializeField] private Image resultImage;

        [Tooltip("鱼胜利时显示的图片")]
        [SerializeField] private Sprite fishWinSprite;

        [Tooltip("渔夫胜利时显示的图片")]
        [SerializeField] private Sprite fisherWinSprite;

        [Header("按钮（可选，新设计不使用）")]
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
        [Tooltip("淡入动画时长")]
        [SerializeField] private float fadeInDuration = 0.5f;

        [Header("返回设置")]
        [Tooltip("是否启用按任意键返回")]
        [SerializeField] private bool enableAnyKeyReturn = true;

        [Tooltip("动画完成后的等待时间（防止立即触发返回）")]
        [SerializeField] private float returnDelayAfterAnimation = 0.3f;

        [Tooltip("返回的目标场景名")]
        [SerializeField] private string titleSceneName = "TitlePage";

        [Header("ESP32设置")]
        [Tooltip("是否启用ESP32按钮触发返回")]
        [SerializeField] private bool enableESP32Return = true;

        private CanvasGroup canvasGroup;
        private bool isAnimating = false;
        private float animationTimer = 0f;
        private bool isFullyDisplayed = false;
        private bool wasESP32ButtonPressed = false;

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

            // 检测按任意键返回
            if (isFullyDisplayed && enableAnyKeyReturn)
            {
                CheckReturnInput();
            }
        }

        /// <summary>
        /// 显示结果面板
        /// </summary>
        public void ShowResult(GameResult result)
        {
            if (resultPanel == null) return;

            // 设置文本和图片内容
            switch (result)
            {
                case GameResult.FishWins:
                    SetResultContent(fishWinTitle, fishWinDescription, fishWinColor);
                    SetResultImage(fishWinSprite);
                    break;
                case GameResult.FisherWins:
                    SetResultContent(fisherWinTitle, fisherWinDescription, fisherWinColor);
                    SetResultImage(fisherWinSprite);
                    break;
            }

            // 重置状态
            isFullyDisplayed = false;
            wasESP32ButtonPressed = false;

            // 显示面板
            Show();

            Debug.Log($"[GameResultUI] 显示结果: {result}");
        }

        /// <summary>
        /// 设置结果图片
        /// </summary>
        private void SetResultImage(Sprite sprite)
        {
            if (resultImage != null)
            {
                if (sprite != null)
                {
                    resultImage.sprite = sprite;
                    resultImage.gameObject.SetActive(true);
                }
                else
                {
                    resultImage.gameObject.SetActive(false);
                }
            }
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

            // 隐藏按钮（新设计使用按任意键返回）
            if (restartButton != null)
            {
                restartButton.gameObject.SetActive(false);
            }
            if (quitButton != null)
            {
                quitButton.gameObject.SetActive(false);
            }

            // 启动淡入动画
            isAnimating = true;
            animationTimer = 0f;
            resultPanel.transform.localScale = Vector3.one;
            canvasGroup.alpha = 0f;
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
        /// 更新显示动画（纯淡入）
        /// </summary>
        private void UpdateShowAnimation()
        {
            animationTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(animationTimer / fadeInDuration);

            // 纯淡入动画
            canvasGroup.alpha = progress;

            // 动画完成
            if (progress >= 1f)
            {
                isAnimating = false;
                // 延迟启用返回功能
                StartCoroutine(DelayedEnableReturn());
            }
        }

        /// <summary>
        /// 延迟启用返回功能
        /// </summary>
        private IEnumerator DelayedEnableReturn()
        {
            yield return new WaitForSeconds(returnDelayAfterAnimation);
            isFullyDisplayed = true;
            Debug.Log("[GameResultUI] 按任意键返回标题页面");
        }

        /// <summary>
        /// 检测返回输入（键鼠任意键 + ESP32按钮）
        /// </summary>
        private void CheckReturnInput()
        {
            bool shouldReturn = false;

            // 检测键鼠任意键
            if (UnityEngine.Input.anyKeyDown)
            {
                Debug.Log("[GameResultUI] 检测到键鼠输入");
                shouldReturn = true;
            }

            // 检测ESP32渔夫按钮
            if (enableESP32Return && !shouldReturn)
            {
                shouldReturn = CheckESP32Button();
            }

            // 执行返回
            if (shouldReturn)
            {
                ReturnToTitlePage();
            }
        }

        /// <summary>
        /// 检测ESP32按钮输入（边沿触发）
        /// </summary>
        private bool CheckESP32Button()
        {
            FisherReceiver fisherReceiver = null;

            // 优先从ESP32Manager获取
            if (ESP32Manager.Instance != null)
            {
                fisherReceiver = ESP32Manager.Instance.fisherReceiver;
            }

            // 如果ESP32Manager不存在，尝试直接查找
            if (fisherReceiver == null)
            {
                fisherReceiver = FindFirstObjectByType<FisherReceiver>();
            }

            // 检查连接状态和按钮输入
            if (fisherReceiver != null && fisherReceiver.isConnected)
            {
                bool isButtonPressed = fisherReceiver.buttonPressed;

                // 边沿检测：只在按钮从未按下变为按下时触发
                if (isButtonPressed && !wasESP32ButtonPressed)
                {
                    wasESP32ButtonPressed = isButtonPressed;
                    Debug.Log("[GameResultUI] 检测到ESP32按钮输入");
                    return true;
                }

                wasESP32ButtonPressed = isButtonPressed;
            }
            else
            {
                wasESP32ButtonPressed = false;
            }

            return false;
        }

        /// <summary>
        /// 返回标题页面
        /// </summary>
        private void ReturnToTitlePage()
        {
            Debug.Log($"[GameResultUI] 正在返回场景: {titleSceneName}");

            // 禁用进一步的输入检测
            isFullyDisplayed = false;

            UnityEngine.SceneManagement.SceneManager.LoadScene(titleSceneName);
        }

        /// <summary>
        /// 重新开始按钮点击
        /// </summary>
        private void OnRestartButtonClicked()
        {
            Debug.Log("[GameResultUI] 点击重新开始");

            // 优先尝试GameManager（Phase1），然后尝试通过场景管理器重启（Phase2）
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
            else
            {
                // Phase2场景中，直接重新加载Main场景
                Debug.Log("[GameResultUI] 从Phase2返回Main场景");
                UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
            }
        }

        /// <summary>
        /// 退出按钮点击
        /// </summary>
        private void OnQuitButtonClicked()
        {
            Debug.Log("[GameResultUI] 点击退出");

            // 优先尝试GameManager，否则直接退出
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
            else
            {
                Debug.Log("[GameResultUI] 退出游戏");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
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
            fadeInDuration = 0.5f;
            enableAnyKeyReturn = true;
            returnDelayAfterAnimation = 0.3f;
            titleSceneName = "TitlePage";
            enableESP32Return = true;
        }
    }
}

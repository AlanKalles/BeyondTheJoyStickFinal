using UnityEngine;

namespace FishAndFisher.Phase2
{
    /// <summary>
    /// 游戏结果枚举（Phase2使用）
    /// </summary>
    public enum GameResult
    {
        FishWins,   // 鱼胜利
        FisherWins  // 渔夫胜利
    }

    /// <summary>
    /// Phase2管理器 - 统一管理Phase2的所有子系统
    /// 包括倒计时、胜负判定、结果显示
    /// </summary>
    public class Phase2Manager : MonoBehaviour
    {
        [Header("子系统引用")]
        [Tooltip("输入归一化器")]
        [SerializeField] private Phase2InputNormalizer inputNormalizer;

        [Tooltip("力度检测器")]
        [SerializeField] private Phase2ForceDetector forceDetector;

        [Tooltip("进度条逻辑")]
        [SerializeField] private Phase2ProgressBar progressBar;

        [Tooltip("音效管理器")]
        [SerializeField] private Phase2AudioManager audioManager;

        [Header("玩家引用")]
        [Tooltip("鱼玩家控制器")]
        [SerializeField] private Fish.FishController fishController;

        [Tooltip("渔夫控制器")]
        [SerializeField] private Fisher.FisherController fisherController;

        [Header("UI引用")]
        [Tooltip("进度条UI")]
        [SerializeField] private Phase2ProgressBarUI progressBarUI;

        [Tooltip("计时器UI")]
        [SerializeField] private GameTimerUI timerUI;

        [Tooltip("结果UI")]
        [SerializeField] private GameResultUI resultUI;

        // Phase2倒计时
        private float remainingTime;
        private bool isPhase2Running = false;

        // 单例模式
        private static Phase2Manager instance;
        public static Phase2Manager Instance => instance;

        private void Awake()
        {
            // 单例模式
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }

            // 自动查找组件（如果未设置）
            AutoFindComponents();

            // 从Phase2DataTransfer读取剩余时间
            InitializeFromDataTransfer();
        }

        /// <summary>
        /// 从Phase2DataTransfer初始化数据
        /// </summary>
        private void InitializeFromDataTransfer()
        {
            var dataTransfer = Phase2DataTransfer.Instance;
            if (dataTransfer != null)
            {
                remainingTime = dataTransfer.RemainingTime;
                Debug.Log($"[Phase2Manager] 从DataTransfer读取剩余时间：{remainingTime:F1}秒");
            }
            else
            {
                Debug.LogError("[Phase2Manager] Phase2DataTransfer未找到！使用默认时间60秒");
                remainingTime = 60f;
            }
        }

        /// <summary>
        /// 自动查找组件
        /// </summary>
        private void AutoFindComponents()
        {
            if (inputNormalizer == null)
            {
                inputNormalizer = GetComponent<Phase2InputNormalizer>();
                if (inputNormalizer == null)
                {
                    inputNormalizer = FindFirstObjectByType<Phase2InputNormalizer>();
                }
            }

            if (forceDetector == null)
            {
                forceDetector = GetComponent<Phase2ForceDetector>();
                if (forceDetector == null)
                {
                    forceDetector = FindFirstObjectByType<Phase2ForceDetector>();
                }
            }

            if (progressBar == null)
            {
                progressBar = GetComponent<Phase2ProgressBar>();
                if (progressBar == null)
                {
                    progressBar = FindFirstObjectByType<Phase2ProgressBar>();
                }
            }

            if (audioManager == null)
            {
                audioManager = FindFirstObjectByType<Phase2AudioManager>();
            }

            if (progressBarUI == null)
            {
                progressBarUI = FindFirstObjectByType<Phase2ProgressBarUI>();
            }

            if (fishController == null)
            {
                fishController = FindFirstObjectByType<Fish.FishController>();
            }

            if (fisherController == null)
            {
                fisherController = FindFirstObjectByType<Fisher.FisherController>();
            }

            if (timerUI == null)
            {
                timerUI = FindFirstObjectByType<GameTimerUI>();
            }

            if (resultUI == null)
            {
                resultUI = FindFirstObjectByType<GameResultUI>();
            }
        }

        /// <summary>
        /// 开始Phase2
        /// </summary>
        public void StartPhase2()
        {
            Debug.Log("[Phase2Manager] 开始Phase2争斗阶段");

            isPhase2Running = true;

            // 1. 启用输入归一化
            if (inputNormalizer != null)
            {
                inputNormalizer.ActivatePhase2Input();
            }

            // 2. 启用力度检测
            if (forceDetector != null)
            {
                forceDetector.ActivatePhase2Force();
            }

            // 3. 显示进度条UI
            if (progressBarUI != null)
            {
                progressBarUI.Show();
            }
            else
            {
                Debug.LogWarning("[Phase2Manager] 进度条UI未设置！");
            }

            // 4. 启用玩家Phase2模式
            if (fishController != null)
            {
                fishController.EnablePhase2Mode();
            }

            if (fisherController != null)
            {
                fisherController.EnablePhase2Mode();
            }

            // 5. 通知ESP32控制器切换到Phase2
            if (ESP32FishController.Instance != null)
            {
                ESP32FishController.Instance.SetGamePhase(2);
                Debug.Log("[Phase2Manager] 已通知ESP32FishController切换到Phase2");
            }

            if (ESP32FisherController.Instance != null)
            {
                ESP32FisherController.Instance.SetGamePhase(2);
                Debug.Log("[Phase2Manager] 已通知ESP32FisherController切换到Phase2");
            }

            // 5. 播放Phase2开始音效
            if (audioManager != null)
            {
                audioManager.PlayPhase2StartSound();
                audioManager.PlayStruggleLoopSound();
            }

            // 6. 启动进度条逻辑
            if (progressBar != null)
            {
                progressBar.StartStruggle();
            }

            // 7. 显示计时器UI并开始倒计时
            if (timerUI != null)
            {
                timerUI.UpdateTimer(remainingTime);
            }
        }

        /// <summary>
        /// 结束Phase2
        /// </summary>
        public void EndPhase2()
        {
            Debug.Log("[Phase2Manager] Phase2争斗阶段结束");

            isPhase2Running = false;

            // 1. 停用输入归一化
            if (inputNormalizer != null)
            {
                inputNormalizer.DeactivatePhase2Input();
            }

            // 2. 停用力度检测
            if (forceDetector != null)
            {
                forceDetector.DeactivatePhase2Force();
            }

            // 3. 隐藏进度条UI
            if (progressBarUI != null)
            {
                progressBarUI.Hide();
            }

            // 4. 停止进度条逻辑
            if (progressBar != null)
            {
                progressBar.StopStruggle();
            }

            // 5. 禁用玩家Phase2模式
            if (fishController != null)
            {
                fishController.DisablePhase2Mode();
            }

            if (fisherController != null)
            {
                fisherController.DisablePhase2Mode();
            }

            // 6. 通知ESP32控制器切换回Phase1
            if (ESP32FishController.Instance != null)
            {
                ESP32FishController.Instance.SetGamePhase(1);
            }

            if (ESP32FisherController.Instance != null)
            {
                ESP32FisherController.Instance.SetGamePhase(1);
            }

            // 7. 停止争斗音效
            if (audioManager != null)
            {
                audioManager.StopStruggleLoopSound();
            }
        }

        /// <summary>
        /// 进度条完成回调（由Phase2ProgressBar调用）
        /// </summary>
        /// <param name="fisherWins">渔夫是否胜利</param>
        public void OnProgressComplete(bool fisherWins)
        {
            Debug.Log($"[Phase2Manager] 进度条判定完成：{(fisherWins ? "渔夫胜利" : "鱼胜利")}");

            // 结束Phase2系统
            EndPhase2();

            // 显示结果
            ShowGameResult(fisherWins ? GameResult.FisherWins : GameResult.FishWins);
        }

        /// <summary>
        /// 时间到回调
        /// </summary>
        private void OnTimeUp()
        {
            Debug.Log("[Phase2Manager] 时间到！根据进度条位置判定胜负");

            // 结束Phase2系统
            EndPhase2();

            // 根据进度条位置判定胜负
            if (progressBar != null)
            {
                float progress = progressBar.GetCurrentProgress();
                bool fisherWins = progress >= 50f;
                Debug.Log($"[Phase2Manager] 进度条位置：{progress:F1}，{(fisherWins ? "渔夫胜利" : "鱼胜利")}");
                ShowGameResult(fisherWins ? GameResult.FisherWins : GameResult.FishWins);
            }
            else
            {
                Debug.LogWarning("[Phase2Manager] 进度条未找到，默认鱼胜利");
                ShowGameResult(GameResult.FishWins);
            }
        }

        /// <summary>
        /// 显示游戏结果
        /// </summary>
        private void ShowGameResult(GameResult result)
        {
            Debug.Log($"[Phase2Manager] 显示游戏结果：{result}");

            if (resultUI != null)
            {
                resultUI.ShowResult(result);
            }
            else
            {
                Debug.LogError("[Phase2Manager] 结果UI未设置！");
            }

            // Phase2结束后销毁Phase2DataTransfer
            Phase2DataTransfer.DestroyInstance();
        }

        /// <summary>
        /// 更新Phase2系统
        /// </summary>
        private void Update()
        {
            if (!isPhase2Running)
            {
                return;
            }

            // 更新倒计时
            UpdateTimer();

            // Phase2期间，将输入归一化器的方向同步到鱼玩家
            if (inputNormalizer != null && fishController != null)
            {
                fishController.SetPhase2DirectionInput(inputNormalizer.FishDirection);
            }
        }

        /// <summary>
        /// 更新计时器
        /// </summary>
        private void UpdateTimer()
        {
            remainingTime -= Time.deltaTime;

            // 更新UI
            if (timerUI != null)
            {
                timerUI.UpdateTimer(remainingTime);
            }

            // 检查时间是否耗尽
            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                OnTimeUp();
            }
        }
    }
}

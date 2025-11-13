using UnityEngine;
using UnityEngine.SceneManagement;

namespace FishAndFisher
{
    /// <summary>
    /// 游戏管理器 - 管理游戏状态、计时和胜负判定
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("游戏设置")]
        [Tooltip("游戏时长（秒）")]
        [SerializeField] private float gameDuration = 60f;

        [Header("UI引用")]
        [Tooltip("计时器UI")]
        [SerializeField] private GameTimerUI timerUI;

        [Tooltip("结果面板UI")]
        [SerializeField] private GameResultUI resultUI;

        [Header("Phase2 UI引用")]
        [Tooltip("抓到鱼提示UI")]
        [SerializeField] private FishCaughtNotificationUI fishCaughtUI;

        [Tooltip("Phase2进度条")]
        [SerializeField] private Phase2.Phase2ProgressBar phase2ProgressBar;

        [Header("Phase2系统")]
        [Tooltip("Phase2管理器")]
        [SerializeField] private Phase2.Phase2Manager phase2Manager;

        // 游戏状态
        private GameState currentState = GameState.Ready;
        private float remainingTime;
        private bool isGameRunning = false;

        // Phase2相关
        private float phase2CountdownTime = 3f; // Phase2准备倒计时
        private float currentCountdown = 0f;
        private bool isCountingDown = false;

        // 单例模式
        private static GameManager instance;
        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<GameManager>();
                }
                return instance;
            }
        }

        // 公共属性
        public GameState CurrentState => currentState;
        public float RemainingTime => remainingTime;
        public bool IsGameRunning => isGameRunning;

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

            // 验证引用
            if (timerUI == null)
            {
                Debug.LogWarning("[GameManager] 计时器UI未设置！请在Inspector中分配。");
            }

            if (resultUI == null)
            {
                Debug.LogWarning("[GameManager] 结果面板UI未设置！请在Inspector中分配。");
            }

            if (fishCaughtUI == null)
            {
                Debug.LogWarning("[GameManager] 抓到鱼提示UI未设置！请在Inspector中分配。");
            }

            if (phase2ProgressBar == null)
            {
                Debug.LogWarning("[GameManager] Phase2进度条未设置！请在Inspector中分配。");
            }

            if (phase2Manager == null)
            {
                phase2Manager = FindFirstObjectByType<Phase2.Phase2Manager>();
                if (phase2Manager == null)
                {
                    Debug.LogWarning("[GameManager] Phase2管理器未找到！将尝试在运行时查找。");
                }
            }
        }

        private void Start()
        {
            // 初始化游戏
            InitializeGame();
        }

        private void Update()
        {
            if (isGameRunning)
            {
                UpdateTimer();
            }

            // Phase2倒计时更新
            if (isCountingDown)
            {
                UpdatePhase2Countdown();
            }
        }

        /// <summary>
        /// 初始化游戏
        /// </summary>
        private void InitializeGame()
        {
            remainingTime = gameDuration;
            currentState = GameState.Ready;

            // 隐藏结果面板
            if (resultUI != null)
            {
                resultUI.Hide();
            }

            // 更新计时器显示
            if (timerUI != null)
            {
                timerUI.UpdateTimer(remainingTime);
            }

            Debug.Log("[GameManager] 游戏初始化完成");

            // 自动开始游戏（可选，也可以等待玩家按键）
            StartGame();
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        public void StartGame()
        {
            if (isGameRunning)
            {
                Debug.LogWarning("[GameManager] 游戏已经在运行中！");
                return;
            }

            currentState = GameState.Playing;
            isGameRunning = true;
            remainingTime = gameDuration;

            Debug.Log("[GameManager] 游戏开始！");

            // 通知其他系统游戏开始
            OnGameStarted();
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

        /// <summary>
        /// 更新Phase2倒计时
        /// </summary>
        private void UpdatePhase2Countdown()
        {
            currentCountdown -= Time.deltaTime;

            // 更新UI显示
            if (fishCaughtUI != null)
            {
                fishCaughtUI.UpdateCountdown(Mathf.CeilToInt(currentCountdown));
            }

            // 倒计时结束，开始Phase2
            if (currentCountdown <= 0f)
            {
                isCountingDown = false;
                StartPhase2Struggle();
            }
        }

        /// <summary>
        /// 时间耗尽处理
        /// </summary>
        private void OnTimeUp()
        {
            // 如果在Phase2争斗阶段,根据进度条位置判断胜负
            if (currentState == GameState.Phase2Struggle)
            {
                if (phase2ProgressBar != null)
                {
                    float progress = phase2ProgressBar.GetCurrentProgress();
                    if (progress < 50f)
                    {
                        Debug.Log("[GameManager] 时间到！进度条在左侧，鱼胜利！");
                        EndGame(GameResult.FishWins);
                    }
                    else
                    {
                        Debug.Log("[GameManager] 时间到！进度条在右侧，渔夫胜利！");
                        EndGame(GameResult.FisherWins);
                    }
                }
                else
                {
                    Debug.LogWarning("[GameManager] Phase2进度条未设置，默认鱼胜利！");
                    EndGame(GameResult.FishWins);
                }
            }
            else
            {
                // Phase1时间到，鱼直接胜利
                Debug.Log("[GameManager] 时间到！鱼胜利！");
                EndGame(GameResult.FishWins);
            }
        }

        /// <summary>
        /// 渔夫钩中鱼 - 进入Phase2准备阶段
        /// </summary>
        public void OnFishHooked()
        {
            if (!isGameRunning)
            {
                Debug.LogWarning("[GameManager] 游戏未运行，无法进入Phase2！");
                return;
            }

            if (currentState != GameState.Playing)
            {
                Debug.LogWarning("[GameManager] 当前状态不是Playing，无法进入Phase2！");
                return;
            }

            Debug.Log("[GameManager] 渔夫钩中鱼！进入Phase2准备阶段...");
            StartPhase2Preparation();
        }

        /// <summary>
        /// 开始Phase2准备阶段 - 显示提示UI
        /// </summary>
        private void StartPhase2Preparation()
        {
            currentState = GameState.Phase2Preparation;

            // 显示抓到鱼的提示UI
            if (fishCaughtUI != null)
            {
                fishCaughtUI.Show();
            }
            else
            {
                Debug.LogError("[GameManager] 抓到鱼提示UI未设置！");
                // 如果UI未设置，直接开始倒计时
                StartPhase2Countdown();
            }

            Debug.Log("[GameManager] Phase2准备阶段开始，等待玩家确认...");
        }

        /// <summary>
        /// 开始Phase2倒计时 - 由UI按钮触发
        /// </summary>
        public void StartPhase2Countdown()
        {
            if (currentState != GameState.Phase2Preparation)
            {
                Debug.LogWarning("[GameManager] 当前状态不是Phase2Preparation，无法开始倒计时！");
                return;
            }

            Debug.Log("[GameManager] 开始Phase2倒计时...");
            currentCountdown = phase2CountdownTime;
            isCountingDown = true;
        }

        /// <summary>
        /// 开始Phase2争斗阶段
        /// </summary>
        private void StartPhase2Struggle()
        {
            currentState = GameState.Phase2Struggle;

            // 隐藏提示UI
            if (fishCaughtUI != null)
            {
                fishCaughtUI.Hide();
            }

            // 通过Phase2Manager启动所有Phase2系统
            if (phase2Manager != null)
            {
                phase2Manager.StartPhase2();
            }
            else
            {
                // 后备方案：手动启动进度条
                if (phase2ProgressBar != null)
                {
                    phase2ProgressBar.StartStruggle();
                }
                else
                {
                    Debug.LogError("[GameManager] Phase2进度条未设置！");
                }
            }

            Debug.Log("[GameManager] Phase2争斗阶段开始！");

            // 通知其他系统进入Phase2
            OnPhase2Started();
        }

        /// <summary>
        /// Phase2进度条达到边界 - 判定胜负
        /// </summary>
        public void OnPhase2ProgressComplete(bool fisherWins)
        {
            if (currentState != GameState.Phase2Struggle)
            {
                return;
            }

            // 结束Phase2系统
            if (phase2Manager != null)
            {
                phase2Manager.EndPhase2();
            }

            if (fisherWins)
            {
                Debug.Log("[GameManager] 进度条到达右侧！渔夫胜利！");
                EndGame(GameResult.FisherWins);
            }
            else
            {
                Debug.Log("[GameManager] 进度条到达左侧！鱼逃脱！鱼胜利！");
                EndGame(GameResult.FishWins);
            }
        }

        /// <summary>
        /// 结束游戏
        /// </summary>
        private void EndGame(GameResult result)
        {
            if (!isGameRunning)
            {
                return;
            }

            isGameRunning = false;
            currentState = GameState.Ended;

            Debug.Log($"[GameManager] 游戏结束！结果: {result}");

            // 显示结果面板
            if (resultUI != null)
            {
                resultUI.ShowResult(result);
            }
            else
            {
                Debug.LogError("[GameManager] 结果面板UI未设置，无法显示结果！");
            }

            // 通知其他系统游戏结束
            OnGameEnded(result);
        }

        /// <summary>
        /// 重新开始游戏
        /// </summary>
        public void RestartGame()
        {
            Debug.Log("[GameManager] 重新开始游戏");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// 退出游戏
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("[GameManager] 退出游戏");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// 游戏开始事件
        /// </summary>
        private void OnGameStarted()
        {
            // 可以在这里通知其他系统游戏开始
            // 例如：启用玩家输入、开始背景音乐等
        }

        /// <summary>
        /// Phase2开始事件
        /// </summary>
        private void OnPhase2Started()
        {
            // 通知鱼和渔夫控制器进入Phase2模式
            // 例如：切换相机、改变输入模式、启动Phase2动画等
        }

        /// <summary>
        /// 游戏结束事件
        /// </summary>
        private void OnGameEnded(GameResult result)
        {
            // 可以在这里通知其他系统游戏结束
            // 例如：禁用玩家输入、停止背景音乐、播放胜利/失败音效等
        }

        /// <summary>
        /// 获取格式化的时间字符串（MM:SS）
        /// </summary>
        public string GetFormattedTime()
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        /// <summary>
        /// 绘制调试信息
        /// </summary>
        private void OnGUI()
        {
            // 左上角显示调试信息
            GUI.color = Color.white;
            GUIStyle style = new GUIStyle();
            style.fontSize = 16;
            style.normal.textColor = Color.white;

            string debugInfo = $"State: {currentState}\n";
            debugInfo += $"Time: {GetFormattedTime()}\n";
            debugInfo += $"Running: {isGameRunning}";

            GUI.Label(new Rect(10, 10, 200, 100), debugInfo, style);
        }
    }

    /// <summary>
    /// 游戏状态枚举
    /// </summary>
    public enum GameState
    {
        Ready,              // 准备中
        Playing,            // Phase1游戏中
        Phase2Preparation,  // Phase2准备阶段（显示提示UI）
        Phase2Struggle,     // Phase2争斗阶段
        Ended               // 已结束
    }

    /// <summary>
    /// 游戏结果枚举
    /// </summary>
    public enum GameResult
    {
        FishWins,   // 鱼胜利
        FisherWins  // 渔夫胜利
    }
}

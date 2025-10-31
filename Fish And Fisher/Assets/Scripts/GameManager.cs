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

        // 游戏状态
        private GameState currentState = GameState.Ready;
        private float remainingTime;
        private bool isGameRunning = false;

        // 单例模式
        private static GameManager instance;
        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<GameManager>();
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
        /// 时间耗尽 - 鱼胜利
        /// </summary>
        private void OnTimeUp()
        {
            Debug.Log("[GameManager] 时间到！鱼胜利！");
            EndGame(GameResult.FishWins);
        }

        /// <summary>
        /// 渔夫钓到鱼 - 渔夫胜利
        /// </summary>
        public void OnFishCaught()
        {
            if (!isGameRunning)
            {
                Debug.LogWarning("[GameManager] 游戏未运行，无法判定胜负！");
                return;
            }

            Debug.Log("[GameManager] 渔夫钓到鱼！渔夫胜利！");
            EndGame(GameResult.FisherWins);
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
        Ready,      // 准备中
        Playing,    // 游戏中
        Ended       // 已结束
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

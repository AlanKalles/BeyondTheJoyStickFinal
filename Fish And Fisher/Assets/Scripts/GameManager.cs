using UnityEngine;
using UnityEngine.SceneManagement;
using FishAndFisher.Phase2;

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

        [Header("Phase2 UI引用")]
        [Tooltip("抓到鱼提示UI")]
        [SerializeField] private FishCaughtNotificationUI fishCaughtUI;

        [Tooltip("转场UI（用于场景切换）")]
        [SerializeField] private Phase2TransitionUI transitionUI;

        [Header("Phase2场景设置")]
        [Tooltip("Phase2场景名称")]
        [SerializeField] private string phase2SceneName = "Phase2";

        [Header("结果UI引用")]
        [Tooltip("游戏结果UI（Phase1时间到时显示）")]
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

            if (fishCaughtUI == null)
            {
                Debug.LogWarning("[GameManager] 抓到鱼提示UI未设置！请在Inspector中分配。");
            }

            if (transitionUI == null)
            {
                Debug.LogWarning("[GameManager] 转场UI未设置！请在Inspector中分配。");
            }
        }

        private void Start()
        {
            // 注册FishCaughtNotificationUI的自动转场事件
            if (fishCaughtUI != null)
            {
                fishCaughtUI.OnAutoTransition += OnAutoTransitionTriggered;
            }

            // 初始化游戏
            InitializeGame();
        }

        private void OnDestroy()
        {
            // 取消注册事件
            if (fishCaughtUI != null)
            {
                fishCaughtUI.OnAutoTransition -= OnAutoTransitionTriggered;
            }
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
        /// 时间耗尽处理（Phase1专用）
        /// </summary>
        private void OnTimeUp()
        {
            // Phase1时间到，鱼直接胜利
            Debug.Log("[GameManager] Phase1时间到！鱼胜利！");

            // 停止游戏
            isGameRunning = false;
            currentState = GameState.Ended;

            // 显示结果UI - 鱼胜利
            if (resultUI != null)
            {
                resultUI.ShowResult(Phase2.GameResult.FishWins);
            }
            else
            {
                Debug.LogWarning("[GameManager] 结果UI未设置！请在Inspector中分配。");
            }

            Debug.Log("[GameManager] Phase1结束：时间到，鱼未被抓住");
        }

        /// <summary>
        /// 渔夫钩中鱼 - 进入Phase2准备阶段（保存数据并显示提示UI）
        /// </summary>
        /// <param name="hookPosition">钩子位置</param>
        public void OnFishHooked(Vector3 hookPosition)
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

            // 保存Phase2数据
            SavePhase2Data(hookPosition);

            // 显示提示UI
            StartPhase2Preparation();
        }

        /// <summary>
        /// 保存Phase2需要的数据到数据传递系统（仅传递剩余时间）
        /// </summary>
        private void SavePhase2Data(Vector3 hookPosition)
        {
            var dataTransfer = Phase2DataTransfer.Instance;
            if (dataTransfer != null)
            {
                dataTransfer.SaveRemainingTime(remainingTime);
                Debug.Log($"[GameManager] Phase2数据已保存：剩余时间={remainingTime:F1}秒");
            }
            else
            {
                Debug.LogError("[GameManager] 无法保存Phase2数据：Phase2DataTransfer未找到！");
            }
        }

        /// <summary>
        /// 开始Phase2准备阶段 - 显示提示UI（1.5秒后自动触发转场）
        /// </summary>
        private void StartPhase2Preparation()
        {
            currentState = GameState.Phase2Preparation;

            // 显示抓到鱼的提示UI（将在1.5秒后自动触发OnAutoTransition事件）
            if (fishCaughtUI != null)
            {
                fishCaughtUI.Show();
            }
            else
            {
                Debug.LogError("[GameManager] 抓到鱼提示UI未设置！直接开始转场...");
                // 如果UI未设置，直接开始转场
                OnAutoTransitionTriggered();
            }

            Debug.Log("[GameManager] Phase2准备阶段开始，将在1.5秒后自动转场...");
        }

        /// <summary>
        /// FishCaughtNotificationUI自动转场事件回调
        /// </summary>
        private void OnAutoTransitionTriggered()
        {
            Debug.Log("[GameManager] 自动转场触发，开始场景切换...");
            StartTransitionToPhase2();
        }

        /// <summary>
        /// 开始转场到Phase2场景
        /// </summary>
        private void StartTransitionToPhase2()
        {
            if (transitionUI != null)
            {
                // 播放转场动画，在中间回调时加载场景
                transitionUI.PlayTransition(() =>
                {
                    Debug.Log("[GameManager] 转场动画中间点，开始加载Phase2场景...");
                    StartCoroutine(LoadPhase2Scene());
                });
            }
            else
            {
                Debug.LogError("[GameManager] 转场UI未设置！直接加载场景...");
                StartCoroutine(LoadPhase2Scene());
            }
        }

        /// <summary>
        /// 异步加载Phase2场景
        /// </summary>
        private System.Collections.IEnumerator LoadPhase2Scene()
        {
            Debug.Log($"[GameManager] 开始异步加载场景: {phase2SceneName}");

            // 异步加载Phase2场景
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(phase2SceneName);

            if (asyncLoad == null)
            {
                Debug.LogError($"[GameManager] 无法加载场景: {phase2SceneName}，请确保场景已添加到Build Settings！");
                yield break;
            }

            // 先不激活场景，等待加载完成
            asyncLoad.allowSceneActivation = false;

            // 等待加载完成（progress会到0.9就停止）
            while (asyncLoad.progress < 0.9f)
            {
                Debug.Log($"[GameManager] 加载进度: {asyncLoad.progress * 100}%");
                yield return null;
            }

            Debug.Log("[GameManager] 场景加载完成，准备激活...");

            // 激活场景
            asyncLoad.allowSceneActivation = true;

            // 等待场景真正激活
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            Debug.Log("[GameManager] Phase2场景已激活！");
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
    /// 游戏状态枚举（Phase1专用）
    /// </summary>
    public enum GameState
    {
        Ready,              // 准备中
        Playing,            // Phase1游戏中
        Phase2Preparation,  // Phase2准备阶段（显示提示UI）
        Ended               // Phase1已结束
    }
}

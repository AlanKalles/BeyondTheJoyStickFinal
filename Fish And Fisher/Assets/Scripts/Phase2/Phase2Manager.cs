using UnityEngine;

namespace FishAndFisher.Phase2
{
    /// <summary>
    /// Phase2管理器 - 统一管理Phase2的所有子系统
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

        [Tooltip("相机控制器")]
        [SerializeField] private Phase2CameraController cameraController;

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

            if (cameraController == null)
            {
                cameraController = FindFirstObjectByType<Phase2CameraController>();
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
        }

        /// <summary>
        /// 开始Phase2
        /// </summary>
        public void StartPhase2()
        {
            Debug.Log("[Phase2Manager] 开始Phase2争斗阶段");

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

            // 4. 切换相机
            if (cameraController != null)
            {
                cameraController.SwitchToPhase2Cameras();
            }

            // 5. 启用玩家Phase2模式
            if (fishController != null)
            {
                fishController.EnablePhase2Mode();
            }

            if (fisherController != null)
            {
                fisherController.EnablePhase2Mode();
            }

            // 6. 播放Phase2开始音效
            if (audioManager != null)
            {
                audioManager.PlayPhase2StartSound();
                audioManager.PlayStruggleLoopSound();
            }

            // 7. 启动进度条逻辑
            if (progressBar != null)
            {
                progressBar.StartStruggle();
            }
        }

        /// <summary>
        /// 结束Phase2
        /// </summary>
        public void EndPhase2()
        {
            Debug.Log("[Phase2Manager] Phase2争斗阶段结束");

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

            // 6. 停止争斗音效
            if (audioManager != null)
            {
                audioManager.StopStruggleLoopSound();
            }
        }

        /// <summary>
        /// 更新Phase2系统（如果需要手动更新）
        /// </summary>
        private void Update()
        {
            // Phase2期间，将输入归一化器的方向同步到鱼玩家
            if (inputNormalizer != null && fishController != null)
            {
                fishController.SetPhase2DirectionInput(inputNormalizer.FishDirection);
            }
        }
    }
}

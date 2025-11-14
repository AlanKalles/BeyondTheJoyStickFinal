using UnityEngine;

namespace FishAndFisher.Phase2
{
    /// <summary>
    /// Phase2进度条核心逻辑 - 管理争斗进度条的移动和胜负判定
    /// </summary>
    public class Phase2ProgressBar : MonoBehaviour
    {
        [Header("进度条参数")]
        [Tooltip("进度条总长度")]
        [SerializeField] private float maxProgress = 100f;

        [Tooltip("初始进度（中间位置）")]
        [SerializeField] private float initialProgress = 50f;

        [Tooltip("基础移动速度（单位/秒）")]
        [SerializeField] private float baseSpeed = 5f;

        [Tooltip("Log归一化基准值（用于力度差归一化）")]
        [SerializeField] private float logNormalizationBase = 10f;

        [Header("组件引用")]
        [Tooltip("输入归一化器")]
        [SerializeField] private Phase2InputNormalizer inputNormalizer;

        [Tooltip("力度检测器")]
        [SerializeField] private Phase2ForceDetector forceDetector;

        // 当前进度值（0=鱼胜利，100=渔夫胜利）
        private float currentProgress = 50f;

        // 是否正在争斗
        private bool isStruggling = false;

        public float CurrentProgress => currentProgress;
        public float MaxProgress => maxProgress;
        public float ProgressPercentage => currentProgress / maxProgress;

        private void Awake()
        {
            // 自动查找组件（如果未设置）
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

            // 验证引用
            if (inputNormalizer == null)
            {
                Debug.LogError("[Phase2ProgressBar] 输入归一化器未找到！");
            }

            if (forceDetector == null)
            {
                Debug.LogError("[Phase2ProgressBar] 力度检测器未找到！");
            }
        }

        private void Update()
        {
            if (!isStruggling)
            {
                return;
            }

            // 计算并应用进度条移动
            UpdateProgress();

            // 检查胜负条件
            CheckWinCondition();
        }

        /// <summary>
        /// 开始争斗
        /// </summary>
        public void StartStruggle()
        {
            currentProgress = initialProgress;
            isStruggling = true;

            Debug.Log("[Phase2ProgressBar] 争斗开始！初始进度: " + currentProgress);
        }

        /// <summary>
        /// 停止争斗
        /// </summary>
        public void StopStruggle()
        {
            isStruggling = false;
            Debug.Log("[Phase2ProgressBar] 争斗停止！");
        }

        /// <summary>
        /// 更新进度条
        /// </summary>
        private void UpdateProgress()
        {
            if (inputNormalizer == null || forceDetector == null)
            {
                return;
            }

            // 获取输入状态
            bool fishNoInput = inputNormalizer.FishNoInput();
            bool fisherNoInput = inputNormalizer.FisherNoInput();
            bool bothNoInput = inputNormalizer.BothNoInput();
            bool directionsMatch = inputNormalizer.AreDirectionsMatching();

            // 获取力度值
            float fishForce = forceDetector.FishForce;
            float fisherForce = forceDetector.FisherForce;
            float forceDiff = fisherForce - fishForce;

            // 计算移动速度
            float speed = CalculateSpeed(fishNoInput, fisherNoInput, bothNoInput, directionsMatch, forceDiff);

            // 应用速度到进度条
            currentProgress += speed * Time.deltaTime;

            // 限制进度范围
            currentProgress = Mathf.Clamp(currentProgress, 0f, maxProgress);
        }

        /// <summary>
        /// 计算进度条移动速度
        /// 核心逻辑：方向一致性决定移动方向，力度差只影响速度大小
        /// </summary>
        private float CalculateSpeed(bool fishNoInput, bool fisherNoInput, bool bothNoInput, bool directionsMatch, float forceDiff)
        {
            // 特殊情况1：都无输入 → 向右移动（渔夫占优）
            if (bothNoInput)
            {
                return baseSpeed; // 向右（正方向）
            }

            // 特殊情况2：鱼无输入 → 必向右移动（渔夫占优）
            if (fishNoInput)
            {
                return baseSpeed; // 向右（正方向）
            }

            // 特殊情况3：渔夫无输入但鱼有输入 → 向左移动（鱼占优）
            if (fisherNoInput && !fishNoInput)
            {
                return -baseSpeed; // 向左（负方向）
            }

            // 双方都有输入：方向一致性决定移动方向
            // Log归一化力度差（归一化到0-1范围，避免力度差过大导致速度过快）
            float normalizedForceDiff = LogNormalize(Mathf.Abs(forceDiff), logNormalizationBase);

            // 计算速度倍数：基础速度 * (1 + 力度差影响)
            // 力度差越大，速度越快，但不会改变方向
            float speedMultiplier = 1f + normalizedForceDiff;

            // 方向一致：向左移动（鱼占优）
            if (directionsMatch)
            {
                // 力度差影响速度：
                // - 渔夫力度大（forceDiff > 0）→ 减慢向左速度
                // - 鱼力度大（forceDiff < 0）→ 加快向左速度
                if (forceDiff > 0)
                {
                    // 渔夫力度大，减慢向左速度（但仍然向左）
                    speedMultiplier = Mathf.Max(0.2f, 1f - normalizedForceDiff); // 最慢0.2倍，不会停止或反转
                }
                else
                {
                    // 鱼力度大，加快向左速度
                    speedMultiplier = 1f + normalizedForceDiff;
                }
                return -baseSpeed * speedMultiplier; // 向左（负方向）
            }
            // 方向不一致：向右移动（渔夫占优）
            else
            {
                // 力度差影响速度：
                // - 渔夫力度大（forceDiff > 0）→ 加快向右速度
                // - 鱼力度大（forceDiff < 0）→ 减慢向右速度
                if (forceDiff > 0)
                {
                    // 渔夫力度大，加快向右速度
                    speedMultiplier = 1f + normalizedForceDiff;
                }
                else
                {
                    // 鱼力度大，减慢向右速度（但仍然向右）
                    speedMultiplier = Mathf.Max(0.2f, 1f - normalizedForceDiff); // 最慢0.2倍，不会停止或反转
                }
                return baseSpeed * speedMultiplier; // 向右（正方向）
            }
        }

        /// <summary>
        /// Log归一化（平滑处理力度差）
        /// </summary>
        private float LogNormalize(float value, float baseValue)
        {
            // 使用对数函数归一化：log(1 + value) / log(1 + baseValue)
            // 这样可以让大的力度差不会导致速度过快
            if (baseValue <= 0)
            {
                baseValue = 10f; // 默认基准值
            }

            return Mathf.Log(1f + value) / Mathf.Log(1f + baseValue);
        }

        /// <summary>
        /// 检查胜负条件
        /// </summary>
        private void CheckWinCondition()
        {
            if (currentProgress <= 0f)
            {
                // 鱼胜利
                isStruggling = false;
                GameManager.Instance?.OnPhase2ProgressComplete(false);
            }
            else if (currentProgress >= maxProgress)
            {
                // 渔夫胜利
                isStruggling = false;
                GameManager.Instance?.OnPhase2ProgressComplete(true);
            }
        }

        /// <summary>
        /// 获取当前进度
        /// </summary>
        public float GetCurrentProgress()
        {
            return currentProgress;
        }

        /// <summary>
        /// 调试信息显示
        /// </summary>
        private void OnGUI()
        {
            if (!isStruggling)
            {
                return;
            }

            GUIStyle style = new GUIStyle();
            style.fontSize = 14;
            style.normal.textColor = Color.green;

            string debugInfo = "[Phase2 Progress]\n";
            debugInfo += $"进度: {currentProgress:F1} / {maxProgress}\n";
            debugInfo += $"百分比: {(ProgressPercentage * 100f):F1}%\n";

            // 显示进度条状态
            if (currentProgress < 25f)
            {
                debugInfo += "状态: 鱼占优势！\n";
            }
            else if (currentProgress < 45f)
            {
                debugInfo += "状态: 鱼稍占优势\n";
            }
            else if (currentProgress < 55f)
            {
                debugInfo += "状态: 势均力敌\n";
            }
            else if (currentProgress < 75f)
            {
                debugInfo += "状态: 渔夫稍占优势\n";
            }
            else
            {
                debugInfo += "状态: 渔夫占优势！\n";
            }

            GUI.Label(new Rect(10, 350, 300, 120), debugInfo, style);
        }
    }
}

using UnityEngine;
using System.Collections;
using TMPro;

namespace FishAndFisher.Phase2
{
    /// <summary>
    /// Phase2场景初始化器
    /// 负责启动Phase2流程（转场淡出 → 提示 → 倒计时 → 开始争斗）
    /// </summary>
    public class Phase2SceneInitializer : MonoBehaviour
    {
        [Header("系统引用")]
        [Tooltip("Phase2管理器")]
        [SerializeField] private Phase2Manager phase2Manager;

        [Header("UI引用")]
        [Tooltip("\"Fight!\"提示文本")]
        [SerializeField] private TextMeshProUGUI startText;

        [Tooltip("倒计时文本（显示3、2、1）")]
        [SerializeField] private TextMeshProUGUI countdownText;

        [Tooltip("转场UI（用于淡出）")]
        [SerializeField] private Phase2TransitionUI transitionUI;

        [Header("倒计时设置")]
        [Tooltip("倒计时秒数")]
        [SerializeField] private int countdownSeconds = 3;

        [Tooltip("\"Fight\"提示显示时长")]
        [SerializeField] private float startTextDuration = 1f;

        private void Start()
        {
            Debug.Log("[Phase2SceneInitializer] Phase2场景初始化开始...");
            InitializePhase2();
        }

        /// <summary>
        /// 初始化Phase2场景
        /// </summary>
        private void InitializePhase2()
        {
            // 验证Phase2DataTransfer存在（剩余时间由Phase2Manager读取）
            var dataTransfer = Phase2DataTransfer.Instance;
            if (dataTransfer == null)
            {
                Debug.LogError("[Phase2SceneInitializer] Phase2DataTransfer未找到！");
            }
            else
            {
                Debug.Log($"[Phase2SceneInitializer] 初始化完成，剩余时间: {dataTransfer.RemainingTime:F1}秒");
            }

            // 开始Phase2启动序列
            StartCoroutine(Phase2StartSequence());
        }

        /// <summary>
        /// Phase2启动序列协程
        /// </summary>
        private IEnumerator Phase2StartSequence()
        {
            // 步骤1：显式调用转场UI淡出
            Debug.Log("[Phase2SceneInitializer] 开始转场UI淡出...");
            if (transitionUI != null)
            {
                transitionUI.FadeOutOnly();
                yield return new WaitForSeconds(0.75f); // 等待淡出完成
            }
            else
            {
                Debug.LogWarning("[Phase2SceneInitializer] 转场UI未设置，跳过淡出动画");
                yield return new WaitForSeconds(0.75f);
            }

            // 步骤2：显示"Fight!"提示
            if (startText != null)
            {
                startText.text = "Fight!";
                startText.gameObject.SetActive(true);
                Debug.Log("[Phase2SceneInitializer] 显示\"Fight!\"提示");

                yield return new WaitForSeconds(startTextDuration);

                startText.gameObject.SetActive(false);
            }

            // 步骤3：3秒倒计时
            if (countdownText != null)
            {
                for (int i = countdownSeconds; i > 0; i--)
                {
                    countdownText.text = i.ToString();
                    countdownText.gameObject.SetActive(true);
                    Debug.Log($"[Phase2SceneInitializer] 倒计时: {i}");

                    yield return new WaitForSeconds(1f);
                }

                countdownText.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("[Phase2SceneInitializer] 倒计时文本未设置，跳过倒计时动画");
                yield return new WaitForSeconds(countdownSeconds);
            }

            // 步骤4：启动Phase2争斗
            StartPhase2Struggle();
        }

        /// <summary>
        /// 启动Phase2争斗阶段
        /// </summary>
        private void StartPhase2Struggle()
        {
            Debug.Log("[Phase2SceneInitializer] 启动Phase2争斗阶段！");

            if (phase2Manager != null)
            {
                phase2Manager.StartPhase2();
            }
            else
            {
                Debug.LogError("[Phase2SceneInitializer] Phase2Manager未设置！无法启动Phase2系统。");
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// 编辑器中的验证
        /// </summary>
        private void OnValidate()
        {
            // 确保倒计时秒数为正值
            if (countdownSeconds <= 0)
                countdownSeconds = 3;

            if (startTextDuration <= 0)
                startTextDuration = 1f;

            // 自动查找组件
            if (phase2Manager == null)
            {
                phase2Manager = FindFirstObjectByType<Phase2Manager>();
            }
        }
#endif
    }
}

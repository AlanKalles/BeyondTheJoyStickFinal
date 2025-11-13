using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace FishAndFisher.Phase2
{
    /// <summary>
    /// Phase2力度检测器 - 检测按键频率并计算力度值
    /// </summary>
    public class Phase2ForceDetector : MonoBehaviour
    {
        [Header("力度参数")]
        [Tooltip("鱼玩家基础力度")]
        [SerializeField] private float fishBaseForce = 10f;

        [Tooltip("鱼玩家力度倍率")]
        [SerializeField] private float fishForceMultiplier = 1.0f;

        [Tooltip("渔夫玩家基础力度")]
        [SerializeField] private float fisherBaseForce = 15f;

        [Tooltip("渔夫玩家力度倍率")]
        [SerializeField] private float fisherForceMultiplier = 1.0f;

        [Tooltip("频率检测窗口（秒）")]
        [SerializeField] private float detectionWindow = 0.5f;

        [Header("输入动作引用")]
        [Tooltip("输入动作资源")]
        [SerializeField] private InputActionAsset inputActions;

        // 输入动作
        private InputAction fishJumpAction;    // 鱼的Jump键（空格）
        private InputAction fisherAttackAction; // 渔夫的Attack键（鼠标左键）

        // 按键时间戳记录（用于计算频率）
        private List<float> fishPressTimestamps = new List<float>();
        private List<float> fisherPressTimestamps = new List<float>();

        // 当前力度值
        private float currentFishForce = 0f;
        private float currentFisherForce = 0f;

        // 是否启用Phase2力度检测
        private bool isPhase2Active = false;

        public float FishForce => currentFishForce;
        public float FisherForce => currentFisherForce;

        private void Awake()
        {
            // 获取输入动作
            if (inputActions == null)
            {
                inputActions = FindFirstObjectByType<UnityEngine.InputSystem.PlayerInput>()?.actions;
            }

            if (inputActions != null)
            {
                fishJumpAction = inputActions.FindAction("Player/Jump");
                fisherAttackAction = inputActions.FindAction("Player/Attack");
            }
            else
            {
                Debug.LogError("[Phase2ForceDetector] 输入动作资源未找到！");
            }
        }

        private void OnEnable()
        {
            // 订阅按键按下事件
            if (fishJumpAction != null)
            {
                fishJumpAction.performed += OnFishJumpPressed;
            }

            if (fisherAttackAction != null)
            {
                fisherAttackAction.performed += OnFisherAttackPressed;
            }
        }

        private void OnDisable()
        {
            // 取消订阅
            if (fishJumpAction != null)
            {
                fishJumpAction.performed -= OnFishJumpPressed;
            }

            if (fisherAttackAction != null)
            {
                fisherAttackAction.performed -= OnFisherAttackPressed;
            }
        }

        private void Update()
        {
            if (!isPhase2Active)
            {
                return;
            }

            // 更新力度值（清理过期时间戳并重新计算）
            UpdateForce();
        }

        /// <summary>
        /// 启动Phase2力度检测
        /// </summary>
        public void ActivatePhase2Force()
        {
            isPhase2Active = true;

            // 清空历史数据
            fishPressTimestamps.Clear();
            fisherPressTimestamps.Clear();
            currentFishForce = 0f;
            currentFisherForce = 0f;

            Debug.Log("[Phase2ForceDetector] Phase2力度检测已激活");
        }

        /// <summary>
        /// 停用Phase2力度检测
        /// </summary>
        public void DeactivatePhase2Force()
        {
            isPhase2Active = false;
            currentFishForce = 0f;
            currentFisherForce = 0f;

            Debug.Log("[Phase2ForceDetector] Phase2力度检测已停用");
        }

        /// <summary>
        /// 鱼玩家Jump键按下
        /// </summary>
        private void OnFishJumpPressed(InputAction.CallbackContext context)
        {
            if (!isPhase2Active)
            {
                return;
            }

            // 记录按键时间戳
            fishPressTimestamps.Add(Time.time);
        }

        /// <summary>
        /// 渔夫玩家Attack键按下
        /// </summary>
        private void OnFisherAttackPressed(InputAction.CallbackContext context)
        {
            if (!isPhase2Active)
            {
                return;
            }

            // 记录按键时间戳
            fisherPressTimestamps.Add(Time.time);
        }

        /// <summary>
        /// 更新力度值
        /// </summary>
        private void UpdateForce()
        {
            // 清理过期的时间戳（超出检测窗口）
            CleanOldTimestamps(fishPressTimestamps);
            CleanOldTimestamps(fisherPressTimestamps);

            // 计算当前窗口内的按键频率
            float fishFrequency = CalculateFrequency(fishPressTimestamps);
            float fisherFrequency = CalculateFrequency(fisherPressTimestamps);

            // 计算力度：频率 × 倍率 × 基础值
            currentFishForce = fishFrequency * fishForceMultiplier * fishBaseForce;
            currentFisherForce = fisherFrequency * fisherForceMultiplier * fisherBaseForce;
        }

        /// <summary>
        /// 清理过期的时间戳
        /// </summary>
        private void CleanOldTimestamps(List<float> timestamps)
        {
            float currentTime = Time.time;
            float threshold = currentTime - detectionWindow;

            // 移除所有早于阈值的时间戳
            timestamps.RemoveAll(t => t < threshold);
        }

        /// <summary>
        /// 计算频率（窗口内的按键次数）
        /// </summary>
        private float CalculateFrequency(List<float> timestamps)
        {
            // 频率 = 窗口内按键次数
            return timestamps.Count;
        }

        /// <summary>
        /// 获取力度差（渔夫力度 - 鱼力度）
        /// </summary>
        public float GetForceDifference()
        {
            return currentFisherForce - currentFishForce;
        }

        /// <summary>
        /// 调试信息显示
        /// </summary>
        private void OnGUI()
        {
            if (!isPhase2Active)
            {
                return;
            }

            GUIStyle style = new GUIStyle();
            style.fontSize = 14;
            style.normal.textColor = Color.cyan;

            string debugInfo = "[Phase2 Force]\n";
            debugInfo += $"鱼力度: {currentFishForce:F1} (频率: {fishPressTimestamps.Count})\n";
            debugInfo += $"渔夫力度: {currentFisherForce:F1} (频率: {fisherPressTimestamps.Count})\n";
            debugInfo += $"力度差: {GetForceDifference():F1}\n";

            GUI.Label(new Rect(10, 250, 300, 100), debugInfo, style);
        }
    }
}

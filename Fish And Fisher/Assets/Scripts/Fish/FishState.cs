using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FishAndFisher.Fish
{
    /// <summary>
    /// 鱼的状态类型枚举
    /// </summary>
    public enum FishStateType
    {
        Idle,       // 待机
        Swimming,   // 游泳
        Sprinting,  // 冲刺
        Turning,    // 转向
        Stunned,    // 眩晕
        Escaping,   // 逃脱（被钩中后挣扎）
        Caught      // 被捕获
    }

    /// <summary>
    /// 鱼玩家的状态管理器
    /// 负责管理和切换不同的游戏状态
    /// </summary>
    public class FishState : MonoBehaviour
    {
        [Header("状态设置")]
        [SerializeField] private FishStateType currentState = FishStateType.Idle;
        [SerializeField] private FishStateType previousState = FishStateType.Idle;

        [Header("状态持续时间")]
        [SerializeField] private float stunnedDuration = 2f;       // 眩晕持续时间
        [SerializeField] private float escapingMaxDuration = 5f;   // 最大逃脱时间
        [SerializeField] private float turnStateThreshold = 0.5f;  // 进入转向状态的阈值

        [Header("逃脱参数")]
        [SerializeField] private float escapeProgress = 0f;        // 逃脱进度(0-100)
        [SerializeField] private float escapeDecayRate = 10f;      // 逃脱进度衰减速度
        [SerializeField] private float escapeSuccessThreshold = 100f; // 成功逃脱所需进度

        [Header("体力系统")]
        [SerializeField] private float maxStamina = 100f;          // 最大体力
        [SerializeField] private float currentStamina = 100f;      // 当前体力
        [SerializeField] private float staminaDrainRate = 5f;      // 冲刺时体力消耗速度
        [SerializeField] private float staminaRecoveryRate = 3f;   // 体力恢复速度

        // 状态计时器
        private float stateTimer;
        private float escapingTimer;

        // 组件引用
        private FishMovement movement;
        private FishInputHandler inputHandler;

        // 事件
        public event Action<FishStateType> OnStateChanged;
        public event Action<FishStateType, FishStateType> OnStateTransition;
        public event Action OnEscapeSuccess;
        public event Action OnCaught;
        public event Action<float> OnStaminaChanged;

        // 属性访问器
        public FishStateType CurrentState => currentState;
        public bool CanMove => currentState != FishStateType.Stunned &&
                               currentState != FishStateType.Caught;
        public bool CanAccelerate => currentState == FishStateType.Swimming ||
                                     currentState == FishStateType.Sprinting;
        public float Stamina => currentStamina;
        public float StaminaPercentage => currentStamina / maxStamina;
        public float EscapeProgress => escapeProgress;

        private void Awake()
        {
            movement = GetComponent<FishMovement>();
            inputHandler = GetComponent<FishInputHandler>();
        }

        /// <summary>
        /// 初始化状态系统
        /// </summary>
        public void Initialize()
        {
            currentState = FishStateType.Idle;
            previousState = FishStateType.Idle;
            currentStamina = maxStamina;
            escapeProgress = 0f;

            Debug.Log("鱼玩家状态系统初始化完成");
        }

        private void Update()
        {
            // 更新当前状态
            UpdateCurrentState();

            // 更新体力
            UpdateStamina();

            // 检查状态转换
            CheckStateTransitions();
        }

        /// <summary>
        /// 更新当前状态逻辑
        /// </summary>
        private void UpdateCurrentState()
        {
            switch (currentState)
            {
                case FishStateType.Idle:
                    UpdateIdleState();
                    break;

                case FishStateType.Swimming:
                    UpdateSwimmingState();
                    break;

                case FishStateType.Sprinting:
                    UpdateSprintingState();
                    break;

                case FishStateType.Turning:
                    UpdateTurningState();
                    break;

                case FishStateType.Stunned:
                    UpdateStunnedState();
                    break;

                case FishStateType.Escaping:
                    UpdateEscapingState();
                    break;

                case FishStateType.Caught:
                    UpdateCaughtState();
                    break;
            }
        }

        /// <summary>
        /// 检查状态转换条件
        /// </summary>
        private void CheckStateTransitions()
        {
            // 被眩晕或捕获时不能自动转换
            if (currentState == FishStateType.Stunned ||
                currentState == FishStateType.Caught ||
                currentState == FishStateType.Escaping)
            {
                return;
            }

            // 根据速度自动切换待机/游泳/冲刺状态
            if (movement != null)
            {
                float speed = movement.CurrentSpeed;

                if (speed < 0.5f)
                {
                    if (currentState != FishStateType.Idle)
                    {
                        TransitionToState(FishStateType.Idle);
                    }
                }
                else if (speed > 5f && currentStamina > 10f)
                {
                    if (currentState != FishStateType.Sprinting)
                    {
                        TransitionToState(FishStateType.Sprinting);
                    }
                }
                else
                {
                    if (currentState != FishStateType.Swimming)
                    {
                        TransitionToState(FishStateType.Swimming);
                    }
                }
            }
        }

        #region 状态更新方法

        private void UpdateIdleState()
        {
            // 待机状态逻辑
            stateTimer += Time.deltaTime;
        }

        private void UpdateSwimmingState()
        {
            // 正常游泳状态逻辑
            stateTimer += Time.deltaTime;
        }

        private void UpdateSprintingState()
        {
            // 冲刺状态逻辑
            stateTimer += Time.deltaTime;

            // 消耗体力
            if (currentStamina <= 0)
            {
                // 体力耗尽，退出冲刺
                TransitionToState(FishStateType.Swimming);
            }
        }

        private void UpdateTurningState()
        {
            // 转向状态逻辑
            stateTimer += Time.deltaTime;

            // 短暂的转向状态后恢复
            if (stateTimer > turnStateThreshold)
            {
                TransitionToState(previousState);
            }
        }

        private void UpdateStunnedState()
        {
            // 眩晕状态逻辑
            stateTimer += Time.deltaTime;

            // 眩晕时间结束
            if (stateTimer >= stunnedDuration)
            {
                TransitionToState(FishStateType.Swimming);
            }
        }

        private void UpdateEscapingState()
        {
            // 逃脱状态逻辑
            escapingTimer += Time.deltaTime;

            // 逃脱进度衰减
            escapeProgress = Mathf.Max(0, escapeProgress - escapeDecayRate * Time.deltaTime);

            // 检查逃脱成功
            if (escapeProgress >= escapeSuccessThreshold)
            {
                OnEscapeSuccessful();
            }
            // 检查逃脱失败（超时）
            else if (escapingTimer >= escapingMaxDuration)
            {
                OnEscapeFailed();
            }
        }

        private void UpdateCaughtState()
        {
            // 被捕获状态逻辑
            // 游戏结束或等待重置
        }

        #endregion

        #region 体力系统

        /// <summary>
        /// 更新体力值
        /// </summary>
        private void UpdateStamina()
        {
            float previousStamina = currentStamina;

            if (currentState == FishStateType.Sprinting)
            {
                // 冲刺时消耗体力
                currentStamina = Mathf.Max(0, currentStamina - staminaDrainRate * Time.deltaTime);
            }
            else if (currentState == FishStateType.Swimming || currentState == FishStateType.Idle)
            {
                // 正常状态恢复体力
                currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRecoveryRate * Time.deltaTime);
            }

            // 触发体力变化事件
            if (Mathf.Abs(currentStamina - previousStamina) > 0.1f)
            {
                OnStaminaChanged?.Invoke(currentStamina / maxStamina);
            }
        }

        /// <summary>
        /// 消耗体力
        /// </summary>
        public void ConsumeStamina(float amount)
        {
            currentStamina = Mathf.Max(0, currentStamina - amount);
            OnStaminaChanged?.Invoke(currentStamina / maxStamina);
        }

        /// <summary>
        /// 恢复体力
        /// </summary>
        public void RecoverStamina(float amount)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
            OnStaminaChanged?.Invoke(currentStamina / maxStamina);
        }

        #endregion

        #region 状态转换

        /// <summary>
        /// 转换到指定状态
        /// </summary>
        public void TransitionToState(FishStateType newState)
        {
            if (currentState == newState) return;

            previousState = currentState;
            currentState = newState;
            stateTimer = 0f;

            // 进入新状态的初始化
            OnEnterState(newState);

            // 触发状态变化事件
            OnStateTransition?.Invoke(previousState, newState);
            OnStateChanged?.Invoke(newState);

            Debug.Log($"鱼状态转换: {previousState} -> {newState}");
        }

        /// <summary>
        /// 进入新状态时的初始化
        /// </summary>
        private void OnEnterState(FishStateType state)
        {
            switch (state)
            {
                case FishStateType.Stunned:
                    // 眩晕时禁用输入
                    inputHandler?.DisableInput();
                    break;

                case FishStateType.Escaping:
                    // 开始逃脱
                    escapingTimer = 0f;
                    escapeProgress = 0f;
                    break;

                case FishStateType.Caught:
                    // 被捕获
                    inputHandler?.DisableInput();
                    OnCaught?.Invoke();
                    break;

                case FishStateType.Swimming:
                case FishStateType.Idle:
                    // 恢复输入
                    inputHandler?.EnableInput();
                    break;
            }
        }

        #endregion

        #region 逃脱系统

        /// <summary>
        /// 增加逃脱进度（通过玩家操作）
        /// </summary>
        public void AddEscapeProgress(float amount)
        {
            if (currentState != FishStateType.Escaping) return;

            escapeProgress = Mathf.Min(escapeSuccessThreshold, escapeProgress + amount);
        }

        /// <summary>
        /// 逃脱成功
        /// </summary>
        private void OnEscapeSuccessful()
        {
            Debug.Log("逃脱成功！");
            TransitionToState(FishStateType.Swimming);
            OnEscapeSuccess?.Invoke();
        }

        /// <summary>
        /// 逃脱失败
        /// </summary>
        private void OnEscapeFailed()
        {
            Debug.Log("逃脱失败！");
            TransitionToState(FishStateType.Caught);
        }

        #endregion

        #region 外部接口

        /// <summary>
        /// 造成眩晕
        /// </summary>
        public void ApplyStun(float duration = 0f)
        {
            if (currentState == FishStateType.Caught) return;

            stunnedDuration = duration > 0 ? duration : stunnedDuration;
            TransitionToState(FishStateType.Stunned);
        }

        /// <summary>
        /// 被鱼钩钩中
        /// </summary>
        public void OnHooked()
        {
            if (currentState == FishStateType.Caught) return;

            TransitionToState(FishStateType.Escaping);
        }

        /// <summary>
        /// 重置状态
        /// </summary>
        public void ResetState()
        {
            currentState = FishStateType.Idle;
            previousState = FishStateType.Idle;
            currentStamina = maxStamina;
            escapeProgress = 0f;
            stateTimer = 0f;
            escapingTimer = 0f;

            inputHandler?.EnableInput();
        }

        #endregion

        /// <summary>
        /// 获取状态信息字符串
        /// </summary>
        public string GetStateInfo()
        {
            return $"State: {currentState} | Stamina: {currentStamina:F0}/{maxStamina:F0}";
        }
    }
}
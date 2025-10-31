using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FishAndFisher.Fish
{
    /// <summary>
    /// 鱼玩家的输入处理器
    /// 负责接收和处理来自InputSystem的输入
    /// </summary>
    public class FishInputHandler : MonoBehaviour
    {
        [Header("输入设置")]
        [SerializeField] private bool enableInput = true;          // 是否启用输入
        [SerializeField] private float inputDeadZone = 0.2f;       // 输入死区
        [SerializeField] private bool invertY = false;             // 是否反转Y轴
        [SerializeField] private bool invertX = false;             // 是否反转X轴

        [Header("输入平滑")]
        [SerializeField] private bool smoothInput = true;          // 是否平滑输入
        [SerializeField] private float inputSmoothTime = 0.1f;     // 输入平滑时间

        [Header("调试")]
        [SerializeField] private bool debugInput = false;          // 显示输入调试信息
        [SerializeField] private Vector2 rawInput;                 // 原始输入
        [SerializeField] private Vector2 processedInput;           // 处理后的输入

        // InputSystem引用
        private InputSystem_Actions inputActions;
        private InputAction moveAction;
        private InputAction jumpAction;

        // 输入状态
        private Vector2 currentInput;
        private Vector2 targetInput;
        private Vector2 inputVelocity;
        private bool isJumpPressed;
        private float lastJumpTime;

        // 事件
        public event Action<Vector2> OnMoveInput;
        public event Action<bool> OnJumpInput;
        public event Action OnInputEnabled;
        public event Action OnInputDisabled;

        // 属性访问器
        public bool IsInputEnabled => enableInput && inputActions != null && inputActions.asset.enabled;
        public Vector2 CurrentInput => currentInput;
        public bool IsJumping => isJumpPressed;

        private void Awake()
        {
            // 创建输入动作实例
            inputActions = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            // 启用输入动作
            if (inputActions != null)
            {
                inputActions.Enable();

                // 获取动作引用
                moveAction = inputActions.Player.Move;
                jumpAction = inputActions.Player.Jump;

                // 订阅输入事件
                if (moveAction != null)
                {
                    moveAction.performed += OnMove;
                    moveAction.canceled += OnMove;
                }

                if (jumpAction != null)
                {
                    jumpAction.performed += OnJump;
                    jumpAction.canceled += OnJump;
                }

                Debug.Log("鱼玩家输入系统已启用");
            }
        }

        private void OnDisable()
        {
            // 取消订阅并禁用输入动作
            if (inputActions != null)
            {
                if (moveAction != null)
                {
                    moveAction.performed -= OnMove;
                    moveAction.canceled -= OnMove;
                }

                if (jumpAction != null)
                {
                    jumpAction.performed -= OnJump;
                    jumpAction.canceled -= OnJump;
                }

                inputActions.Disable();
            }
        }

        private void OnDestroy()
        {
            // 清理输入动作
            inputActions?.Dispose();
        }

        private void Update()
        {
            if (!enableInput) return;

            // 处理输入平滑
            if (smoothInput)
            {
                currentInput = Vector2.SmoothDamp(
                    currentInput,
                    targetInput,
                    ref inputVelocity,
                    inputSmoothTime
                );
            }
            else
            {
                currentInput = targetInput;
            }

            // 更新处理后的输入
            processedInput = currentInput;

            // 触发移动事件
            if (currentInput.magnitude > 0.01f || rawInput.magnitude > 0.01f)
            {
                OnMoveInput?.Invoke(currentInput);
            }

            // 调试输出
            if (debugInput && (currentInput.magnitude > 0.01f || isJumpPressed))
            {
                Debug.Log($"Fish Input - Move: {currentInput}, Jump: {isJumpPressed}");
            }
        }

        /// <summary>
        /// 处理移动输入
        /// </summary>
        private void OnMove(InputAction.CallbackContext context)
        {
            if (!enableInput) return;

            Vector2 input = context.ReadValue<Vector2>();
            rawInput = input;

            // 应用死区
            if (input.magnitude < inputDeadZone)
            {
                input = Vector2.zero;
            }
            else
            {
                // 重新映射输入范围
                float magnitude = (input.magnitude - inputDeadZone) / (1 - inputDeadZone);
                input = input.normalized * magnitude;
            }

            // 应用反转
            if (invertX) input.x *= -1;
            if (invertY) input.y *= -1;

            // 限制输入范围
            input.x = Mathf.Clamp(input.x, -1f, 1f);
            input.y = Mathf.Clamp(input.y, -1f, 1f);

            targetInput = input;
        }

        /// <summary>
        /// 处理跳跃输入
        /// </summary>
        private void OnJump(InputAction.CallbackContext context)
        {
            if (!enableInput) return;

            bool wasPressed = isJumpPressed;
            isJumpPressed = context.performed;

            // 只在状态改变时触发事件
            if (isJumpPressed != wasPressed)
            {
                OnJumpInput?.Invoke(isJumpPressed);

                if (isJumpPressed)
                {
                    lastJumpTime = Time.time;
                }
            }
        }

        /// <summary>
        /// 启用输入
        /// </summary>
        public void EnableInput()
        {
            if (enableInput) return;

            enableInput = true;
            inputActions?.Enable();
            OnInputEnabled?.Invoke();

            Debug.Log("鱼玩家输入已启用");
        }

        /// <summary>
        /// 禁用输入
        /// </summary>
        public void DisableInput()
        {
            if (!enableInput) return;

            enableInput = false;
            currentInput = Vector2.zero;
            targetInput = Vector2.zero;
            isJumpPressed = false;

            OnInputDisabled?.Invoke();

            Debug.Log("鱼玩家输入已禁用");
        }

        /// <summary>
        /// 重置输入
        /// </summary>
        public void ResetInput()
        {
            currentInput = Vector2.zero;
            targetInput = Vector2.zero;
            inputVelocity = Vector2.zero;
            isJumpPressed = false;
        }

        /// <summary>
        /// 设置输入死区
        /// </summary>
        public void SetDeadZone(float deadZone)
        {
            inputDeadZone = Mathf.Clamp01(deadZone);
        }

        /// <summary>
        /// 获取Jump按键频率
        /// </summary>
        public float GetJumpFrequency(float timeWindow = 1f)
        {
            // 这里可以实现更复杂的频率计算逻辑
            // 目前返回简单的布尔值
            return isJumpPressed ? 1f : 0f;
        }

        /// <summary>
        /// 震动反馈（如果支持）
        /// </summary>
        public void TriggerVibration(float intensity = 0.5f, float duration = 0.2f)
        {
            // 获取当前游戏手柄
            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                StartCoroutine(VibrateGamepad(gamepad, intensity, duration));
            }
        }

        /// <summary>
        /// 手柄震动协程
        /// </summary>
        private IEnumerator VibrateGamepad(Gamepad gamepad, float intensity, float duration)
        {
            gamepad.SetMotorSpeeds(intensity, intensity);
            yield return new WaitForSeconds(duration);
            gamepad.SetMotorSpeeds(0, 0);
        }
    }
}
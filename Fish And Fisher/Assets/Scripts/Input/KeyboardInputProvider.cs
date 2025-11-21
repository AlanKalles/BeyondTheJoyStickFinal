using UnityEngine;
using UnityEngine.InputSystem;

namespace FishAndFisher.Input
{
    /// <summary>
    /// 键盘输入提供者 - 使用 Unity InputSystem 处理键盘、鼠标和手柄输入
    /// </summary>
    public class KeyboardInputProvider : IInputProvider
    {
        private InputSystem_Actions inputActions;
        private Vector2 currentMovement;
        private Vector2 currentLookPosition;
        private bool isJumpPressed;
        private bool isAttackPressed;
        private bool isInitialized;

        public bool IsConnected => isInitialized;

        public string GetDeviceName()
        {
            return "键盘/鼠标";
        }

        public void Initialize()
        {
            if (isInitialized)
            {
                Debug.LogWarning("[KeyboardInputProvider] 已经初始化，跳过重复初始化");
                return;
            }

            // 创建输入动作实例
            inputActions = new InputSystem_Actions();

            // 订阅 Move 输入
            inputActions.Player.Move.performed += OnMovePerformed;
            inputActions.Player.Move.canceled += OnMoveCanceled;

            // 订阅 Jump 输入
            inputActions.Player.Jump.performed += OnJumpPerformed;
            inputActions.Player.Jump.canceled += OnJumpCanceled;

            // 订阅 Look 输入（鼠标位置）
            inputActions.Player.Look.performed += OnLookPerformed;
            inputActions.Player.Look.canceled += OnLookCanceled;

            // 订阅 Attack 输入
            inputActions.Player.Attack.performed += OnAttackPerformed;
            inputActions.Player.Attack.canceled += OnAttackCanceled;

            // 启用输入
            inputActions.Enable();

            isInitialized = true;
            Debug.Log("[KeyboardInputProvider] 键盘输入已初始化");
        }

        public void Cleanup()
        {
            if (!isInitialized || inputActions == null)
                return;

            // 取消订阅所有输入
            inputActions.Player.Move.performed -= OnMovePerformed;
            inputActions.Player.Move.canceled -= OnMoveCanceled;

            inputActions.Player.Jump.performed -= OnJumpPerformed;
            inputActions.Player.Jump.canceled -= OnJumpCanceled;

            inputActions.Player.Look.performed -= OnLookPerformed;
            inputActions.Player.Look.canceled -= OnLookCanceled;

            inputActions.Player.Attack.performed -= OnAttackPerformed;
            inputActions.Player.Attack.canceled -= OnAttackCanceled;

            // 禁用并释放输入
            inputActions.Disable();
            inputActions.Dispose();
            inputActions = null;

            isInitialized = false;
            Debug.Log("[KeyboardInputProvider] 键盘输入已清理");
        }

        public Vector2 GetMovement()
        {
            return currentMovement;
        }

        public bool GetJumpPressed()
        {
            return isJumpPressed;
        }

        public Vector2 GetLookPosition()
        {
            // 返回鼠标屏幕位置
            return currentLookPosition;
        }

        public bool GetAttackPressed()
        {
            return isAttackPressed;
        }

        #region 输入回调

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            currentMovement = context.ReadValue<Vector2>();
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            currentMovement = Vector2.zero;
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            isJumpPressed = true;
        }

        private void OnJumpCanceled(InputAction.CallbackContext context)
        {
            isJumpPressed = false;
        }

        private void OnLookPerformed(InputAction.CallbackContext context)
        {
            currentLookPosition = context.ReadValue<Vector2>();
        }

        private void OnLookCanceled(InputAction.CallbackContext context)
        {
            // 保持最后的鼠标位置
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            isAttackPressed = true;
        }

        private void OnAttackCanceled(InputAction.CallbackContext context)
        {
            isAttackPressed = false;
        }

        #endregion
    }
}

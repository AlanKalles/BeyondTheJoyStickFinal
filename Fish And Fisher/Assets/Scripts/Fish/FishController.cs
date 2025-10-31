using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FishAndFisher.Fish
{
    /// <summary>
    /// 鱼玩家的主控制器
    /// 负责协调输入、移动、状态等各个子系统
    /// </summary>
    public class FishController : MonoBehaviour
    {
        [Header("组件引用")]
        private FishMovement fishMovement;
        private FishInputHandler inputHandler;
        private FishState fishState;
        private FishAnimator fishAnimator;

        [Header("调试信息")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private Vector2 currentInputVector;
        [SerializeField] private float currentSpeed;
        [SerializeField] private float currentDirection;

        private void Awake()
        {
            // 获取或添加必要组件
            fishMovement = GetComponent<FishMovement>() ?? gameObject.AddComponent<FishMovement>();
            inputHandler = GetComponent<FishInputHandler>() ?? gameObject.AddComponent<FishInputHandler>();
            fishState = GetComponent<FishState>() ?? gameObject.AddComponent<FishState>();
            fishAnimator = GetComponent<FishAnimator>() ?? gameObject.AddComponent<FishAnimator>();
        }

        private void Start()
        {
            // 订阅输入事件
            if (inputHandler != null)
            {
                inputHandler.OnMoveInput += HandleMoveInput;
                inputHandler.OnJumpInput += HandleJumpInput;
            }

            // 初始化状态
            if (fishState != null)
            {
                fishState.Initialize();
            }

            Debug.Log("鱼玩家控制器初始化完成");
        }

        private void OnDestroy()
        {
            // 取消订阅事件
            if (inputHandler != null)
            {
                inputHandler.OnMoveInput -= HandleMoveInput;
                inputHandler.OnJumpInput -= HandleJumpInput;
            }
        }

        private void Update()
        {
            // 更新调试信息
            if (debugMode && fishMovement != null)
            {
                currentSpeed = fishMovement.CurrentSpeed;
                currentDirection = fishMovement.CurrentDirection;
            }
        }

        /// <summary>
        /// 处理移动输入
        /// </summary>
        private void HandleMoveInput(Vector2 inputVector)
        {
            currentInputVector = inputVector;

            if (fishMovement != null && fishState != null && fishState.CanMove)
            {
                fishMovement.SetMoveInput(inputVector);
            }
        }

        /// <summary>
        /// 处理跳跃输入（用于加速）
        /// </summary>
        private void HandleJumpInput(bool isPressed)
        {
            if (fishMovement != null && fishState != null && fishState.CanAccelerate)
            {
                fishMovement.OnJumpPressed(isPressed);
            }
        }

        /// <summary>
        /// 获取当前状态信息
        /// </summary>
        public FishStatusInfo GetStatus()
        {
            return new FishStatusInfo
            {
                position = transform.position,
                direction = fishMovement?.CurrentDirection ?? 0f,
                speed = fishMovement?.CurrentSpeed ?? 0f,
                state = fishState?.CurrentState ?? FishStateType.Idle,
                inputVector = currentInputVector
            };
        }

        /// <summary>
        /// 被渔夫钩中时调用
        /// </summary>
        public void OnHooked()
        {
            if (fishState != null)
            {
                fishState.TransitionToState(FishStateType.Escaping);
            }
        }

        /// <summary>
        /// 挣脱鱼钩时调用
        /// </summary>
        public void OnEscaped()
        {
            if (fishState != null)
            {
                fishState.TransitionToState(FishStateType.Swimming);
            }
        }
    }

    /// <summary>
    /// 鱼的状态信息结构体
    /// </summary>
    [System.Serializable]
    public struct FishStatusInfo
    {
        public Vector3 position;
        public float direction;
        public float speed;
        public FishStateType state;
        public Vector2 inputVector;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FishAndFisher.Fish
{
    /// <summary>
    /// 鱼玩家的移动系统
    /// 处理惯性移动、方向控制和速度管理
    /// </summary>
    public class FishMovement : MonoBehaviour
    {
        [Header("移动参数")]
        [SerializeField] private float baseSpeed = 2.0f;           // 基础速度
        [SerializeField] private float maxSpeed = 8.0f;            // 最大速度
        [SerializeField] private float acceleration = 3.0f;        // 加速度
        [SerializeField] private float deceleration = 1.5f;        // 减速度
        [SerializeField] private float turnDampening = 0.85f;      // 转向时的速度衰减

        [Header("方向控制")]
        [SerializeField] private float turnSpeedBase = 120f;       // 基础转向速度（度/秒）
        [SerializeField] private float turnSpeedAtMaxSpeed = 60f;  // 最高速时的转向速度
        [SerializeField] private float turnAngleLarge = 60f;       // 纯左/右输入的转向角度
        [SerializeField] private float turnAngleSmall = 30f;       // 前+左/右输入的转向角度
        [SerializeField] private float directionSmoothTime = 0.1f; // 方向平滑时间

        [Header("Jump加速参数")]
        [SerializeField] private float jumpBoostMultiplier = 1.5f; // Jump加速倍数
        [SerializeField] private float jumpBoostDuration = 0.5f;   // 单次加速持续时间
        [SerializeField] private float jumpComboWindow = 0.3f;     // 连击时间窗口
        [SerializeField] private int maxComboLevel = 3;            // 最大连击等级

        [Header("平面约束")]
        [SerializeField] private float swimDepth = 0f;             // 游泳深度（Y轴高度）
        [SerializeField] private bool constrainToPlane = true;     // 是否约束在平面上

        [Header("边界设置")]
        [SerializeField] private bool useGlobalBoundary = true;    // 使用全局边界（GameBoundary）
        [SerializeField] private float boundaryPushForce = 5f;     // 边界推力

        [Header("Phase2设置")]
        [SerializeField] private float phase2LeftAngle = -45f;     // Phase2左方向角度
        [SerializeField] private float phase2RightAngle = 45f;     // Phase2右方向角度
        [SerializeField] private float phase2RotationSpeed = 180f; // Phase2旋转速度

        // 内部状态
        private float currentSpeed;                    // 当前速度
        private float targetSpeed;                     // 目标速度
        private float currentDirection;                // 当前朝向（角度）
        private float targetDirection;                 // 目标朝向
        private Vector2 moveInput;                     // 移动输入
        private Vector3 velocity;                      // 速度向量

        // Jump加速相关
        private float lastJumpTime;                    // 上次Jump时间
        private int jumpComboLevel;                    // 当前连击等级
        private float jumpBoostEndTime;                // 加速结束时间
        private bool isJumpBoosting;                   // 是否正在加速

        // 方向平滑
        private float directionVelocity;               // 方向插值速度

        // Phase2状态
        private bool isPhase2Mode;                     // 是否处于Phase2模式
        private Vector2 phase2DirectionInput;          // Phase2方向输入（归一化）

        // 属性访问器
        public float CurrentSpeed => currentSpeed;
        public float CurrentDirection => currentDirection;
        public Vector3 Velocity => velocity;
        public bool IsMoving => currentSpeed > 0.1f;

        private void Start()
        {
            // 初始化
            currentSpeed = baseSpeed;
            targetSpeed = baseSpeed;
            currentDirection = transform.eulerAngles.y;
            targetDirection = currentDirection;

            // 设置初始高度
            if (constrainToPlane)
            {
                Vector3 pos = transform.position;
                pos.y = swimDepth;
                transform.position = pos;
            }
        }

        private void Update()
        {
            if (isPhase2Mode)
            {
                // Phase2模式：只更新朝向和动画，不移动
                UpdatePhase2Direction();
                // Jump按键在Phase2仍然影响动画速度
                UpdateJumpBoost();
            }
            else
            {
                // Phase1模式：正常移动逻辑
                // 处理Jump加速状态
                UpdateJumpBoost();

                // 更新方向
                UpdateDirection();

                // 更新速度
                UpdateSpeed();

                // 应用移动
                ApplyMovement();

                // 检查边界
                CheckBoundaries();
            }
        }

        /// <summary>
        /// 设置移动输入
        /// </summary>
        public void SetMoveInput(Vector2 input)
        {
            // 过滤掉向后的输入（鱼不能倒退）
            if (input.y < 0)
            {
                input.y = 0;
            }

            moveInput = input;

            // 计算目标方向
            CalculateTargetDirection();
        }

        /// <summary>
        /// 处理Jump按键（用于加速）
        /// </summary>
        public void OnJumpPressed(bool isPressed)
        {
            if (!isPressed) return;

            float currentTime = Time.time;

            // 检查连击窗口
            if (currentTime - lastJumpTime < jumpComboWindow)
            {
                jumpComboLevel = Mathf.Min(jumpComboLevel + 1, maxComboLevel);
            }
            else
            {
                jumpComboLevel = 1;
            }

            lastJumpTime = currentTime;
            isJumpBoosting = true;
            jumpBoostEndTime = currentTime + jumpBoostDuration;

            // 立即应用加速
            float boostMultiplier = 1f + (jumpBoostMultiplier - 1f) * (jumpComboLevel / (float)maxComboLevel);
            targetSpeed = Mathf.Min(currentSpeed * boostMultiplier, maxSpeed);
        }

        /// <summary>
        /// 计算目标方向
        /// </summary>
        private void CalculateTargetDirection()
        {
            // 无输入时保持当前方向
            if (moveInput.magnitude < 0.1f)
            {
                targetDirection = currentDirection;
                return;
            }

            float turnAngle = 0f;

            // 根据输入计算转向角度
            if (Mathf.Abs(moveInput.x) > 0.1f) // 有左右输入
            {
                if (moveInput.y > 0.1f) // 同时有前进输入
                {
                    // 前+左/右：转向30度
                    turnAngle = moveInput.x > 0 ? turnAngleSmall : -turnAngleSmall;
                }
                else
                {
                    // 纯左/右：转向60度
                    turnAngle = moveInput.x > 0 ? turnAngleLarge : -turnAngleLarge;
                }
            }
            // 纯前进输入：保持当前方向

            targetDirection = currentDirection + turnAngle;
        }

        /// <summary>
        /// 更新Jump加速状态
        /// </summary>
        private void UpdateJumpBoost()
        {
            if (isJumpBoosting && Time.time > jumpBoostEndTime)
            {
                isJumpBoosting = false;

                // 检查是否还在连击窗口内
                if (Time.time - lastJumpTime > jumpComboWindow * 1.5f)
                {
                    jumpComboLevel = 0;
                }
            }
        }

        /// <summary>
        /// 更新方向
        /// </summary>
        private void UpdateDirection()
        {
            // 根据速度调整转向速度
            float speedRatio = currentSpeed / maxSpeed;
            float currentTurnSpeed = Mathf.Lerp(turnSpeedBase, turnSpeedAtMaxSpeed, speedRatio);

            // 平滑插值到目标方向
            currentDirection = Mathf.SmoothDampAngle(
                currentDirection,
                targetDirection,
                ref directionVelocity,
                directionSmoothTime,
                currentTurnSpeed
            );

            // 应用旋转
            transform.rotation = Quaternion.Euler(0, currentDirection, 0);
        }

        /// <summary>
        /// 更新速度
        /// </summary>
        private void UpdateSpeed()
        {
            // 确定目标速度
            if (!isJumpBoosting)
            {
                if (moveInput.magnitude < 0.1f)
                {
                    // 无输入时减速但保持基础速度
                    targetSpeed = baseSpeed * 0.5f;
                }
                else
                {
                    targetSpeed = baseSpeed;
                }
            }

            // 平滑过渡到目标速度
            if (currentSpeed < targetSpeed)
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
            }
            else
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, deceleration * Time.deltaTime);
            }

            // 转向时的速度衰减（注意:这里应该降低目标速度而不是直接修改当前速度）
            // 移除了原来每帧都乘以dampeningFactor导致速度归零的bug
            // 如果需要转向衰减,应该修改targetSpeed而不是currentSpeed
        }

        /// <summary>
        /// 应用移动
        /// </summary>
        private void ApplyMovement()
        {
            // 计算前进方向
            Vector3 forward = transform.forward;

            // 计算速度向量
            velocity = forward * currentSpeed;

            // 约束在平面上
            if (constrainToPlane)
            {
                velocity.y = 0;
            }

            // 应用位置更新
            Vector3 newPosition = transform.position + velocity * Time.deltaTime;

            // 保持Y轴高度
            if (constrainToPlane)
            {
                newPosition.y = swimDepth;
            }

            // 调试输出
            if (moveInput.magnitude > 0.1f)
            {
                Debug.Log($"[ApplyMovement] currentSpeed={currentSpeed:F2}, targetSpeed={targetSpeed:F2}, " +
                         $"velocity={velocity.magnitude:F2}, moveInput={moveInput}, " +
                         $"angleDiff={Mathf.Abs(Mathf.DeltaAngle(currentDirection, targetDirection)):F1}, " +
                         $"isJumpBoosting={isJumpBoosting}");
            }

            transform.position = newPosition;
        }

        /// <summary>
        /// 检查边界
        /// </summary>
        private void CheckBoundaries()
        {
            // 如果使用全局边界
            if (useGlobalBoundary && GameBoundary.Instance != null)
            {
                CheckGlobalBoundary();
            }
        }

        /// <summary>
        /// 检查全局边界
        /// </summary>
        private void CheckGlobalBoundary()
        {
            GameBoundary boundary = GameBoundary.Instance;
            Vector3 pos = transform.position;
            Vector3 originalPos = pos;

            // 获取边界
            Vector2 minBounds = boundary.MinBounds;
            Vector2 maxBounds = boundary.MaxBounds;

            bool hitBoundary = false;

            // X轴边界
            if (pos.x < minBounds.x || pos.x > maxBounds.x)
            {
                float pushDirection = pos.x < minBounds.x ? 1f : -1f;
                pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);

                // 添加反向推力
                velocity.x += pushDirection * boundaryPushForce;
                hitBoundary = true;
            }

            // Z轴边界
            if (pos.z < minBounds.y || pos.z > maxBounds.y)
            {
                float pushDirection = pos.z < minBounds.y ? 1f : -1f;
                pos.z = Mathf.Clamp(pos.z, minBounds.y, maxBounds.y);

                // 添加反向推力
                velocity.z += pushDirection * boundaryPushForce;
                hitBoundary = true;
            }

            if (hitBoundary)
            {
                transform.position = pos;

                // 减速
                currentSpeed *= 0.5f;

                // 调整方向朝向边界中心
                Vector3 toCenter = boundary.BoundaryCenter - pos;
                toCenter.y = 0;
                if (toCenter.magnitude > 0.1f)
                {
                    targetDirection = Quaternion.LookRotation(toCenter.normalized).eulerAngles.y;
                }
            }
        }

        /// <summary>
        /// 启用Phase2模式
        /// </summary>
        public void EnablePhase2Mode()
        {
            isPhase2Mode = true;
            phase2DirectionInput = Vector2.zero;

            Debug.Log("[FishMovement] Phase2模式已启用");
        }

        /// <summary>
        /// 禁用Phase2模式
        /// </summary>
        public void DisablePhase2Mode()
        {
            isPhase2Mode = false;

            Debug.Log("[FishMovement] Phase2模式已禁用");
        }

        /// <summary>
        /// 设置Phase2方向输入（归一化）
        /// </summary>
        public void SetPhase2DirectionInput(Vector2 direction)
        {
            phase2DirectionInput = direction;
        }

        /// <summary>
        /// 更新Phase2朝向
        /// </summary>
        private void UpdatePhase2Direction()
        {
            // 根据归一化方向输入确定目标角度
            float targetAngle = currentDirection;

            if (phase2DirectionInput.x > 0.5f)
            {
                // 左方向
                targetAngle = phase2LeftAngle;
            }
            else if (phase2DirectionInput.x < -0.5f)
            {
                // 右方向
                targetAngle = phase2RightAngle;
            }

            // 平滑旋转到目标角度
            currentDirection = Mathf.MoveTowardsAngle(
                currentDirection,
                targetAngle,
                phase2RotationSpeed * Time.deltaTime
            );

            // 应用旋转
            transform.rotation = Quaternion.Euler(0, currentDirection, 0);
        }

        /// <summary>
        /// 获取Jump连击等级（用于Phase2动画速度）
        /// </summary>
        public int GetJumpComboLevel()
        {
            return jumpComboLevel;
        }

        /// <summary>
        /// 获取是否正在Jump加速
        /// </summary>
        public bool IsJumpBoosting()
        {
            return isJumpBoosting;
        }

        /// <summary>
        /// 获取调试信息
        /// </summary>
        public string GetDebugInfo()
        {
            if (isPhase2Mode)
            {
                return $"[Phase2] Direction: {currentDirection:F0}° | " +
                       $"Input: {phase2DirectionInput} | " +
                       $"Jump Combo: {jumpComboLevel}";
            }

            return $"Speed: {currentSpeed:F1}/{maxSpeed:F1} | " +
                   $"Direction: {currentDirection:F0}° | " +
                   $"Jump Combo: {jumpComboLevel} | " +
                   $"Boosting: {isJumpBoosting}";
        }

        private void OnDrawGizmosSelected()
        {
            // 绘制前进方向
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, transform.forward * 2f);

            // 绘制速度向量
            if (Application.isPlaying)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, velocity.normalized * (currentSpeed / maxSpeed * 3f));
            }
        }
    }
}
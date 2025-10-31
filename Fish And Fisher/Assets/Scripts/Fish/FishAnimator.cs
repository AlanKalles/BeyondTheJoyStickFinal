using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FishAndFisher.Fish
{
    /// <summary>
    /// 鱼玩家的动画控制器
    /// 负责根据运动状态控制鱼的视觉表现
    /// </summary>
    public class FishAnimator : MonoBehaviour
    {
        [Header("动画组件")]
        [SerializeField] private Animator animator;                // Animator组件（如果使用）
        [SerializeField] private bool useAnimator = false;         // 是否使用Animator

        [Header("程序化动画")]
        [SerializeField] private bool useProceduralAnimation = true;   // 使用程序化动画
        [SerializeField] private Transform fishBody;                    // 鱼身主体
        [SerializeField] private Transform fishTail;                    // 鱼尾
        [SerializeField] private Transform[] fishFins;                  // 鱼鳍数组

        [Header("游泳动画参数")]
        [SerializeField] private float swimCycleSpeed = 2f;         // 游泳循环速度
        [SerializeField] private float tailSwingAmplitude = 30f;    // 尾巴摆动幅度
        [SerializeField] private float tailSwingOffset = 0.5f;      // 尾巴摆动相位偏移
        [SerializeField] private float bodyWaveAmplitude = 5f;      // 身体波动幅度

        [Header("转向动画参数")]
        [SerializeField] private float turnTiltAmount = 15f;        // 转向倾斜角度
        [SerializeField] private float turnTiltSpeed = 5f;          // 倾斜速度
        [SerializeField] private float finSpreadAmount = 20f;       // 鱼鳍展开角度

        [Header("速度响应")]
        [SerializeField] private float minAnimSpeed = 0.5f;         // 最小动画速度
        [SerializeField] private float maxAnimSpeed = 3f;           // 最大动画速度
        [SerializeField] private float speedResponseTime = 0.3f;    // 速度响应时间

        [Header("状态动画")]
        [SerializeField] private float stunnedWobbleSpeed = 4f;     // 眩晕摇摆速度
        [SerializeField] private float stunnedWobbleAmount = 10f;   // 眩晕摇摆幅度
        [SerializeField] private float escapingShakeAmount = 5f;    // 逃脱挣扎幅度
        [SerializeField] private float escapingShakeSpeed = 10f;    // 逃脱挣扎速度

        // 组件引用
        private FishMovement movement;
        private FishState fishState;

        // 动画状态
        private float swimPhase;                    // 游泳相位
        private float currentAnimSpeed;             // 当前动画速度
        private float currentTilt;                  // 当前倾斜角度
        private float targetTilt;                   // 目标倾斜角度
        private Vector3 originalBodyRotation;       // 原始身体旋转
        private Vector3 originalTailRotation;       // 原始尾巴旋转
        private Vector3[] originalFinRotations;     // 原始鱼鳍旋转

        // 状态相关
        private bool isStunned;
        private bool isEscaping;
        private float stateAnimPhase;

        private void Awake()
        {
            // 获取组件引用
            movement = GetComponent<FishMovement>();
            fishState = GetComponent<FishState>();

            // 如果没有指定，尝试自动查找
            if (fishBody == null) fishBody = transform.Find("Body");
            if (fishTail == null) fishTail = transform.Find("Tail");

            // 查找Animator组件
            if (animator == null && useAnimator)
            {
                animator = GetComponentInChildren<Animator>();
            }

            // 保存原始旋转
            SaveOriginalRotations();
        }

        private void Start()
        {
            // 订阅状态变化事件
            if (fishState != null)
            {
                fishState.OnStateChanged += OnStateChanged;
            }
        }

        private void OnDestroy()
        {
            // 取消订阅事件
            if (fishState != null)
            {
                fishState.OnStateChanged -= OnStateChanged;
            }
        }

        private void Update()
        {
            if (useProceduralAnimation)
            {
                UpdateProceduralAnimation();
            }

            if (useAnimator && animator != null)
            {
                UpdateAnimatorParameters();
            }
        }

        /// <summary>
        /// 保存原始旋转值
        /// </summary>
        private void SaveOriginalRotations()
        {
            if (fishBody != null)
                originalBodyRotation = fishBody.localEulerAngles;

            if (fishTail != null)
                originalTailRotation = fishTail.localEulerAngles;

            if (fishFins != null && fishFins.Length > 0)
            {
                originalFinRotations = new Vector3[fishFins.Length];
                for (int i = 0; i < fishFins.Length; i++)
                {
                    if (fishFins[i] != null)
                        originalFinRotations[i] = fishFins[i].localEulerAngles;
                }
            }
        }

        /// <summary>
        /// 更新程序化动画
        /// </summary>
        private void UpdateProceduralAnimation()
        {
            // 获取速度信息
            float speed = movement != null ? movement.CurrentSpeed : 0f;
            float normalizedSpeed = Mathf.Clamp01(speed / 8f); // 假设最大速度为8

            // 计算动画速度
            float targetAnimSpeed = Mathf.Lerp(minAnimSpeed, maxAnimSpeed, normalizedSpeed);
            currentAnimSpeed = Mathf.Lerp(currentAnimSpeed, targetAnimSpeed, Time.deltaTime / speedResponseTime);

            // 更新游泳相位
            swimPhase += currentAnimSpeed * swimCycleSpeed * Time.deltaTime;

            // 应用不同状态的动画
            if (isStunned)
            {
                AnimateStunnedState();
            }
            else if (isEscaping)
            {
                AnimateEscapingState();
            }
            else
            {
                AnimateSwimmingState(normalizedSpeed);
            }

            // 应用转向动画
            AnimateTurning();
        }

        /// <summary>
        /// 游泳状态动画
        /// </summary>
        private void AnimateSwimmingState(float speedFactor)
        {
            // 身体波动
            if (fishBody != null)
            {
                float bodyWave = Mathf.Sin(swimPhase) * bodyWaveAmplitude * speedFactor;
                Vector3 bodyRotation = originalBodyRotation;
                bodyRotation.y += bodyWave;
                fishBody.localRotation = Quaternion.Euler(bodyRotation);
            }

            // 尾巴摆动
            if (fishTail != null)
            {
                float tailSwing = Mathf.Sin(swimPhase + tailSwingOffset) * tailSwingAmplitude * speedFactor;
                Vector3 tailRotation = originalTailRotation;
                tailRotation.y += tailSwing;
                fishTail.localRotation = Quaternion.Euler(tailRotation);
            }

            // 鱼鳍动画
            AnimateFins(speedFactor);
        }

        /// <summary>
        /// 眩晕状态动画
        /// </summary>
        private void AnimateStunnedState()
        {
            stateAnimPhase += Time.deltaTime * stunnedWobbleSpeed;

            if (fishBody != null)
            {
                float wobble = Mathf.Sin(stateAnimPhase) * stunnedWobbleAmount;
                Vector3 bodyRotation = originalBodyRotation;
                bodyRotation.z += wobble;
                bodyRotation.x += Mathf.Cos(stateAnimPhase * 0.7f) * stunnedWobbleAmount * 0.5f;
                fishBody.localRotation = Quaternion.Euler(bodyRotation);
            }
        }

        /// <summary>
        /// 逃脱状态动画
        /// </summary>
        private void AnimateEscapingState()
        {
            stateAnimPhase += Time.deltaTime * escapingShakeSpeed;

            if (fishBody != null)
            {
                float shakeX = Mathf.PerlinNoise(stateAnimPhase, 0) * escapingShakeAmount - escapingShakeAmount * 0.5f;
                float shakeZ = Mathf.PerlinNoise(0, stateAnimPhase) * escapingShakeAmount - escapingShakeAmount * 0.5f;

                Vector3 bodyRotation = originalBodyRotation;
                bodyRotation.x += shakeX;
                bodyRotation.z += shakeZ;
                fishBody.localRotation = Quaternion.Euler(bodyRotation);
            }

            // 尾巴剧烈摆动
            if (fishTail != null)
            {
                float tailShake = Mathf.Sin(stateAnimPhase * 2f) * tailSwingAmplitude * 1.5f;
                Vector3 tailRotation = originalTailRotation;
                tailRotation.y += tailShake;
                fishTail.localRotation = Quaternion.Euler(tailRotation);
            }
        }

        /// <summary>
        /// 转向动画
        /// </summary>
        private void AnimateTurning()
        {
            if (movement == null) return;

            // 获取输入方向
            Vector2 input = Vector2.zero;
            if (movement != null)
            {
                // 这里需要从movement获取转向信息
                // 简化处理：根据角速度计算倾斜
                float angularVelocity = 0f; // 需要从movement获取
                targetTilt = angularVelocity * turnTiltAmount;
            }

            // 平滑过渡倾斜角度
            currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * turnTiltSpeed);

            // 应用倾斜
            if (fishBody != null && Mathf.Abs(currentTilt) > 0.1f)
            {
                Vector3 rotation = fishBody.localEulerAngles;
                rotation.z = currentTilt;
                fishBody.localRotation = Quaternion.Euler(rotation);
            }
        }

        /// <summary>
        /// 鱼鳍动画
        /// </summary>
        private void AnimateFins(float speedFactor)
        {
            if (fishFins == null || fishFins.Length == 0) return;

            for (int i = 0; i < fishFins.Length; i++)
            {
                if (fishFins[i] == null) continue;

                // 每个鱼鳍有不同的相位
                float finPhase = swimPhase + i * 0.3f;
                float finMovement = Mathf.Sin(finPhase) * 15f * speedFactor;

                Vector3 finRotation = originalFinRotations[i];
                finRotation.z += finMovement;

                // 转向时展开鱼鳍
                if (Mathf.Abs(currentTilt) > 5f)
                {
                    finRotation.y += Mathf.Sign(currentTilt) * finSpreadAmount * (i % 2 == 0 ? 1 : -1);
                }

                fishFins[i].localRotation = Quaternion.Euler(finRotation);
            }
        }

        /// <summary>
        /// 更新Animator参数
        /// </summary>
        private void UpdateAnimatorParameters()
        {
            if (animator == null) return;

            // 设置速度参数
            float speed = movement != null ? movement.CurrentSpeed : 0f;
            animator.SetFloat("Speed", speed);

            // 设置方向参数
            float direction = movement != null ? movement.CurrentDirection : 0f;
            animator.SetFloat("Direction", direction);

            // 设置状态参数
            if (fishState != null)
            {
                animator.SetInteger("State", (int)fishState.CurrentState);
            }
        }

        /// <summary>
        /// 状态变化回调
        /// </summary>
        private void OnStateChanged(FishStateType newState)
        {
            // 重置状态动画相位
            stateAnimPhase = 0f;

            // 更新状态标志
            isStunned = (newState == FishStateType.Stunned);
            isEscaping = (newState == FishStateType.Escaping);

            // 如果使用Animator，触发相应的动画
            if (useAnimator && animator != null)
            {
                switch (newState)
                {
                    case FishStateType.Idle:
                        animator.SetTrigger("Idle");
                        break;
                    case FishStateType.Swimming:
                        animator.SetTrigger("Swim");
                        break;
                    case FishStateType.Sprinting:
                        animator.SetTrigger("Sprint");
                        break;
                    case FishStateType.Stunned:
                        animator.SetTrigger("Stunned");
                        break;
                    case FishStateType.Escaping:
                        animator.SetTrigger("Escape");
                        break;
                    case FishStateType.Caught:
                        animator.SetTrigger("Caught");
                        break;
                }
            }
        }

        /// <summary>
        /// 播放特定动画
        /// </summary>
        public void PlayAnimation(string animationName)
        {
            if (useAnimator && animator != null)
            {
                animator.Play(animationName);
            }
        }

        /// <summary>
        /// 触发动画事件
        /// </summary>
        public void TriggerAnimationEvent(string eventName)
        {
            if (useAnimator && animator != null)
            {
                animator.SetTrigger(eventName);
            }
        }

        /// <summary>
        /// 重置动画状态
        /// </summary>
        public void ResetAnimation()
        {
            swimPhase = 0f;
            currentAnimSpeed = minAnimSpeed;
            currentTilt = 0f;
            targetTilt = 0f;
            stateAnimPhase = 0f;
            isStunned = false;
            isEscaping = false;

            // 重置所有变换
            if (fishBody != null)
                fishBody.localRotation = Quaternion.Euler(originalBodyRotation);

            if (fishTail != null)
                fishTail.localRotation = Quaternion.Euler(originalTailRotation);

            if (fishFins != null && originalFinRotations != null)
            {
                for (int i = 0; i < fishFins.Length && i < originalFinRotations.Length; i++)
                {
                    if (fishFins[i] != null)
                        fishFins[i].localRotation = Quaternion.Euler(originalFinRotations[i]);
                }
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FishAndFisher.Fish
{
    /// <summary>
    /// 鱼玩家的动画控制器
    /// 负责根据运动状态控制鱼的视觉表现
    /// 只使用 Animator Controller，不创建模型
    /// </summary>
    public class FishAnimator : MonoBehaviour
    {
        [Header("动画组件")]
        [SerializeField] private Animator animator;                // Animator组件引用
        [SerializeField] private string moveAnimationName = "Swim"; // 移动动画名称

        [Header("速度控制")]
        [SerializeField] private float minSpeed = 0f;              // 最小速度
        [SerializeField] private float maxSpeed = 8f;              // 最大速度（用于归一化）
        [SerializeField] private float minAnimSpeed = 0.5f;        // 最小动画播放速度
        [SerializeField] private float maxAnimSpeed = 3f;          // 最大动画播放速度

        // 组件引用
        private FishMovement movement;

        private void Awake()
        {
            // 获取组件引用
            movement = GetComponent<FishMovement>();

            // 查找Animator组件
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        private void Start()
        {
            // 如果有Animator且有移动动画，播放它
            if (animator != null && !string.IsNullOrEmpty(moveAnimationName))
            {
                animator.Play(moveAnimationName);
            }
        }

        private void Update()
        {
            // 根据速度更新动画播放速度
            UpdateAnimationSpeed();
        }

        /// <summary>
        /// 更新动画播放速度
        /// 根据鱼的移动速度调整动画播放速度
        /// </summary>
        private void UpdateAnimationSpeed()
        {
            // 如果没有Animator，直接返回
            if (animator == null) return;

            // 获取当前移动速度
            float currentSpeed = movement != null ? movement.CurrentSpeed : 0f;

            // 将速度归一化到0-1范围
            float normalizedSpeed = Mathf.Clamp01((currentSpeed - minSpeed) / (maxSpeed - minSpeed));

            // 根据归一化速度计算动画播放速度
            float animSpeed = Mathf.Lerp(minAnimSpeed, maxAnimSpeed, normalizedSpeed);

            // 设置Animator的播放速度
            animator.speed = animSpeed;
        }

        /// <summary>
        /// 播放特定动画
        /// 通过名称播放AnimatorController中的动画
        /// </summary>
        /// <param name="animationName">动画状态名称</param>
        public void PlayAnimation(string animationName)
        {
            if (animator != null && !string.IsNullOrEmpty(animationName))
            {
                animator.Play(animationName);
            }
        }

        /// <summary>
        /// 设置动画播放速度
        /// </summary>
        /// <param name="speed">播放速度倍率</param>
        public void SetAnimationSpeed(float speed)
        {
            if (animator != null)
            {
                animator.speed = speed;
            }
        }

        /// <summary>
        /// 获取当前动画播放速度
        /// </summary>
        public float GetAnimationSpeed()
        {
            return animator != null ? animator.speed : 0f;
        }
    }
}
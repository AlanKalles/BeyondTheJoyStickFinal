using UnityEngine;
using UnityEngine.UI;

namespace FishAndFisher
{
    /// <summary>
    /// 进度条柄Sprite切换器 - 根据进度条的增减方向动态切换柄的Sprite
    /// </summary>
    public class ProgressBarHandleSpriteSwitcher : MonoBehaviour
    {
        [Header("组件引用")]
        [Tooltip("进度条Slider组件")]
        [SerializeField] private Slider progressSlider;

        [Tooltip("柄的Image组件")]
        [SerializeField] private Image handleImage;

        [Header("Sprite设置")]
        [Tooltip("进度增加时显示的Sprite")]
        [SerializeField] private Sprite increasingSprite;

        [Tooltip("进度减少时显示的Sprite")]
        [SerializeField] private Sprite decreasingSprite;

        [Tooltip("进度不变时显示的Sprite（可选，留空则保持当前Sprite）")]
        [SerializeField] private Sprite idleSprite;

        [Header("检测设置")]
        [Tooltip("变化阈值（小于此值视为不变）")]
        [SerializeField] private float changeThreshold = 0.001f;

        [Header("调试")]
        [Tooltip("启用调试日志")]
        [SerializeField] private bool enableDebugLog = false;

        // 上一帧的进度值
        private float previousValue;

        // 当前方向状态（-1=减少，0=不变，1=增加）
        private int currentDirection = 0;

        private void Awake()
        {
            // 自动查找组件（如果未设置）
            if (progressSlider == null)
            {
                progressSlider = GetComponent<Slider>();
                if (progressSlider == null)
                {
                    progressSlider = GetComponentInParent<Slider>();
                }
            }

            if (handleImage == null && progressSlider != null)
            {
                // 尝试从Slider的handleRect获取Image
                if (progressSlider.handleRect != null)
                {
                    handleImage = progressSlider.handleRect.GetComponent<Image>();
                }
            }

            // 验证引用
            if (progressSlider == null)
            {
                Debug.LogError("[ProgressBarHandleSpriteSwitcher] Slider组件未找到！");
                enabled = false;
                return;
            }

            if (handleImage == null)
            {
                Debug.LogError("[ProgressBarHandleSpriteSwitcher] 柄的Image组件未找到！");
                enabled = false;
                return;
            }
        }

        private void OnEnable()
        {
            if (progressSlider != null)
            {
                previousValue = progressSlider.value;

                // 设置初始Sprite
                if (idleSprite != null)
                {
                    handleImage.sprite = idleSprite;
                }
            }
        }

        private void Update()
        {
            if (progressSlider == null || handleImage == null)
            {
                return;
            }

            float currentValue = progressSlider.value;
            float delta = currentValue - previousValue;

            // 判断变化方向
            int newDirection;
            if (delta > changeThreshold)
            {
                newDirection = 1; // 增加
            }
            else if (delta < -changeThreshold)
            {
                newDirection = -1; // 减少
            }
            else
            {
                newDirection = 0; // 不变
            }

            // 方向变化时更新Sprite
            if (newDirection != currentDirection)
            {
                UpdateHandleSprite(newDirection);
                currentDirection = newDirection;
            }

            previousValue = currentValue;
        }

        /// <summary>
        /// 根据方向更新柄的Sprite
        /// </summary>
        private void UpdateHandleSprite(int direction)
        {
            Sprite targetSprite = null;

            switch (direction)
            {
                case 1: // 增加
                    targetSprite = increasingSprite;
                    break;
                case -1: // 减少
                    targetSprite = decreasingSprite;
                    break;
                case 0: // 不变
                    targetSprite = idleSprite;
                    break;
            }

            // 如果目标Sprite为空，保持当前Sprite不变
            if (targetSprite != null)
            {
                handleImage.sprite = targetSprite;

                if (enableDebugLog)
                {
                    string directionName = direction > 0 ? "增加" : (direction < 0 ? "减少" : "不变");
                    Debug.Log($"[ProgressBarHandleSpriteSwitcher] 进度{directionName} → 切换Sprite");
                }
            }
        }

        /// <summary>
        /// 手动设置Slider引用（运行时设置）
        /// </summary>
        public void SetSlider(Slider slider)
        {
            progressSlider = slider;
            if (progressSlider != null)
            {
                previousValue = progressSlider.value;
            }
        }

        /// <summary>
        /// 手动设置柄的Image引用（运行时设置）
        /// </summary>
        public void SetHandleImage(Image image)
        {
            handleImage = image;
        }
    }
}

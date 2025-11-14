using UnityEngine;
using UnityEngine.InputSystem;

namespace FishAndFisher.Fisher
{
    /// <summary>
    /// 渔夫主控制器 - 管理渔夫的整体行为和鱼竿操作
    /// </summary>
    [RequireComponent(typeof(FisherCrosshairController))]
    public class FisherController : MonoBehaviour
    {
        [Header("鱼竿设置")]
        [Tooltip("鱼竿Transform（可选，用于动画和视觉效果）")]
        [SerializeField] private Transform fishingRod;

        [Tooltip("鱼竿挥动动画时长（秒）")]
        [SerializeField] private float swingDuration = 0.5f;

        [Tooltip("鱼竿挥动冷却时间（秒）")]
        [SerializeField] private float swingCooldown = 1f;

        [Header("钩子设置")]
        [Tooltip("钩子检测半径")]
        [SerializeField] private float hookDetectionRadius = 0.5f;

        [Tooltip("可被钩住的图层")]
        [SerializeField] private LayerMask hookableLayer;

        [Header("Phase2设置")]
        [Tooltip("Phase2鱼竿旋转轴心（用于左右旋转）")]
        [SerializeField] private Transform rodPivot;

        [Tooltip("左方向旋转角度")]
        [SerializeField] private float leftRotationAngle = -45f;

        [Tooltip("右方向旋转角度")]
        [SerializeField] private float rightRotationAngle = 45f;

        [Tooltip("旋转平滑时间")]
        [SerializeField] private float rotationSmoothTime = 0.3f;

        // 组件引用
        private FisherCrosshairController crosshairController;
        private InputSystem_Actions inputActions;

        // 鱼竿状态
        private bool isSwinging = false;
        private float lastSwingTime = -999f;
        private float swingTimer = 0f;

        // Phase2状态
        private bool isPhase2Mode = false;
        private float currentRotationAngle = 0f;
        private float rotationVelocity = 0f;

        private void Awake()
        {
            // 获取准心控制器
            crosshairController = GetComponent<FisherCrosshairController>();

            // 初始化输入系统
            inputActions = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            inputActions.Enable();

            // 订阅Attack输入
            inputActions.Player.Attack.performed += OnAttackPerformed;
        }

        private void OnDisable()
        {
            inputActions.Player.Attack.performed -= OnAttackPerformed;

            inputActions.Disable();
        }

        private void Update()
        {
            if (isPhase2Mode)
            {
                // Phase2模式：更新鱼竿旋转
                UpdatePhase2Rotation();
            }
            else
            {
                // Phase1模式：更新鱼竿挥动动画
                if (isSwinging)
                {
                    UpdateSwingAnimation();
                }
            }
        }

        /// <summary>
        /// Attack输入回调 - 挥动鱼竿
        /// </summary>
        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            TrySwingRod();
        }

        /// <summary>
        /// 尝试挥动鱼竿
        /// </summary>
        private void TrySwingRod()
        {
            // 检查冷却时间
            if (Time.time - lastSwingTime < swingCooldown)
            {
                Debug.Log($"[FisherController] 鱼竿冷却中... 剩余时间: {swingCooldown - (Time.time - lastSwingTime):F2}秒");
                return;
            }

            // 检查是否正在挥动
            if (isSwinging)
            {
                return;
            }

            // 开始挥动
            StartSwing();
        }

        /// <summary>
        /// 开始挥动鱼竿
        /// </summary>
        private void StartSwing()
        {
            isSwinging = true;
            swingTimer = 0f;
            lastSwingTime = Time.time;

            Debug.Log("[FisherController] 开始挥动鱼竿！");

            // 在挥动开始时检测钩子位置的碰撞
            CheckHookCollision();
        }

        /// <summary>
        /// 更新挥动动画
        /// </summary>
        private void UpdateSwingAnimation()
        {
            swingTimer += Time.deltaTime;

            // 动画进度 (0 到 1)
            float progress = swingTimer / swingDuration;

            if (progress >= 1f)
            {
                // 动画结束
                EndSwing();
                return;
            }

            // TODO: 在这里添加鱼竿的动画效果
            // 例如：旋转、缩放、位置变化等
            if (fishingRod != null)
            {
                // 简单示例：让鱼竿进行一个挥动旋转
                // 使用Sin曲线实现挥动效果
                float angle = Mathf.Sin(progress * Mathf.PI) * 30f;
                fishingRod.localRotation = Quaternion.Euler(-angle, 0, 0);
            }
        }

        /// <summary>
        /// 结束挥动
        /// </summary>
        private void EndSwing()
        {
            isSwinging = false;
            swingTimer = 0f;

            Debug.Log("[FisherController] 挥动结束！");

            // 重置鱼竿位置
            if (fishingRod != null)
            {
                fishingRod.localRotation = Quaternion.identity;
            }
        }

        /// <summary>
        /// 检测钩子位置的碰撞
        /// </summary>
        private void CheckHookCollision()
        {
            if (crosshairController == null) return;

            // 获取逻辑准心位置（即钩子的实际位置）
            Vector3 hookPosition = crosshairController.GetLogicCrosshairPosition();

            // 使用球形检测
            Collider[] hitColliders = Physics.OverlapSphere(hookPosition, hookDetectionRadius, hookableLayer);

            if (hitColliders.Length > 0)
            {
                Debug.Log($"[FisherController] 钩住了 {hitColliders.Length} 个对象！");

                foreach (Collider col in hitColliders)
                {
                    // 尝试获取鱼的组件
                    // TODO: 根据实际的鱼脚本类型进行处理
                    Debug.Log($"[FisherController] 钩住对象: {col.gameObject.name}");

                    // 示例：触发钩住事件
                    OnHookHit(col.gameObject);
                }
            }
            else
            {
                Debug.Log("[FisherController] 未钩住任何对象");
            }
        }

        /// <summary>
        /// 钩住对象时的处理
        /// </summary>
        private void OnHookHit(GameObject target)
        {
            Debug.Log($"[FisherController] 成功钩住: {target.name}");

            // 检查是否钩住了鱼（可能钩住的是子对象，需要检查父对象）
            Fish.FishController fishController = target.GetComponent<Fish.FishController>();

            // 如果当前对象没有FishController，尝试在父对象中查找
            if (fishController == null && target.transform.parent != null)
            {
                fishController = target.transform.parent.GetComponent<Fish.FishController>();
            }

            // 或者使用更通用的方法：从当前对象向上查找整个层级
            if (fishController == null)
            {
                fishController = target.GetComponentInParent<Fish.FishController>();
            }

            if (fishController != null)
            {
                Debug.Log("[FisherController] 钩住了鱼！进入Phase2准备阶段");

                // 通知GameManager进入Phase2（不再直接判定胜负）
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.OnFishHooked();
                }
                else
                {
                    Debug.LogError("[FisherController] GameManager不存在！");
                }

                // TODO: 添加其他效果
                // - 播放音效
                // - 显示特效
            }
            else
            {
                Debug.Log($"[FisherController] 钩住的对象不是鱼: {target.name}");
            }
        }

        /// <summary>
        /// 启用Phase2模式
        /// </summary>
        public void EnablePhase2Mode()
        {
            isPhase2Mode = true;

            // 冻结准心位置
            if (crosshairController != null)
            {
                crosshairController.enabled = false;
            }

            // 重置旋转角度
            currentRotationAngle = 0f;

            Debug.Log("[FisherController] Phase2模式已启用");
        }

        /// <summary>
        /// 禁用Phase2模式
        /// </summary>
        public void DisablePhase2Mode()
        {
            isPhase2Mode = false;

            // 恢复准心控制
            if (crosshairController != null)
            {
                crosshairController.enabled = true;
            }

            Debug.Log("[FisherController] Phase2模式已禁用");
        }

        /// <summary>
        /// 更新Phase2鱼竿旋转
        /// </summary>
        private void UpdatePhase2Rotation()
        {
            if (rodPivot == null)
            {
                return;
            }

            // 获取鼠标位置相对屏幕中心的方向
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            float screenCenterX = Screen.width / 2f;
            float deltaX = mousePosition.x - screenCenterX;

            // 确定目标旋转角度
            float targetAngle = 0f;
            float deadZone = 5f;

            if (Mathf.Abs(deltaX) < deadZone)
            {
                // 中间位置
                targetAngle = 0f;
            }
            else if (deltaX < 0)
            {
                // 左侧
                targetAngle = leftRotationAngle;
            }
            else
            {
                // 右侧
                targetAngle = rightRotationAngle;
            }

            // 平滑插值到目标角度
            currentRotationAngle = Mathf.SmoothDamp(
                currentRotationAngle,
                targetAngle,
                ref rotationVelocity,
                rotationSmoothTime
            );

            // 应用旋转（Y轴旋转）
            rodPivot.localRotation = Quaternion.Euler(0f, currentRotationAngle, 0f);
        }

        /// <summary>
        /// 获取鱼竿是否正在挥动
        /// </summary>
        public bool IsSwinging()
        {
            return isSwinging;
        }

        /// <summary>
        /// 获取鱼竿冷却剩余时间
        /// </summary>
        public float GetSwingCooldownRemaining()
        {
            float remaining = swingCooldown - (Time.time - lastSwingTime);
            return Mathf.Max(0f, remaining);
        }

        // 可视化调试信息
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            if (crosshairController == null) return;

            // 绘制钩子检测范围
            Gizmos.color = isSwinging ? Color.red : Color.blue;
            Vector3 hookPosition = crosshairController.GetLogicCrosshairPosition();
            Gizmos.DrawWireSphere(hookPosition, hookDetectionRadius);
        }
    }
}

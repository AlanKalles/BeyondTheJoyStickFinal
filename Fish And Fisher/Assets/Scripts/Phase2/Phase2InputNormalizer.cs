using UnityEngine;
using UnityEngine.InputSystem;

namespace FishAndFisher.Phase2
{
    /// <summary>
    /// Phase2输入归一化器 - 将鱼和渔夫的输入归一化为左/右/无方向
    /// </summary>
    public class Phase2InputNormalizer : MonoBehaviour
    {
        [Header("输入动作引用")]
        [Tooltip("输入动作资源")]
        [SerializeField] private InputActionAsset inputActions;

        [Header("渔夫设置")]
        [Tooltip("渔夫相机（用于获取屏幕中心）")]
        [SerializeField] private Camera fisherCamera;

        // 输入动作
        private InputAction fishMoveAction;
        private InputAction fisherLookAction;

        // 当前方向状态
        private Vector2 fishDirection = Vector2.zero;    // (1,0)左 / (-1,0)右 / (0,0)无
        private Vector2 fisherDirection = Vector2.zero;  // (1,0)左 / (-1,0)右 / (0,0)无

        // 鱼玩家最后按下的方向键
        private bool fishPressedLeft = false;
        private bool fishPressedRight = false;

        // 是否启用Phase2输入
        private bool isPhase2Active = false;

        public Vector2 FishDirection => fishDirection;
        public Vector2 FisherDirection => fisherDirection;

        private void Awake()
        {
            // 获取输入动作
            if (inputActions == null)
            {
                inputActions = FindFirstObjectByType<UnityEngine.InputSystem.PlayerInput>()?.actions;
            }

            if (inputActions != null)
            {
                fishMoveAction = inputActions.FindAction("Player/Move");
                fisherLookAction = inputActions.FindAction("Player/Look");
            }

            // 验证渔夫相机
            if (fisherCamera == null)
            {
                Debug.LogWarning("[Phase2InputNormalizer] 渔夫相机未设置！将在运行时查找主相机。");
                fisherCamera = Camera.main;
            }
        }

        private void OnEnable()
        {
            // 订阅鱼玩家的Move输入（键盘事件）
            if (fishMoveAction != null)
            {
                fishMoveAction.performed += OnFishMovePerformed;
                fishMoveAction.canceled += OnFishMoveCanceled;
            }
        }

        private void OnDisable()
        {
            // 取消订阅
            if (fishMoveAction != null)
            {
                fishMoveAction.performed -= OnFishMovePerformed;
                fishMoveAction.canceled -= OnFishMoveCanceled;
            }
        }

        private void Update()
        {
            if (!isPhase2Active)
            {
                return;
            }

            // 更新渔夫方向（基于鼠标位置）
            UpdateFisherDirection();
        }

        /// <summary>
        /// 启动Phase2输入检测
        /// </summary>
        public void ActivatePhase2Input()
        {
            isPhase2Active = true;

            // 重置方向状态
            fishDirection = Vector2.zero;
            fisherDirection = Vector2.zero;
            fishPressedLeft = false;
            fishPressedRight = false;

            Debug.Log("[Phase2InputNormalizer] Phase2输入已激活");
        }

        /// <summary>
        /// 停用Phase2输入检测
        /// </summary>
        public void DeactivatePhase2Input()
        {
            isPhase2Active = false;
            fishDirection = Vector2.zero;
            fisherDirection = Vector2.zero;

            Debug.Log("[Phase2InputNormalizer] Phase2输入已停用");
        }

        /// <summary>
        /// 鱼玩家Move输入 - 检测A/D按键
        /// </summary>
        private void OnFishMovePerformed(InputAction.CallbackContext context)
        {
            if (!isPhase2Active)
            {
                return;
            }

            Vector2 input = context.ReadValue<Vector2>();

            // 只处理左右输入（忽略前后）
            // A键（左）: x < 0
            // D键（右）: x > 0
            if (input.x < -0.5f)
            {
                // 按下A键（左）
                fishPressedLeft = true;
                fishPressedRight = false;
                fishDirection = new Vector2(1, 0); // 左方向
            }
            else if (input.x > 0.5f)
            {
                // 按下D键（右）
                fishPressedRight = true;
                fishPressedLeft = false;
                fishDirection = new Vector2(-1, 0); // 右方向
            }
        }

        /// <summary>
        /// 鱼玩家Move输入取消
        /// </summary>
        private void OnFishMoveCanceled(InputAction.CallbackContext context)
        {
            if (!isPhase2Active)
            {
                return;
            }

            // 检查当前输入状态
            Vector2 currentInput = fishMoveAction.ReadValue<Vector2>();

            // 如果横向输入归零，说明玩家松开了所有左右键
            if (Mathf.Abs(currentInput.x) < 0.1f)
            {
                // 不改变方向状态，保持最后按下的方向
                // 除非玩家按下了反方向键
            }
        }

        /// <summary>
        /// 更新渔夫方向 - 基于鼠标相对渔夫相机视口中心的位置
        /// </summary>
        private void UpdateFisherDirection()
        {
            if (fisherCamera == null)
            {
                return;
            }

            // 获取鼠标屏幕位置
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            // 获取渔夫相机的视口矩形（在屏幕空间中）
            Rect viewportRect = fisherCamera.pixelRect;

            // 计算渔夫相机视口的中心X坐标
            float fisherViewportCenterX = viewportRect.x + viewportRect.width / 2f;

            // 判断鼠标位置相对于渔夫视口中心
            float deltaX = mousePosition.x - fisherViewportCenterX;

            // 死区：刚好在中心线附近（±5像素）视为无输入
            float deadZone = 5f;

            if (Mathf.Abs(deltaX) < deadZone)
            {
                // 无输入
                fisherDirection = Vector2.zero;
            }
            else if (deltaX < 0)
            {
                // 鼠标在视口中心左侧
                fisherDirection = new Vector2(1, 0); // 左方向
            }
            else
            {
                // 鼠标在视口中心右侧
                fisherDirection = new Vector2(-1, 0); // 右方向
            }
        }

        /// <summary>
        /// 检查两个玩家方向是否一致
        /// </summary>
        public bool AreDirectionsMatching()
        {
            // 使用Vector2.Dot来判断方向是否相同
            // 如果点积 > 0.5，认为方向一致
            if (fishDirection == Vector2.zero || fisherDirection == Vector2.zero)
            {
                // 有玩家无输入，不算一致
                return false;
            }

            return Vector2.Dot(fishDirection, fisherDirection) > 0.5f;
        }

        /// <summary>
        /// 检查是否都无输入
        /// </summary>
        public bool BothNoInput()
        {
            return fishDirection == Vector2.zero && fisherDirection == Vector2.zero;
        }

        /// <summary>
        /// 检查鱼无输入
        /// </summary>
        public bool FishNoInput()
        {
            return fishDirection == Vector2.zero;
        }

        /// <summary>
        /// 检查渔夫无输入
        /// </summary>
        public bool FisherNoInput()
        {
            return fisherDirection == Vector2.zero;
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
            style.normal.textColor = Color.yellow;

            string debugInfo = "[Phase2 Input]\n";
            debugInfo += $"鱼方向: {GetDirectionString(fishDirection)}\n";
            debugInfo += $"渔夫方向: {GetDirectionString(fisherDirection)}\n";
            debugInfo += $"方向一致: {AreDirectionsMatching()}\n";

            GUI.Label(new Rect(10, 150, 300, 100), debugInfo, style);
        }

        private string GetDirectionString(Vector2 dir)
        {
            if (dir == Vector2.zero) return "无";
            if (dir.x > 0) return "左";
            if (dir.x < 0) return "右";
            return "未知";
        }
    }
}

using UnityEngine;

namespace FishAndFisher
{
    /// <summary>
    /// 距离追踪器 - 计算并输出鱼和渔夫准心之间的距离数据
    /// 用于数据传输和游戏逻辑判断
    /// </summary>
    public class DistanceTracker : MonoBehaviour
    {
        [Header("引用设置")]
        [Tooltip("鱼的Transform（通常是Fish GameObject）")]
        [SerializeField] private Transform fishTransform;

        [Tooltip("渔夫准心控制器")]
        [SerializeField] private Fisher.FisherCrosshairController fisherCrosshair;

        [Header("输出设置")]
        [Tooltip("是否在控制台输出距离数据")]
        [SerializeField] private bool logDistance = false;

        [Tooltip("日志输出频率（秒）")]
        [SerializeField] private float logInterval = 0.5f;

        [Header("距离映射设置")]
        [Tooltip("最小距离阈值（小于此距离返回0）")]
        [SerializeField] private float minDistanceThreshold = 1f;

        [Tooltip("最大距离阈值系数（相对于边界对角线的比例，1.0 = 整个对角线）")]
        [SerializeField] private float maxDistanceMultiplier = 1.0f;

        [Tooltip("最大输出电压值")]
        [SerializeField] private float maxOutputValue = 3.3f;

        [Tooltip("档位数量")]
        [SerializeField] private int levelCount = 4;

        // 单例实例
        private static DistanceTracker instance;

        // 当前距离数据
        private float currentDistance2D;      // 二维欧氏距离（XZ平面）
        private Vector2 fishPosition2D;       // 鱼的2D位置（XZ平面）
        private Vector2 crosshairPosition2D;  // 准心的2D位置（XZ平面）
        private Vector2 directionToCrosshair; // 从鱼指向准心的方向向量（归一化）

        // 距离映射数据（用于调试）
        private int currentProximityLevel;    // 当前振动档位
        private float currentProximityValue;  // 当前振动值

        // 日志计时器
        private float lastLogTime;

        /// <summary>
        /// 单例访问器
        /// </summary>
        public static DistanceTracker Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<DistanceTracker>();
                    if (instance == null)
                    {
                        Debug.LogWarning("[DistanceTracker] 场景中未找到 DistanceTracker！");
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// 当前二维欧氏距离（XZ平面）
        /// </summary>
        public float CurrentDistance2D => currentDistance2D;

        /// <summary>
        /// 鱼的2D位置（XZ平面）
        /// </summary>
        public Vector2 FishPosition2D => fishPosition2D;

        /// <summary>
        /// 准心的2D位置（XZ平面）
        /// </summary>
        public Vector2 CrosshairPosition2D => crosshairPosition2D;

        /// <summary>
        /// 从鱼指向准心的方向向量（归一化）
        /// </summary>
        public Vector2 DirectionToCrosshair => directionToCrosshair;

        /// <summary>
        /// 当前振动档位（只读，用于调试）
        /// </summary>
        public int CurrentProximityLevel => currentProximityLevel;

        /// <summary>
        /// 当前振动值（只读，用于调试）
        /// </summary>
        public float CurrentProximityValue => currentProximityValue;

        private void Awake()
        {
            // 单例模式检查
            if (instance != null && instance != this)
            {
                Debug.LogWarning($"[DistanceTracker] 场景中存在多个 DistanceTracker！销毁 {gameObject.name} 上的重复实例。");
                Destroy(this);
                return;
            }

            instance = this;
        }

        private void Start()
        {
            // 自动查找引用（如果未手动设置）
            if (fishTransform == null)
            {
                Fish.FishController fishController = FindFirstObjectByType<Fish.FishController>();
                if (fishController != null)
                {
                    fishTransform = fishController.transform;
                    Debug.Log($"[DistanceTracker] 自动找到鱼: {fishTransform.name}");
                }
                else
                {
                    Debug.LogError("[DistanceTracker] 未找到鱼的Transform！请在Inspector中手动设置。");
                }
            }

            if (fisherCrosshair == null)
            {
                fisherCrosshair = FindFirstObjectByType<Fisher.FisherCrosshairController>();
                if (fisherCrosshair != null)
                {
                    Debug.Log("[DistanceTracker] 自动找到渔夫准心控制器");
                }
                else
                {
                    Debug.LogError("[DistanceTracker] 未找到渔夫准心控制器！请在Inspector中手动设置。");
                }
            }
        }

        private void Update()
        {
            // 更新距离数据
            UpdateDistanceData();

            // 更新振动档位和值
            UpdateProximityData();

            // 输出日志（如果启用）
            if (logDistance && Time.time - lastLogTime > logInterval)
            {
                LogDistanceData();
                lastLogTime = Time.time;
            }
        }

        /// <summary>
        /// 更新距离数据
        /// </summary>
        private void UpdateDistanceData()
        {
            if (fishTransform == null || fisherCrosshair == null)
            {
                currentDistance2D = 0f;
                fishPosition2D = Vector2.zero;
                crosshairPosition2D = Vector2.zero;
                directionToCrosshair = Vector2.zero;
                return;
            }

            // 获取鱼的位置（XZ平面）
            Vector3 fishPos3D = fishTransform.position;
            fishPosition2D = new Vector2(fishPos3D.x, fishPos3D.z);

            // 获取准心的逻辑位置（XZ平面）
            Vector3 crosshairPos3D = fisherCrosshair.GetLogicCrosshairPosition();
            crosshairPosition2D = new Vector2(crosshairPos3D.x, crosshairPos3D.z);

            // 计算二维欧氏距离
            currentDistance2D = Vector2.Distance(fishPosition2D, crosshairPosition2D);

            // 计算从鱼指向准心的方向（归一化）
            Vector2 delta = crosshairPosition2D - fishPosition2D;
            directionToCrosshair = delta.magnitude > 0.001f ? delta.normalized : Vector2.zero;
        }

        /// <summary>
        /// 输出距离数据到控制台
        /// </summary>
        private void LogDistanceData()
        {
            Debug.Log($"[DistanceTracker] 距离: {currentDistance2D:F2} | " +
                     $"鱼位置: ({fishPosition2D.x:F2}, {fishPosition2D.y:F2}) | " +
                     $"准心位置: ({crosshairPosition2D.x:F2}, {crosshairPosition2D.y:F2}) | " +
                     $"方向: ({directionToCrosshair.x:F2}, {directionToCrosshair.y:F2})");
        }

        /// <summary>
        /// 获取格式化的距离数据字符串（用于外部传输）
        /// </summary>
        public string GetDistanceDataString()
        {
            return $"{currentDistance2D:F3},{fishPosition2D.x:F3},{fishPosition2D.y:F3}," +
                   $"{crosshairPosition2D.x:F3},{crosshairPosition2D.y:F3}," +
                   $"{directionToCrosshair.x:F3},{directionToCrosshair.y:F3}";
        }

        /// <summary>
        /// 获取JSON格式的距离数据（用于外部传输）
        /// </summary>
        public string GetDistanceDataJson()
        {
            return $"{{" +
                   $"\"distance\":{currentDistance2D:F3}," +
                   $"\"fishX\":{fishPosition2D.x:F3}," +
                   $"\"fishZ\":{fishPosition2D.y:F3}," +
                   $"\"crosshairX\":{crosshairPosition2D.x:F3}," +
                   $"\"crosshairZ\":{crosshairPosition2D.y:F3}," +
                   $"\"directionX\":{directionToCrosshair.x:F3}," +
                   $"\"directionZ\":{directionToCrosshair.y:F3}" +
                   $"}}";
        }

        /// <summary>
        /// 手动设置鱼的Transform引用
        /// </summary>
        public void SetFishTransform(Transform fish)
        {
            fishTransform = fish;
        }

        /// <summary>
        /// 手动设置渔夫准心控制器引用
        /// </summary>
        public void SetFisherCrosshair(Fisher.FisherCrosshairController crosshair)
        {
            fisherCrosshair = crosshair;
        }

        #region 距离映射功能

        /// <summary>
        /// 更新振动档位和值
        /// </summary>
        private void UpdateProximityData()
        {
            currentProximityLevel = CalculateProximityLevel();
            currentProximityValue = CalculateProximityValue(currentProximityLevel);
        }

        /// <summary>
        /// 从GameBoundary获取最大距离（对角线长度）
        /// </summary>
        private float GetMaxDistanceFromBoundary()
        {
            if (GameBoundary.Instance == null) return 50f; // 默认值

            Vector2 size = GameBoundary.Instance.BoundarySize;
            float diagonal = Mathf.Sqrt(size.x * size.x + size.y * size.y);
            return diagonal * maxDistanceMultiplier;
        }

        /// <summary>
        /// 计算当前振动档位（0表示超出范围，1-levelCount表示有效档位）
        /// </summary>
        private int CalculateProximityLevel()
        {
            float maxDistance = GetMaxDistanceFromBoundary();

            // 超出范围返回0
            if (currentDistance2D < minDistanceThreshold || currentDistance2D > maxDistance)
                return 0;

            // 计算归一化距离 (0 = 最近, 1 = 最远)
            float normalizedDistance = (currentDistance2D - minDistanceThreshold) / (maxDistance - minDistanceThreshold);

            // 分档：距离越小档位越高
            // normalizedDistance 0-0.25 → level 4
            // normalizedDistance 0.25-0.5 → level 3
            // normalizedDistance 0.5-0.75 → level 2
            // normalizedDistance 0.75-1.0 → level 1
            int level = levelCount - Mathf.FloorToInt(normalizedDistance * levelCount);
            return Mathf.Clamp(level, 1, levelCount);
        }

        /// <summary>
        /// 根据档位计算振动值
        /// </summary>
        private float CalculateProximityValue(int level)
        {
            if (level == 0) return 0f;

            // 每档的电压值 = maxOutputValue / levelCount * level
            return (maxOutputValue / levelCount) * level;
        }

        /// <summary>
        /// 获取当前振动档位（0-levelCount，0表示超出范围）
        /// </summary>
        public int GetProximityLevel()
        {
            return currentProximityLevel;
        }

        /// <summary>
        /// 获取当前振动值（0 或 档位对应的电压值）
        /// </summary>
        public float GetProximityValue()
        {
            return currentProximityValue;
        }

        #endregion

        /// <summary>
        /// 绘制Gizmos（可视化距离）
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || fishTransform == null || fisherCrosshair == null)
                return;

            Vector3 fishPos = fishTransform.position;
            Vector3 crosshairPos = fisherCrosshair.GetLogicCrosshairPosition();

            // 绘制连接线
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(fishPos, crosshairPos);

            // 在中点显示距离
            Vector3 midPoint = (fishPos + crosshairPos) / 2f + Vector3.up * 0.5f;
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(midPoint, 0.2f);

#if UNITY_EDITOR
            // 显示距离文本
            UnityEditor.Handles.Label(midPoint, $"距离: {currentDistance2D:F2}");
#endif
        }
    }
}

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FishAndFisher
{
    /// <summary>
    /// 游戏边界管理器
    /// 定义鱼和渔夫共享的活动范围
    /// 这是一个单例，确保全局只有一个边界定义
    /// </summary>
    public class GameBoundary : MonoBehaviour
    {
        [Header("边界设置")]
        [SerializeField] private Vector2 boundarySize = new Vector2(50f, 50f); // XZ平面边界大小
        [SerializeField] private float boundaryHeight = 0f;                     // 边界中心高度（Y轴）

        [Header("游戏平面设置")]
        [Tooltip("游戏平面的Y轴高度 - 鱼和渔夫准心都在此平面上移动")]
        [SerializeField] private float gamePlaneY = 0f;                         // 游戏平面高度（鱼和准心共享）

        [Header("可视化设置")]
        [SerializeField] private Color boundaryColor = new Color(0f, 1f, 0f, 0.3f);
        [SerializeField] private bool showBoundary = true;
        [SerializeField] private float gridLineSpacing = 5f;                    // 网格线间距

        // 单例实例
        private static GameBoundary instance;

        /// <summary>
        /// 单例访问器
        /// </summary>
        public static GameBoundary Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<GameBoundary>();
                    if (instance == null)
                    {
                        Debug.LogWarning("场景中未找到 GameBoundary！请通过菜单创建：GameObject > Fish And Fisher > Create Game Boundary");
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// 边界大小（只读）
        /// </summary>
        public Vector2 BoundarySize => boundarySize;

        /// <summary>
        /// 边界中心（世界坐标）
        /// </summary>
        public Vector3 BoundaryCenter => transform.position;

        /// <summary>
        /// 边界高度
        /// </summary>
        public float BoundaryHeight => transform.position.y + boundaryHeight;

        /// <summary>
        /// 游戏平面高度（鱼和渔夫准心共享的Y轴坐标）
        /// </summary>
        public float GamePlaneY => gamePlaneY;

        /// <summary>
        /// 边界最小点（XZ平面）
        /// </summary>
        public Vector2 MinBounds => new Vector2(
            transform.position.x - boundarySize.x / 2f,
            transform.position.z - boundarySize.y / 2f
        );

        /// <summary>
        /// 边界最大点（XZ平面）
        /// </summary>
        public Vector2 MaxBounds => new Vector2(
            transform.position.x + boundarySize.x / 2f,
            transform.position.z + boundarySize.y / 2f
        );

        private void Awake()
        {
            // 单例模式检查
            if (instance != null && instance != this)
            {
                Debug.LogWarning($"场景中存在多个 GameBoundary！销毁 {gameObject.name} 上的重复实例。");
                Destroy(this);
                return;
            }

            instance = this;
        }

        /// <summary>
        /// 检查点是否在边界内（XZ平面）
        /// </summary>
        public bool IsPointInBounds(Vector3 point)
        {
            Vector2 minBounds = MinBounds;
            Vector2 maxBounds = MaxBounds;

            return point.x >= minBounds.x && point.x <= maxBounds.x &&
                   point.z >= minBounds.y && point.z <= maxBounds.y;
        }

        /// <summary>
        /// 将点约束在边界内（XZ平面）
        /// </summary>
        public Vector3 ClampPointToBounds(Vector3 point)
        {
            Vector2 minBounds = MinBounds;
            Vector2 maxBounds = MaxBounds;

            point.x = Mathf.Clamp(point.x, minBounds.x, maxBounds.x);
            point.z = Mathf.Clamp(point.z, minBounds.y, maxBounds.y);

            return point;
        }

        /// <summary>
        /// 将2D点约束在边界内
        /// </summary>
        public Vector2 ClampPoint2DToBounds(Vector2 point)
        {
            Vector2 minBounds = MinBounds;
            Vector2 maxBounds = MaxBounds;

            point.x = Mathf.Clamp(point.x, minBounds.x, maxBounds.x);
            point.y = Mathf.Clamp(point.y, minBounds.y, maxBounds.y);

            return point;
        }

        /// <summary>
        /// 获取点到边界的最短距离（负值表示在边界内）
        /// </summary>
        public float GetDistanceToEdge(Vector3 point)
        {
            Vector2 minBounds = MinBounds;
            Vector2 maxBounds = MaxBounds;

            float distX = Mathf.Min(point.x - minBounds.x, maxBounds.x - point.x);
            float distZ = Mathf.Min(point.z - minBounds.y, maxBounds.y - point.z);

            return Mathf.Min(distX, distZ);
        }

        /// <summary>
        /// 获取随机位置（在边界内）
        /// </summary>
        public Vector3 GetRandomPositionInBounds()
        {
            Vector2 minBounds = MinBounds;
            Vector2 maxBounds = MaxBounds;

            return new Vector3(
                Random.Range(minBounds.x, maxBounds.x),
                BoundaryHeight,
                Random.Range(minBounds.y, maxBounds.y)
            );
        }

        /// <summary>
        /// 绘制Gizmos
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!showBoundary) return;

            Vector3 center = new Vector3(transform.position.x, BoundaryHeight, transform.position.z);
            Vector2 minBounds = MinBounds;
            Vector2 maxBounds = MaxBounds;

            // 绘制边界框
            Gizmos.color = boundaryColor;
            Vector3 size = new Vector3(boundarySize.x, 0.1f, boundarySize.y);
            Gizmos.DrawCube(center, size);

            // 绘制边界线
            Gizmos.color = new Color(boundaryColor.r, boundaryColor.g, boundaryColor.b, 1f);

            Vector3 topLeft = new Vector3(minBounds.x, BoundaryHeight, maxBounds.y);
            Vector3 topRight = new Vector3(maxBounds.x, BoundaryHeight, maxBounds.y);
            Vector3 bottomLeft = new Vector3(minBounds.x, BoundaryHeight, minBounds.y);
            Vector3 bottomRight = new Vector3(maxBounds.x, BoundaryHeight, minBounds.y);

            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);

            // 绘制网格线
            if (gridLineSpacing > 0)
            {
                Gizmos.color = new Color(boundaryColor.r, boundaryColor.g, boundaryColor.b, 0.2f);

                // 垂直线（沿Z轴）
                for (float x = minBounds.x; x <= maxBounds.x; x += gridLineSpacing)
                {
                    Vector3 start = new Vector3(x, BoundaryHeight, minBounds.y);
                    Vector3 end = new Vector3(x, BoundaryHeight, maxBounds.y);
                    Gizmos.DrawLine(start, end);
                }

                // 水平线（沿X轴）
                for (float z = minBounds.y; z <= maxBounds.y; z += gridLineSpacing)
                {
                    Vector3 start = new Vector3(minBounds.x, BoundaryHeight, z);
                    Vector3 end = new Vector3(maxBounds.x, BoundaryHeight, z);
                    Gizmos.DrawLine(start, end);
                }
            }

            // 绘制中心点
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, 0.5f);
        }

        /// <summary>
        /// 绘制已选中的Gizmos
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            // 绘制尺寸标注
            Vector2 minBounds = MinBounds;
            Vector2 maxBounds = MaxBounds;
            Vector3 center = new Vector3(transform.position.x, BoundaryHeight, transform.position.z);

#if UNITY_EDITOR
            // 显示尺寸信息
            Handles.Label(center + Vector3.up * 2f,
                $"边界大小: {boundarySize.x} × {boundarySize.y}\n高度: {BoundaryHeight}");

            Handles.Label(new Vector3(minBounds.x, BoundaryHeight, maxBounds.y),
                $"({minBounds.x:F1}, {maxBounds.y:F1})");

            Handles.Label(new Vector3(maxBounds.x, BoundaryHeight, minBounds.y),
                $"({maxBounds.x:F1}, {minBounds.y:F1})");
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// 创建游戏边界GameObject
        /// 菜单：GameObject > Fish And Fisher > Create Game Boundary
        /// </summary>
        [MenuItem("GameObject/Fish And Fisher/Create Game Boundary", false, 0)]
        public static void CreateGameBoundary()
        {
            // 检查是否已存在GameBoundary
            GameBoundary existing = FindObjectOfType<GameBoundary>();
            if (existing != null)
            {
                Debug.LogWarning($"场景中已存在 GameBoundary（在 {existing.gameObject.name} 上）！");
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            // 创建新的GameObject
            GameObject boundaryObj = new GameObject("GameBoundary");
            boundaryObj.transform.position = Vector3.zero;

            // 添加GameBoundary组件
            GameBoundary boundary = boundaryObj.AddComponent<GameBoundary>();

            // 设置默认参数
            boundary.boundarySize = new Vector2(50f, 50f);
            boundary.boundaryHeight = 0f;
            boundary.showBoundary = true;

            // 选中新创建的对象
            Selection.activeGameObject = boundaryObj;

            Debug.Log("游戏边界已创建！鱼和渔夫将共享此边界范围。");
        }
#endif
    }
}

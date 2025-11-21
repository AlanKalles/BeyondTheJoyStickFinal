using UnityEngine;

/// <summary>
/// LineRenderer连接器 - 使LineRenderer始终连接两个Transform位置
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class LineRendererConnector : MonoBehaviour
{
    [Header("连接点设置")]
    [SerializeField]
    [Tooltip("线的起点Transform")]
    private Transform startPoint;

    [SerializeField]
    [Tooltip("线的终点Transform")]
    private Transform endPoint;

    [Header("线设置")]
    [SerializeField]
    [Tooltip("线的宽度")]
    private float lineWidth = 0.1f;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        // 获取LineRenderer组件
        lineRenderer = GetComponent<LineRenderer>();

        // 初始化LineRenderer设置
        InitializeLineRenderer();
    }

    private void Start()
    {
        // 验证Transform是否已设置
        if (startPoint == null || endPoint == null)
        {
            Debug.LogWarning($"LineRendererConnector ({gameObject.name}): 起点或终点Transform未设置！", this);
        }
    }

    private void LateUpdate()
    {
        // 每帧更新线的位置
        UpdateLinePositions();
    }

    /// <summary>
    /// 初始化LineRenderer的基本设置
    /// </summary>
    private void InitializeLineRenderer()
    {
        if (lineRenderer == null) return;

        // 设置点数量为2（起点和终点）
        lineRenderer.positionCount = 2;

        // 设置线宽
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        // 使用世界空间坐标
        lineRenderer.useWorldSpace = true;
    }

    /// <summary>
    /// 更新线的起点和终点位置
    /// </summary>
    private void UpdateLinePositions()
    {
        if (lineRenderer == null || startPoint == null || endPoint == null)
            return;

        // 设置起点位置
        lineRenderer.SetPosition(0, startPoint.position);

        // 设置终点位置
        lineRenderer.SetPosition(1, endPoint.position);
    }

    /// <summary>
    /// 设置起点Transform
    /// </summary>
    public void SetStartPoint(Transform newStartPoint)
    {
        startPoint = newStartPoint;
    }

    /// <summary>
    /// 设置终点Transform
    /// </summary>
    public void SetEndPoint(Transform newEndPoint)
    {
        endPoint = newEndPoint;
    }

    /// <summary>
    /// 同时设置起点和终点
    /// </summary>
    public void SetPoints(Transform newStartPoint, Transform newEndPoint)
    {
        startPoint = newStartPoint;
        endPoint = newEndPoint;
    }

    /// <summary>
    /// 设置线宽
    /// </summary>
    public void SetLineWidth(float width)
    {
        lineWidth = width;
        if (lineRenderer != null)
        {
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
        }
    }

    // 在编辑器中绘制辅助线（仅在Scene视图中可见）
    private void OnDrawGizmos()
    {
        if (startPoint != null && endPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(startPoint.position, endPoint.position);

            // 绘制起点和终点标记
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startPoint.position, 0.1f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(endPoint.position, 0.1f);
        }
    }
}

using UnityEngine;

/// <summary>
/// 波浪循环滚动效果 - 使用物理移动方式
/// 通过移动物体位置并在到达边界时重置位置来实现无缝循环
/// 适用于头尾可无缝连接的纹理
/// </summary>
public class WaveLoopScrolling : MonoBehaviour
{
    [Header("滚动设置")]
    [Tooltip("滚动速度(单位/秒)")]
    [SerializeField] private float scrollSpeed = 1f;

    [Tooltip("滚动方向")]
    [SerializeField] private ScrollDirection direction = ScrollDirection.Left;

    [Header("循环设置")]
    [Tooltip("波浪纹理的宽度(用于计算循环位置)")]
    [SerializeField] private float waveWidth = 19.2f;

    [Tooltip("是否自动计算波浪宽度(从SpriteRenderer获取)")]
    [SerializeField] private bool autoCalculateWidth = true;

    [Tooltip("是否自动播放")]
    [SerializeField] private bool autoPlay = true;

    private Vector3 startPosition;
    private bool isScrolling;
    private SpriteRenderer spriteRenderer;

    public enum ScrollDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 自动计算波浪宽度
        if (autoCalculateWidth && spriteRenderer != null && spriteRenderer.sprite != null)
        {
            waveWidth = spriteRenderer.sprite.bounds.size.x * transform.localScale.x;
        }
    }

    private void Start()
    {
        // 记录起始位置
        startPosition = transform.localPosition;

        if (autoPlay)
        {
            StartScrolling();
        }
    }

    private void Update()
    {
        if (!isScrolling) return;

        // 根据方向计算移动
        Vector3 movement = GetMovementVector() * scrollSpeed * Time.deltaTime;
        transform.localPosition += movement;

        // 检查是否需要循环重置
        CheckAndResetPosition();
    }

    /// <summary>
    /// 根据方向获取移动向量
    /// </summary>
    private Vector3 GetMovementVector()
    {
        switch (direction)
        {
            case ScrollDirection.Left:
                return Vector3.left;
            case ScrollDirection.Right:
                return Vector3.right;
            case ScrollDirection.Up:
                return Vector3.up;
            case ScrollDirection.Down:
                return Vector3.down;
            default:
                return Vector3.left;
        }
    }

    /// <summary>
    /// 检查并重置位置以实现循环效果
    /// </summary>
    private void CheckAndResetPosition()
    {
        Vector3 currentPos = transform.localPosition;
        bool needReset = false;

        switch (direction)
        {
            case ScrollDirection.Left:
                if (currentPos.x <= startPosition.x - waveWidth)
                {
                    currentPos.x = startPosition.x;
                    needReset = true;
                }
                break;

            case ScrollDirection.Right:
                if (currentPos.x >= startPosition.x + waveWidth)
                {
                    currentPos.x = startPosition.x;
                    needReset = true;
                }
                break;

            case ScrollDirection.Up:
                if (currentPos.y >= startPosition.y + waveWidth)
                {
                    currentPos.y = startPosition.y;
                    needReset = true;
                }
                break;

            case ScrollDirection.Down:
                if (currentPos.y <= startPosition.y - waveWidth)
                {
                    currentPos.y = startPosition.y;
                    needReset = true;
                }
                break;
        }

        if (needReset)
        {
            transform.localPosition = currentPos;
        }
    }

    /// <summary>
    /// 开始滚动
    /// </summary>
    public void StartScrolling()
    {
        isScrolling = true;
    }

    /// <summary>
    /// 停止滚动
    /// </summary>
    public void StopScrolling()
    {
        isScrolling = false;
    }

    /// <summary>
    /// 设置滚动速度
    /// </summary>
    public void SetScrollSpeed(float speed)
    {
        scrollSpeed = speed;
    }

    /// <summary>
    /// 重置到起始位置
    /// </summary>
    public void ResetPosition()
    {
        transform.localPosition = startPosition;
    }

    /// <summary>
    /// 设置波浪宽度
    /// </summary>
    public void SetWaveWidth(float width)
    {
        waveWidth = width;
    }

    // 在编辑器中可视化调试信息
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            startPosition = transform.localPosition;
        }

        // 绘制循环边界
        Gizmos.color = Color.yellow;
        Vector3 worldStart = transform.parent != null
            ? transform.parent.TransformPoint(startPosition)
            : startPosition;

        switch (direction)
        {
            case ScrollDirection.Left:
            case ScrollDirection.Right:
                Vector3 leftBound = worldStart + Vector3.left * waveWidth;
                Gizmos.DrawLine(leftBound + Vector3.up * 2, leftBound + Vector3.down * 2);
                Gizmos.DrawLine(worldStart + Vector3.up * 2, worldStart + Vector3.down * 2);
                break;

            case ScrollDirection.Up:
            case ScrollDirection.Down:
                Vector3 downBound = worldStart + Vector3.down * waveWidth;
                Gizmos.DrawLine(downBound + Vector3.left * 2, downBound + Vector3.right * 2);
                Gizmos.DrawLine(worldStart + Vector3.left * 2, worldStart + Vector3.right * 2);
                break;
        }
    }
}

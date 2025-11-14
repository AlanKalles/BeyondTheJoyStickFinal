using UnityEngine;

/// <summary>
/// 波浪滚动效果脚本
/// 通过修改材质的UV偏移实现无缝循环滚动效果
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class WaveScrolling : MonoBehaviour
{
    [Header("滚动设置")]
    [Tooltip("水平滚动速度")]
    [SerializeField] private float scrollSpeedX = 0.1f;

    [Tooltip("垂直滚动速度")]
    [SerializeField] private float scrollSpeedY = 0f;

    [Header("可选设置")]
    [Tooltip("是否自动播放")]
    [SerializeField] private bool autoPlay = true;

    [Tooltip("是否使用独立材质(避免影响其他使用相同材质的对象)")]
    [SerializeField] private bool useInstancedMaterial = true;

    private SpriteRenderer spriteRenderer;
    private Material materialInstance;
    private Vector2 currentOffset;
    private bool isScrolling;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 创建材质实例,避免修改共享材质
        if (useInstancedMaterial && spriteRenderer.material != null)
        {
            materialInstance = new Material(spriteRenderer.material);
            spriteRenderer.material = materialInstance;
        }
        else
        {
            materialInstance = spriteRenderer.material;
        }
    }

    private void Start()
    {
        currentOffset = Vector2.zero;

        if (autoPlay)
        {
            StartScrolling();
        }
    }

    private void Update()
    {
        if (isScrolling && materialInstance != null)
        {
            // 更新UV偏移
            currentOffset.x += scrollSpeedX * Time.deltaTime;
            currentOffset.y += scrollSpeedY * Time.deltaTime;

            // 保持偏移在0-1范围内(避免数值过大)
            currentOffset.x = currentOffset.x % 1f;
            currentOffset.y = currentOffset.y % 1f;

            // 应用到材质
            materialInstance.mainTextureOffset = currentOffset;
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
    public void SetScrollSpeed(float speedX, float speedY)
    {
        scrollSpeedX = speedX;
        scrollSpeedY = speedY;
    }

    /// <summary>
    /// 重置偏移
    /// </summary>
    public void ResetOffset()
    {
        currentOffset = Vector2.zero;
        if (materialInstance != null)
        {
            materialInstance.mainTextureOffset = currentOffset;
        }
    }

    private void OnDestroy()
    {
        // 清理材质实例
        if (useInstancedMaterial && materialInstance != null)
        {
            Destroy(materialInstance);
        }
    }
}

using UnityEngine;

/// <summary>
/// 波浪循环组管理器
/// 自动创建多个波浪副本来实现无缝循环滚动
/// </summary>
public class WaveLoopGroup : MonoBehaviour
{
    [Header("组设置")]
    [Tooltip("波浪预制体或模板对象")]
    [SerializeField] private GameObject waveTemplate;

    [Tooltip("创建的副本数量(通常2-3个就够了)")]
    [SerializeField] private int copyCount = 2;

    [Tooltip("滚动速度")]
    [SerializeField] private float scrollSpeed = 1f;

    [Tooltip("滚动方向")]
    [SerializeField] private WaveLoopScrolling.ScrollDirection direction = WaveLoopScrolling.ScrollDirection.Left;

    [Header("间距设置")]
    [Tooltip("波浪之间的间距(0表示无缝连接)")]
    [SerializeField] private float spacing = 0f;

    [Tooltip("是否自动计算间距(基于Sprite宽度)")]
    [SerializeField] private bool autoCalculateSpacing = true;

    [Header("运行时设置")]
    [Tooltip("是否在Start时自动创建副本")]
    [SerializeField] private bool createOnStart = true;

    [Tooltip("是否在Start时自动开始滚动")]
    [SerializeField] private bool autoPlay = true;

    private GameObject[] waveInstances;
    private float waveWidth;

    private void Start()
    {
        if (createOnStart)
        {
            CreateWaveLoop();

            if (autoPlay)
            {
                StartScrolling();
            }
        }
    }

    /// <summary>
    /// 创建波浪循环组
    /// </summary>
    public void CreateWaveLoop()
    {
        if (waveTemplate == null)
        {
            Debug.LogError("WaveLoopGroup: 波浪模板未设置!");
            return;
        }

        // 清理已有的实例
        ClearExistingInstances();

        // 获取波浪宽度
        SpriteRenderer templateRenderer = waveTemplate.GetComponent<SpriteRenderer>();
        if (templateRenderer != null && templateRenderer.sprite != null)
        {
            waveWidth = templateRenderer.sprite.bounds.size.x * waveTemplate.transform.localScale.x;
        }
        else
        {
            Debug.LogWarning("WaveLoopGroup: 无法获取波浪宽度,使用默认值19.2");
            waveWidth = 19.2f;
        }

        // 计算间距
        float actualSpacing = autoCalculateSpacing ? waveWidth : spacing;

        // 创建实例数组
        waveInstances = new GameObject[copyCount];

        // 创建副本
        for (int i = 0; i < copyCount; i++)
        {
            GameObject instance;

            if (i == 0)
            {
                // 第一个使用模板本身
                instance = waveTemplate;
            }
            else
            {
                // 后续创建副本
                instance = Instantiate(waveTemplate, transform);
                instance.name = $"{waveTemplate.name}_Copy{i}";
            }

            // 设置位置
            Vector3 pos = instance.transform.localPosition;
            switch (direction)
            {
                case WaveLoopScrolling.ScrollDirection.Left:
                case WaveLoopScrolling.ScrollDirection.Right:
                    pos.x = waveTemplate.transform.localPosition.x + (actualSpacing + spacing) * i;
                    break;

                case WaveLoopScrolling.ScrollDirection.Up:
                case WaveLoopScrolling.ScrollDirection.Down:
                    pos.y = waveTemplate.transform.localPosition.y + (actualSpacing + spacing) * i;
                    break;
            }
            instance.transform.localPosition = pos;

            // 添加或配置WaveLoopScrolling组件
            WaveLoopScrolling scrolling = instance.GetComponent<WaveLoopScrolling>();
            if (scrolling == null)
            {
                scrolling = instance.AddComponent<WaveLoopScrolling>();
            }

            scrolling.SetScrollSpeed(scrollSpeed);
            scrolling.SetWaveWidth(waveWidth * copyCount);

            // 确保不自动播放(由这个管理器统一控制)
            scrolling.StopScrolling();

            waveInstances[i] = instance;
        }

        Debug.Log($"WaveLoopGroup: 创建了 {copyCount} 个波浪副本,宽度: {waveWidth}");
    }

    /// <summary>
    /// 清理已有的实例
    /// </summary>
    private void ClearExistingInstances()
    {
        if (waveInstances != null)
        {
            for (int i = 1; i < waveInstances.Length; i++) // 从1开始,跳过模板
            {
                if (waveInstances[i] != null)
                {
                    if (Application.isPlaying)
                        Destroy(waveInstances[i]);
                    else
                        DestroyImmediate(waveInstances[i]);
                }
            }
        }
    }

    /// <summary>
    /// 开始所有波浪的滚动
    /// </summary>
    public void StartScrolling()
    {
        if (waveInstances == null) return;

        foreach (GameObject wave in waveInstances)
        {
            if (wave != null)
            {
                WaveLoopScrolling scrolling = wave.GetComponent<WaveLoopScrolling>();
                if (scrolling != null)
                {
                    scrolling.StartScrolling();
                }
            }
        }
    }

    /// <summary>
    /// 停止所有波浪的滚动
    /// </summary>
    public void StopScrolling()
    {
        if (waveInstances == null) return;

        foreach (GameObject wave in waveInstances)
        {
            if (wave != null)
            {
                WaveLoopScrolling scrolling = wave.GetComponent<WaveLoopScrolling>();
                if (scrolling != null)
                {
                    scrolling.StopScrolling();
                }
            }
        }
    }

    /// <summary>
    /// 设置滚动速度
    /// </summary>
    public void SetScrollSpeed(float speed)
    {
        scrollSpeed = speed;

        if (waveInstances == null) return;

        foreach (GameObject wave in waveInstances)
        {
            if (wave != null)
            {
                WaveLoopScrolling scrolling = wave.GetComponent<WaveLoopScrolling>();
                if (scrolling != null)
                {
                    scrolling.SetScrollSpeed(speed);
                }
            }
        }
    }

    private void OnDestroy()
    {
        ClearExistingInstances();
    }

#if UNITY_EDITOR
    /// <summary>
    /// 编辑器辅助功能:在编辑器中创建循环
    /// </summary>
    [ContextMenu("在编辑器中创建波浪循环")]
    private void CreateWaveLoopInEditor()
    {
        CreateWaveLoop();
        Debug.Log("波浪循环已在编辑器中创建");
    }

    [ContextMenu("清理波浪副本")]
    private void ClearWaveLoopInEditor()
    {
        ClearExistingInstances();
        waveInstances = null;
        Debug.Log("波浪副本已清理");
    }
#endif
}

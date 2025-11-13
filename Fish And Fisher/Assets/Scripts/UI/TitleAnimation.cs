using UnityEngine;
using System.Collections;

/// <summary>
/// 标题页面动画管理器
/// 管理标题界面中所有动画元素的播放和触发
/// </summary>
public class TitleAnimation : MonoBehaviour
{
    [Header("鱼动画设置")]
    [Tooltip("标题页面中的鱼游戏对象")]
    [SerializeField] private GameObject fishObject;

    [Tooltip("鱼的动画控制器")]
    [SerializeField] private Animator fishAnimator;

    [Tooltip("鱼动画名称（如果不使用trigger而是直接播放动画）")]
    [SerializeField] private string fishAnimationName = "title_fish";

    [Tooltip("随机播放间隔的最小时间（秒）")]
    [SerializeField] private float minInterval = 3f;

    [Tooltip("随机播放间隔的最大时间（秒）")]
    [SerializeField] private float maxInterval = 8f;

    [Header("钩子动画设置")]
    [Tooltip("钩子游戏对象")]
    [SerializeField] private GameObject hookObject;

    [Tooltip("钩子的动画控制器")]
    [SerializeField] private Animator hookAnimator;

    [Header("波浪动画设置")]
    [Tooltip("波浪游戏对象")]
    [SerializeField] private GameObject waveObject;

    [Tooltip("波浪的动画控制器")]
    [SerializeField] private Animator waveAnimator;

    [Tooltip("前景波浪循环组")]
    [SerializeField] private WaveLoopGroup waveFrontLoopGroup;

    [Tooltip("背景波浪循环组")]
    [SerializeField] private WaveLoopGroup waveBackLoopGroup;

    private Coroutine fishAnimationCoroutine;

    private void Start()
    {
        // 启动时开始鱼的随机动画循环
        if (fishObject != null && fishAnimator != null)
        {
            fishAnimationCoroutine = StartCoroutine(PlayFishAnimationRandomly());
        }
        else
        {
            Debug.LogWarning("TitleAnimation: 鱼对象或动画控制器未设置!");
        }

        // 启动波浪滚动动画
        if (waveFrontLoopGroup != null)
        {
            waveFrontLoopGroup.StartScrolling();
        }
        if (waveBackLoopGroup != null)
        {
            waveBackLoopGroup.StartScrolling();
        }
    }

    private void OnDisable()
    {
        // 停止协程避免内存泄漏
        if (fishAnimationCoroutine != null)
        {
            StopCoroutine(fishAnimationCoroutine);
        }
    }

    /// <summary>
    /// 随机播放鱼动画的协程
    /// 每隔随机时间播放一次动画
    /// </summary>
    private IEnumerator PlayFishAnimationRandomly()
    {
        while (true)
        {
            // 等待随机时间
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            // 播放鱼动画
            PlayFishAnimation();
        }
    }

    /// <summary>
    /// 播放鱼动画
    /// </summary>
    private void PlayFishAnimation()
    {
        if (fishAnimator == null)
        {
            Debug.LogWarning("TitleAnimation: 鱼动画控制器未设置!");
            return;
        }

        // 如果使用trigger触发动画
        if (fishAnimator.parameters.Length > 0)
        {
            // 尝试触发名为 "Play" 的trigger
            fishAnimator.SetTrigger("Play");
        }
        else
        {
            // 直接播放指定名称的动画
            fishAnimator.Play(fishAnimationName, 0, 0f);
        }
    }

    /// <summary>
    /// 手动触发鱼动画（可以从外部调用）
    /// </summary>
    public void TriggerFishAnimation()
    {
        PlayFishAnimation();
    }

    /// <summary>
    /// 设置随机播放间隔范围
    /// </summary>
    /// <param name="min">最小间隔（秒）</param>
    /// <param name="max">最大间隔（秒）</param>
    public void SetAnimationInterval(float min, float max)
    {
        minInterval = Mathf.Max(0f, min);
        maxInterval = Mathf.Max(minInterval, max);
    }

    /// <summary>
    /// 停止鱼的随机动画循环
    /// </summary>
    public void StopFishAnimation()
    {
        if (fishAnimationCoroutine != null)
        {
            StopCoroutine(fishAnimationCoroutine);
            fishAnimationCoroutine = null;
        }
    }

    /// <summary>
    /// 重新启动鱼的随机动画循环
    /// </summary>
    public void ResumeFishAnimation()
    {
        if (fishAnimationCoroutine == null && fishObject != null && fishAnimator != null)
        {
            fishAnimationCoroutine = StartCoroutine(PlayFishAnimationRandomly());
        }
    }
}

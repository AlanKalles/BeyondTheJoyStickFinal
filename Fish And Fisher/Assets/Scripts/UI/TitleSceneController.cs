using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 标题场景控制器
/// 处理玩家点击进入游戏的逻辑和场景切换
/// 支持鼠标点击和ESP32按钮触发
/// </summary>
public class TitleSceneController : MonoBehaviour
{
    [Header("场景设置")]
    [Tooltip("要加载的游戏场景名称")]
    [SerializeField] private string gameSceneName = "Main";

    [Header("钩子动画设置")]
    [Tooltip("钩子的Animator组件")]
    [SerializeField] private Animator hookAnimator;

    [Tooltip("触发过渡的Trigger名称")]
    [SerializeField] private string startTriggerName = "start";

    [Tooltip("up动画播放完毕后等待的时间再切换场景(秒)")]
    [SerializeField] private float waitBeforeSceneLoad = 0.5f;

    [Header("ESP32设置")]
    [Tooltip("是否启用ESP32按钮触发")]
    [SerializeField] private bool enableESP32Trigger = true;

    [Header("可选设置")]
    [Tooltip("是否允许重复点击")]
    [SerializeField] private bool allowMultipleClicks = false;

    [Tooltip("是否在控制台输出调试信息")]
    [SerializeField] private bool debugMode = false;

    private bool isTransitioning = false;

    // ESP32按钮状态跟踪（用于边沿检测）
    private bool wasButtonPressed = false;

    private void Update()
    {
        // 检测鼠标左键点击
        if (Input.GetMouseButtonDown(0))
        {
            if (!isTransitioning || allowMultipleClicks)
            {
                if (debugMode)
                {
                    Debug.Log("TitleSceneController: 鼠标点击触发");
                }
                StartGameTransition();
            }
        }

        // 检测ESP32渔夫按钮
        if (enableESP32Trigger)
        {
            CheckESP32ButtonInput();
        }
    }

    /// <summary>
    /// 检测ESP32渔夫按钮输入（边沿触发）
    /// </summary>
    private void CheckESP32ButtonInput()
    {
        // 获取FisherReceiver
        FisherReceiver fisherReceiver = null;

        // 优先从ESP32Manager获取
        if (ESP32Manager.Instance != null)
        {
            fisherReceiver = ESP32Manager.Instance.fisherReceiver;
        }

        // 如果ESP32Manager不存在，尝试直接查找
        if (fisherReceiver == null)
        {
            fisherReceiver = FindFirstObjectByType<FisherReceiver>();
        }

        // 检查连接状态和按钮输入
        if (fisherReceiver != null && fisherReceiver.isConnected)
        {
            bool isButtonPressed = fisherReceiver.buttonPressed;

            // 边沿检测：只在按钮从未按下变为按下时触发
            if (isButtonPressed && !wasButtonPressed)
            {
                if (!isTransitioning || allowMultipleClicks)
                {
                    if (debugMode)
                    {
                        Debug.Log("TitleSceneController: ESP32按钮触发");
                    }
                    StartGameTransition();
                }
            }

            wasButtonPressed = isButtonPressed;
        }
        else
        {
            // 未连接时重置状态
            wasButtonPressed = false;
        }
    }

    /// <summary>
    /// 开始游戏过渡流程
    /// </summary>
    private void StartGameTransition()
    {
        if (hookAnimator == null)
        {
            Debug.LogError("TitleSceneController: 钩子Animator未设置!无法播放过渡动画。");
            // 直接加载场景
            LoadGameScene();
            return;
        }

        isTransitioning = true;

        if (debugMode)
        {
            Debug.Log($"TitleSceneController: 触发Trigger '{startTriggerName}'，开始播放up动画");
        }

        // 触发Animator的start trigger
        hookAnimator.SetTrigger(startTriggerName);

        // 启动协程等待动画播放完毕
        StartCoroutine(WaitForAnimationAndLoadScene());
    }

    /// <summary>
    /// 等待动画播放完毕后加载场景的协程
    /// </summary>
    private IEnumerator WaitForAnimationAndLoadScene()
    {
        // 等待一帧，确保Animator有时间处理Trigger并进入up动画状态
        yield return null;

        // 获取当前动画状态信息（假设up动画在Layer 0）
        AnimatorStateInfo stateInfo = hookAnimator.GetCurrentAnimatorStateInfo(0);

        if (debugMode)
        {
            Debug.Log($"TitleSceneController: up动画长度: {stateInfo.length:F2} 秒");
        }

        // 等待动画播放完毕（normalizedTime从0到1）
        while (stateInfo.normalizedTime < 1.0f)
        {
            stateInfo = hookAnimator.GetCurrentAnimatorStateInfo(0);

            if (debugMode)
            {
                Debug.Log($"TitleSceneController: 动画进度: {stateInfo.normalizedTime:F2}");
            }

            yield return null;
        }

        if (debugMode)
        {
            Debug.Log($"TitleSceneController: up动画播放完毕,等待 {waitBeforeSceneLoad} 秒后加载场景");
        }

        // 额外等待指定时间
        yield return new WaitForSeconds(waitBeforeSceneLoad);

        // 加载游戏场景
        LoadGameScene();
    }

    /// <summary>
    /// 加载游戏场景
    /// </summary>
    private void LoadGameScene()
    {
        if (debugMode)
        {
            Debug.Log($"TitleSceneController: 正在加载场景 '{gameSceneName}'");
        }

        // 检查场景是否存在于Build Settings中
        if (SceneExists(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError($"TitleSceneController: 场景 '{gameSceneName}' 不存在于Build Settings中!请在File -> Build Settings中添加场景。");
        }
    }

    /// <summary>
    /// 检查场景是否存在于Build Settings中
    /// </summary>
    private bool SceneExists(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneNameInBuild == sceneName)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 手动触发过渡(可以从其他脚本或UI按钮调用)
    /// </summary>
    public void ManualTriggerTransition()
    {
        if (!isTransitioning || allowMultipleClicks)
        {
            StartGameTransition();
        }
    }

    /// <summary>
    /// 直接跳过动画并立即加载场景(用于测试或跳过动画)
    /// </summary>
    public void SkipToGame()
    {
        if (debugMode)
        {
            Debug.Log("TitleSceneController: 跳过动画,直接加载场景");
        }

        StopAllCoroutines();
        LoadGameScene();
    }

#if UNITY_EDITOR
    /// <summary>
    /// 编辑器辅助:测试动画过渡
    /// </summary>
    [ContextMenu("测试过渡动画")]
    private void TestTransitionAnimation()
    {
        if (Application.isPlaying)
        {
            StartGameTransition();
        }
        else
        {
            Debug.LogWarning("TitleSceneController: 请在Play模式下测试");
        }
    }

    /// <summary>
    /// 编辑器辅助:重置Trigger
    /// </summary>
    [ContextMenu("重置Trigger")]
    private void ResetTrigger()
    {
        if (hookAnimator != null)
        {
            hookAnimator.ResetTrigger(startTriggerName);
            Debug.Log($"TitleSceneController: Trigger '{startTriggerName}' 已重置");
        }
    }
#endif
}

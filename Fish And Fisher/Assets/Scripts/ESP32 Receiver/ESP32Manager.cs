using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ESP32管理器 - 统一管理所有ESP32相关组件的跨场景持久化
/// 将此组件放在包含所有ESP32组件的父GameObject上
/// </summary>
public class ESP32Manager : MonoBehaviour
{
    // 单例实例
    public static ESP32Manager Instance { get; private set; }

    [Header("ESP32组件引用")]
    [Tooltip("鱼ESP32接收器")]
    public FishReceiver fishReceiver;

    [Tooltip("渔夫ESP32接收器")]
    public FisherReceiver fisherReceiver;

    [Tooltip("鱼ESP32控制器")]
    public ESP32FishController fishController;

    [Tooltip("渔夫ESP32控制器")]
    public ESP32FisherController fisherController;

    [Header("调试设置")]
    [Tooltip("是否显示调试信息")]
    public bool showDebugInfo = true;

    private void Awake()
    {
        // 单例模式：确保只有一个实例
        if (Instance != null && Instance != this)
        {
            Debug.Log("[ESP32Manager] 已存在实例，销毁重复的GameObject");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[ESP32Manager] 初始化完成，已设置为跨场景持久化");

        // 自动查找子组件
        FindChildComponents();

        // 订阅场景加载事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // 取消订阅场景加载事件
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // 清除单例引用
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// 查找子对象中的ESP32组件
    /// </summary>
    private void FindChildComponents()
    {
        if (fishReceiver == null)
        {
            fishReceiver = GetComponentInChildren<FishReceiver>();
        }

        if (fisherReceiver == null)
        {
            fisherReceiver = GetComponentInChildren<FisherReceiver>();
        }

        if (fishController == null)
        {
            fishController = GetComponentInChildren<ESP32FishController>();
        }

        if (fisherController == null)
        {
            fisherController = GetComponentInChildren<ESP32FisherController>();
        }
    }

    /// <summary>
    /// 场景加载完成时的处理
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[ESP32Manager] 场景 '{scene.name}' 已加载");

        // ESP32FishController 和 ESP32FisherController 会自动处理场景组件的重新查找
        // 这里可以添加额外的初始化逻辑
    }

    /// <summary>
    /// 获取鱼ESP32连接状态
    /// </summary>
    public bool IsFishConnected()
    {
        return fishReceiver != null && fishReceiver.isConnected;
    }

    /// <summary>
    /// 获取渔夫ESP32连接状态
    /// </summary>
    public bool IsFisherConnected()
    {
        return fisherReceiver != null && fisherReceiver.isConnected;
    }

    /// <summary>
    /// 设置游戏阶段（同时设置鱼和渔夫控制器）
    /// </summary>
    public void SetGamePhase(int phase)
    {
        if (fishController != null)
        {
            fishController.SetGamePhase(phase);
        }

        if (fisherController != null)
        {
            fisherController.SetGamePhase(phase);
        }

        Debug.Log($"[ESP32Manager] 设置游戏阶段为 Phase{phase}");
    }

    /// <summary>
    /// 重新校准所有ESP32设备
    /// </summary>
    public void RecalibrateAll()
    {
        if (fishController != null)
        {
            fishController.Recalibrate();
        }

        if (fisherController != null)
        {
            fisherController.Recalibrate();
        }

        Debug.Log("[ESP32Manager] 已重新校准所有ESP32设备");
    }

    /// <summary>
    /// 启用/禁用ESP32控制
    /// </summary>
    public void SetESP32ControlEnabled(bool enabled)
    {
        if (fishController != null)
        {
            fishController.enableESP32Control = enabled;
        }

        if (fisherController != null)
        {
            fisherController.enableESP32Control = enabled;
        }

        Debug.Log($"[ESP32Manager] ESP32控制已{(enabled ? "启用" : "禁用")}");
    }

    private void OnGUI()
    {
        if (!showDebugInfo) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 14;
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;

        string info = "[ESP32Manager]\n";
        info += $"鱼连接: {(IsFishConnected() ? "✓" : "✗")}\n";
        info += $"渔夫连接: {(IsFisherConnected() ? "✓" : "✗")}\n";

        if (fishController != null)
        {
            info += $"鱼控制器: Phase{fishController.GetCurrentPhase()}\n";
        }

        if (fisherController != null)
        {
            info += $"渔夫控制器: Phase{fisherController.GetCurrentPhase()}\n";
        }

        // 显示在屏幕右上角
        GUI.Label(new Rect(Screen.width - 220, 10, 210, 120), info, style);
    }
}

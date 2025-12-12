using UnityEngine;
using UnityEngine.SceneManagement;
using FishAndFisher;
using FishAndFisher.Fisher;
using FishAndFisher.Phase2;

/// <summary>
/// ESP32 渔夫控制器 - 将 FisherReceiver 数据转换为渔夫的控制输入
/// 支持 Phase1（追捕阶段）和 Phase2（争斗阶段）两种模式
/// 使用 DontDestroyOnLoad 实现跨场景持久化
/// </summary>
public class ESP32FisherController : MonoBehaviour
{
    // 单例实例
    public static ESP32FisherController Instance { get; private set; }
    [Header("启用设置")]
    [Tooltip("是否启用ESP32控制（禁用时使用键盘/鼠标控制）")]
    public bool enableESP32Control = true;

    [Header("Phase1设置 - 准心映射")]
    [Tooltip("Y轴最小角度（对应边界最下方）")]
    public float yAxisMin = -45f;

    [Tooltip("Y轴最大角度（对应边界最上方）")]
    public float yAxisMax = 45f;

    [Tooltip("Z轴最小角度（对应边界最左方）")]
    public float zAxisMin = -45f;

    [Tooltip("Z轴最大角度（对应边界最右方）")]
    public float zAxisMax = 45f;

    [Header("Phase1设置 - 准心平滑")]
    [Tooltip("准心位置平滑时间")]
    [Range(0f, 0.3f)]
    public float crosshairSmoothTime = 0.1f;

    [Header("Phase2设置")]
    [Tooltip("Phase2旋转死区（度）- eulerAngles.z 需要超过此值才判定为左/右")]
    public float phase2RotationDeadZone = 15f;

    [Tooltip("Phase2旋转灵敏度")]
    [Range(0.5f, 2f)]
    public float phase2RotationSensitivity = 1f;

    [Tooltip("编码器速度归一化的最大值")]
    public float encoderSpeedMax = 500f;

    [Header("组件引用")]
    [Tooltip("FisherReceiver组件（ESP32数据源）")]
    public FisherReceiver fisherReceiver;

    [Tooltip("FisherCrosshairController组件（准心控制）")]
    public FisherCrosshairController crosshairController;

    [Tooltip("FisherController组件（渔夫主控制器）")]
    public FisherController fisherController;

    [Tooltip("Phase2力度检测器（可选）")]
    public Phase2ForceDetector phase2ForceDetector;

    [Tooltip("Phase2输入归一化器（可选）")]
    public Phase2InputNormalizer phase2InputNormalizer;

    // 边界范围（从GameBoundary获取）
    private float minBoundX = -25f;
    private float maxBoundX = 25f;
    private float minBoundZ = -25f;
    private float maxBoundZ = 25f;

    // 当前游戏阶段
    private int currentPhase = 1;

    // 按钮状态跟踪
    private bool wasButtonPressed = false;

    // 准心平滑
    private Vector3 currentCrosshairPosition;
    private Vector3 crosshairVelocity;

    // Phase2输出（供外部读取）
    public float CurrentPhase2Force { get; private set; }
    public float CurrentPhase2Rotation { get; private set; }

    private void Awake()
    {
        // 单例模式：确保只有一个实例
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 跨场景持久化：如果父对象为空，设置DontDestroyOnLoad
        // 如果有父对象（ESP32Manager），由父对象负责DontDestroyOnLoad
        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }

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
    /// 场景加载完成时重新查找场景中的组件
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[ESP32FisherController] 场景 '{scene.name}' 已加载，重新查找组件...");
        FindSceneComponents();
    }

    private void Start()
    {
        // 初始查找组件
        FindSceneComponents();
    }

    /// <summary>
    /// 查找当前场景中的组件引用
    /// FisherReceiver 应该在同一个持久化对象上，不需要重新查找
    /// </summary>
    private void FindSceneComponents()
    {
        // FisherReceiver 应该在同一个持久化GameObject上，如果为空则查找
        if (fisherReceiver == null)
        {
            fisherReceiver = GetComponentInChildren<FisherReceiver>();
            if (fisherReceiver == null)
            {
                fisherReceiver = FindFirstObjectByType<FisherReceiver>();
            }
            if (fisherReceiver == null)
            {
                Debug.LogWarning("[ESP32FisherController] 未找到 FisherReceiver 组件！");
            }
        }

        // FisherCrosshairController 是场景中的组件，每次场景加载都需要重新查找
        crosshairController = FindFirstObjectByType<FisherCrosshairController>();
        if (crosshairController == null)
        {
            Debug.LogWarning("[ESP32FisherController] 当前场景未找到 FisherCrosshairController 组件");
        }
        else
        {
            Debug.Log($"[ESP32FisherController] 找到 FisherCrosshairController: {crosshairController.gameObject.name}");

            // 启用ESP32输入模式，禁用鼠标输入
            if (enableESP32Control)
            {
                crosshairController.SetUseESP32Input(true);
                Debug.Log("[ESP32FisherController] 已启用 FisherCrosshairController 的 ESP32 输入模式");
            }
        }

        // FisherController 是场景中的组件，需要重新查找
        fisherController = FindFirstObjectByType<FisherController>();
        if (fisherController == null)
        {
            Debug.LogWarning("[ESP32FisherController] 当前场景未找到 FisherController 组件");
        }
        else
        {
            Debug.Log($"[ESP32FisherController] 找到 FisherController: {fisherController.gameObject.name}");
        }

        // Phase2组件是场景中的组件，需要重新查找
        phase2ForceDetector = FindFirstObjectByType<Phase2ForceDetector>();
        phase2InputNormalizer = FindFirstObjectByType<Phase2InputNormalizer>();

        // 启用Phase2组件的ESP32输入模式
        if (enableESP32Control)
        {
            if (phase2ForceDetector != null)
            {
                phase2ForceDetector.SetUseESP32Force(true);
                Debug.Log("[ESP32FisherController] 已启用 Phase2ForceDetector 的 ESP32 力度模式");
            }

            if (phase2InputNormalizer != null)
            {
                phase2InputNormalizer.SetUseESP32Input(true);
                Debug.Log("[ESP32FisherController] 已启用 Phase2InputNormalizer 的 ESP32 输入模式");
            }
        }

        // 从GameBoundary获取边界（每个场景可能不同）
        if (GameBoundary.Instance != null)
        {
            Vector2 minBounds = GameBoundary.Instance.MinBounds;
            Vector2 maxBounds = GameBoundary.Instance.MaxBounds;
            minBoundX = minBounds.x;
            maxBoundX = maxBounds.x;
            minBoundZ = minBounds.y;
            maxBoundZ = maxBounds.y;
            Debug.Log($"[ESP32FisherController] 边界已更新: X[{minBoundX}, {maxBoundX}] Z[{minBoundZ}, {maxBoundZ}]");
        }

        // 重置状态
        currentPhase = 1;
        wasButtonPressed = false;
        currentCrosshairPosition = Vector3.zero;
        crosshairVelocity = Vector3.zero;
    }

    private void Update()
    {
        if (!enableESP32Control) return;
        if (fisherReceiver == null || !fisherReceiver.isConnected) return;

        if (currentPhase == 1)
        {
            UpdatePhase1Input();
        }
        else if (currentPhase == 2)
        {
            UpdatePhase2Input();
        }
    }

    /// <summary>
    /// Phase1: 处理准心位置和攻击输入
    /// </summary>
    private void UpdatePhase1Input()
    {
        // === 准心位置映射（反转方向）===
        // Y轴（上下）→ Z坐标（反转：鱼竿往上，渔钩往上/Z增大）
        float eulerY = fisherReceiver.eulerAngles.y;
        float normalizedY = Mathf.InverseLerp(yAxisMin, yAxisMax, eulerY);
        float targetZ = Mathf.Lerp(maxBoundZ, minBoundZ, normalizedY);

        // Z轴（左右）→ X坐标（反转：鱼竿往右，渔钩往右/X增大）
        float eulerZ = fisherReceiver.eulerAngles.z;
        float normalizedZ = Mathf.InverseLerp(zAxisMin, zAxisMax, eulerZ);
        float targetX = Mathf.Lerp(maxBoundX, minBoundX, normalizedZ);

        // 目标位置
        Vector3 targetPosition = new Vector3(targetX, 0, targetZ);

        // 平滑移动
        currentCrosshairPosition = Vector3.SmoothDamp(
            currentCrosshairPosition,
            targetPosition,
            ref crosshairVelocity,
            crosshairSmoothTime
        );

        // 设置准心位置
        if (crosshairController != null)
        {
            crosshairController.SetTargetPosition(currentCrosshairPosition);
        }

        // === 攻击控制 ===
        bool isButtonPressed = fisherReceiver.buttonPressed;

        // 检测按钮按下事件（边沿触发）
        if (isButtonPressed && !wasButtonPressed)
        {
            // 触发攻击
            if (fisherController != null)
            {
                fisherController.TriggerAttack();
                Debug.Log("[ESP32FisherController] 触发攻击！");
            }
        }

        wasButtonPressed = isButtonPressed;
    }

    /// <summary>
    /// Phase2: 处理旋转和力度输入
    /// </summary>
    private void UpdatePhase2Input()
    {
        // === 旋转控制（带死区，与方向判定同步）===
        // 使用eulerAngles.z作为旋转输入
        float eulerZ = fisherReceiver.eulerAngles.z;
        CurrentPhase2Rotation = eulerZ * phase2RotationSensitivity;

        // 设置Phase2输入归一化器（用于方向判定）
        if (phase2InputNormalizer != null)
        {
            phase2InputNormalizer.SetESP32FisherRotation(eulerZ);
        }

        // 计算鱼竿旋转输入（带死区）
        float rotationInput;
        if (Mathf.Abs(eulerZ) < phase2RotationDeadZone)
        {
            // 死区内：无旋转
            rotationInput = 0f;
        }
        else
        {
            // 死区外：计算旋转量
            // 反转方向：负角度 → 右方向，正角度 → 左方向（与Phase1准心映射一致）
            float effectiveAngle = eulerZ - Mathf.Sign(eulerZ) * phase2RotationDeadZone;
            rotationInput = -effectiveAngle / (90f - phase2RotationDeadZone) * phase2RotationSensitivity;
            rotationInput = Mathf.Clamp(rotationInput, -1f, 1f);
        }

        // 设置FisherController的Phase2旋转
        if (fisherController != null)
        {
            fisherController.SetESP32Phase2Rotation(rotationInput);
        }

        // === 力度控制 ===
        // 使用encoderSpeed作为力度输入
        float speed = Mathf.Abs(fisherReceiver.encoderSpeed);
        CurrentPhase2Force = Mathf.Clamp01(speed / encoderSpeedMax);

        // 设置Phase2力度检测器
        if (phase2ForceDetector != null)
        {
            phase2ForceDetector.SetESP32FisherForce(CurrentPhase2Force);
        }
    }

    /// <summary>
    /// 设置当前游戏阶段
    /// </summary>
    /// <param name="phase">1=Phase1追捕阶段, 2=Phase2争斗阶段</param>
    public void SetGamePhase(int phase)
    {
        currentPhase = phase;
        Debug.Log($"[ESP32FisherController] 切换到Phase{phase}");

        if (phase == 2)
        {
            // 进入Phase2时启用Phase2模式
            if (fisherController != null)
            {
                fisherController.EnablePhase2Mode();
                fisherController.EnableESP32Phase2Rotation();
            }
        }
        else
        {
            // 退出Phase2时禁用Phase2模式
            if (fisherController != null)
            {
                fisherController.DisablePhase2Mode();
                fisherController.DisableESP32Phase2Rotation();
            }
        }
    }

    /// <summary>
    /// 获取当前游戏阶段
    /// </summary>
    public int GetCurrentPhase()
    {
        return currentPhase;
    }

    /// <summary>
    /// 重新校准ESP32（转发到FisherReceiver）
    /// </summary>
    public void Recalibrate()
    {
        if (fisherReceiver != null)
        {
            fisherReceiver.Recalibrate();
            Debug.Log("[ESP32FisherController] 已重新校准");
        }
    }

    /// <summary>
    /// 重置编码器计数
    /// </summary>
    public void ResetEncoder()
    {
        if (fisherReceiver != null)
        {
            fisherReceiver.ResetEncoderCount();
            Debug.Log("[ESP32FisherController] 已重置编码器");
        }
    }

    private void OnGUI()
    {
        if (!enableESP32Control) return;

        // 调试信息显示（在屏幕右侧）
        GUIStyle style = new GUIStyle();
        style.fontSize = 12;
        style.normal.textColor = Color.cyan;

        string status = fisherReceiver != null && fisherReceiver.isConnected ? "已连接" : "未连接";
        string debugInfo = $"[ESP32渔夫控制器] Phase{currentPhase}\n";
        debugInfo += $"状态: {status}\n";

        if (fisherReceiver != null && fisherReceiver.isConnected)
        {
            debugInfo += $"EulerY: {fisherReceiver.eulerAngles.y:F1}° (映射上下)\n";
            debugInfo += $"EulerZ: {fisherReceiver.eulerAngles.z:F1}° (映射左右)\n";
            debugInfo += $"按钮: {(fisherReceiver.buttonPressed ? "按下" : "释放")}\n";

            if (currentPhase == 1)
            {
                debugInfo += $"准心位置: ({currentCrosshairPosition.x:F1}, {currentCrosshairPosition.z:F1})\n";
            }
            else
            {
                debugInfo += $"EncoderSpeed: {fisherReceiver.encoderSpeed:F1}°/s\n";
                debugInfo += $"Phase2力度: {CurrentPhase2Force:F2}\n";
                debugInfo += $"Phase2旋转: {CurrentPhase2Rotation:F1}°\n";
            }
        }

        GUI.Label(new Rect(Screen.width - 310, 10, 300, 150), debugInfo, style);
    }
}

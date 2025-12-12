using UnityEngine;
using UnityEngine.SceneManagement;
using FishAndFisher.Fish;
using FishAndFisher.Phase2;

/// <summary>
/// ESP32 鱼控制器 - 将 FishReceiver 数据转换为鱼的控制输入
/// 支持 Phase1（追捕阶段）和 Phase2（争斗阶段）两种模式
/// 使用 DontDestroyOnLoad 实现跨场景持久化
/// </summary>
public class ESP32FishController : MonoBehaviour
{
    // 单例实例
    public static ESP32FishController Instance { get; private set; }
    [Header("启用设置")]
    [Tooltip("是否启用ESP32控制（禁用时使用键盘控制）")]
    public bool enableESP32Control = true;

    [Header("Phase1设置 - 移动控制")]
    [Tooltip("X轴欧拉角死区（度）- 死区内保持直行")]
    [Range(0f, 45f)]
    public float eulerXDeadZone = 15f;

    [Tooltip("死区外的转向灵敏度")]
    [Range(0.5f, 2f)]
    public float turnSensitivity = 1f;

    [Header("Phase1设置 - 动力控制")]
    [Tooltip("陀螺仪触发加速的阈值（静止时约0-50，运动时100-800）")]
    public float gyroThreshold = 50f;

    [Tooltip("陀螺仪最大值（用于归一化，运动时可达800）")]
    public float gyroMaxValue = 800f;

    [Tooltip("最小Jump触发间隔（秒）- gyroMagnitude最大时的间隔")]
    public float minJumpInterval = 0.05f;

    [Tooltip("最大Jump触发间隔（秒）- gyroMagnitude刚超过阈值时的间隔")]
    public float maxJumpInterval = 0.3f;

    [Tooltip("动力输入的平滑时间")]
    [Range(0f, 0.5f)]
    public float powerSmoothTime = 0.1f;

    [Header("Phase2设置")]
    [Tooltip("Phase2旋转灵敏度")]
    [Range(0.5f, 2f)]
    public float phase2RotationSensitivity = 1f;

    [Header("组件引用")]
    [Tooltip("FishReceiver组件（ESP32数据源）")]
    public FishReceiver fishReceiver;

    [Tooltip("FishMovement组件（鱼移动控制）")]
    public FishMovement fishMovement;

    [Tooltip("Phase2力度检测器（可选）")]
    public Phase2ForceDetector phase2ForceDetector;

    [Tooltip("Phase2输入归一化器（可选）")]
    public Phase2InputNormalizer phase2InputNormalizer;

    // 当前游戏阶段
    private int currentPhase = 1;

    // 动力平滑
    private float currentPower = 0f;
    private float powerVelocity = 0f;

    // 上一帧的加速状态（用于检测变化）
    private bool wasAccelerating = false;

    // Jump触发时间控制
    private float lastJumpTriggerTime = 0f;

    // Phase2力度输出（供外部读取）
    public float CurrentPhase2Force { get; private set; }

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
        Debug.Log($"[ESP32FishController] 场景 '{scene.name}' 已加载，重新查找组件...");
        FindSceneComponents();
    }

    private void Start()
    {
        // 初始查找组件
        FindSceneComponents();
    }

    /// <summary>
    /// 查找当前场景中的组件引用
    /// FishReceiver 应该在同一个持久化对象上，不需要重新查找
    /// </summary>
    private void FindSceneComponents()
    {
        // FishReceiver 应该在同一个持久化GameObject上，如果为空则查找
        if (fishReceiver == null)
        {
            fishReceiver = GetComponentInChildren<FishReceiver>();
            if (fishReceiver == null)
            {
                fishReceiver = FindFirstObjectByType<FishReceiver>();
            }
            if (fishReceiver == null)
            {
                Debug.LogWarning("[ESP32FishController] 未找到 FishReceiver 组件！");
            }
        }

        // FishMovement 是场景中的组件，每次场景加载都需要重新查找
        fishMovement = FindFirstObjectByType<FishMovement>();
        if (fishMovement == null)
        {
            Debug.LogWarning("[ESP32FishController] 当前场景未找到 FishMovement 组件");
        }
        else
        {
            Debug.Log($"[ESP32FishController] 找到 FishMovement: {fishMovement.gameObject.name}");
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
                Debug.Log("[ESP32FishController] 已启用 Phase2ForceDetector 的 ESP32 力度模式");
            }

            if (phase2InputNormalizer != null)
            {
                phase2InputNormalizer.SetUseESP32Input(true);
                Debug.Log("[ESP32FishController] 已启用 Phase2InputNormalizer 的 ESP32 输入模式");
            }
        }

        // 重置状态
        currentPhase = 1;
        currentPower = 0f;
        wasAccelerating = false;
    }

    private void Update()
    {
        if (!enableESP32Control) return;
        if (fishReceiver == null || !fishReceiver.isConnected) return;
        if (fishMovement == null) return;

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
    /// Phase1: 处理移动和动力输入
    /// </summary>
    private void UpdatePhase1Input()
    {
        // === 方向控制 ===
        float eulerX = fishReceiver.eulerAngles.x;

        Vector2 moveInput;
        if (Mathf.Abs(eulerX) < eulerXDeadZone)
        {
            // 死区内：直行（等同于按W键）
            moveInput = new Vector2(0f, 1f);
        }
        else
        {
            // 死区外：计算转向量
            // 负角度 → 左转（负x），正角度 → 右转（正x）
            float turnAmount = (eulerX - Mathf.Sign(eulerX) * eulerXDeadZone) / (90f - eulerXDeadZone);
            turnAmount = Mathf.Clamp(turnAmount * turnSensitivity, -1f, 1f);

            // 始终有前进输入
            moveInput = new Vector2(turnAmount, 1f);
        }

        // 设置移动输入
        fishMovement.SetMoveInput(moveInput);

        // === 动力控制 ===
        float gyroMag = fishReceiver.gyroMagnitude;
        bool isAccelerating = gyroMag > gyroThreshold;

        if (isAccelerating)
        {
            // 根据gyroMagnitude计算Jump触发间隔
            // gyroMagnitude越大，间隔越小（触发越频繁）
            float normalizedGyro = Mathf.Clamp01((gyroMag - gyroThreshold) / (gyroMaxValue - gyroThreshold));
            float currentInterval = Mathf.Lerp(maxJumpInterval, minJumpInterval, normalizedGyro);

            // 检查是否可以触发Jump
            if (Time.time - lastJumpTriggerTime >= currentInterval)
            {
                fishMovement.OnJumpPressed(true);
                lastJumpTriggerTime = Time.time;
            }
        }
        else
        {
            // 低于阈值时，如果之前在加速，发送释放信号
            if (wasAccelerating)
            {
                fishMovement.OnJumpPressed(false);
            }
        }

        wasAccelerating = isAccelerating;
    }

    /// <summary>
    /// Phase2: 处理旋转和力度输入
    /// </summary>
    private void UpdatePhase2Input()
    {
        // === 旋转控制（无死区）===
        float eulerX = fishReceiver.eulerAngles.x;

        // 直接映射角度到方向输入
        // 负角度 → 左方向（x>0），正角度 → 右方向（x<0）
        float rotationInput = -eulerX / 90f * phase2RotationSensitivity;
        rotationInput = Mathf.Clamp(rotationInput, -1f, 1f);

        // 设置Phase2方向输入
        fishMovement.SetPhase2DirectionInput(new Vector2(rotationInput, 0f));

        // 如果有Phase2InputNormalizer，也设置其ESP32输入
        if (phase2InputNormalizer != null)
        {
            phase2InputNormalizer.SetESP32FishRotation(eulerX);
        }

        // === 力度控制 ===
        float gyroMag = fishReceiver.gyroMagnitude;
        float normalizedForce = Mathf.Clamp01(gyroMag / gyroMaxValue);

        // 平滑力度变化
        currentPower = Mathf.SmoothDamp(currentPower, normalizedForce, ref powerVelocity, powerSmoothTime);
        CurrentPhase2Force = currentPower;

        // 设置Phase2力度
        if (phase2ForceDetector != null)
        {
            phase2ForceDetector.SetESP32FishForce(currentPower);
        }

        // Phase2中也触发Jump来影响动画
        if (gyroMag > gyroThreshold)
        {
            fishMovement.OnJumpPressed(true);
        }
    }

    /// <summary>
    /// 设置当前游戏阶段
    /// </summary>
    /// <param name="phase">1=Phase1追捕阶段, 2=Phase2争斗阶段</param>
    public void SetGamePhase(int phase)
    {
        currentPhase = phase;
        Debug.Log($"[ESP32FishController] 切换到Phase{phase}");

        if (phase == 2)
        {
            // 进入Phase2时启用Phase2模式
            if (fishMovement != null)
            {
                fishMovement.EnablePhase2Mode();
            }
        }
        else
        {
            // 退出Phase2时禁用Phase2模式
            if (fishMovement != null)
            {
                fishMovement.DisablePhase2Mode();
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
    /// 重新校准ESP32（转发到FishReceiver）
    /// </summary>
    public void Recalibrate()
    {
        if (fishReceiver != null)
        {
            fishReceiver.Recalibrate();
            Debug.Log("[ESP32FishController] 已重新校准");
        }
    }

    private void OnGUI()
    {
        if (!enableESP32Control) return;

        // 调试信息显示
        GUIStyle style = new GUIStyle();
        style.fontSize = 12;
        style.normal.textColor = Color.yellow;

        string status = fishReceiver != null && fishReceiver.isConnected ? "已连接" : "未连接";
        string debugInfo = $"[ESP32鱼控制器] Phase{currentPhase}\n";
        debugInfo += $"状态: {status}\n";

        if (fishReceiver != null && fishReceiver.isConnected)
        {
            debugInfo += $"EulerX: {fishReceiver.eulerAngles.x:F1}° (死区: ±{eulerXDeadZone}°)\n";
            debugInfo += $"GyroMag: {fishReceiver.gyroMagnitude:F2} (阈值: {gyroThreshold})\n";

            if (currentPhase == 2)
            {
                debugInfo += $"Phase2力度: {CurrentPhase2Force:F2}\n";
            }
        }

        GUI.Label(new Rect(10, 10, 300, 120), debugInfo, style);
    }
}

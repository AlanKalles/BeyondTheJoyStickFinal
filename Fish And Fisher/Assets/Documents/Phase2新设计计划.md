# Phase2 场景切换重构设计计划

## 📋 项目概述

**目标**：将Phase2从Main.unity场景内切换改为独立场景切换，实现更清晰的架构和更流畅的转场效果。

**核心变更**：
- Phase1 (Main.unity) → 全屏转场 → Phase2 (Phase2.unity)
- 取消分屏，Phase2使用单一俯视相机
- 1.5秒白屏转场效果
- 自动化流程，无需玩家点击"开始争斗"按钮

---

## 🎯 设计目标

1. ✅ **清晰的场景分离**：Phase1和Phase2完全独立
2. ✅ **流畅的转场体验**：1.5秒白屏过渡，覆盖场景加载
3. ✅ **简化相机系统**：Phase2使用单一俯视相机
4. ✅ **自动化流程**：减少玩家操作，提升流畅度
5. ✅ **数据持久化**：确保游戏状态正确传递

---

## 📊 场景切换流程图

```
┌─────────────────────────────────────────────────────────────┐
│                    Phase1 (Main.unity)                      │
├─────────────────────────────────────────────────────────────┤
│  1. 渔夫钩中鱼 (FisherController.OnHookHit)                 │
│  2. 保存数据到 Phase2DataTransfer                           │
│  3. 调用 GameManager.OnFishHooked()                         │
│  4. 显示 "抓到鱼了!" UI (FishCaughtNotificationUI)          │
│                                                             │
│  ⏱️  等待 1.5 秒                                            │
│                                                             │
│  5. 自动触发转场 (GameManager.StartTransitionToPhase2)      │
│  6. 显示全屏白色转场UI (Phase2TransitionUI 淡入)            │
│  7. 开始异步加载 Phase2.unity 场景                          │
│                                                             │
│  ⏱️  转场动画进行中 (1.5 秒)                                │
│                                                             │
│  8. 场景加载完成                                            │
│  9. 激活 Phase2.unity 场景                                  │
│ 10. 卸载 Main.unity 场景                                    │
└─────────────────────────────────────────────────────────────┘
                            ⬇️
┌─────────────────────────────────────────────────────────────┐
│                   Phase2 (Phase2.unity)                     │
├─────────────────────────────────────────────────────────────┤
│ 11. Phase2SceneInitializer.Start() 执行                     │
│ 12. 从 Phase2DataTransfer 读取数据                          │
│ 13. 设置鱼和渔夫的位置、旋转                                │
│ 14. 设置游戏剩余时间                                        │
│ 15. 转场UI淡出 (Phase2TransitionUI)                         │
│                                                             │
│ 16. 显示 "开始争斗!" 提示                                   │
│ 17. 开始 3 秒倒计时 (3... 2... 1...)                        │
│                                                             │
│  ⏱️  等待 3 秒                                              │
│                                                             │
│ 18. 倒计时结束，调用 Phase2Manager.StartPhase2()            │
│ 19. 启动所有Phase2子系统（输入、力度、进度条等）            │
│ 20. 开始Phase2争斗！                                        │
└─────────────────────────────────────────────────────────────┘
```

---

## 🗂️ 数据传递系统设计

### 需要传递的数据

| 数据项 | 类型 | 说明 |
|-------|------|------|
| **fishPosition** | Vector3 | 鱼的世界坐标位置 |
| **fishRotation** | Quaternion | 鱼的旋转（用于Phase2初始朝向） |
| **fisherHookPosition** | Vector3 | 渔夫钩子的位置（鱼竿Pivot中心） |
| **remainingTime** | float | 游戏剩余时间（秒） |
| **initialProgress** | float | Phase2初始进度值（默认50） |

### Phase2DataTransfer 类设计

```csharp
// Scripts/Phase2/Phase2DataTransfer.cs

using UnityEngine;

namespace Phase2
{
    /// <summary>
    /// Phase1到Phase2的数据传递单例
    /// 使用DontDestroyOnLoad确保场景切换时不被销毁
    /// </summary>
    public class Phase2DataTransfer : MonoBehaviour
    {
        // 单例实例
        public static Phase2DataTransfer Instance { get; private set; }

        // 传递数据
        public Vector3 FishPosition { get; private set; }
        public Quaternion FishRotation { get; private set; }
        public Vector3 FisherHookPosition { get; private set; }
        public float RemainingTime { get; private set; }
        public float InitialProgress { get; private set; }

        // 数据保存方法
        public void SavePhase2Data(
            Vector3 fishPos,
            Quaternion fishRot,
            Vector3 hookPos,
            float timeRemaining,
            float progress = 50f)
        {
            FishPosition = fishPos;
            FishRotation = fishRot;
            FisherHookPosition = hookPos;
            RemainingTime = timeRemaining;
            InitialProgress = progress;
        }

        // 单例初始化
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
```

---

## 🎨 转场UI系统设计

### UI层级结构

```
Canvas (ScreenSpace-Overlay, SortOrder: 9999)
└── Phase2TransitionPanel
    ├── WhiteBackground (Image, Color: White, Alpha: 0→1→0)
    └── CenterImage (Image, 您提供的图片资源)
```

### Phase2TransitionUI 脚本设计

**功能**：
- 淡入动画：0.75秒，Alpha 0 → 1
- 淡出动画：0.75秒，Alpha 1 → 0
- 总时长：1.5秒
- 提供回调事件：OnTransitionComplete

**关键方法**：
```csharp
public void PlayTransition(System.Action onComplete)
{
    StartCoroutine(TransitionSequence(onComplete));
}

private IEnumerator TransitionSequence(System.Action onComplete)
{
    // 淡入 (0.75秒)
    yield return FadeIn(0.75f);

    // 中间可以执行场景切换
    onComplete?.Invoke();

    // 淡出 (0.75秒)
    yield return FadeOut(0.75f);
}
```

---

## 📝 TODO 任务清单

### ✅ 第一阶段：核心系统创建

- [ ] **任务1**：创建 Phase2DataTransfer 数据传递系统
  - 文件：`Scripts/Phase2/Phase2DataTransfer.cs`
  - 功能：单例模式、DontDestroyOnLoad、数据保存/读取
  - 测试：在Main场景创建GameObject挂载此脚本

- [ ] **任务2**：创建 Phase2TransitionUI 转场UI系统
  - 文件：`Scripts/UI/Phase2TransitionUI.cs`
  - 功能：白屏淡入淡出动画、回调事件
  - UI配置：在Main场景Canvas下创建转场UI

- [ ] **任务3**：创建 Phase2SceneInitializer 场景初始化器
  - 文件：`Scripts/Phase2/Phase2SceneInitializer.cs`
  - 功能：读取传递数据、初始化对象、启动倒计时

### ✅ 第二阶段：修改现有系统

- [ ] **任务4**：修改 FishCaughtNotificationUI
  - 文件：`Scripts/UI/FishCaughtNotificationUI.cs`
  - 修改：移除按钮交互，改为1.5秒自动触发
  - 新增：OnAutoTransition 回调事件

- [ ] **任务5**：修改 GameManager 实现场景切换
  - 文件：`Scripts/GameManager.cs`
  - 修改内容：
    - 删除 phase2Manager 和 phase2ProgressBar 引用
    - OnFishHooked() 保存数据到 Phase2DataTransfer
    - 新增 StartTransitionToPhase2() 方法
    - 新增场景异步加载逻辑
    - 移除 StartPhase2Countdown() 和 StartPhase2Struggle()

- [ ] **任务6**：修改 Phase2Manager 移除相机切换
  - 文件：`Scripts/Phase2/Phase2Manager.cs`
  - 修改：
    - 删除 cameraController 引用
    - StartPhase2() 方法中移除相机切换调用
    - 保留其他子系统（输入、力度、进度条、音效）

### ✅ 第三阶段：场景配置

- [ ] **任务7**：从 Main.unity 删除 Phase2 对象
  - 删除对象：
    - `------Phase2-----/CM_Fish P2`
    - `------Phase2-----/CM_Fisher P2`
    - `------Phase2-----/Phase2CameraControl`
    - `------Phase2-----` 父对象（如果为空）
  - 移除引用：GameManager 组件中的 phase2Manager、phase2ProgressBar

- [ ] **任务8**：配置 Phase2.unity 场景（需要您手动完成）
  - 创建俯视相机
  - 放置鱼和渔夫对象（预设位置）
  - 创建Phase2Manager GameObject
  - 配置所有Phase2 UI
  - 添加Phase2SceneInitializer

- [ ] **任务9**：Build Settings 配置
  - 添加 Phase2.unity 到构建场景列表
  - 确保场景索引：Main.unity (0), Phase2.unity (1)

### ✅ 第四阶段：测试与优化

- [ ] **任务10**：测试完整流程
  - Phase1游戏流程正常
  - 钩中鱼触发转场
  - 数据正确传递到Phase2
  - Phase2场景初始化正确
  - Phase2游戏流程正常

- [ ] **任务11**：优化与调试
  - 转场动画流畅度
  - 场景加载性能
  - UI显示时机
  - 音效衔接

---

## 🛠️ 需要修改的文件清单

### 新建文件 (3个)

1. **Scripts/Phase2/Phase2DataTransfer.cs**
   - 数据传递单例系统

2. **Scripts/UI/Phase2TransitionUI.cs**
   - 全屏转场UI控制器

3. **Scripts/Phase2/Phase2SceneInitializer.cs**
   - Phase2场景初始化控制器

### 修改文件 (3个)

1. **Scripts/GameManager.cs**
   - 删除：phase2Manager、phase2ProgressBar字段
   - 修改：OnFishHooked() 方法
   - 新增：StartTransitionToPhase2() 方法
   - 新增：场景加载协程

2. **Scripts/UI/FishCaughtNotificationUI.cs**
   - 删除：开始按钮的事件监听
   - 修改：显示逻辑改为1.5秒自动触发
   - 新增：OnAutoTransition 事件

3. **Scripts/Phase2/Phase2Manager.cs**
   - 删除：cameraController 引用
   - 修改：StartPhase2() 移除相机切换调用

### 保留但不再使用的文件

1. **Scripts/Phase2/Phase2CameraController.cs**
   - 可以保留以备将来使用，但当前流程中不再调用

---

## 🎮 Phase2 场景配置要求

### 必需的 GameObject

| GameObject名称 | 组件 | 说明 |
|---------------|------|------|
| **Phase2Manager** | Phase2Manager<br>Phase2InputNormalizer<br>Phase2ForceDetector<br>Phase2ProgressBar<br>Phase2AudioManager | Phase2系统总控制器 |
| **Phase2Initializer** | Phase2SceneInitializer | 场景初始化脚本 |
| **CM_Phase2_Overhead** | CinemachineCamera | 俯视相机，Priority=10 |
| **Fish** | 鱼相关组件 | 从Prefab生成或预设 |
| **FisherRodPivot** | 渔夫鱼竿组件 | 预设位置 |

### 必需的 UI

| UI名称 | 说明 |
|--------|------|
| **Phase2ProgressBarUI** | 争斗进度条（鱼 vs 渔夫） |
| **Phase2TransitionUI** | 转场白屏UI（复用Main场景的） |
| **StartStruggleText** | "开始争斗!"提示文字 |
| **CountdownText** | 3秒倒计时 (3...2...1...) |
| **GameTimerUI** | 游戏总计时（可选） |

### 场景设置建议

- **光照**：根据Phase2需求调整（可以与Phase1不同）
- **后处理**：可以设置不同的后处理效果增强氛围
- **背景**：俯视角度需要设计合适的背景环境
- **音效**：Phase2AudioManager自动管理音效

---

## 🔄 详细实施步骤

### 步骤1：创建数据传递系统

```bash
# 创建脚本
Scripts/Phase2/Phase2DataTransfer.cs
```

**实现要点**：
- 单例模式
- DontDestroyOnLoad
- 提供SavePhase2Data()和各属性的Get方法

**测试**：
- 在Main场景创建空GameObject "Phase2DataTransfer"
- 挂载 Phase2DataTransfer 脚本

---

### 步骤2：创建转场UI系统

```bash
# 创建脚本
Scripts/UI/Phase2TransitionUI.cs
```

**UI创建**：
1. 在Main场景Canvas下创建新GameObject "Phase2TransitionPanel"
2. 添加Image组件，设置为白色背景
3. 添加子对象，放置中间图片
4. 挂载 Phase2TransitionUI 脚本
5. 初始设置为不可见（CanvasGroup.alpha = 0）

**脚本要点**：
- 使用CanvasGroup控制透明度
- 协程实现淡入淡出
- 提供回调接口

---

### 步骤3：修改 FishCaughtNotificationUI

**主要修改**：

```csharp
// 删除这部分代码
// private void OnStartButtonClicked() { ... }

// 修改Show()方法
public void Show(System.Action onAutoTransition = null)
{
    gameObject.SetActive(true);
    StartCoroutine(ShowSequence(onAutoTransition));
}

private IEnumerator ShowSequence(System.Action callback)
{
    // 播放弹出动画
    yield return StartCoroutine(FadeIn());
    yield return StartCoroutine(ScalePopup());

    // 等待1.5秒
    yield return new WaitForSeconds(1.5f);

    // 自动触发转场
    callback?.Invoke();
}
```

---

### 步骤4：修改 GameManager

**主要修改点**：

```csharp
// 删除这些字段
// [SerializeField] private Phase2.Phase2Manager phase2Manager;
// [SerializeField] private Phase2.Phase2ProgressBar phase2ProgressBar;

// 新增字段
[SerializeField] private Phase2TransitionUI transitionUI;
private Phase2.Phase2DataTransfer dataTransfer;

// 修改 OnFishHooked() 方法
public void OnFishHooked(Vector3 hookPosition)
{
    if (currentState != GameState.Playing) return;

    currentState = GameState.Phase2Preparation;

    // 保存数据
    dataTransfer = Phase2.Phase2DataTransfer.Instance;
    if (dataTransfer != null)
    {
        dataTransfer.SavePhase2Data(
            fishController.transform.position,
            fishController.transform.rotation,
            hookPosition,
            RemainingTime,
            50f
        );
    }

    // 显示"抓到鱼"UI，1.5秒后触发转场
    fishCaughtUI.Show(() => StartTransitionToPhase2());
}

// 新增转场方法
private void StartTransitionToPhase2()
{
    transitionUI.PlayTransition(() => {
        StartCoroutine(LoadPhase2Scene());
    });
}

// 新增场景加载协程
private IEnumerator LoadPhase2Scene()
{
    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Phase2");
    asyncLoad.allowSceneActivation = false;

    // 等待加载完成
    while (asyncLoad.progress < 0.9f)
    {
        yield return null;
    }

    // 激活场景
    asyncLoad.allowSceneActivation = true;
}
```

**注意**：需要添加 `using UnityEngine.SceneManagement;`

---

### 步骤5：创建 Phase2SceneInitializer

```bash
# 创建脚本
Scripts/Phase2/Phase2SceneInitializer.cs
```

**实现要点**：

```csharp
using UnityEngine;
using System.Collections;
using TMPro;

namespace Phase2
{
    public class Phase2SceneInitializer : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private Transform fishTransform;
        [SerializeField] private Transform fisherRodPivot;
        [SerializeField] private Phase2Manager phase2Manager;

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI startText;
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private Phase2TransitionUI transitionUI;

        private void Start()
        {
            InitializePhase2();
        }

        private void InitializePhase2()
        {
            // 读取传递数据
            var dataTransfer = Phase2DataTransfer.Instance;
            if (dataTransfer != null)
            {
                // 设置鱼位置
                fishTransform.position = dataTransfer.FishPosition;
                fishTransform.rotation = dataTransfer.FishRotation;

                // 设置渔夫钩子位置
                fisherRodPivot.position = dataTransfer.FisherHookPosition;

                // 设置剩余时间（需要访问GameManager或自己管理）
                // GameManager.Instance.SetRemainingTime(dataTransfer.RemainingTime);
            }

            // 开始流程
            StartCoroutine(Phase2StartSequence());
        }

        private IEnumerator Phase2StartSequence()
        {
            // 等待转场UI淡出
            yield return new WaitForSeconds(0.75f);

            // 显示"开始争斗!"
            startText.text = "开始争斗!";
            startText.gameObject.SetActive(true);
            yield return new WaitForSeconds(1f);
            startText.gameObject.SetActive(false);

            // 3秒倒计时
            for (int i = 3; i > 0; i--)
            {
                countdownText.text = i.ToString();
                countdownText.gameObject.SetActive(true);
                yield return new WaitForSeconds(1f);
            }
            countdownText.gameObject.SetActive(false);

            // 启动Phase2
            phase2Manager.StartPhase2();
        }
    }
}
```

---

### 步骤6：修改 Phase2Manager

**删除相机相关代码**：

```csharp
// 删除这个字段
// [SerializeField] private Phase2CameraController cameraController;

// 修改 StartPhase2() 方法，移除相机切换部分
public void StartPhase2()
{
    Debug.Log("[Phase2Manager] Phase2争斗开始");
    isPhase2Active = true;

    // 激活输入归一化
    if (inputNormalizer != null)
        inputNormalizer.ActivatePhase2Input();

    // 激活力度检测
    if (forceDetector != null)
        forceDetector.ActivatePhase2Force();

    // 显示进度条UI
    if (progressBarUI != null)
        progressBarUI.Show();

    // 删除这部分：
    // if (cameraController != null)
    //     cameraController.SwitchToPhase2Cameras();

    // 启用玩家Phase2模式
    if (fishController != null)
        fishController.EnablePhase2Mode();

    if (fisherController != null)
        fisherController.EnablePhase2Mode();

    // 播放音效
    if (audioManager != null)
        audioManager.PlayPhase2StartSound();

    // 启动进度条逻辑
    if (progressBar != null)
        progressBar.StartStruggle();
}
```

---

### 步骤7：从 Main.unity 删除对象

**手动操作**：
1. 打开 Main.unity 场景
2. 在Hierarchy中找到并删除：
   - `------Phase2-----/CM_Fish P2`
   - `------Phase2-----/CM_Fisher P2`
   - `------Phase2-----/Phase2CameraControl`
   - `------Phase2-----` (如果为空)
3. 选择 GameManager GameObject
4. 在Inspector中移除 phase2Manager 和 phase2ProgressBar 的引用
5. 保存场景

---

### 步骤8：配置 Phase2.unity 场景

**您需要手动完成**：

1. **创建新场景**
   - File → New Scene
   - 保存为 `Assets/Scenes/Phase2.unity`

2. **创建俯视相机**
   - GameObject → Cinemachine → Cinemachine Camera
   - 命名：CM_Phase2_Overhead
   - 设置位置和旋转以获得俯视效果
   - Priority: 10

3. **放置玩家对象**
   - 从Prefab或复制Main场景中的Fish和FisherRodPivot
   - 设置合适的初始位置（将被Phase2SceneInitializer覆盖）

4. **创建Phase2Manager**
   - 空GameObject，命名 "Phase2Manager"
   - 挂载所有Phase2子系统脚本
   - 配置引用

5. **创建Phase2Initializer**
   - 空GameObject，命名 "Phase2Initializer"
   - 挂载 Phase2SceneInitializer 脚本
   - 配置所有引用

6. **创建UI**
   - 复制Main场景的Canvas或创建新的
   - 添加所有Phase2必需UI

7. **Build Settings**
   - File → Build Settings
   - 添加 Phase2.unity 到场景列表

---

## ⚠️ 注意事项

### 1. 场景加载性能

- 使用 `LoadSceneAsync` 异步加载
- 1.5秒转场时间足够加载完成
- 如果场景复杂，可以延长转场时间

### 2. 数据持久化

- Phase2DataTransfer 使用 DontDestroyOnLoad
- 确保在Main场景中提前创建此对象
- Phase2结束后可以选择是否销毁

### 3. UI层级

- 转场UI必须使用 ScreenSpace-Overlay
- SortOrder设置为最高（如9999）
- 确保不受相机切换影响

### 4. 音效衔接

- Main场景的背景音乐需要淡出
- Phase2场景的音效在初始化时启动
- 考虑使用AudioMixerGroup统一管理

### 5. 测试要点

- 数据是否正确传递
- 鱼和渔夫位置是否正确
- 剩余时间是否正确
- 转场动画是否流畅
- Phase2游戏逻辑是否正常

---

## 🔍 调试建议

### 启用调试日志

在关键位置添加Debug.Log：

```csharp
// GameManager.OnFishHooked
Debug.Log($"[GameManager] 保存数据：鱼位置={fishPos}, 剩余时间={time}");

// Phase2SceneInitializer.InitializePhase2
Debug.Log($"[Phase2Init] 读取数据：鱼位置={dataTransfer.FishPosition}");

// 转场UI
Debug.Log("[TransitionUI] 开始淡入");
Debug.Log("[TransitionUI] 场景切换回调执行");
Debug.Log("[TransitionUI] 开始淡出");
```

### Scene视图检查

- 使用Scene视图确认鱼和渔夫位置是否正确
- 检查相机视角是否符合预期
- 验证UI元素是否正确显示

---

## 📅 预计时间

| 阶段 | 预计时间 |
|------|---------|
| 创建新脚本（3个） | 1-2小时 |
| 修改现有脚本（3个） | 1-2小时 |
| 场景配置（Phase2.unity） | 2-3小时 |
| 测试与调试 | 1-2小时 |
| **总计** | **5-9小时** |

---

## ✅ 完成标准

- [ ] 渔夫钩中鱼后自动触发转场
- [ ] 白屏转场效果流畅（1.5秒）
- [ ] Phase2场景正确加载
- [ ] 鱼和渔夫位置、旋转正确初始化
- [ ] 游戏剩余时间正确传递
- [ ] "开始争斗"提示和3秒倒计时正常显示
- [ ] Phase2争斗逻辑完全正常
- [ ] 无Console错误或警告
- [ ] Phase2使用单一俯视相机
- [ ] 玩家体验流畅，无明显卡顿

---

## 📚 相关文档

- [Phase2_Implementation_Summary.md](Phase2_Implementation_Summary.md) - Phase2系统实现总结
- [Phase2_Dependency_Updates.md](Phase2_Dependency_Updates.md) - 依赖项更新说明
- [CameraSetup.md](CameraSetup.md) - 相机配置指南（Phase1部分仍然适用）

---

**创建日期**：2025-01-21
**版本**：1.0
**状态**：待实施

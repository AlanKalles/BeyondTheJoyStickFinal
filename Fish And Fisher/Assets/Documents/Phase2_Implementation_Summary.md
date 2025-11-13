# Phase2争斗系统实现总结

## 概述

已成功实现了"鱼与渔夫"游戏的Phase2争斗阶段。当渔夫钩中鱼后,不再立即结束游戏,而是进入一个玩家对抗的争斗阶段。

---

## 系统架构

### 核心组件

1. **Phase2Manager** (`Scripts/Phase2/Phase2Manager.cs`)
   - 统一管理所有Phase2子系统
   - 处理Phase2的启动和结束流程
   - 单例模式,全局访问

2. **Phase2InputNormalizer** (`Scripts/Phase2/Phase2InputNormalizer.cs`)
   - 将玩家输入归一化为左/右/无方向
   - 鱼玩家:A/D键 → (1,0)左 / (-1,0)右
   - 渔夫玩家:鼠标相对屏幕中心 → 左/右方向

3. **Phase2ForceDetector** (`Scripts/Phase2/Phase2ForceDetector.cs`)
   - 检测按键频率并计算力度值
   - 鱼玩家:空格键(Jump)频率
   - 渔夫玩家:鼠标左键(Attack)频率
   - 公式:`频率 × 倍率 × 基础值 = 最终力度`

4. **Phase2ProgressBar** (`Scripts/Phase2/Phase2ProgressBar.cs`)
   - 进度条核心逻辑
   - 总长度100,初始50(中间)
   - 0=鱼胜利,100=渔夫胜利
   - 使用log归一化处理力度差,确保平滑移动

5. **Phase2ProgressBarUI** (`Scripts/UI/Phase2ProgressBarUI.cs`)
   - 进度条UI可视化
   - 颜色渐变:左蓝(鱼) → 中白 → 右橙(渔夫)
   - 显示当前优势状态

6. **FishCaughtNotificationUI** (`Scripts/UI/FishCaughtNotificationUI.cs`)
   - "抓到鱼!"提示UI
   - 带"开始争斗"按钮
   - 3秒倒计时(3...2...1...)
   - 使用DOTween动画

7. **Phase2CameraController** (`Scripts/Phase2/Phase2CameraController.cs`)
   - 管理Phase1和Phase2相机切换
   - 支持Cinemachine虚拟相机
   - 平滑过渡

8. **Phase2AudioManager** (`Scripts/Phase2/Phase2AudioManager.cs`)
   - 音效接口预留
   - 包含:钩中音效、倒计时音效、Phase2开始音效、争斗循环音效等

---

## 游戏流程

### Phase1 → Phase2转换

```
1. 渔夫钩中鱼
   ↓
2. FisherController.OnHookHit()
   → GameManager.OnFishHooked()
   ↓
3. 进入Phase2Preparation状态
   → 显示"抓到鱼"提示UI
   ↓
4. 玩家点击"开始争斗"按钮
   → GameManager.StartPhase2Countdown()
   ↓
5. 3秒倒计时
   ↓
6. GameManager.StartPhase2Struggle()
   → Phase2Manager.StartPhase2()
   ↓
7. Phase2争斗开始!
```

### Phase2争斗规则

#### 输入系统
- **鱼玩家**:
  - A/D键控制方向(左/右)
  - 只要按下就保持该方向,直到按反方向
  - 空格键(Jump)频率影响力度
  - 朝向根据方向输入旋转(±45°)

- **渔夫玩家**:
  - 鼠标X坐标相对屏幕中心控制方向
  - 鼠标左键频率影响力度
  - 鱼竿pivot根据方向旋转(可配置角度)

#### 进度条移动规则

**基础规则:**
- 两玩家方向一致 → 向左移动(基础速度5单位/秒)
- 两玩家方向不一致 → 向右移动(基础速度5单位/秒)
- 鱼无输入 → 必向右移动
- 渔夫无输入且鱼有输入 → 向左移动
- 两者都无输入 → 向右移动

**力度差影响:**
```csharp
力度差 = 渔夫力度 - 鱼力度
归一化系数 = log(1 + |力度差|) / log(基准值)
最终速度 = 基础速度 × (1 + 归一化系数 × sign(力度差))
```

**力度计算参数:**
- 鱼基础力度:10
- 渔夫基础力度:15(略高于鱼)
- 力度倍率:1.0
- 频率检测窗口:0.5秒

#### 胜负判定

1. **进度条到达边界**:
   - 进度 ≤ 0 → 鱼胜利
   - 进度 ≥ 100 → 渔夫胜利

2. **时间耗尽(Phase2中)**:
   - 进度 < 50 → 鱼胜利
   - 进度 ≥ 50 → 渔夫胜利

---

## 修改的现有文件

### 1. GameManager.cs
- 添加新状态:`Phase2Preparation`、`Phase2Struggle`
- 新方法:
  - `OnFishHooked()` - 渔夫钩中鱼(取代原`OnFishCaught()`)
  - `StartPhase2Preparation()` - 开始准备阶段
  - `StartPhase2Countdown()` - 开始倒计时
  - `StartPhase2Struggle()` - 开始争斗
  - `OnPhase2ProgressComplete()` - 进度条达到边界

### 2. FisherController.cs
- 修改`OnHookHit()`:调用`OnFishHooked()`而非直接结束游戏
- 添加Phase2模式:
  - `EnablePhase2Mode()` - 启用Phase2(冻结准心)
  - `DisablePhase2Mode()` - 禁用Phase2
  - `UpdatePhase2Rotation()` - 根据鼠标位置旋转鱼竿pivot

### 3. FishMovement.cs
- 添加Phase2模式:
  - `EnablePhase2Mode()` - 启用Phase2(禁用移动)
  - `DisablePhase2Mode()` - 禁用Phase2
  - `SetPhase2DirectionInput()` - 设置Phase2方向
  - `UpdatePhase2Direction()` - 更新朝向(±45°)
  - Jump按键仍然影响动画速度

### 4. FishController.cs
- 添加Phase2方法:
  - `EnablePhase2Mode()` - 启用Phase2
  - `DisablePhase2Mode()` - 禁用Phase2
  - `SetPhase2DirectionInput()` - 设置方向输入
- `OnHooked()`方法已存在,确保正确调用流程

### 5. GameTimerUI.cs
- 无需修改,GameManager.OnTimeUp()已处理Phase2逻辑

---

## 新增文件清单

```
Assets/Scripts/Phase2/           (新文件夹)
├── Phase2Manager.cs             (统一管理器)
├── Phase2InputNormalizer.cs     (输入归一化)
├── Phase2ForceDetector.cs       (力度检测)
├── Phase2ProgressBar.cs         (进度条逻辑)
├── Phase2CameraController.cs    (相机切换)
└── Phase2AudioManager.cs        (音效管理)

Assets/Scripts/UI/
├── Phase2ProgressBarUI.cs       (进度条UI)
└── FishCaughtNotificationUI.cs  (提示UI)
```

---

## 场景配置需求

### 必需GameObject和组件

1. **Phase2Manager** (新建空GameObject)
   - 添加`Phase2Manager`组件
   - 引用所有Phase2子系统

2. **Phase2InputNormalizer** (可作为Phase2Manager子对象)
   - 添加`Phase2InputNormalizer`组件
   - 设置`inputActions`和`fisherCamera`

3. **Phase2ForceDetector** (可作为Phase2Manager子对象)
   - 添加`Phase2ForceDetector`组件
   - 配置力度参数

4. **Phase2ProgressBar** (可作为Phase2Manager子对象)
   - 添加`Phase2ProgressBar`组件
   - 引用`inputNormalizer`和`forceDetector`

5. **Phase2CameraController** (新建GameObject)
   - 添加`Phase2CameraController`组件
   - 创建4个Cinemachine虚拟相机:
     - fishPhase1Camera (Phase1鱼相机)
     - fisherPhase1Camera (Phase1渔夫相机)
     - fishPhase2Camera (Phase2鱼相机)
     - fisherPhase2Camera (Phase2渔夫相机)

6. **Phase2AudioManager** (可作为Phase2Manager子对象)
   - 添加`Phase2AudioManager`组件
   - 稍后添加音效片段

### UI结构

```
Canvas/
├── GameTimerUI (已存在)
├── GameResultUI (已存在)
├── FishCaughtNotificationUI (新建)
│   ├── NotificationText (TextMeshProUGUI)
│   ├── CountdownText (TextMeshProUGUI)
│   └── StartButton (Button)
│       └── ButtonText (TextMeshProUGUI)
└── Phase2ProgressBarUI (新建)
    ├── Slider (Slider组件)
    │   └── Fill (Image - 用于颜色渐变)
    └── PercentageText (TextMeshProUGUI)
```

### 渔夫鱼竿Pivot

在渔夫GameObject下创建:
```
Fisher/
└── RodPivot (新建Transform)
    └── FishingRod (现有鱼竿模型)
```

---

## 调试功能

所有Phase2组件都包含OnGUI调试信息:

- **Phase2InputNormalizer**:显示鱼/渔夫方向、方向是否一致
- **Phase2ForceDetector**:显示鱼/渔夫力度、力度差
- **Phase2ProgressBar**:显示进度值、百分比、当前优势状态

可在开发阶段启用,发布时禁用。

---

## 参数调优

### 力度系统
```csharp
// Phase2ForceDetector
fishBaseForce = 10f;          // 鱼基础力度
fisherBaseForce = 15f;        // 渔夫基础力度(略高)
detectionWindow = 0.5f;       // 频率检测窗口
```

### 进度条速度
```csharp
// Phase2ProgressBar
baseSpeed = 5f;               // 基础移动速度
logNormalizationBase = 10f;   // Log归一化基准值(影响速度曲线)
```

### 旋转角度
```csharp
// FishMovement
phase2LeftAngle = -45f;       // 鱼左转角度
phase2RightAngle = 45f;       // 鱼右转角度

// FisherController
leftRotationAngle = -45f;     // 渔夫鱼竿左转角度
rightRotationAngle = 45f;     // 渔夫鱼竿右转角度
```

---

## 依赖项

- **DOTween** - 用于FishCaughtNotificationUI动画(如果项目中未安装,需要安装或改用Unity Animation)
- **Cinemachine** - 用于Phase2相机切换
- **TextMeshPro** - 用于所有UI文本显示

---

## 后续工作

1. **场景配置**:
   - 创建所有必需的GameObject和UI
   - 在GameManager中分配所有引用
   - 配置Cinemachine相机

2. **音效添加**:
   - 在Phase2AudioManager中添加音效片段
   - 测试音效触发时机

3. **视觉效果**:
   - 添加粒子效果(钩中、争斗等)
   - 优化UI动画
   - 添加屏幕振动效果

4. **平衡性调整**:
   - 测试并调整力度参数
   - 调整进度条移动速度
   - 确保游戏公平性

5. **测试**:
   - 单人测试(模拟双人输入)
   - 双人对战测试
   - 边界情况测试

---

## 技术亮点

1. **Log归一化**:使用对数函数平滑处理力度差,避免速度过快或过慢
2. **双Transform系统**:渔夫的逻辑准心(碰撞检测)和视觉准心(显示)分离
3. **模块化设计**:所有Phase2系统独立,易于维护和扩展
4. **事件驱动**:通过GameManager协调各系统,解耦合
5. **调试友好**:丰富的Debug.Log和OnGUI调试信息

---

## 联系与支持

如有问题或需要进一步说明,请参考各个脚本文件中的详细注释。

**文档版本**: 1.0
**最后更新**: 2025-11-13
**作者**: Claude Code Assistant

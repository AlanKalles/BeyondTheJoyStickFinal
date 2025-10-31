# 鱼玩家控制系统使用指南

## 快速开始

### 方法1：使用菜单创建（推荐）
1. 在Unity编辑器中，点击菜单 `GameObject > Fish And Fisher > Create Fish Player`
2. 系统会自动创建一个配置好的鱼玩家GameObject，包含所有必要组件和视觉占位符

### 方法2：手动创建
1. 创建一个空GameObject
2. 添加以下核心组件：
   - `FishController` - 主控制器
   - `FishMovement` - 移动系统
   - `FishInputHandler` - 输入处理
   - `FishState` - 状态管理
   - `FishAnimator` - 动画控制（可选）
   - `FishDebugUI` - 调试界面（可选）

### 验证设置
选中鱼玩家GameObject，点击菜单 `GameObject > Fish And Fisher > Validate Fish Player Setup` 验证配置是否正确

## 组件说明

### FishController（主控制器）
协调所有子系统的核心组件。
- **调试模式**：在Inspector中勾选Debug Mode查看运行时信息

### FishMovement（移动系统）
处理鱼的移动逻辑，包括惯性、方向控制和速度管理。

**关键参数：**
- `Base Speed`：基础移动速度（默认2.0）
- `Max Speed`：最大速度（默认8.0）
- `Acceleration`：加速度（默认3.0）
- `Turn Angle Large`：纯左右转向角度（默认60度）
- `Turn Angle Small`：前进+左右转向角度（默认30度）
- `Boundary Size`：游戏区域边界大小

### FishInputHandler（输入处理）
处理玩家输入并转换为游戏事件。

**关键参数：**
- `Enable Input`：是否启用输入
- `Input Dead Zone`：输入死区（默认0.2）
- `Smooth Input`：是否平滑输入
- `Debug Input`：显示输入调试信息

### FishState（状态管理）
管理鱼的各种游戏状态。

**状态类型：**
- `Idle`：待机
- `Swimming`：游泳
- `Sprinting`：冲刺
- `Stunned`：眩晕
- `Escaping`：逃脱（被钩中后）
- `Caught`：被捕获

**关键参数：**
- `Max Stamina`：最大体力值（默认100）
- `Stamina Drain Rate`：冲刺时体力消耗速度
- `Escape Success Threshold`：成功逃脱所需进度

### FishAnimator（动画控制）
控制鱼的视觉表现。

**动画模式：**
- **程序化动画**：通过代码控制Transform实现动画
- **Animator动画**：使用Unity Animator组件

**关键参数：**
- `Use Procedural Animation`：使用程序化动画
- `Swim Cycle Speed`：游泳动画速度
- `Tail Swing Amplitude`：尾巴摆动幅度

### FishDebugUI（调试界面）
运行时显示调试信息。

**显示内容：**
- 移动信息（速度、方向、位置）
- 状态信息（当前状态、体力）
- 输入信息（移动输入、Jump按键）
- 性能信息（FPS）

## 控制方式

### 键盘控制
- **W/上箭头**：前进（保持当前方向）
- **A/左箭头**：左转60度
- **D/右箭头**：右转60度
- **W+A**：左转30度
- **W+D**：右转30度
- **空格键**：加速（连续按键提高速度）

### 手柄控制
- **左摇杆**：控制方向
- **A/×按钮**：加速

## 参数调整建议

### 休闲游戏风格
```
Base Speed: 1.5
Max Speed: 5.0
Turn Angle Large: 45
Turn Angle Small: 20
```

### 竞技游戏风格
```
Base Speed: 3.0
Max Speed: 10.0
Turn Angle Large: 60
Turn Angle Small: 30
Turn Dampening: 0.7
```

### 真实模拟风格
```
Base Speed: 2.0
Max Speed: 8.0
Acceleration: 2.0
Deceleration: 1.0
Turn Speed At Max Speed: 45
```

## 常见问题

### Q: 鱼不能移动
A: 检查以下几点：
1. FishInputHandler的`Enable Input`是否勾选
2. Input System是否正确配置
3. 当前状态是否允许移动（检查FishState）

### Q: 转向太慢/太快
A: 调整FishMovement中的：
- `Turn Speed Base`：基础转向速度
- `Turn Speed At Max Speed`：高速时的转向速度
- `Direction Smooth Time`：方向平滑时间

### Q: Jump加速无效
A: 检查：
1. Jump Action在InputSystem_Actions中是否配置
2. `Jump Boost Multiplier`是否大于1
3. 当前体力是否充足（FishState）

### Q: 动画不播放
A: 确认：
1. FishAnimator组件是否添加
2. `Use Procedural Animation`是否勾选
3. fishBody、fishTail等Transform是否正确分配

## 扩展开发

### 添加新状态
1. 在`FishStateType`枚举中添加新状态
2. 在`FishState.UpdateCurrentState()`中添加处理逻辑
3. 在`FishState.OnEnterState()`中添加初始化逻辑

### 自定义输入处理
1. 继承`FishInputHandler`类
2. 重写`OnMove`和`OnJump`方法
3. 添加自定义输入事件

### 集成网络同步
1. 在FishController中添加网络组件
2. 同步以下关键数据：
   - Position（位置）
   - CurrentDirection（方向）
   - CurrentSpeed（速度）
   - CurrentState（状态）

## 性能优化建议

1. **降低Update频率**：对于不需要每帧更新的逻辑，使用协程或计时器
2. **简化碰撞检测**：使用简单的碰撞体形状
3. **优化动画**：移动端关闭程序化动画，使用简单的Animator
4. **对象池**：对于特效等频繁创建销毁的对象使用对象池

## 版本历史

### v1.0.0 (2025-10-31)
- 初始版本发布
- 实现基础移动系统
- 支持方向控制（60度/30度转向）
- Jump键节奏加速功能
- 状态管理系统
- 程序化动画系统
- 调试UI界面

---

如有问题或建议，请在项目Issues中反馈。
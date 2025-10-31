# CLAUDE.md

此文件为 Claude Code (claude.ai/code) 在此代码库中工作时提供指导。

## 🚨 重要说明 🚨

**所有后续对话请使用中文进行。** 这是一个中文开发项目，请确保：
- 所有代码注释使用中文
- 所有文档使用中文编写
- 与用户交流时使用中文
- 提交信息(commit messages)使用中文

## 项目概览

**Fish And Fisher（鱼与渔夫）** - 一个使用 Unity 6 和通用渲染管线 (URP) 的游戏项目。目前处于早期开发阶段，基础模板已配置完成，但尚未实现自定义游戏逻辑。

- **Unity 版本:** 6000.0.58f2 (Unity 6) - 需要精确版本
- **渲染管线:** Universal Render Pipeline (URP) v17.0.4
- **输入系统:** Unity InputSystem 1.14.2 已配置玩家操作

## 架构概览

### 核心系统

1. **输入系统** - 已完全配置 InputSystem_Actions.inputactions，包含玩家控制：
   - 移动控制（Move、Look）
   - 动作控制（Attack、Interact、Jump、Crouch）
   - 导航控制（Previous、Next）

2. **渲染管线** - 平台特定配置：
   - PC_Renderer.asset / PC_RPAsset.asset - 桌面端渲染
   - Mobile_Renderer.asset / Mobile_RPAsset.asset - 移动端优化
   - 两个质量预设：Mobile（优化）和 Standard（标准）

3. **场景结构** - 单一场景（SampleScene.unity）包含：
   - 带 URP 阴影的方向光
   - 用于后处理的全局体积
   - 带 RawImage 组件的 Canvas UI 系统
   - 配置为 URP 的主摄像机

### 项目结构

```
Assets/
├── Documents/          # 游戏设计文档等各类文档
├── Scenes/            # 游戏场景（目前仅有 SampleScene.unity）
├── Scripts/           # 游戏脚本
│   ├── Fisher/        # 渔夫相关脚本
│   ├── Fish/          # 鱼相关脚本
│   ├── UI/            # UI相关脚本
│   └── GameManager.cs # 游戏管理器
├── Settings/          # URP 渲染管线和质量设置
├── Texture/           # 纹理资源
└── TutorialInfo/      # 模板教程系统（可以移除）

ProjectSettings/       # Unity 配置（不要直接修改）
Packages/             # 通过 manifest.json 管理的依赖项
```

### 关键依赖

- **渲染:** com.unity.render-pipelines.universal (17.0.4)
- **输入:** com.unity.inputsystem (1.14.2)
- **摄像机:** com.unity.cinemachine (3.1.4)
- **UI:** com.unity.ugui (2.0.0)
- **网络:** com.unity.multiplayer.center (1.0.0) - 已安装但未配置
- **AI:** com.unity.ai.navigation (2.0.9)

## 开发说明

### 当前状态
- 项目基于 URP 空模板
- **渔夫系统已实现** - 使用双Transform系统（逻辑准心 + 视觉准心）
- 输入系统已完全配置并准备就绪
- 平台特定的渲染设置已就位

### 已实现的游戏系统

#### 游戏管理系统 (Game Management)
位于 `Assets/Scripts/`，包含：

1. **GameManager.cs** - 游戏管理器（单例模式）
   - 游戏状态管理（Ready、Playing、Ended）
   - 倒计时系统（默认60秒）
   - 胜负判定逻辑
   - 场景重启和退出功能
   - 提供单例访问：`GameManager.Instance`

2. **GameTimerUI.cs** - 计时器UI
   - 使用TextMeshPro显示倒计时（MM:SS格式）
   - 根据剩余时间自动改变颜色（正常/警告/危险）
   - 时间紧急时启用脉冲动画
   - 可配置颜色阈值和动画效果

3. **GameResultUI.cs** - 结果面板UI
   - 显示胜负结果（英文）
   - 支持两种结果："FISH WINS!" 或 "FISHER WINS!"
   - 带有缩放弹出动画（EaseOutBack）
   - 提供重新开始和退出按钮

**游戏规则：**
- 倒计时结束时，如果鱼未被抓到 → 鱼胜利
- 渔夫在倒计时内钓到鱼 → 渔夫胜利

#### 渔夫系统 (Fisher)
位于 `Assets/Scripts/Fisher/`，包含：

1. **FisherCrosshairController.cs** - 准心控制器
   - 双Transform系统：逻辑准心（鱼平面）+ 视觉准心（倾斜平面）
   - 通过鼠标Look输入控制准心移动
   - 矩形边界限制（50×50），与鱼活动范围完全对等
   - 平滑移动和实时同步
   - 包含Gizmos调试可视化（黄色矩形框显示边界）

2. **FisherController.cs** - 渔夫主控制器
   - 管理鱼竿挥动动作（Attack输入）
   - 冷却时间系统
   - 钩子碰撞检测（使用逻辑准心位置，球形检测半径0.5）
   - 钩住鱼时自动通知GameManager触发胜利判定
   - 可配置钩子检测半径和图层

3. **CrosshairMaterial.shader** - 准心专用URP Shader
   - 透明混合支持
   - 可配置颜色和透明度
   - 适配URP渲染管线

4. **CrosshairTextureGenerator.cs** - 准心纹理生成工具
   - 编辑器菜单：FishAndFisher/生成准心纹理
   - 支持十字准心和圆形准心两种样式
   - 自动生成到 `Assets/Texture/` 目录

### 添加游戏逻辑时
1. 脚本已组织在 `Assets/Scripts/` 中
2. 按功能组织：`Scripts/Fisher/`（已完成）、`Scripts/Fish/`（待实现）、`Scripts/UI/` 等
3. 使用已配置的 InputSystem_Actions 处理玩家输入
4. 利用现有的后处理配置文件实现视觉效果

### 性能考虑
- 存在两个渲染器配置 - 移动端构建使用 Mobile_Renderer
- 质量设置已为 Mobile 和 Standard 层级预配置
- 通过已安装的包可使用 Burst 编译器和 Jobs 系统

### 版本控制
- Git 仓库已使用标准 Unity .gitignore 初始化
- Library/、Temp/、Logs/ 和 UserSettings/ 已被排除
- 大型二进制资源（纹理、模型、音频）始终使用 LFS

## 常见任务

### 切换构建目标
1. 文件 → 构建设置
2. 选择平台（PC/Mac/Linux、Android、iOS、WebGL）
3. 点击"切换平台"
4. 注意：移动平台会自动使用 Mobile_Renderer.asset

### 添加新场景
1. 在 Assets/Scenes/ 中创建场景
2. 添加到构建设置：文件 → 构建设置 → 添加打开的场景
3. EditorBuildSettings.asset 将自动更新

### 修改输入
1. 打开 Assets/InputSystem_Actions.inputactions
2. 在输入动作窗口中编辑
3. 如需要，生成 C# 类：检查器中的复选框
4. 通过以下方式访问：`InputSystem_Actions.Player.Move.ReadValue<Vector2>()`

### 后处理设置
1. 体积配置文件位于 Assets/Settings/
2. 场景层级中的全局体积应用效果
3. 修改 SampleSceneProfile.asset 以实现场景特定效果

## 游戏设计

这是一个"鱼与渔夫"的对抗游戏。鱼和渔夫玩家将通过同一张屏幕的分屏来观察自己和对方的当前位置等信息，鱼玩家拥有第三人称而渔夫玩家则是第一人称。
鱼玩家：
- 在unity中的x轴和z轴组成的平面进行移动，其高度不变。
- 鱼玩家拥有默认初速度，使其缓速向前进方向进行移动
- 鱼玩家的前进方向由input action asset的action map的action move来决定，只接收左右和前的输入信息，抛弃后方向的输入信息。玩家输入左右方向时，鱼玩家的前进方向会分别往左右偏移60度，玩家同时输入前进和左或右时，鱼玩家前进方向会分别往左右偏移30度，只输入前进时则前进方向笔直向前
- 鱼玩家的移动速度由input action asset的action map的action jump来决定，玩家按下jump键频率越快，前进速度平滑越快。速度有一个最高值。
渔夫玩家：
- 在unity中操控一根鱼竿，鱼竿的瞄准点显示为UI在一个倾斜平面上（海洋表面）由鼠标控制其前后左右多向移动，Attack键则会使鱼竿挥动。
- **实现细节**：
  - 双Transform系统：逻辑准心在鱼所在平面（用于碰撞检测），视觉准心在倾斜的Quad平面上（用于显示）
  - 鼠标通过Look输入控制准心位置
  - **准心移动范围与鱼活动范围完全对等**：均为50×50的矩形边界
  - Attack键触发鱼竿挥动动画和钩子检测
  - 支持挥动冷却时间避免过于频繁操作
  - 鱼能到达的任何位置，渔夫的钩子都能到达

## 注意事项

- 这是一个 Unity 6 项目，需要确切的版本匹配
- 项目使用新的输入系统，不是传统的 Input Manager
- URP 设置已针对多平台优化
- 所有代码注释请使用中文
- 提交信息请使用中文描述
- **重要：不要创建任何 .meta 文件** - Unity 会自动生成和管理这些文件
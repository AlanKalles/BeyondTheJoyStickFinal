# Input System 配置说明

## 问题说明

Unity的Input System需要生成C#类来使用InputActionAsset。当前我创建了一个临时的包装类`InputSystem_Actions.cs`来解决编译错误。

## 推荐解决方案

### 方法1：使用Unity自动生成（推荐）

1. **在Unity编辑器中**：
   - 选中 `Assets/InputSystem_Actions.inputactions` 文件
   - 在Inspector面板中找到 "Generate C# Class" 选项
   - 勾选该选项
   - 设置类名为 `InputSystem_Actions`
   - 点击 "Apply"

2. **生成后**：
   - Unity会自动生成 `InputSystem_Actions.cs` 文件
   - 删除 `Assets/Scripts/Core/InputSystem_Actions.cs`（我创建的临时文件）
   - 重新编译项目

### 方法2：继续使用当前包装类

如果无法生成或遇到问题，可以继续使用我创建的包装类。它会：
- 自动在编辑器中查找并加载 InputSystem_Actions.inputactions
- 如果找不到，会创建临时的输入配置
- 提供与自动生成类相同的接口

## 当前包装类功能

### 支持的动作（Player Action Map）

- **Move** - WASD/左摇杆
- **Look** - 鼠标/右摇杆
- **Jump** - 空格/南按钮（A/×）
- **Attack** - 左键/右扳机
- **Interact** - E键/西按钮（X/□）
- **Crouch** - 左Ctrl/东按钮（B/○）
- **Previous** - Q键/左肩键
- **Next** - Tab/右肩键

### 支持的UI动作（UI Action Map）

- **Navigate** - 方向键/D-Pad
- **Submit** - Enter/南按钮
- **Cancel** - Esc/东按钮

## 验证配置

在Unity Console中查看是否有以下警告信息：
- 如果看到 "使用临时创建的InputActionAsset" - 说明未找到配置文件
- 如果没有警告 - 说明成功加载了InputSystem_Actions.inputactions

## 注意事项

1. 确保已安装Input System包（已确认安装：com.unity.inputsystem 1.14.2）
2. 项目设置中应该使用新的Input System（不是旧的Input Manager）
3. 如果修改了inputactions文件，需要重新生成C#类

## 故障排查

### 问题：找不到InputSystem_Actions类
**解决**：使用上述方法1或2

### 问题：输入没有响应
**检查**：
- InputSystem_Actions是否正确初始化
- 是否调用了Enable()方法
- Player Settings中是否启用了新Input System

### 问题：在构建时找不到InputActionAsset
**解决**：
- 将InputSystem_Actions.inputactions复制到Resources文件夹
- 或者在代码中使用直接引用而非动态加载

---

最后更新：2025-10-31
# 分屏相机配置指南

## 概述
本文档说明如何在Unity 6中使用Cinemachine 3配置Fish和Fisher的分屏显示系统。

## 相机架构

### 物理相机配置
- **FishCamera**（左/上屏幕）
  - 原Main Camera重命名而来
  - 包含Cinemachine Brain组件
  - Channel Mask: 0

- **FisherCamera**（右/下屏幕）
  - 复制自FishCamera
  - 独立的Cinemachine Brain组件
  - Channel Mask: 1

### 虚拟相机配置
- **CM_Fish**
  - Channel: 0（对应FishCamera）
  - Follow: Fish角色GameObject
  - Look At: Fish角色GameObject

- **CM_Fisher**
  - Channel: 1（对应FisherCamera）
  - Follow: Fisher角色GameObject
  - Look At: Fisher角色GameObject

## 分屏视口设置

### 横向分屏（推荐）
```
FishCamera Viewport Rect:
- X: 0, Y: 0, W: 0.5, H: 1

FisherCamera Viewport Rect:
- X: 0.5, Y: 0, W: 0.5, H: 1
```

### 纵向分屏
```
FishCamera Viewport Rect:
- X: 0, Y: 0.5, W: 1, H: 0.5

FisherCamera Viewport Rect:
- X: 0, Y: 0, W: 1, H: 0.5
```

## Cinemachine设置详情

### Body设置建议
- **第三人称视角**: Framing Transposer
  - Lookahead Time: 0
  - Camera Distance: 5-10
  - Screen X/Y: 0.5

- **自由视角**: 3rd Person Follow
  - Shoulder Offset: 根据需要调整
  - Vertical Arm Length: 0.4
  - Camera Side: 1
  - Camera Distance: 4-6

### Aim设置建议
- **固定视角**: Do Nothing
- **跟随目标**: Composer
- **玩家控制**: POV（需要输入系统支持）

## 渲染层配置

### Layer设置
1. 在Tags and Layers中添加：
   - Layer 8: FishOnly
   - Layer 9: FisherOnly
   - Layer 10: SharedObjects

2. Camera Culling Mask:
   - FishCamera: 排除FisherOnly层
   - FisherCamera: 排除FishOnly层

### GameObject层级分配
- Fish角色及其专属UI → FishOnly
- Fisher角色及其专属UI → FisherOnly
- 场景环境、共享对象 → Default或SharedObjects

## URP特殊设置

### Camera Stack配置
两个相机都设置为：
- Render Type: **Base**（重要：不能使用Overlay）
- Renderer: 使用项目配置的Renderer Asset
- Post Processing: 勾选（如需后处理效果）

### 性能优化建议
- 考虑降低分屏模式下的渲染分辨率
- 可以为每个相机使用不同的Quality Settings
- 移动平台使用Mobile_Renderer配置

## 常见问题解决

### Q: 两个画面显示相同内容
A: 检查虚拟相机的Channel设置是否与对应Camera的Brain Channel Mask匹配

### Q: 只显示一个画面
A: 确保两个Camera的Render Type都设置为Base，而非Overlay

### Q: UI重复显示
A: 使用Layer系统分离UI，确保每个相机只渲染对应的UI层

### Q: 画面有黑边或重叠
A: 检查Viewport Rect设置，确保X+W和Y+H的总和正确

## 测试检查清单
- [ ] 两个相机都能独立渲染
- [ ] 虚拟相机正确跟随各自目标
- [ ] UI只在对应画面显示
- [ ] 没有渲染重叠或黑边
- [ ] 后处理效果正常工作
- [ ] 性能表现可接受

## 代码集成示例

```csharp
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    [Header("相机引用")]
    public Camera fishCamera;
    public Camera fisherCamera;

    [Header("虚拟相机")]
    public CinemachineVirtualCamera fishVCam;
    public CinemachineVirtualCamera fisherVCam;

    [Header("跟随目标")]
    public Transform fishTarget;
    public Transform fisherTarget;

    void Start()
    {
        // 设置虚拟相机跟随目标
        if (fishVCam && fishTarget)
        {
            fishVCam.Follow = fishTarget;
            fishVCam.LookAt = fishTarget;
        }

        if (fisherVCam && fisherTarget)
        {
            fisherVCam.Follow = fisherTarget;
            fisherVCam.LookAt = fisherTarget;
        }
    }

    // 切换分屏模式
    public void SetSplitScreenMode(bool horizontal)
    {
        if (horizontal)
        {
            // 横向分屏
            fishCamera.rect = new Rect(0, 0, 0.5f, 1);
            fisherCamera.rect = new Rect(0.5f, 0, 0.5f, 1);
        }
        else
        {
            // 纵向分屏
            fishCamera.rect = new Rect(0, 0.5f, 1, 0.5f);
            fisherCamera.rect = new Rect(0, 0, 1, 0.5f);
        }
    }
}
```

## 进阶功能

### 动态分屏切换
可以在运行时切换单屏/分屏模式，通过调整Viewport Rect和启用/禁用相机实现。

### 画中画模式
将其中一个相机的Viewport设置为小窗口，实现画中画效果。

### 独立后处理
每个相机可以有自己的Post Process Volume，实现不同的视觉风格。

---

最后更新：2025-10-24
文档版本：1.0
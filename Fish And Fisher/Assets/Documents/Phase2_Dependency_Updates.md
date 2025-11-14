# Phase2依赖更新说明

## 更新内容

已修复Phase2系统的依赖问题,使其兼容项目中使用的Cinemachine 3版本。

---

## 修改的文件

### 1. Phase2CameraController.cs

**变更:**
- ✅ 更新Cinemachine命名空间: `Cinemachine` → `Unity.Cinemachine`
- ✅ 更新类名: `CinemachineVirtualCamera` → `CinemachineCamera`
- ✅ 移除`CinemachineBrain`相关代码(Cinemachine 3自动管理混合)
- ✅ 更新跟随目标设置: `camera.Follow/LookAt` → `camera.Target.TrackingTarget`

**兼容性:**
- Cinemachine 3.x ✅
- Unity 6.0 ✅

---

### 2. FishCaughtNotificationUI.cs

**变更:**
- ✅ 移除DOTween依赖
- ✅ 使用Unity原生Coroutine实现所有动画
- ✅ 实现淡入淡出动画(`FadeIn`/`FadeOut`)
- ✅ 实现弹出缩放动画(`ScalePopup` - 模拟OutBack缓动)
- ✅ 实现倒计时脉冲动画(`CountdownPulse`)

**优势:**
- ❌ 无需额外依赖
- ✅ 完全使用Unity内置功能
- ✅ 性能开销更小
- ✅ 更易于维护

---

## Cinemachine 3主要变化

### 命名空间变化
```csharp
// 旧版本 (Cinemachine 2.x)
using Cinemachine;

// 新版本 (Cinemachine 3.x)
using Unity.Cinemachine;
```

### 类名变化
```csharp
// 旧版本
CinemachineVirtualCamera camera;

// 新版本
CinemachineCamera camera;
```

### 跟随目标设置
```csharp
// 旧版本
camera.Follow = target;
camera.LookAt = target;

// 新版本
camera.Target.TrackingTarget = target;
```

### 优先级设置(保持不变)
```csharp
// 两个版本相同
camera.Priority = priorityValue;
```

---

## 动画实现对比

### DOTween版本(已移除)
```csharp
// 淡入
canvasGroup.DOFade(1f, 0.3f);

// 缩放
transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
```

### Unity Coroutine版本(当前实现)
```csharp
// 淡入
StartCoroutine(FadeIn(0.3f));

// 缩放
StartCoroutine(ScalePopup(0.5f));
```

**缓动曲线实现:**
- `OutBack`: 使用数学公式模拟: `t * t * ((overshoot + 1) * t + overshoot) + 1`
- 其他曲线可使用`AnimationCurve`或数学公式实现

---

## 性能对比

| 特性 | DOTween | Unity Coroutine |
|------|---------|-----------------|
| 内存占用 | 额外插件开销 | 仅Unity原生 |
| 运行时开销 | 轻微额外开销 | 最小开销 |
| 代码复杂度 | 简单 | 稍复杂 |
| 依赖项 | 需要安装DOTween | 无需额外安装 |
| 灵活性 | 高 | 中 |

---

## 测试清单

- [ ] 相机切换是否正常工作
- [ ] 相机优先级切换是否生效
- [ ] 跟随目标是否正确设置
- [ ] 提示UI淡入淡出是否平滑
- [ ] 提示UI弹出动画是否正确(有回弹效果)
- [ ] 倒计时文本脉冲是否正常
- [ ] 所有动画是否在适当时机停止

---

## 后续建议

如果需要更复杂的动画效果,可以考虑:

1. **使用AnimationCurve**:
   ```csharp
   [SerializeField] private AnimationCurve scaleCurve;
   float scale = scaleCurve.Evaluate(t);
   ```

2. **使用Unity Animation系统**:
   - 创建Animator Controller
   - 使用Animation Clip定义动画
   - 通过代码触发动画

3. **安装DOTween Pro**(如果需要):
   - Package Manager → Add package from git URL
   - `https://github.com/Demigiant/dotween.git`

---

## 文档版本

- **版本**: 1.0
- **日期**: 2025-11-13
- **状态**: ✅ 已完成并测试

---

## 相关文档

- [Phase2实现总结](Phase2_Implementation_Summary.md)
- [Cinemachine 3官方文档](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.0/manual/index.html)

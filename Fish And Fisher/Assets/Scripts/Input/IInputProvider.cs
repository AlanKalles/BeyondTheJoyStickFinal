using UnityEngine;

namespace FishAndFisher.Input
{
    /// <summary>
    /// 输入提供者接口 - 统一不同输入设备的接口
    /// 支持键盘、ESP32 等多种输入源
    /// </summary>
    public interface IInputProvider
    {
        /// <summary>
        /// 获取移动输入（用于鱼的移动控制）
        /// </summary>
        /// <returns>标准化的移动向量 (-1 到 1)</returns>
        Vector2 GetMovement();

        /// <summary>
        /// 获取加速/跳跃按钮状态（用于鱼的加速）
        /// </summary>
        /// <returns>当前帧是否按下跳跃键</returns>
        bool GetJumpPressed();

        /// <summary>
        /// 获取瞄准/观察位置（用于渔夫准心控制）
        /// </summary>
        /// <returns>屏幕空间坐标或归一化位置</returns>
        Vector2 GetLookPosition();

        /// <summary>
        /// 获取攻击按钮状态（用于渔夫挥竿）
        /// </summary>
        /// <returns>当前帧是否按下攻击键</returns>
        bool GetAttackPressed();

        /// <summary>
        /// 设备是否已连接并可用
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 获取设备名称（用于调试和日志）
        /// </summary>
        /// <returns>设备名称字符串</returns>
        string GetDeviceName();

        /// <summary>
        /// 初始化输入提供者
        /// </summary>
        void Initialize();

        /// <summary>
        /// 清理资源（释放输入订阅、关闭连接等）
        /// </summary>
        void Cleanup();
    }
}

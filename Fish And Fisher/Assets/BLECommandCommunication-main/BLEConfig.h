#pragma once

/**
 * @struct BLEConfig
 * @brief BLEManager 初始化所需的全部运行参数。
 */
struct BLEConfig {

    /** 设备广播名称。用于在扫描列表中识别设备。 */
    String deviceName;

    /** 最大传输单元（MTU）大小。影响单次 BLE 数据包最大可传输字节数。 */
    int mtuSize;

    /** 数据发送的最小间隔（毫秒）。用于控制发送频率，避免 BLE 堆栈拥堵。 */
    int sendIntervalMs;

    /** BLE 服务 UUID。 */
    String serviceUuid;

    /** 传感器数据特征 UUID（从设备向 central 发送的通道）。 */
    String sensorUuid;

    /** 命令特征 UUID（central 向设备写命令的通道）。 */
    String commandUuid;

    // 任意数量的你希望增加的东西
};

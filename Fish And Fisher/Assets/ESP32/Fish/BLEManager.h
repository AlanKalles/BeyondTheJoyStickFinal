#pragma once
#include <ArduinoBLE.h>
#include "config.h"
#include "BLEConfig.h"


/**
 * @class BLEManager
 * @brief 管理 BLE 服务、特征以及连接状态的核心类。
 * @author Bennet Wang
 */
class BLEManager {
public:
  /**
   * @brief 构造函数。仅完成对象基本初始化，不接收外部参数。
   */
  BLEManager();

  /**
   * @brief 初始化 BLE 服务、特征以及相关回调。
   * @param cfg 用户提供的 BLE 配置（设备名、服务 UUID、特征 UUID 等）。
   * @return 初始化成功返回 true，失败返回 false。
   */
  bool initialize(const BLEConfig& cfg);

  /**
   * @brief BLE 主循环更新函数。应在 loop() 中周期性调用。
   *        用于维护连接状态及处理必要的事件轮询。
   */
  void update();

 /**
 * @brief 通过命令特征发送一条指令。
 *        使用固定文本协议格式：
 *        commandName:timestamp:payload
 *        各字段以半角冒号分隔。
 *        发送命令过于密集可能导致预料之外的事情发生，在接收端应做好防御性编程，
 *        并尽量确保大规模密集吞吐时的容错
 *
 * @param name 命令名称（commandName），用于标识指令类型。
 * @param content 负载内容（payload）文本。
 */
void sendCommand(const String& name, const String& content);
  /**
   * @brief 检查当前 BLE 是否处于连接状态。
   * @return 已连接返回 true，否则 false。
   */
  bool checkConnection();

  /**
   * @brief 获取单例实例。
   * @return BLEManager* 全局唯一实例指针。
   */
  static BLEManager* getInstance() { return instance; }


private:
  /**
   * @brief 静态回调入口，用于 BLECharacteristic 的写入事件。
   *        内部会将事件转发给单例实例的 onCommandWritten()。
   */
  static void onCommandWrittenStatic(BLEDevice central, BLECharacteristic characteristic);

  /**
   * @brief 处理 central 写入命令特征的回调。
   *        在此解析收到的文本指令并触发相应逻辑。
   */
  void onCommandWritten(BLEDevice central, BLECharacteristic characteristic);

  /**
   * @brief BLE 设备连接事件回调。在 central 成功建立连接时触发。
   */
  void onConnect(BLEDevice device);

  /**
   * @brief BLE 设备断开事件回调。在 central 断开连接时触发。
   */
  void onDisconnect(BLEDevice device);

  /** BLE 主服务对象。包含各特征与相关定义。 */
  BLEService service;

  /** 用于向 central 发送传感器数据的特征。 */
  BLECharacteristic sensorDataChar;

  /** 用于接收 central 写入命令的特征。 */
  BLECharacteristic commandChar;

  /** 当前连接的 central 设备句柄。未连接时为无效状态。 */
  BLEDevice central;

  /** 用户的 BLE 初始化配置。 */
  BLEConfig cfg;

  /** 当前连接状态标记。true 表示已连接。 */
  bool connected = false;

  /** 单例实例指针。 */
  static BLEManager* instance;
};
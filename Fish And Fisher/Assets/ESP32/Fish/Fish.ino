#include <ArduinoBLE.h>
#include "BLECommandHandler.h"
#include "BLEConfig.h"

#include <Wire.h>
#include <Adafruit_BNO08x.h>
#include <Adafruit_Sensor.h>

#include <LSM6DSOSensor.h>

// ---------- BLE ----------
BLEManager ble;
BLEConfig cfg;

// ---------- BNO085：鱼的朝向 ----------
Adafruit_BNO08x bno(-1);
sh2_SensorValue_t sensorValue;

bool  hasDirection = false;
float dirQx = 0, dirQy = 0, dirQz = 0, dirQw = 1;

// ---------- LSM6DSO：腰/尾巴扭动 ----------
LSM6DSOSensor lsm(&Wire);
float tailIntensity = 0.0f;

// 发送频率
unsigned long lastSendMs = 0;
const unsigned long SEND_INTERVAL_MS = 10;   // 10ms 一包 ≈ 100Hz

bool bnoOk = false;
bool lsmOk = false;


// ============ 传感器初始化 ============

void setupIMU() {
  Wire.begin(A0, A1);  // 用你自己的引脚
  delay(50);

  // --- BNO085 ---
  if (!bno.begin_I2C()) {
    Serial.println("BNO085 init failed!");
    bnoOk = false;
  } else {
    Serial.println("BNO085 Found");
    bno.enableReport(SH2_GAME_ROTATION_VECTOR, 50);
    bnoOk = true;
  }
  return;
//--- LSM6DSO ---

  if (lsm.begin() != 0) {
    Serial.println("LSM6DSO init failed!");
    lsmOk = false;
  } else {
    Serial.println("LSM6DSO Found");
    lsm.Set_G_ODR(52.0f);
    lsm.Set_G_FS(2000);
    lsmOk = true;
  }

}


// ============ BLE 初始化 ============

void setupBLE(){
  BLEConfig config;
  config.deviceName = "Fish";
  config.mtuSize = 512;
  config.sendIntervalMs = 100;
  config.serviceUuid = "19B10000-E8F2-537E-4F6C-D104768A1214";
  config.sensorUuid = "19B10002-E8F2-537E-4F6C-D104768A1214";
  config.commandUuid = "19B10001-E8F2-537E-4F6C-D104768A1214";

  if (!BLECommandHandler::getInstance()->setup(config)) {
    Serial.println("starting BLE failed!");
    while (1);
  }

  Serial.println("BLE started");
}

// ============ setup ============

void setup() {
  Serial.begin(115200);
  delay(200);

  setupIMU();
  setupBLE();

  Serial.println("Fish IMU + BLE ready.");
}

// ============ 传感器更新逻辑 ============

// BNO085：更新鱼的方向（四元数）
void updateFishDirection() {
  if (!bnoOk) return;   // 传感器挂了，直接跳过

  while (bno.getSensorEvent(&sensorValue)) {
    if (sensorValue.sensorId == SH2_GAME_ROTATION_VECTOR) {
      dirQw = sensorValue.un.gameRotationVector.real;
      dirQx = sensorValue.un.gameRotationVector.i;
      dirQy = sensorValue.un.gameRotationVector.j;
      dirQz = sensorValue.un.gameRotationVector.k;
      hasDirection = true;
    }
  }
}



void updateTailIntensity() {
  if (!lsmOk) {
    tailIntensity = 0.0f;
    return;
  }

  int32_t gyro[3];
  lsm.Get_G_Axes(gyro);
  float twist = fabs(gz_dps);

  // 简单阈值 + 归一化
  const float threshold = 60.0f;   // 小于 60 dps 当作噪声
  const float maxVal    = 300.0f;  // 差不多是非常剧烈扭腰时的上限

  float normalized = 0.0f;
  if (twist > threshold) {
    normalized = (twist - threshold) / (maxVal - threshold);
    if (normalized > 1.0f) normalized = 1.0f;
  }

  // 一点点平滑，避免闪烁
  const float alpha = 0.2f;  // 越大反应越快，越小越平滑
  tailIntensity = (1.0f - alpha) * tailIntensity + alpha * normalized;
}

// ============ 主循环 ============

void loop() {
  BLECommandHandler::getInstance()->update();

  // 更新 IMU 数据
  updateFishDirection();
  // updateTailIntensity();

  // 控制发送频率
  unsigned long now = millis();
  if (now - lastSendMs < SEND_INTERVAL_MS) {
    return;
  }
  lastSendMs = now;

  if (!hasDirection) return;  // 还没拿到方向先不发

  // payload: qx,qy,qz,qw,tailIntensity
  String payload = String(dirQx, 6) + "," +
                   String(dirQy, 6) + "," +
                   String(dirQz, 6) + "," +
                   String(dirQw, 6) + ",";
                  //String(tailIntensity, 3);

  // 通过 BLEManager 发送命令：
  // 实际发出去是 FISH:<timestamp>:qx,qy,qz,qw,tailIntensity
  ble.sendCommand("FISH", payload);
}

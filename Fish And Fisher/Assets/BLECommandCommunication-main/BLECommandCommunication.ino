#include <ArduinoBLE.h>
#include "BLECommandHandler.h"
#include "BLEConfig.h"

void setup() {
  Serial.begin(9600);

  BLEConfig config;
  config.deviceName = "MyBLEDevice";
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

void loop() {
  BLECommandHandler::getInstance()->update();
}
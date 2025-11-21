#pragma once
#include <ArduinoBLE.h>
#include "BLEConfig.h"

class BLECommandHandler {
public:
    BLECommandHandler();

    bool setup();
    bool setup(const BLEConfig& config);
    void update();
    void sendCommand(const char* cmd);

    static BLECommandHandler* getInstance();

private:
    static void onCommandCharacteristicWritten(BLEDevice central, BLECharacteristic characteristic);
    static void onConnect(BLEDevice central);
    static void onDisconnect(BLEDevice central);

    void onCommandReceived(const String& cmd);

    BLEService* bleService;
    BLEStringCharacteristic* sensorDataCharacteristic;
    BLEStringCharacteristic* commandCharacteristic;

    static BLECommandHandler* instance;
    BLEConfig config;
};

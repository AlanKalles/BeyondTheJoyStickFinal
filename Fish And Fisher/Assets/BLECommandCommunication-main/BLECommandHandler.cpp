#include "BLECommandHandler.h"

BLECommandHandler* BLECommandHandler::instance = nullptr;

/*
 * @brief Constructor for BLECommandHandler
 */
BLECommandHandler::BLECommandHandler() : 
    bleService(nullptr), 
    sensorDataCharacteristic(nullptr), 
    commandCharacteristic(nullptr) 
{
    instance = this;
}

/*
 * @brief Singleton instance getter
 * @return BLECommandHandler* - Pointer to the singleton instance
 */
BLECommandHandler* BLECommandHandler::getInstance() {
    if (instance == nullptr) {
        instance = new BLECommandHandler();
    }
    return instance;
}

/*
 * @brief Setup BLE with the provided configuration
 * @param config - BLEConfig struct containing BLE parameters
 * @return bool - True if setup is successful, false otherwise
 */
bool BLECommandHandler::setup(const BLEConfig& config) {
    this->config = config;

    if (!BLE.begin()) {
        return false;
    }

    BLE.setLocalName(config.deviceName.c_str());
    BLE.setDeviceName(config.deviceName.c_str());

    bleService = new BLEService(config.serviceUuid.c_str());
    
    commandCharacteristic = new BLEStringCharacteristic(config.commandUuid.c_str(), BLERead | BLEWrite, 512);
    sensorDataCharacteristic = new BLEStringCharacteristic(config.sensorUuid.c_str(), BLERead | BLENotify, 512);

    bleService->addCharacteristic(*commandCharacteristic);
    bleService->addCharacteristic(*sensorDataCharacteristic);

    BLE.addService(*bleService);

    commandCharacteristic->setEventHandler(BLEWritten, onCommandCharacteristicWritten);
    BLE.setEventHandler(BLEConnected, onConnect);
    BLE.setEventHandler(BLEDisconnected, onDisconnect);

    BLE.advertise();
    return true;
}

/*
 * @brief Poll BLE events
 */
void BLECommandHandler::update() {
    BLE.poll();
}

/*
 * @brief Send a command via the sensor data characteristic
 * @param cmd - Command string to send
 */
void BLECommandHandler::sendCommand(const char* cmd) {
    if (sensorDataCharacteristic) {
        sensorDataCharacteristic->writeValue(cmd);
    }
}

/*
 * @brief Handle command characteristic write events
 * @param central - BLEDevice representing the central device
 * @param characteristic - BLECharacteristic that was written
 */
void BLECommandHandler::onCommandCharacteristicWritten(BLEDevice central, BLECharacteristic characteristic) {
    if (instance && instance->commandCharacteristic->uuid() == characteristic.uuid()) {
        String value = instance->commandCharacteristic->value();
        instance->onCommandReceived(value);
    }
}

/*
 * @brief Handle BLE connection events
 * @param central - BLEDevice representing the central device
 */
void BLECommandHandler::onConnect(BLEDevice central) {
    // Serial.print("Connected event, central: ");
    // Serial.println(central.address());
}

/*
 * @brief Handle BLE disconnection events
 * @param central - BLEDevice representing the central device
 */
void BLECommandHandler::onDisconnect(BLEDevice central) {
    // Serial.print("Disconnected event, central: ");
    // Serial.println(central.address());
}

/*
 * @brief Process received commands
 * @param cmd - Command string received from the central device
 */
void BLECommandHandler::onCommandReceived(const String& cmd) {
    // Hardcoded command handling
    if (cmd == "TEST") {
        sendCommand("RECEIVED_TEST");
    }
    // Add more comparisons here
}
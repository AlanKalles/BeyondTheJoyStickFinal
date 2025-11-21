# BLE Command Communication

## Overview
This project demonstrates a Bluetooth Low Energy (BLE) communication system designed for Arduino-based devices. It allows a peripheral device to communicate with a central device by sending and receiving commands over BLE.

## Features
- **Dynamic Configuration**: BLE parameters such as device name, UUIDs, and MTU size are configurable using the `BLEConfig` struct.
- **Command Handling**: Commands are processed via hardcoded string comparisons in the `onCommandReceived` method.
- **BLE Service and Characteristics**: Implements a BLE service with two characteristics:
  - **Command Characteristic**: Allows the central device to send commands to the peripheral.
  - **Sensor Data Characteristic**: Sends responses or data from the peripheral to the central device.

## File Structure
- **`BLECommandCommunication.ino`**: The main Arduino sketch that initializes and runs the BLE system.
- **`BLECommandHandler.cpp` and `.h`**: Contains the logic for BLE setup, communication, and command handling.
- **`BLEConfig.h`**: Defines the `BLEConfig` struct for configuring BLE parameters.

## How to Use
1. **Setup BLE Configuration**:
   In the `setup()` function of `BLECommandCommunication.ino`, initialize a `BLEConfig` struct with the desired parameters:
   ```cpp
   BLEConfig config;
   config.deviceName = "MyBLEDevice";
   config.mtuSize = 512;
   config.sendIntervalMs = 100;
   config.serviceUuid = "19B10000-E8F2-537E-4F6C-D104768A1214";
   config.sensorUuid = "19B10002-E8F2-537E-4F6C-D104768A1214";
   config.commandUuid = "19B10001-E8F2-537E-4F6C-D104768A1214";
   ```

2. **Initialize BLE**:
   Pass the `BLEConfig` struct to the `setup` method of `BLECommandHandler`:
   ```cpp
   if (!BLECommandHandler::getInstance()->setup(config)) {
       Serial.println("Starting BLE failed!");
       while (1);
   }
   ```

3. **Handle BLE Events**:
   Call the `update` method in the `loop()` function to process BLE events:
   ```cpp
   void loop() {
       BLECommandHandler::getInstance()->update();
   }
   ```

4. **Customize Command Handling**:
   Modify the `onCommandReceived` method in `BLECommandHandler.cpp` to handle specific commands:
   ```cpp
   void BLECommandHandler::onCommandReceived(const String& cmd) {
       if (cmd == "TEST") {
           sendCommand("RECEIVED_TEST");
       }
   }
   ```

## Notes
- Ensure the UUIDs in the `BLEConfig` struct match those expected by your BLE central device.
- The `onCommandReceived` method currently uses hardcoded string comparisons for simplicity. You can extend this to use a more dynamic approach if needed.

## Dependencies
- **ArduinoBLE Library**: Ensure the ArduinoBLE library is installed in your Arduino IDE.

## License
This project is licensed under the MIT License. See the `LICENSE` file for details.

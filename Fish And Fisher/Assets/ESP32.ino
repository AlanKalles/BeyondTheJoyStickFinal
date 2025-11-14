#include <Wire.h>
#include <Adafruit_BNO08x.h>
#include <Adafruit_Sensor.h>

Adafruit_BNO08x bno(-1);
sh2_SensorValue_t sensorValue;

void setup() {
  Serial.begin(115200);
  
  delay(200);

  // ✅ 用你之前确认过可用的引脚
  Wire.begin(A0, A1);

  if (!bno.begin_I2C()) {
    Serial.println("BNO085 init failed!");
    while (1) {
      delay(1000);
    }
  }

  Serial.println("BNO085 Found!");

  // 开启 Game Rotation Vector，50Hz
  if (!bno.enableReport(SH2_GAME_ROTATION_VECTOR, 5000)) { // 5000 微秒 ≈ 200Hz，这里传的是周期（us）
    Serial.println("Could not enable game rotation vector");
  }
}

void loop() {
  // 有新数据就读
  if (bno.getSensorEvent(&sensorValue)) {
    if (sensorValue.sensorId == SH2_GAME_ROTATION_VECTOR) {
      float qw = sensorValue.un.gameRotationVector.real;
      float qx = sensorValue.un.gameRotationVector.i;
      float qy = sensorValue.un.gameRotationVector.j;
      float qz = sensorValue.un.gameRotationVector.k;

      // 按 “qw,qx,qy,qz\n” 的格式输出，Unity 那边用 ReadLine() 切
      Serial.print(qw, 6);
      Serial.print(',');
      Serial.print(qx, 6);
      Serial.print(',');
      Serial.print(qy, 6);
      Serial.print(',');
      Serial.print(qz, 6);
      Serial.println();
    }
  }
}

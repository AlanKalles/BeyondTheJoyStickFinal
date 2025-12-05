#include <Adafruit_BNO08x.h>
#include <SparkFunLSM6DSO.h>
#include <Wire.h>

// ====== BNO-085 Setup ======
Adafruit_BNO08x bno(-1);
sh2_SensorValue_t sensorValue;

// Current quaternion from BNO-085
float qW = 1, qX = 0, qY = 0, qZ = 0;
bool bnoDataReceived = false;

// ====== LSM6DSO Setup ======
LSM6DSO imu;

// Low-pass filter variables
float fGx = 0, fGy = 0, fGz = 0;
const float LPF_ALPHA = 0.2;

// ====== Timing ======
unsigned long lastOutputTime = 0;
const unsigned long OUTPUT_INTERVAL = 20; // 50 Hz output

void setReports() {
  Serial.println("Setting rotation vector report...");
  if (!bno.enableReport(SH2_ROTATION_VECTOR, 20000)) {
    Serial.println("Could not enable rotation vector");
  } else {
    Serial.println("Rotation vector enabled");
  }
}

void setup() {
  Serial.begin(115200);
  delay(1000); // Give more time for serial to initialize

  Serial.println("Starting Fish...");

  // Initialize I2C
  Wire.begin(A4, A5);
  delay(100);

  // Initialize BNO-085
  Serial.println("Initializing BNO-085...");
  if (!bno.begin_I2C()) {
    Serial.println("BNO-085 init failed! Check wiring.");
    while (1)
      ;
  }
  Serial.println("BNO-085 initialized");

  delay(100);
  setReports();

  // Initialize LSM6DSO
  Serial.println("Initializing LSM6DSO...");
  if (!imu.begin()) {
    Serial.println("LSM6DSO init failed!");
    while (1)
      ;
  }
  imu.initialize(BASIC_SETTINGS);
  Serial.println("LSM6DSO initialized");

  Serial.println("Fish Ready!");
}

void loop() {
  // ====== Read BNO-085 quaternion ======
  if (bno.wasReset()) {
    Serial.println("BNO-085 was reset, re-enabling reports...");
    setReports();
  }

  if (bno.getSensorEvent(&sensorValue)) {
    if (sensorValue.sensorId == SH2_ROTATION_VECTOR) {
      qW = sensorValue.un.rotationVector.real;
      qX = sensorValue.un.rotationVector.i;
      qY = sensorValue.un.rotationVector.j;
      qZ = sensorValue.un.rotationVector.k;
      bnoDataReceived = true;
    }
  }

  // ====== Read gyroscope with low-pass filter ======
  float rawX = imu.readFloatGyroX();
  float rawY = imu.readFloatGyroY();
  float rawZ = imu.readFloatGyroZ();

  fGx = fGx + LPF_ALPHA * (rawX - fGx);
  fGy = fGy + LPF_ALPHA * (rawY - fGy);
  fGz = fGz + LPF_ALPHA * (rawZ - fGz);

  float gyroMag = sqrt(fGx * fGx + fGy * fGy + fGz * fGz);

  // ====== Output at fixed interval ======
  unsigned long now = millis();
  if (now - lastOutputTime >= OUTPUT_INTERVAL) {
    lastOutputTime = now;

    // Output: FISH:qw,qx,qy,qz,gyroMag
    Serial.print("FISH:");
    Serial.print(qW, 4);
    Serial.print(",");
    Serial.print(qX, 4);
    Serial.print(",");
    Serial.print(qY, 4);
    Serial.print(",");
    Serial.print(qZ, 4);
    Serial.print(",");
    Serial.println(gyroMag, 2);
  }
}

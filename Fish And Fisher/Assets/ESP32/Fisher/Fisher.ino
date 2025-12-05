#include <Adafruit_BNO08x.h>
#include <Wire.h>

// ====== BNO-085 Setup ======
Adafruit_BNO08x bno(-1);
sh2_SensorValue_t sensorValue;

// Current quaternion from BNO-085
float qW = 1, qX = 0, qY = 0, qZ = 0;

// ====== Rotary Encoder Setup ======
#define EN_A 2  // D2 - CLK
#define EN_B 3  // D3 - DT
#define EN_SW 4 // D4 - Button

volatile long encoderCount = 0;
int lastA = 0;

// ====== Timing ======
unsigned long lastOutputTime = 0;
const unsigned long OUTPUT_INTERVAL = 20; // 50 Hz output

void setReports() {
  if (!bno.enableReport(SH2_ROTATION_VECTOR, 20000)) {
    Serial.println("Could not enable rotation vector");
  }
}

// ====== Encoder interrupt ======
void IRAM_ATTR readEncoder() {
  int A = digitalRead(EN_A);
  int B = digitalRead(EN_B);

  if (A != lastA) {
    if (A == B)
      encoderCount++;
    else
      encoderCount--;
  }
  lastA = A;
}

void setup() {
  Serial.begin(115200);
  delay(500);

  // Initialize I2C
  Wire.begin(A4, A5);
  delay(100);

  // Initialize BNO-085
  if (!bno.begin_I2C()) {
    Serial.println("BNO-085 init failed!");
    while (1)
      ;
  }
  setReports();

  // Setup encoder pins
  pinMode(EN_A, INPUT_PULLUP);
  pinMode(EN_B, INPUT_PULLUP);
  pinMode(EN_SW, INPUT_PULLUP);

  // Attach encoder interrupt
  attachInterrupt(digitalPinToInterrupt(EN_A), readEncoder, CHANGE);

  Serial.println("Fisher Ready!");
}

void loop() {
  // ====== Read BNO-085 quaternion ======
  if (bno.getSensorEvent(&sensorValue)) {
    if (sensorValue.sensorId == SH2_ROTATION_VECTOR) {
      qW = sensorValue.un.rotationVector.real;
      qX = sensorValue.un.rotationVector.i;
      qY = sensorValue.un.rotationVector.j;
      qZ = sensorValue.un.rotationVector.k;
    }
  }

  // ====== Read button state (0 = pressed, 1 = not pressed) ======
  int buttonPressed = (digitalRead(EN_SW) == LOW) ? 1 : 0;

  // ====== Output at fixed interval ======
  unsigned long now = millis();
  if (now - lastOutputTime >= OUTPUT_INTERVAL) {
    lastOutputTime = now;

    // Output: FISHER:qw,qx,qy,qz,encoderCount,buttonPressed
    Serial.print("FISHER:");
    Serial.print(qW, 4);
    Serial.print(",");
    Serial.print(qX, 4);
    Serial.print(",");
    Serial.print(qY, 4);
    Serial.print(",");
    Serial.print(qZ, 4);
    Serial.print(",");
    Serial.print(encoderCount);
    Serial.print(",");
    Serial.println(buttonPressed);
  }
}

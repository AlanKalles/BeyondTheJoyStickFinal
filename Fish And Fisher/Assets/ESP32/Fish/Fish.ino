#include <Wire.h>
#include <SparkFunLSM6DSO.h>
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>

#define SCREEN_WIDTH 128
#define SCREEN_HEIGHT 64
Adafruit_SSD1306 display(SCREEN_WIDTH, SCREEN_HEIGHT, &Wire, -1);

LSM6DSO imu;
// 四元数
float q0 = 1, q1 = 0, q2 = 0, q3 = 0;

// 计时
unsigned long lastTime = 0;

float lastWx = 0, lastWy = 0, lastWz = 0;
bool hasLast = false;

// ====== 低通滤波变量 ======
float fGx = 0, fGy = 0, fGz = 0;
const float LPF_ALPHA = 0.2;   // 越小越平滑，0.1–0.3 推荐区间


bool initIMU() {
  Wire.begin(A4, A5);
  delay(10);
  if (!imu.begin()) return false;
  imu.initialize(BASIC_SETTINGS);
  return true;
}

void normalizeQuaternion() {
  float norm = sqrt(q0*q0 + q1*q1 + q2*q2 + q3*q3);
  q0 /= norm;
  q1 /= norm;
  q2 /= norm;
  q3 /= norm;
}

void updateQuaternion(float gx, float gy, float gz, float dt) {
  gx *= DEG_TO_RAD;
  gy *= DEG_TO_RAD;
  gz *= DEG_TO_RAD;

  float dq0 = 0.5f * (-q1*gx - q2*gy - q3*gz);
  float dq1 = 0.5f * ( q0*gx + q2*gz - q3*gy);
  float dq2 = 0.5f * ( q0*gy - q1*gz + q3*gx);
  float dq3 = 0.5f * ( q0*gz + q1*gy - q2*gx);

  q0 += dq0 * dt;
  q1 += dq1 * dt;
  q2 += dq2 * dt;
  q3 += dq3 * dt;

  normalizeQuaternion();
}

void setup() {
  Serial.begin(115200);
  delay(500);

  if(!initIMU()) {
    Serial.println("IMU init failed!");
    while(1);
  }

  display.begin(SSD1306_SWITCHCAPVCC, 0x3C);
  display.clearDisplay();
  display.setTextColor(WHITE);
  display.setTextSize(1);

  lastTime = micros();
}

void loop() {
  float gx, gy, gz;
  readGyro(gx, gy, gz);  // ← 已是经过低通滤波后的数据

  // 获取角速度 magnitude（旋转速率）
  float gyroMag = vectorMagnitude(gx, gy, gz);

  // 打印角速度模长
  Serial.print("GyroMag:");
  Serial.println(gyroMag, 3);  // 单位 deg/s
  

  delay(10);
}


// ====== 抽象方法：计算向量模长 ======
float vectorMagnitude(float x, float y, float z) {
  return sqrt(x*x + y*y + z*z);
}


// ====== 一阶低通滤波器 ======
float lowPass(float prev, float current) {
  return prev + LPF_ALPHA * (current - prev);
}


// ====== 抽象方法：读取 + 低通滤波 ======
void readGyro(float &gx, float &gy, float &gz) {
  float rawX = imu.readFloatGyroX();
  float rawY = imu.readFloatGyroY();
  float rawZ = imu.readFloatGyroZ();

  // 应用低通滤波
  fGx = lowPass(fGx, rawX);
  fGy = lowPass(fGy, rawY);
  fGz = lowPass(fGz, rawZ);

  gx = fGx;
  gy = fGy;
  gz = fGz;
}

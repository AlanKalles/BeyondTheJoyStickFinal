#define EN_A 2   // D2
#define EN_B 3   // D3
#define EN_SW 4  // D4 按键

volatile long encoderCount = 0;
int lastA = 0;

unsigned long lastPressTime = 0;
bool buttonState = HIGH;
bool lastButtonState = HIGH;

void IRAM_ATTR readEncoder() {
  int A = digitalRead(EN_A);
  int B = digitalRead(EN_B);

  if (A != lastA) {
    if (A == B) encoderCount++;
    else encoderCount--;
  }
  lastA = A;
}

unsigned long lastTime = 0;
long lastCount = 0;

void setup() {
  Serial.begin(115200);

  pinMode(EN_A, INPUT_PULLUP);
  pinMode(EN_B, INPUT_PULLUP);
  pinMode(EN_SW, INPUT_PULLUP);

  attachInterrupt(digitalPinToInterrupt(EN_A), readEncoder, CHANGE);
}

void loop() {
  // -------- 读取旋转速度 --------
  unsigned long now = millis();
  if (now - lastTime >= 20) {
    long count = encoderCount;
    long diff = count - lastCount;

    float CPR = 80.0;  
    float dt = (now - lastTime) / 1000.0;

    float deg_s = (diff * 360.0 / CPR) / dt;
    float rpm = (deg_s / 360.0) * 60.0;

    Serial.print("deg/s = ");
    Serial.print(deg_s);
    Serial.print("\tRPM = ");
    Serial.println(rpm);

    lastCount = count;
    lastTime = now;
  }

  // -------- 读取按键（去抖 + 长短按） --------
  int reading = digitalRead(EN_SW);

  if (reading != lastButtonState) {
    delay(5); // 简单去抖
  }

  if (reading != buttonState) {
    buttonState = reading;

    if (buttonState == LOW) {
      lastPressTime = millis();
    } 
    else {
      unsigned long pressDuration = millis() - lastPressTime;

      if (pressDuration < 400) {
        Serial.println("Short Press!");
      } else {
        Serial.println("Long Press!");
      }
    }
  }

  lastButtonState = reading;
}

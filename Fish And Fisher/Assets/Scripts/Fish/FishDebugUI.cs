using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FishAndFisher.Fish
{
    /// <summary>
    /// 鱼玩家的调试UI
    /// 在游戏运行时显示调试信息
    /// </summary>
    public class FishDebugUI : MonoBehaviour
    {
        [Header("调试设置")]
        [SerializeField] private bool showDebugUI = true;
        [SerializeField] private bool showMovementInfo = true;
        [SerializeField] private bool showStateInfo = true;
        [SerializeField] private bool showInputInfo = true;
        [SerializeField] private bool showPerformanceInfo = true;

        [Header("UI设置")]
        [SerializeField] private Vector2 uiPosition = new Vector2(10, 10);
        [SerializeField] private float uiWidth = 300f;
        [SerializeField] private int fontSize = 14;

        // 组件引用
        private FishController controller;
        private FishMovement movement;
        private FishState state;
        private FishInputHandler inputHandler;

        // 性能统计
        private float fps;
        private float deltaTime;

        // GUI样式
        private GUIStyle boxStyle;
        private GUIStyle labelStyle;
        private GUIStyle valueStyle;

        private void Awake()
        {
            // 获取组件引用
            controller = GetComponent<FishController>();
            movement = GetComponent<FishMovement>();
            state = GetComponent<FishState>();
            inputHandler = GetComponent<FishInputHandler>();
        }

        private void Update()
        {
            // 计算FPS
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            fps = 1.0f / deltaTime;
        }

        private void OnGUI()
        {
            if (!showDebugUI) return;

            InitializeStyles();

            float yPos = uiPosition.y;

            // 标题
            GUI.Box(new Rect(uiPosition.x, yPos, uiWidth, 30), "鱼玩家调试信息", boxStyle);
            yPos += 35;

            // 移动信息
            if (showMovementInfo && movement != null)
            {
                GUI.Box(new Rect(uiPosition.x, yPos, uiWidth, 100), "", boxStyle);
                DrawMovementInfo(uiPosition.x + 10, yPos + 5);
                yPos += 105;
            }

            // 状态信息
            if (showStateInfo && state != null)
            {
                GUI.Box(new Rect(uiPosition.x, yPos, uiWidth, 80), "", boxStyle);
                DrawStateInfo(uiPosition.x + 10, yPos + 5);
                yPos += 85;
            }

            // 输入信息
            if (showInputInfo && inputHandler != null)
            {
                GUI.Box(new Rect(uiPosition.x, yPos, uiWidth, 60), "", boxStyle);
                DrawInputInfo(uiPosition.x + 10, yPos + 5);
                yPos += 65;
            }

            // 性能信息
            if (showPerformanceInfo)
            {
                GUI.Box(new Rect(uiPosition.x, yPos, uiWidth, 40), "", boxStyle);
                DrawPerformanceInfo(uiPosition.x + 10, yPos + 5);
                yPos += 45;
            }

            // 控制提示
            DrawControlHints();
        }

        private void InitializeStyles()
        {
            if (boxStyle == null)
            {
                boxStyle = new GUIStyle(GUI.skin.box);
                boxStyle.normal.background = MakeTexture(2, 2, new Color(0, 0, 0, 0.7f));
            }

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.fontSize = fontSize;
                labelStyle.normal.textColor = Color.yellow;
            }

            if (valueStyle == null)
            {
                valueStyle = new GUIStyle(GUI.skin.label);
                valueStyle.fontSize = fontSize;
                valueStyle.normal.textColor = Color.white;
            }
        }

        private void DrawMovementInfo(float x, float y)
        {
            GUI.Label(new Rect(x, y, 200, 20), "=== 移动信息 ===", labelStyle);
            y += 20;

            GUI.Label(new Rect(x, y, 100, 20), "当前速度:", labelStyle);
            GUI.Label(new Rect(x + 80, y, 100, 20), $"{movement.CurrentSpeed:F1} m/s", valueStyle);
            y += 18;

            GUI.Label(new Rect(x, y, 100, 20), "当前方向:", labelStyle);
            GUI.Label(new Rect(x + 80, y, 100, 20), $"{movement.CurrentDirection:F0}°", valueStyle);
            y += 18;

            GUI.Label(new Rect(x, y, 100, 20), "位置:", labelStyle);
            Vector3 pos = transform.position;
            GUI.Label(new Rect(x + 80, y, 200, 20), $"({pos.x:F1}, {pos.y:F1}, {pos.z:F1})", valueStyle);
        }

        private void DrawStateInfo(float x, float y)
        {
            GUI.Label(new Rect(x, y, 200, 20), "=== 状态信息 ===", labelStyle);
            y += 20;

            GUI.Label(new Rect(x, y, 100, 20), "当前状态:", labelStyle);
            GUI.Label(new Rect(x + 80, y, 100, 20), state.CurrentState.ToString(), valueStyle);
            y += 18;

            GUI.Label(new Rect(x, y, 100, 20), "体力:", labelStyle);
            GUI.Label(new Rect(x + 80, y, 100, 20), $"{state.StaminaPercentage * 100:F0}%", valueStyle);
            y += 18;

            if (state.CurrentState == FishStateType.Escaping)
            {
                GUI.Label(new Rect(x, y, 100, 20), "逃脱进度:", labelStyle);
                GUI.Label(new Rect(x + 80, y, 100, 20), $"{state.EscapeProgress:F0}%", valueStyle);
            }
        }

        private void DrawInputInfo(float x, float y)
        {
            GUI.Label(new Rect(x, y, 200, 20), "=== 输入信息 ===", labelStyle);
            y += 20;

            GUI.Label(new Rect(x, y, 100, 20), "移动输入:", labelStyle);
            Vector2 input = inputHandler.CurrentInput;
            GUI.Label(new Rect(x + 80, y, 100, 20), $"({input.x:F2}, {input.y:F2})", valueStyle);
            y += 18;

            GUI.Label(new Rect(x, y, 100, 20), "Jump按键:", labelStyle);
            GUI.Label(new Rect(x + 80, y, 100, 20), inputHandler.IsJumping ? "按下" : "释放", valueStyle);
        }

        private void DrawPerformanceInfo(float x, float y)
        {
            GUI.Label(new Rect(x, y, 200, 20), "=== 性能信息 ===", labelStyle);
            y += 20;

            GUI.Label(new Rect(x, y, 100, 20), "FPS:", labelStyle);
            GUI.Label(new Rect(x + 80, y, 100, 20), $"{fps:F1}", valueStyle);
        }

        private void DrawControlHints()
        {
            float x = Screen.width - 250;
            float y = 10;

            GUI.Box(new Rect(x, y, 240, 140), "控制说明", boxStyle);
            y += 25;

            string controls = "WASD/摇杆: 控制方向\n" +
                            "W: 前进（保持方向）\n" +
                            "A/D: 左右转60°\n" +
                            "W+A/D: 左右转30°\n" +
                            "空格/Jump: 连续按键加速\n" +
                            "ESC: 暂停游戏";

            GUI.Label(new Rect(x + 10, y, 220, 110), controls, valueStyle);
        }

        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }

        public void ToggleDebugUI()
        {
            showDebugUI = !showDebugUI;
        }
    }
}
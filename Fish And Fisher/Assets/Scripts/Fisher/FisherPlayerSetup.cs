using UnityEngine;
using UnityEditor;

namespace FishAndFisher.Fisher
{
    /// <summary>
    /// 渔夫玩家设置工具
    /// 用于在Unity编辑器中快速创建和配置渔夫玩家
    /// </summary>
    public class FisherPlayerSetup : MonoBehaviour
    {
        [Header("基础设置")]
        [SerializeField] private bool autoSetupOnStart = false;
        [SerializeField] private bool createVisualPlaceholder = true;

        [Header("准心设置")]
        [SerializeField] private float fishPlaneY = 0f;
        [SerializeField] private float visualPlaneY = 5f;
        [SerializeField] private Vector2 boundarySize = new Vector2(50f, 50f);

        [Header("视觉占位符设置")]
        [SerializeField] private Color crosshairColor = Color.red;
        [SerializeField] private float crosshairScale = 0.5f;

        /// <summary>
        /// 创建完整的渔夫玩家GameObject
        /// 可以通过菜单调用：GameObject > Fish And Fisher > Create Fisher Player
        /// </summary>
#if UNITY_EDITOR
        [MenuItem("GameObject/Fish And Fisher/Create Fisher Player", false, 11)]
        public static void CreateFisherPlayer()
        {
            // 创建主GameObject
            GameObject fisherPlayer = new GameObject("FisherPlayer");

            // 设置位置
            fisherPlayer.transform.position = Vector3.zero;

            // 创建子对象：逻辑准心
            GameObject logicCrosshair = new GameObject("LogicCrosshair");
            logicCrosshair.transform.parent = fisherPlayer.transform;
            logicCrosshair.transform.localPosition = new Vector3(0, 0, 0);

            // 创建子对象：视觉准心（使用Quad）
            GameObject visualCrosshair = GameObject.CreatePrimitive(PrimitiveType.Quad);
            visualCrosshair.name = "VisualCrosshair";
            visualCrosshair.transform.parent = fisherPlayer.transform;
            visualCrosshair.transform.localPosition = new Vector3(0, 5, 0);
            visualCrosshair.transform.localRotation = Quaternion.Euler(45, 0, 0); // 倾斜45度
            visualCrosshair.transform.localScale = Vector3.one * 0.5f;

            // 为视觉准心创建材质
            Material crosshairMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            crosshairMaterial.color = new Color(1f, 0f, 0f, 0.8f); // 红色半透明

            // 启用透明度
            crosshairMaterial.SetFloat("_Surface", 1); // 设置为透明模式
            crosshairMaterial.SetFloat("_Blend", 0); // Alpha混合
            crosshairMaterial.SetFloat("_AlphaClip", 0);
            crosshairMaterial.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            crosshairMaterial.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            crosshairMaterial.SetFloat("_ZWrite", 0);
            crosshairMaterial.renderQueue = 3000;

            // 应用材质
            visualCrosshair.GetComponent<Renderer>().material = crosshairMaterial;

            // 移除Quad的碰撞体
            var quadCollider = visualCrosshair.GetComponent<Collider>();
            if (quadCollider != null)
            {
                Object.DestroyImmediate(quadCollider);
            }

            // 创建可选的鱼竿占位符
            GameObject fishingRod = CreateFishingRodPlaceholder(fisherPlayer);

            // 添加核心组件到主对象
            var crosshairController = fisherPlayer.AddComponent<FisherCrosshairController>();
            var fisherController = fisherPlayer.AddComponent<FisherController>();

            // 配置准心控制器（使用反射设置私有字段）
            ConfigureCrosshairController(crosshairController, logicCrosshair.transform, visualCrosshair.transform);

            // 配置渔夫控制器
            ConfigureFisherController(fisherController, fishingRod.transform);

            // 添加设置脚本
            var setup = fisherPlayer.AddComponent<FisherPlayerSetup>();
            setup.createVisualPlaceholder = true;
            setup.fishPlaneY = 0f;
            setup.visualPlaneY = 5f;
            setup.boundarySize = new Vector2(50f, 50f);

            // 选中新创建的对象
            Selection.activeGameObject = fisherPlayer;

            Debug.Log("渔夫玩家已创建！请在Inspector中调整参数。\n" +
                      "- LogicCrosshair: 逻辑准心（在鱼平面）\n" +
                      "- VisualCrosshair: 视觉准心（倾斜Quad显示）\n" +
                      "- FishingRod: 鱼竿占位符（可选）");
        }

        /// <summary>
        /// 创建鱼竿占位符
        /// </summary>
        private static GameObject CreateFishingRodPlaceholder(GameObject parent)
        {
            // 创建鱼竿根对象
            GameObject fishingRod = new GameObject("FishingRod");
            fishingRod.transform.parent = parent.transform;
            fishingRod.transform.localPosition = new Vector3(0, 1, 0);
            fishingRod.transform.localRotation = Quaternion.identity;

            // 创建鱼竿杆身
            GameObject rodBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rodBody.name = "RodBody";
            rodBody.transform.parent = fishingRod.transform;
            rodBody.transform.localPosition = Vector3.zero;
            rodBody.transform.localRotation = Quaternion.Euler(90, 0, 0);
            rodBody.transform.localScale = new Vector3(0.05f, 1.5f, 0.05f);

            // 创建鱼竿顶端
            GameObject rodTip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rodTip.name = "RodTip";
            rodTip.transform.parent = fishingRod.transform;
            rodTip.transform.localPosition = new Vector3(0, 0, 3f);
            rodTip.transform.localScale = Vector3.one * 0.1f;

            // 创建鱼线（简单的立方体表示）
            GameObject fishingLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fishingLine.name = "FishingLine";
            fishingLine.transform.parent = fishingRod.transform;
            fishingLine.transform.localPosition = new Vector3(0, -2, 3f);
            fishingLine.transform.localScale = new Vector3(0.02f, 4f, 0.02f);

            // 设置材质颜色
            Material rodMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            rodMaterial.color = new Color(0.4f, 0.3f, 0.2f); // 棕色

            Material lineMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            lineMaterial.color = new Color(0.8f, 0.8f, 0.8f); // 灰白色

            // 应用材质
            rodBody.GetComponent<Renderer>().material = rodMaterial;
            rodTip.GetComponent<Renderer>().material = rodMaterial;
            fishingLine.GetComponent<Renderer>().material = lineMaterial;

            // 移除所有碰撞体
            Object.DestroyImmediate(rodBody.GetComponent<Collider>());
            Object.DestroyImmediate(rodTip.GetComponent<Collider>());
            Object.DestroyImmediate(fishingLine.GetComponent<Collider>());

            return fishingRod;
        }

        /// <summary>
        /// 配置准心控制器
        /// </summary>
        private static void ConfigureCrosshairController(FisherCrosshairController controller,
            Transform logicCrosshair, Transform visualCrosshair)
        {
            var controllerType = controller.GetType();

            // 设置逻辑准心
            var logicField = controllerType.GetField("logicCrosshair",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            logicField?.SetValue(controller, logicCrosshair);

            // 设置视觉准心
            var visualField = controllerType.GetField("visualCrosshair",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            visualField?.SetValue(controller, visualCrosshair);

            // 设置主相机
            var cameraField = controllerType.GetField("targetCamera",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            cameraField?.SetValue(controller, Camera.main);

            // 设置平面高度
            var fishPlaneYField = controllerType.GetField("fishPlaneY",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fishPlaneYField?.SetValue(controller, 0f);

            var visualPlaneYField = controllerType.GetField("visualPlaneY",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            visualPlaneYField?.SetValue(controller, 5f);

            // 设置边界大小（与鱼活动范围对等）
            var boundarySizeField = controllerType.GetField("boundarySize",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            boundarySizeField?.SetValue(controller, new Vector2(50f, 50f));
        }

        /// <summary>
        /// 配置渔夫控制器
        /// </summary>
        private static void ConfigureFisherController(FisherController controller, Transform fishingRod)
        {
            var controllerType = controller.GetType();

            // 设置鱼竿引用
            var rodField = controllerType.GetField("fishingRod",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            rodField?.SetValue(controller, fishingRod);

            // 设置钩子检测图层（默认为Default层）
            var layerField = controllerType.GetField("hookableLayer",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            layerField?.SetValue(controller, LayerMask.GetMask("Default"));
        }

        /// <summary>
        /// 验证渔夫玩家设置
        /// </summary>
        [MenuItem("GameObject/Fish And Fisher/Validate Fisher Player Setup", false, 12)]
        public static void ValidateFisherPlayerSetup()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogError("请先选择一个GameObject！");
                return;
            }

            bool isValid = true;
            string report = "=== 渔夫玩家设置验证报告 ===\n";

            // 检查必要组件
            if (selected.GetComponent<FisherCrosshairController>() == null)
            {
                report += "❌ 缺少 FisherCrosshairController 组件\n";
                isValid = false;
            }
            else
            {
                report += "✓ FisherCrosshairController 组件已找到\n";
            }

            if (selected.GetComponent<FisherController>() == null)
            {
                report += "❌ 缺少 FisherController 组件\n";
                isValid = false;
            }
            else
            {
                report += "✓ FisherController 组件已找到\n";
            }

            // 检查子对象
            Transform logicCrosshair = selected.transform.Find("LogicCrosshair");
            if (logicCrosshair == null)
            {
                report += "❌ 缺少 LogicCrosshair 子对象\n";
                isValid = false;
            }
            else
            {
                report += "✓ LogicCrosshair 子对象已找到\n";
            }

            Transform visualCrosshair = selected.transform.Find("VisualCrosshair");
            if (visualCrosshair == null)
            {
                report += "❌ 缺少 VisualCrosshair 子对象\n";
                isValid = false;
            }
            else
            {
                report += "✓ VisualCrosshair 子对象已找到\n";
            }

            // 检查可选对象
            Transform fishingRod = selected.transform.Find("FishingRod");
            if (fishingRod == null)
            {
                report += "⚠ 缺少 FishingRod 子对象（可选）\n";
            }
            else
            {
                report += "✓ FishingRod 子对象已找到\n";
            }

            // 输出报告
            if (isValid)
            {
                Debug.Log(report + "\n✅ 渔夫玩家设置验证通过！");
            }
            else
            {
                Debug.LogError(report + "\n❌ 渔夫玩家设置验证失败！请添加缺失的组件或子对象。");
            }
        }
#endif

        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupFisherPlayer();
            }
        }

        /// <summary>
        /// 设置渔夫玩家
        /// </summary>
        public void SetupFisherPlayer()
        {
            // 确保所有必要组件都存在
            var crosshairController = EnsureComponent<FisherCrosshairController>();
            var fisherController = EnsureComponent<FisherController>();

            // 查找或创建子对象
            Transform logicCrosshair = transform.Find("LogicCrosshair");
            if (logicCrosshair == null)
            {
                GameObject logicObj = new GameObject("LogicCrosshair");
                logicObj.transform.parent = transform;
                logicObj.transform.localPosition = new Vector3(0, fishPlaneY, 0);
                logicCrosshair = logicObj.transform;
            }

            Transform visualCrosshair = transform.Find("VisualCrosshair");
            if (visualCrosshair == null && createVisualPlaceholder)
            {
                GameObject visualObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                visualObj.name = "VisualCrosshair";
                visualObj.transform.parent = transform;
                visualObj.transform.localPosition = new Vector3(0, visualPlaneY, 0);
                visualObj.transform.localRotation = Quaternion.Euler(45, 0, 0);
                visualObj.transform.localScale = Vector3.one * crosshairScale;
                visualCrosshair = visualObj.transform;

                // 移除碰撞体
                var collider = visualObj.GetComponent<Collider>();
                if (collider != null)
                {
                    Destroy(collider);
                }
            }

            // 配置组件引用
            ConfigureComponents(crosshairController, fisherController, logicCrosshair, visualCrosshair);

            Debug.Log("渔夫玩家设置完成！");
        }

        /// <summary>
        /// 确保组件存在
        /// </summary>
        private T EnsureComponent<T>() where T : Component
        {
            T component = GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
                Debug.Log($"添加组件: {typeof(T).Name}");
            }
            return component;
        }

        /// <summary>
        /// 配置组件参数
        /// </summary>
        private void ConfigureComponents(FisherCrosshairController crosshairController,
            FisherController fisherController,
            Transform logicCrosshair,
            Transform visualCrosshair)
        {
            if (crosshairController != null && logicCrosshair != null && visualCrosshair != null)
            {
                // 使用反射设置私有字段
                var crosshairType = crosshairController.GetType();

                var logicField = crosshairType.GetField("logicCrosshair",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                logicField?.SetValue(crosshairController, logicCrosshair);

                var visualField = crosshairType.GetField("visualCrosshair",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                visualField?.SetValue(crosshairController, visualCrosshair);

                var cameraField = crosshairType.GetField("targetCamera",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                cameraField?.SetValue(crosshairController, Camera.main);

                var fishPlaneYField = crosshairType.GetField("fishPlaneY",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                fishPlaneYField?.SetValue(crosshairController, fishPlaneY);

                var visualPlaneYField = crosshairType.GetField("visualPlaneY",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                visualPlaneYField?.SetValue(crosshairController, visualPlaneY);

                var boundarySizeField = crosshairType.GetField("boundarySize",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                boundarySizeField?.SetValue(crosshairController, boundarySize);
            }

            if (fisherController != null)
            {
                Transform fishingRod = transform.Find("FishingRod");
                if (fishingRod != null)
                {
                    var fisherType = fisherController.GetType();
                    var rodField = fisherType.GetField("fishingRod",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    rodField?.SetValue(fisherController, fishingRod);
                }
            }
        }

        /// <summary>
        /// 在编辑器中重置组件
        /// </summary>
        private void Reset()
        {
            // 当脚本被添加或重置时调用
            crosshairColor = Color.red;
            crosshairScale = 0.5f;
            autoSetupOnStart = false;
            createVisualPlaceholder = true;
            fishPlaneY = 0f;
            visualPlaneY = 5f;
            boundarySize = new Vector2(50f, 50f);
        }

        /// <summary>
        /// 绘制Gizmos
        /// </summary>
        private void OnDrawGizmos()
        {
            // 绘制边界范围矩形（与鱼活动范围对等）
            Gizmos.color = Color.yellow;
            Vector3 boundaryCenter = new Vector3(0, fishPlaneY, 0);
            Vector3 boundaryBoxSize = new Vector3(boundarySize.x, 0.1f, boundarySize.y);
            Gizmos.DrawWireCube(boundaryCenter, boundaryBoxSize);

            // 绘制视觉平面
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Vector3 visualCenter = new Vector3(0, visualPlaneY, 0);
            Gizmos.DrawCube(visualCenter, new Vector3(boundarySize.x, 0.1f, boundarySize.y));

            // 绘制鱼平面
            Gizmos.color = new Color(0, 0, 1, 0.2f);
            Vector3 fishCenter = new Vector3(0, fishPlaneY, 0);
            Gizmos.DrawCube(fishCenter, new Vector3(boundarySize.x, 0.1f, boundarySize.y));
        }
    }
}

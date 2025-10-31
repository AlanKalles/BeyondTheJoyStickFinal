using UnityEngine;
using UnityEditor;

namespace FishAndFisher.Fish
{
    /// <summary>
    /// 鱼玩家设置工具
    /// 用于在Unity编辑器中快速创建和配置鱼玩家
    /// </summary>
    public class FishPlayerSetup : MonoBehaviour
    {
        [Header("基础设置")]
        [SerializeField] private bool autoSetupOnStart = false;
        [SerializeField] private bool addDebugUI = true;
        [SerializeField] private bool createVisualPlaceholder = true;

        [Header("视觉占位符设置")]
        [SerializeField] private Color fishColor = new Color(0.2f, 0.6f, 0.9f);
        [SerializeField] private float fishScale = 1f;

        /// <summary>
        /// 创建完整的鱼玩家GameObject
        /// 可以通过菜单调用：GameObject > Fish And Fisher > Create Fish Player
        /// </summary>
#if UNITY_EDITOR
        [MenuItem("GameObject/Fish And Fisher/Create Fish Player", false, 10)]
        public static void CreateFishPlayer()
        {
            // 创建主GameObject
            GameObject fishPlayer = new GameObject("FishPlayer");

            // 设置位置
            fishPlayer.transform.position = Vector3.zero;

            // 添加核心组件
            fishPlayer.AddComponent<FishController>();
            fishPlayer.AddComponent<FishMovement>();
            fishPlayer.AddComponent<FishInputHandler>();
            fishPlayer.AddComponent<FishState>();
            fishPlayer.AddComponent<FishAnimator>();

            // 添加调试UI
            fishPlayer.AddComponent<FishDebugUI>();

            // 创建视觉占位符
            CreateVisualPlaceholder(fishPlayer);

            // 添加设置脚本
            var setup = fishPlayer.AddComponent<FishPlayerSetup>();
            setup.createVisualPlaceholder = true;

            // 选中新创建的对象
            Selection.activeGameObject = fishPlayer;

            Debug.Log("鱼玩家已创建！请在Inspector中调整参数。");
        }

        /// <summary>
        /// 创建简单的视觉占位符
        /// </summary>
        private static void CreateVisualPlaceholder(GameObject parent)
        {
            // 创建鱼身主体
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.parent = parent.transform;
            body.transform.localPosition = Vector3.zero;
            body.transform.localRotation = Quaternion.Euler(90, 0, 0);
            body.transform.localScale = new Vector3(0.5f, 1f, 0.5f);

            // 创建鱼尾
            GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tail.name = "Tail";
            tail.transform.parent = parent.transform;
            tail.transform.localPosition = new Vector3(0, 0, -1.2f);
            tail.transform.localScale = new Vector3(0.8f, 0.3f, 0.4f);

            // 创建鱼鳍（左）
            GameObject finLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            finLeft.name = "FinLeft";
            finLeft.transform.parent = parent.transform;
            finLeft.transform.localPosition = new Vector3(-0.6f, 0, 0);
            finLeft.transform.localScale = new Vector3(0.3f, 0.1f, 0.5f);
            finLeft.transform.localRotation = Quaternion.Euler(0, -30, 0);

            // 创建鱼鳍（右）
            GameObject finRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            finRight.name = "FinRight";
            finRight.transform.parent = parent.transform;
            finRight.transform.localPosition = new Vector3(0.6f, 0, 0);
            finRight.transform.localScale = new Vector3(0.3f, 0.1f, 0.5f);
            finRight.transform.localRotation = Quaternion.Euler(0, 30, 0);

            // 设置材质颜色
            Material fishMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            fishMaterial.color = new Color(0.2f, 0.6f, 0.9f);

            // 应用材质
            body.GetComponent<Renderer>().material = fishMaterial;
            tail.GetComponent<Renderer>().material = fishMaterial;
            finLeft.GetComponent<Renderer>().material = fishMaterial;
            finRight.GetComponent<Renderer>().material = fishMaterial;

            // 移除不需要的碰撞体
            Destroy(tail.GetComponent<Collider>());
            Destroy(finLeft.GetComponent<Collider>());
            Destroy(finRight.GetComponent<Collider>());

            // 将主碰撞体调整为触发器
            var bodyCollider = body.GetComponent<Collider>();
            if (bodyCollider != null)
            {
                bodyCollider.isTrigger = false;
            }
        }

        /// <summary>
        /// 验证鱼玩家设置
        /// </summary>
        [MenuItem("GameObject/Fish And Fisher/Validate Fish Player Setup", false, 11)]
        public static void ValidateFishPlayerSetup()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogError("请先选择一个GameObject！");
                return;
            }

            bool isValid = true;
            string report = "=== 鱼玩家设置验证报告 ===\n";

            // 检查必要组件
            if (selected.GetComponent<FishController>() == null)
            {
                report += "❌ 缺少 FishController 组件\n";
                isValid = false;
            }
            else
            {
                report += "✓ FishController 组件已找到\n";
            }

            if (selected.GetComponent<FishMovement>() == null)
            {
                report += "❌ 缺少 FishMovement 组件\n";
                isValid = false;
            }
            else
            {
                report += "✓ FishMovement 组件已找到\n";
            }

            if (selected.GetComponent<FishInputHandler>() == null)
            {
                report += "❌ 缺少 FishInputHandler 组件\n";
                isValid = false;
            }
            else
            {
                report += "✓ FishInputHandler 组件已找到\n";
            }

            if (selected.GetComponent<FishState>() == null)
            {
                report += "❌ 缺少 FishState 组件\n";
                isValid = false;
            }
            else
            {
                report += "✓ FishState 组件已找到\n";
            }

            // 检查可选组件
            if (selected.GetComponent<FishAnimator>() == null)
            {
                report += "⚠ 缺少 FishAnimator 组件（可选）\n";
            }
            else
            {
                report += "✓ FishAnimator 组件已找到\n";
            }

            if (selected.GetComponent<FishDebugUI>() == null)
            {
                report += "⚠ 缺少 FishDebugUI 组件（可选）\n";
            }
            else
            {
                report += "✓ FishDebugUI 组件已找到\n";
            }

            // 输出报告
            if (isValid)
            {
                Debug.Log(report + "\n✅ 鱼玩家设置验证通过！");
            }
            else
            {
                Debug.LogError(report + "\n❌ 鱼玩家设置验证失败！请添加缺失的组件。");
            }
        }
#endif

        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupFishPlayer();
            }
        }

        /// <summary>
        /// 设置鱼玩家
        /// </summary>
        public void SetupFishPlayer()
        {
            // 确保所有必要组件都存在
            EnsureComponent<FishController>();
            EnsureComponent<FishMovement>();
            EnsureComponent<FishInputHandler>();
            EnsureComponent<FishState>();
            EnsureComponent<FishAnimator>();

            if (addDebugUI)
            {
                EnsureComponent<FishDebugUI>();
            }

            // 设置组件引用
            ConfigureComponents();

            Debug.Log("鱼玩家设置完成！");
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
        private void ConfigureComponents()
        {
            // 获取组件引用
            var movement = GetComponent<FishMovement>();
            var animator = GetComponent<FishAnimator>();

            // 配置FishAnimator
            if (animator != null && createVisualPlaceholder)
            {
                // 尝试找到视觉元素并分配给动画器
                Transform body = transform.Find("Body");
                Transform tail = transform.Find("Tail");

                if (body != null && tail != null)
                {
                    // 使用反射设置私有字段（在实际项目中应该提供公共接口）
                    var animatorType = animator.GetType();

                    var bodyField = animatorType.GetField("fishBody",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    bodyField?.SetValue(animator, body);

                    var tailField = animatorType.GetField("fishTail",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    tailField?.SetValue(animator, tail);

                    // 查找鱼鳍
                    Transform[] fins = new Transform[]
                    {
                        transform.Find("FinLeft"),
                        transform.Find("FinRight")
                    };

                    var finsField = animatorType.GetField("fishFins",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    finsField?.SetValue(animator, fins);
                }
            }

            // 配置初始位置
            if (movement != null)
            {
                // 设置初始高度
                Vector3 pos = transform.position;
                pos.y = 0; // 设置为水平面
                transform.position = pos;
            }
        }

        /// <summary>
        /// 在编辑器中重置组件
        /// </summary>
        private void Reset()
        {
            // 当脚本被添加或重置时调用
            fishColor = new Color(0.2f, 0.6f, 0.9f);
            fishScale = 1f;
            autoSetupOnStart = false;
            addDebugUI = true;
            createVisualPlaceholder = true;
        }

        /// <summary>
        /// 绘制Gizmos
        /// </summary>
        private void OnDrawGizmos()
        {
            // 绘制鱼的朝向
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * 2f);

            // 绘制移动平面
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Vector3 center = transform.position;
            center.y = 0;
            Gizmos.DrawCube(center, new Vector3(100, 0.1f, 100));
        }
    }
}
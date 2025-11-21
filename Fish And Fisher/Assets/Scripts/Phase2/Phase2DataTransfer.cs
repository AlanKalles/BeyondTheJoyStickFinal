using UnityEngine;

namespace FishAndFisher.Phase2
{
    /// <summary>
    /// Phase1到Phase2的数据传递单例
    /// 使用DontDestroyOnLoad确保场景切换时不被销毁
    /// 仅传递剩余时间
    /// </summary>
    public class Phase2DataTransfer : MonoBehaviour
    {
        // 单例实例
        public static Phase2DataTransfer Instance { get; private set; }

        // 传递数据（仅剩余时间）
        public float RemainingTime { get; private set; }

        /// <summary>
        /// 保存剩余时间
        /// </summary>
        /// <param name="timeRemaining">游戏剩余时间（秒）</param>
        public void SaveRemainingTime(float timeRemaining)
        {
            RemainingTime = timeRemaining;
            Debug.Log($"[Phase2DataTransfer] 剩余时间已保存：{timeRemaining:F1}秒");
        }

        /// <summary>
        /// 清除已保存的数据
        /// </summary>
        public void ClearData()
        {
            RemainingTime = 0f;
            Debug.Log("[Phase2DataTransfer] 数据已清除");
        }

        /// <summary>
        /// 销毁单例实例（Phase2结束时调用）
        /// </summary>
        public static void DestroyInstance()
        {
            if (Instance != null)
            {
                Debug.Log("[Phase2DataTransfer] 销毁单例实例");
                Destroy(Instance.gameObject);
                Instance = null;
            }
        }

        /// <summary>
        /// 单例初始化
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("[Phase2DataTransfer] 单例已创建并设置为DontDestroyOnLoad");
            }
            else
            {
                Debug.LogWarning("[Phase2DataTransfer] 检测到重复实例，销毁新实例");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 销毁时清理
        /// </summary>
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Debug.Log("[Phase2DataTransfer] 单例实例被销毁");
                Instance = null;
            }
        }
    }
}

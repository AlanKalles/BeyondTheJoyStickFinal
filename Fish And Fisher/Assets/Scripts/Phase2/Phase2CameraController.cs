using UnityEngine;
using Unity.Cinemachine;

namespace FishAndFisher.Phase2
{
    /// <summary>
    /// Phase2相机控制器 - 管理Phase1和Phase2之间的相机切换
    /// </summary>
    public class Phase2CameraController : MonoBehaviour
    {
        [Header("Phase1相机")]
        [Tooltip("Phase1鱼玩家相机")]
        [SerializeField] private CinemachineCamera fishPhase1Camera;

        [Tooltip("Phase1渔夫玩家相机")]
        [SerializeField] private CinemachineCamera fisherPhase1Camera;

        [Header("Phase2相机")]
        [Tooltip("Phase2鱼玩家相机")]
        [SerializeField] private CinemachineCamera fishPhase2Camera;

        [Tooltip("Phase2渔夫玩家相机")]
        [SerializeField] private CinemachineCamera fisherPhase2Camera;

        [Header("相机优先级")]
        [Tooltip("激活状态的相机优先级")]
        [SerializeField] private int activePriority = 10;

        [Tooltip("非激活状态的相机优先级")]
        [SerializeField] private int inactivePriority = 0;

        [Header("过渡设置")]
        [Tooltip("相机混合持续时间（秒）")]
        [SerializeField] private float blendDuration = 1.5f;

        private void Awake()
        {

            // 验证相机引用
            if (fishPhase1Camera == null)
            {
                Debug.LogWarning("[Phase2CameraController] Phase1鱼相机未设置！");
            }

            if (fisherPhase1Camera == null)
            {
                Debug.LogWarning("[Phase2CameraController] Phase1渔夫相机未设置！");
            }

            if (fishPhase2Camera == null)
            {
                Debug.LogWarning("[Phase2CameraController] Phase2鱼相机未设置！");
            }

            if (fisherPhase2Camera == null)
            {
                Debug.LogWarning("[Phase2CameraController] Phase2渔夫相机未设置！");
            }

            // 初始化：激活Phase1相机
            SwitchToPhase1Cameras();
        }

        /// <summary>
        /// 切换到Phase1相机
        /// </summary>
        public void SwitchToPhase1Cameras()
        {
            SetCameraPriority(fishPhase1Camera, true);
            SetCameraPriority(fisherPhase1Camera, true);
            SetCameraPriority(fishPhase2Camera, false);
            SetCameraPriority(fisherPhase2Camera, false);

            Debug.Log("[Phase2CameraController] 已切换到Phase1相机");
        }

        /// <summary>
        /// 切换到Phase2相机
        /// </summary>
        public void SwitchToPhase2Cameras()
        {
            SetCameraPriority(fishPhase1Camera, false);
            SetCameraPriority(fisherPhase1Camera, false);
            SetCameraPriority(fishPhase2Camera, true);
            SetCameraPriority(fisherPhase2Camera, true);

            Debug.Log("[Phase2CameraController] 已切换到Phase2相机");
        }

        /// <summary>
        /// 设置相机优先级
        /// </summary>
        private void SetCameraPriority(CinemachineCamera camera, bool isActive)
        {
            if (camera == null)
            {
                return;
            }

            camera.Priority = isActive ? activePriority : inactivePriority;
        }

        /// <summary>
        /// 设置混合持续时间
        /// </summary>
        public void SetBlendDuration(float duration)
        {
            blendDuration = duration;
        }

        /// <summary>
        /// 获取当前激活的鱼相机
        /// </summary>
        public CinemachineCamera GetActiveFishCamera()
        {
            if (fishPhase2Camera != null && fishPhase2Camera.Priority == activePriority)
            {
                return fishPhase2Camera;
            }

            return fishPhase1Camera;
        }

        /// <summary>
        /// 获取当前激活的渔夫相机
        /// </summary>
        public CinemachineCamera GetActiveFisherCamera()
        {
            if (fisherPhase2Camera != null && fisherPhase2Camera.Priority == activePriority)
            {
                return fisherPhase2Camera;
            }

            return fisherPhase1Camera;
        }

        /// <summary>
        /// 设置Phase2鱼相机的跟随目标
        /// </summary>
        public void SetPhase2FishTarget(Transform target)
        {
            if (fishPhase2Camera != null)
            {
                fishPhase2Camera.Target.TrackingTarget = target;
            }
        }

        /// <summary>
        /// 设置Phase2渔夫相机的跟随目标
        /// </summary>
        public void SetPhase2FisherTarget(Transform target)
        {
            if (fisherPhase2Camera != null)
            {
                fisherPhase2Camera.Target.TrackingTarget = target;
            }
        }
    }
}

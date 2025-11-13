using UnityEngine;

namespace FishAndFisher.Phase2
{
    /// <summary>
    /// Phase2音效管理器 - 预留音效触发接口
    /// </summary>
    public class Phase2AudioManager : MonoBehaviour
    {
        [Header("音效片段")]
        [Tooltip("钩中鱼的音效")]
        [SerializeField] private AudioClip fishCaughtSound;

        [Tooltip("倒计时节拍音效")]
        [SerializeField] private AudioClip countdownTickSound;

        [Tooltip("Phase2开始音效")]
        [SerializeField] private AudioClip phase2StartSound;

        [Tooltip("争斗持续背景音效（循环）")]
        [SerializeField] private AudioClip struggleLoopSound;

        [Tooltip("进度条移动音效")]
        [SerializeField] private AudioClip progressMoveSound;

        [Tooltip("鱼逃脱音效")]
        [SerializeField] private AudioClip fishEscapeSound;

        [Tooltip("渔夫胜利音效")]
        [SerializeField] private AudioClip fisherWinSound;

        [Header("音频源")]
        [Tooltip("音效播放器")]
        [SerializeField] private AudioSource sfxAudioSource;

        [Tooltip("背景音乐播放器")]
        [SerializeField] private AudioSource bgmAudioSource;

        private void Awake()
        {
            // 自动创建音频源（如果未设置）
            if (sfxAudioSource == null)
            {
                GameObject sfxObj = new GameObject("Phase2_SFX_AudioSource");
                sfxObj.transform.SetParent(transform);
                sfxAudioSource = sfxObj.AddComponent<AudioSource>();
                sfxAudioSource.playOnAwake = false;
            }

            if (bgmAudioSource == null)
            {
                GameObject bgmObj = new GameObject("Phase2_BGM_AudioSource");
                bgmObj.transform.SetParent(transform);
                bgmAudioSource = bgmObj.AddComponent<AudioSource>();
                bgmAudioSource.playOnAwake = false;
                bgmAudioSource.loop = true;
            }
        }

        /// <summary>
        /// 播放钩中鱼音效
        /// </summary>
        public void PlayFishCaughtSound()
        {
            PlaySFX(fishCaughtSound);
            Debug.Log("[Phase2AudioManager] 播放钩中鱼音效（待实现）");
        }

        /// <summary>
        /// 播放倒计时节拍音效
        /// </summary>
        public void PlayCountdownTickSound()
        {
            PlaySFX(countdownTickSound);
            Debug.Log("[Phase2AudioManager] 播放倒计时节拍音效（待实现）");
        }

        /// <summary>
        /// 播放Phase2开始音效
        /// </summary>
        public void PlayPhase2StartSound()
        {
            PlaySFX(phase2StartSound);
            Debug.Log("[Phase2AudioManager] 播放Phase2开始音效（待实现）");
        }

        /// <summary>
        /// 播放争斗循环背景音效
        /// </summary>
        public void PlayStruggleLoopSound()
        {
            PlayBGM(struggleLoopSound);
            Debug.Log("[Phase2AudioManager] 播放争斗循环音效（待实现）");
        }

        /// <summary>
        /// 停止争斗循环背景音效
        /// </summary>
        public void StopStruggleLoopSound()
        {
            StopBGM();
            Debug.Log("[Phase2AudioManager] 停止争斗循环音效");
        }

        /// <summary>
        /// 播放进度条移动音效
        /// </summary>
        public void PlayProgressMoveSound()
        {
            PlaySFX(progressMoveSound);
            // 这个音效可能会频繁触发，可以添加防抖逻辑
            // Debug.Log("[Phase2AudioManager] 播放进度条移动音效（待实现）");
        }

        /// <summary>
        /// 播放鱼逃脱音效
        /// </summary>
        public void PlayFishEscapeSound()
        {
            PlaySFX(fishEscapeSound);
            Debug.Log("[Phase2AudioManager] 播放鱼逃脱音效（待实现）");
        }

        /// <summary>
        /// 播放渔夫胜利音效
        /// </summary>
        public void PlayFisherWinSound()
        {
            PlaySFX(fisherWinSound);
            Debug.Log("[Phase2AudioManager] 播放渔夫胜利音效（待实现）");
        }

        /// <summary>
        /// 播放音效（内部方法）
        /// </summary>
        private void PlaySFX(AudioClip clip)
        {
            if (clip == null)
            {
                // Debug.LogWarning("[Phase2AudioManager] 音效片段未设置！");
                return;
            }

            if (sfxAudioSource == null)
            {
                Debug.LogError("[Phase2AudioManager] 音效播放器未设置！");
                return;
            }

            sfxAudioSource.PlayOneShot(clip);
        }

        /// <summary>
        /// 播放背景音乐（内部方法）
        /// </summary>
        private void PlayBGM(AudioClip clip)
        {
            if (clip == null)
            {
                // Debug.LogWarning("[Phase2AudioManager] 背景音乐片段未设置！");
                return;
            }

            if (bgmAudioSource == null)
            {
                Debug.LogError("[Phase2AudioManager] 背景音乐播放器未设置！");
                return;
            }

            bgmAudioSource.clip = clip;
            bgmAudioSource.Play();
        }

        /// <summary>
        /// 停止背景音乐（内部方法）
        /// </summary>
        private void StopBGM()
        {
            if (bgmAudioSource != null)
            {
                bgmAudioSource.Stop();
            }
        }

        /// <summary>
        /// 设置音效音量
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            if (sfxAudioSource != null)
            {
                sfxAudioSource.volume = Mathf.Clamp01(volume);
            }
        }

        /// <summary>
        /// 设置背景音乐音量
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            if (bgmAudioSource != null)
            {
                bgmAudioSource.volume = Mathf.Clamp01(volume);
            }
        }
    }
}

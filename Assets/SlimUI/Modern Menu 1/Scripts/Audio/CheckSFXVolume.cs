using UnityEngine;

namespace SlimUI.ModernMenu
{
    public class CheckSFXVolume : MonoBehaviour
    {
        private AudioSource audioSource;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();

            // 注册到音频管理器
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.RegisterSFXSource(audioSource);
            }
            else
            {
                // 如果音频管理器不存在，使用原来的方法
                audioSource.volume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
                Debug.Log(PlayerPrefs.GetFloat("SFXVolume"));
            }
        }

        void OnDestroy()
        {
            // 从音频管理器中取消注册
            if (AudioManager.Instance != null && audioSource != null)
            {
                AudioManager.Instance.UnregisterSFXSource(audioSource);
            }
        }

        public void UpdateVolume()
        {
            if (audioSource != null)
            {
                if (AudioManager.Instance != null)
                {
                    audioSource.volume = AudioManager.Instance.GetSFXVolume();
                }
                else
                {
                    audioSource.volume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
                }
            }
        }
    }
}
using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    // 音量设置键名
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    // 存储所有需要更新音量的音频源
    private List<AudioSource> allMusicSources = new List<AudioSource>();
    private List<AudioSource> allSFXSources = new List<AudioSource>();

    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 初始化音频设置
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudio()
    {
        // 加载保存的音量设置
        float masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.5f);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.5f);

        // 应用音量设置
        SetMasterVolume(masterVolume);
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
    }

    public void SetMasterVolume(float volume)
    {
        // 保存设置
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, volume);
        PlayerPrefs.Save();

        // 应用设置到Audio Mixer（如果使用）
        if (audioMixer != null)
        {
            // 将0-1的线性值转换为分贝值
            float dB = volume > 0 ? 20f * Mathf.Log10(volume) : -80f;
            audioMixer.SetFloat("MasterVolume", dB);
        }
        else
        {
            // 如果没有使用AudioMixer，直接设置AudioListener
            AudioListener.volume = volume;
        }
    }

    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volume);
        PlayerPrefs.Save();

        // 更新所有注册的音乐源
        foreach (AudioSource source in allMusicSources)
        {
            if (source != null)
            {
                source.volume = volume;
            }
        }

        // 更新主音乐源
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }

        // 更新AudioMixer
        if (audioMixer != null)
        {
            float dB = volume > 0 ? 20f * Mathf.Log10(volume) : -80f;
            audioMixer.SetFloat("MusicVolume", dB);
        }
    }

    public void SetSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
        PlayerPrefs.Save();

        // 更新所有注册的音效源
        foreach (AudioSource source in allSFXSources)
        {
            if (source != null)
            {
                source.volume = volume;
            }
        }

        // 更新主音效源
        if (sfxSource != null)
        {
            sfxSource.volume = volume;
        }

        // 更新AudioMixer
        if (audioMixer != null)
        {
            float dB = volume > 0 ? 20f * Mathf.Log10(volume) : -80f;
            audioMixer.SetFloat("SFXVolume", dB);
        }
    }

    // 注册音频源到管理器
    public void RegisterMusicSource(AudioSource source)
    {
        if (!allMusicSources.Contains(source))
        {
            allMusicSources.Add(source);
            // 立即应用当前音量设置
            source.volume = GetMusicVolume();
        }
    }

    public void RegisterSFXSource(AudioSource source)
    {
        if (!allSFXSources.Contains(source))
        {
            allSFXSources.Add(source);
            // 立即应用当前音量设置
            source.volume = GetSFXVolume();
        }
    }

    // 取消注册音频源
    public void UnregisterMusicSource(AudioSource source)
    {
        if (allMusicSources.Contains(source))
        {
            allMusicSources.Remove(source);
        }
    }

    public void UnregisterSFXSource(AudioSource source)
    {
        if (allSFXSources.Contains(source))
        {
            allSFXSources.Remove(source);
        }
    }

    // 获取当前音量设置
    public float GetMasterVolume()
    {
        return PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
    }

    public float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.5f);
    }

    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.5f);
    }

    // 播放音效的方法
    public void PlaySFX(AudioClip clip, float volumeScale = 1.0f)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, volumeScale);
        }
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource != null && clip != null)
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }

    // 停止播放
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void StopSFX()
    {
        if (sfxSource != null)
        {
            sfxSource.Stop();
        }
    }
}
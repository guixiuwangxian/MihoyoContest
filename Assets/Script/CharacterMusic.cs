using UnityEngine;
using System;
using System.Collections.Generic;

public class CharacterMusicController : MonoBehaviour
{
    [Serializable]
    public class Instruments
    {
        public string name;
        public string leftHoldSignal;  // 左键长按信号（特殊效果）
        public string rightHoldSignal; // 右键长按信号（基础交互）
        public Sprite icon; // 物品栏图标（手动赋值）
        public AudioClip holdAudio; // 长按演奏音效（预留资源位）
        public float audioVolume = 0.7f; // 音量调节
    }

    [Header("核心配置")]
    public List<Instruments> 乐器 = new List<Instruments>();
    public int 当前乐器 = 0;
    public AudioSource musicAudioSource; // 绑定主角身上的AudioSource组件

    [Header("内部状态")]
    private bool isLeftHolding = false;
    private bool isRightHolding = false;

    void Update()
    {
        // 左键长按检测（含停止播放）
        if (Input.GetMouseButton(0))
        {
            if (!isLeftHolding)
            {
                Debug.Log("开始长按左键演奏");
                isLeftHolding = true;
            }
            PlayMusicHold(0);
        }
        else
        {
            if (isLeftHolding)
            {
                Debug.Log("结束左键演奏");
                isLeftHolding = false;
                StopMusic(); // 松开左键停止音乐
            }
        }

        // 右键长按检测（含停止播放）
        if (Input.GetMouseButton(1))
        {
            if (!isRightHolding)
            {
                Debug.Log("开始长按右键演奏");
                isRightHolding = true;
            }
            PlayMusicHold(1);
        }
        else
        {
            if (isRightHolding)
            {
                Debug.Log("结束右键演奏");
                isRightHolding = false;
                StopMusic(); // 松开右键停止音乐
            }
        }

        // 物品栏切换逻辑（保持原有）
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            SwitchInstrument(1);
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            SwitchInstrument(-1);
        }
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Keypad1 + i) && i < 乐器.Count)
            {
                当前乐器 = i;
                Debug.Log($"切换到乐器：{乐器[当前乐器].name}");
            }
        }
    }

    // 长按演奏（循环播放+事件触发）
    void PlayMusicHold(int holdType)
    {
        if (乐器.Count == 0 || musicAudioSource == null) return;

        Instruments currentInstrument = 乐器[当前乐器];
        string signal = holdType == 0 ? currentInstrument.leftHoldSignal : currentInstrument.rightHoldSignal;
        
        // 配置并播放音效（仅当未播放时启动）
        if (currentInstrument.holdAudio != null && !musicAudioSource.isPlaying)
        {
            musicAudioSource.clip = currentInstrument.holdAudio;
            musicAudioSource.volume = currentInstrument.audioVolume;
            musicAudioSource.loop = true; // 开启循环，持续演奏
            musicAudioSource.Play();
        }
        
        Debug.Log($"持续演奏 {currentInstrument.name}，发送信号：{signal}");
        EventManager.TriggerEvent(signal);
    }

    // 停止音乐播放（通用方法）
    void StopMusic()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            musicAudioSource.Stop();
            musicAudioSource.loop = false; // 重置循环状态
        }
    }

    // 乐器切换逻辑（保持原有）
    void SwitchInstrument(int direction)
    {
        if (乐器.Count == 0) return;

        当前乐器 = (当前乐器 + direction + 乐器.Count) % 乐器.Count;
        Debug.Log($"切换到乐器：{乐器[当前乐器].name}");
    }
}
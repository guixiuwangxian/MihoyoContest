using UnityEngine;
using System;
using System.Collections.Generic;

public class CharacterMusicController : MonoBehaviour
{
    [Serializable]
    public class Instruments
    {
        public string name;
        public string leftHoldSignal;  // 左键长按信号
        public string rightHoldSignal; // 右键长按信号
        public Sprite icon;
    }

    public List<Instruments> 乐器 = new List<Instruments>();
    public int 当前乐器 = 0;

    // 标记是否正在长按（避免重复触发）
    private bool isLeftHolding = false;
    private bool isRightHolding = false;

    void Update()
    {
        // 1. 鼠标长按检测：持续触发音乐信号
        if (Input.GetMouseButton(0)) // 左键长按
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
            }
        }

        if (Input.GetMouseButton(1)) // 右键长按
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
            }
        }

        // 2. 物品栏切换逻辑（滚轮和数字）
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

    // 长按演奏（持续发送信号）
    void PlayMusicHold(int holdType)
    {
        Instruments currentInstrument = 乐器[当前乐器];
        string signal = holdType == 0 ? currentInstrument.leftHoldSignal : currentInstrument.rightHoldSignal;
        
        Debug.Log($"持续演奏 {currentInstrument.name}，发送信号：{signal}");
        EventManager.TriggerEvent(signal);
        // 此处可添加音频循环播放逻辑（后续补充）
    }

    void SwitchInstrument(int direction)
    {
        当前乐器 = (当前乐器 + direction + 乐器.Count) % 乐器.Count;
        Debug.Log($"切换到乐器：{乐器[当前乐器].name}");
    }
}
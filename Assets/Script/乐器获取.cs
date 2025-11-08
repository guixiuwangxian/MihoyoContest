using UnityEngine;

public class InstrumentUnlocker : MonoBehaviour
{
    [Header("关联组件")]
    public CharacterMusicController musicController; // 拖入主角身上的音乐控制器
    public InventoryUI inventoryUI; // 拖入物品栏UI脚本

    [Header("初始乐器（镇长给予）")]
    public CharacterMusicController.Instruments conductorStick; // 指挥棒数据

    [Header("关卡解锁乐器")]
    public CharacterMusicController.Instruments spiralHorn; // 螺旋号角（关卡一）
    public CharacterMusicController.Instruments harmonyHarp; // 调和竖琴（关卡二）

    void Start()
    {
        // 游戏启动时解锁指挥棒
        UnlockInstrument(conductorStick);
    }

    // 关卡一通关后调用（绑定到关卡结束触发器）
    public void UnlockHorn()
    {
        UnlockInstrument(spiralHorn);
        Debug.Log("通关关卡一，获取乐器：螺旋号角");
    }

    // 关卡二通关后调用（绑定到关卡结束触发器）
    public void UnlockHarp()
    {
        UnlockInstrument(harmonyHarp);
        Debug.Log("通关关卡二，获取乐器：调和竖琴");
    }

    // 通用解锁逻辑（添加乐器+刷新物品栏）
    private void UnlockInstrument(CharacterMusicController.Instruments instrument)
    {
        if (!musicController.乐器.Contains(instrument))
        {
            musicController.乐器.Add(instrument);
            inventoryUI.InitInventorySlots(); // 刷新物品栏显示新乐器
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("UI引用")]
    public GameObject itemSlotPrefab; // 物品槽预制体
    public Transform slotParent; // 物品槽的父物体（拖入InventoryPanel）
    public Color selectedColor = Color.yellow; // 选中状态颜色
    public Color normalColor = Color.white; // 正常状态颜色

    [Header("逻辑关联")]
    public CharacterMusicController musicController; // 关联之前的物品逻辑脚本

    private List<GameObject> itemSlots = new List<GameObject>(); // 存储生成的物品槽


    void Start()
    {
        // 初始化物品栏：根据乐器列表生成物品槽
        InitInventorySlots();
    }

    void Update()
    {
        // 实时更新选中状态（跟随当前选中的乐器）
        UpdateSelectedSlot();
    }

    // 初始化物品槽
    void InitInventorySlots()
    {
        // 清空已有槽位
        foreach (var slot in itemSlots)
        {
            Destroy(slot);
        }
        itemSlots.Clear();

        // 根据乐器列表生成槽位
        for (int i = 0; i < musicController.乐器.Count; i++)
        {
            // 实例化物品槽
            GameObject slot = Instantiate(itemSlotPrefab, slotParent);
            slot.name = $"Slot_{i}";
            itemSlots.Add(slot);

            // 设置槽位位置（横向排列）
            slot.GetComponent<RectTransform>().anchoredPosition = new Vector2(i * 110, 0); // 间隔110像素

            // 设置快捷键数字（1-9）
            slot.transform.Find("SlotNumber").GetComponent<Text>().text = (i + 1).ToString();

            // 设置物品图标（需在Instrument类中添加图标字段，见下方扩展）
            Image icon = slot.transform.Find("ItemIcon").GetComponent<Image>();
            icon.sprite = musicController.乐器[i].icon; // 显示图标
            icon.enabled = true; // 启用图标
        }
    }

    // 更新选中的槽位样式
    void UpdateSelectedSlot()
    {
        for (int i = 0; i < itemSlots.Count; i++)
        {
            // 选中的槽位用高亮颜色，其他用正常颜色
            Image slotImage = itemSlots[i].GetComponent<Image>();
            slotImage.color = (i == musicController.当前乐器) ? selectedColor : normalColor;
        }
    }
}
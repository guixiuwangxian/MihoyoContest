using UnityEngine;

public class BasicInteractController : MonoBehaviour
{
    public bool isInteractable = true; // 是否可交互

    void OnEnable()
    {
        // 订阅指挥棒的基础交互信号
        EventManager.Subscribe("Conductor_BasicInteract", DoBasicInteract);
    }

    void OnDisable()
    {
        EventManager.Unsubscribe("Conductor_BasicInteract", DoBasicInteract);
    }

    // 基础交互逻辑（如触发开关、拾取物品）
    void DoBasicInteract()
    {
        if (isInteractable)
        {
            Debug.Log($"{gameObject.name} 触发指挥棒基础交互");
            // 可添加具体交互：如打开门、点亮灯光等
        }
    }
}
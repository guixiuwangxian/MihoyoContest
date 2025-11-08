using UnityEngine;

public class RhythmHeartCollector : MonoBehaviour
{
    public float speedMultiplier = 1.5f; // 速率提升倍数
    private bool isCollected = false; // 是否已收集（唯一标记）

    void OnTriggerEnter(Collider other)
    {
        // 仅在未收集且碰撞到节奏之心时触发
        if (!isCollected && other.CompareTag("RhythmHeart"))
        {
            isCollected = true;
            Destroy(other.gameObject); // 收集后销毁唯一实例
            Debug.Log("获得了节奏之心！所有交互速率大幅提升");
            UpdateAllInteractSpeed(); // 永久提升交互速率
        }
    }

    // 永久提升所有交互物体速率
    void UpdateAllInteractSpeed()
    {
        // 提升旋转结构速率（螺旋号角关联）
        RotateStructureController[] rotateControllers = FindObjectsOfType<RotateStructureController>();
        foreach (var controller in rotateControllers)
        {
            controller.IncreaseSpeed(speedMultiplier);
        }
    }
}
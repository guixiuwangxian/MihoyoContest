using UnityEngine;

public class RhythmHeartCollector : MonoBehaviour
{
    [Header("速率设置")]
    public float speedMultiplier = 1.5f;

    private int heartsCollected = 0;
    private float currentMultiplier = 1f;

    public void OnHeartCollected()
    {
        heartsCollected++;
        currentMultiplier = 1f + (heartsCollected * (speedMultiplier - 1f));
        ApplySpeedBoost();

        Debug.Log($"节奏之心收集: {heartsCollected}个, 速度倍率: {currentMultiplier}x");
    }

    void ApplySpeedBoost()
    {
        // 提升旋转结构
        RotateStructureController[] rotateControllers = FindObjectsOfType<RotateStructureController>();
        foreach (var controller in rotateControllers)
        {
            controller.IncreaseSpeed(currentMultiplier);
        }

        // 提升平移方块
        TranslateBlock[] translateBlocks = FindObjectsOfType<TranslateBlock>();
        foreach (var block in translateBlocks)
        {
            block.SetSpeedMultiplier(currentMultiplier);
        }

        // 提升旋转方块
        RotateBlock[] rotateBlocks = FindObjectsOfType<RotateBlock>();
        foreach (var block in rotateBlocks)
        {
            block.SetSpeedMultiplier(currentMultiplier);
        }
    }

    // 可选：用于调试或存档
    public int GetCollectedCount() => heartsCollected;
    public float GetCurrentMultiplier() => currentMultiplier;
}
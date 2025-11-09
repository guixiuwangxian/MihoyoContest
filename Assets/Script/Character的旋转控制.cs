using UnityEngine;

public class PlayerOnRotateBlock : MonoBehaviour
{
    [Header("旋转设置")]
    public float gravityRotationSpeed = 5f;

    private RotateBlock currentRotateBlock;
    private bool isOnRotateBlock = false;
    private bool isGravityAligned = true;

    void Update()
    {
        if (isOnRotateBlock && currentRotateBlock != null)
        {
            HandleGravityRotation();
        }
    }

    public void SetCurrentRotateBlock(RotateBlock block)
    {
        currentRotateBlock = block;
        isOnRotateBlock = true;

        // 检查初始对齐状态
        if (currentRotateBlock.requireAlignment)
        {
            isGravityAligned = currentRotateBlock.IsAlignedWithAxis(transform);
        }
    }

    public void ClearRotateBlock()
    {
        currentRotateBlock = null;
        isOnRotateBlock = false;
        isGravityAligned = true;
    }

    void HandleGravityRotation()
    {
        if (currentRotateBlock.shapeType == RotateBlock.RotateShape.Special)
        {
            // 在特殊旋转体上，玩家跟随旋转
            if (currentRotateBlock.IsInGravityZone(transform.position))
            {
                // 平滑旋转玩家朝向
                Quaternion targetRotation = currentRotateBlock.transform.rotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, gravityRotationSpeed * Time.deltaTime);

                // 检查对齐状态
                if (currentRotateBlock.requireAlignment)
                {
                    isGravityAligned = currentRotateBlock.IsAlignedWithAxis(transform);
                }
            }
        }
    }

    public void OnRotationComplete()
    {
        // 旋转完成后强制检查对齐
        if (currentRotateBlock != null && currentRotateBlock.requireAlignment)
        {
            isGravityAligned = currentRotateBlock.IsAlignedWithAxis(transform);
        }
    }

    // 供玩家移动脚本调用，检查是否可以移动
    public bool CanMove()
    {
        return isGravityAligned;
    }
}
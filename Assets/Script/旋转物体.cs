using UnityEngine;

public class RotateBlock : MonoBehaviour
{
    public enum RotateShape { L_Shape, T_Shape, I_Shape, Special }

    [Header("旋转设置")]
    public RotateShape shapeType = RotateShape.L_Shape;
    public Vector3 rotationAxis = Vector3.up;
    public float rotationAngle = 90f;
    public float rotationSpeed = 45f;
    public bool isSingleTrigger = true;

    [Header("特殊重力设置")]
    public Transform gravityZone; // 重力区域（仅Special类型使用）
    public bool requireAlignment = true; // 是否需要对齐才能移动

    private Quaternion targetRotation;
    private bool isRotating = false;
    private bool hasRotated = false;
    private PlayerOnRotateBlock playerOnBlock;

    void Start()
    {
        targetRotation = transform.rotation;
    }

    void OnEnable()
    {
        // 订阅螺旋号角事件
        EventManager.Subscribe("Horn_RotateStructure", TriggerRotation);
    }

    void OnDisable()
    {
        EventManager.Unsubscribe("Horn_RotateStructure", TriggerRotation);
    }

    void Update()
    {
        if (isRotating)
        {
            PerformRotation();
        }
    }

    public void TriggerRotation()
    {
        if (isSingleTrigger && hasRotated) return;

        // 计算目标旋转
        targetRotation = Quaternion.AngleAxis(rotationAngle, rotationAxis) * targetRotation;
        isRotating = true;
        hasRotated = true;
    }

    void PerformRotation()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
        {
            isRotating = false;
            transform.rotation = targetRotation;

            // 特殊旋转体：旋转完成后检查重力对齐
            if (shapeType == RotateShape.Special && playerOnBlock != null)
            {
                playerOnBlock.OnRotationComplete();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && shapeType == RotateShape.Special)
        {
            PlayerOnRotateBlock player = other.GetComponent<PlayerOnRotateBlock>();
            if (player != null)
            {
                playerOnBlock = player;
                player.SetCurrentRotateBlock(this);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && playerOnBlock != null)
        {
            playerOnBlock.ClearRotateBlock();
            playerOnBlock = null;
        }
    }

    // 检查是否在重力区域内
    public bool IsInGravityZone(Vector3 playerPosition)
    {
        if (gravityZone == null) return false;

        Collider zoneCollider = gravityZone.GetComponent<Collider>();
        return zoneCollider.bounds.Contains(playerPosition);
    }

    // 检查是否对齐轴平面
    public bool IsAlignedWithAxis(Transform playerTransform)
    {
        // 简单实现：检查玩家是否大致站在平面上
        Vector3 playerUp = playerTransform.up;
        Vector3 surfaceNormal = transform.up;

        return Vector3.Angle(playerUp, surfaceNormal) < 15f;
    }

    // 设置速度倍率
    public void SetSpeedMultiplier(float multiplier)
    {
        rotationSpeed *= multiplier;
    }
}
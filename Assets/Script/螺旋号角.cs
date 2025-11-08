using UnityEngine;

public class RotateStructureController : MonoBehaviour
{
    public float baseRotateSpeed = 2f; // 基础旋转速率
    private bool isRotating = false;

    void OnEnable()
    {
        // 订阅螺旋号角的旋转信号
        EventManager.Subscribe("Horn_RotateStructure", StartRotate);
    }

    void OnDisable()
    {
        EventManager.Unsubscribe("Horn_RotateStructure", StartRotate);
    }

    void Update()
    {
        if (isRotating)
        {
            // 绕Y轴旋转（可修改旋转轴和方向）
            transform.Rotate(Vector3.up, baseRotateSpeed * Time.deltaTime);
        }
    }

    // 开始旋转
    void StartRotate()
    {
        isRotating = true;
    }

    // 停止旋转（在CharacterMusicController中补充鼠标松开逻辑时调用）
    public void StopRotate()
    {
        isRotating = false;
    }

    // 供节奏之心调用，提升旋转速率
    public void IncreaseSpeed(float multiplier)
    {
        baseRotateSpeed *= multiplier;
    }
}
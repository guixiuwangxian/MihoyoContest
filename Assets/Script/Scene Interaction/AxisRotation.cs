using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisRotation : MonoBehaviour
{
    [Header("旋转设置")]
    public Vector3 rotationAxis = Vector3.up; // 旋转轴方向
    public Vector3 axisPosition = Vector3.zero; // 旋转轴位置（局部坐标）
    public float rotationAngle = 90f; // 旋转角度
    public float rotationSpeed = 90f; // 旋转速度（度/秒）

    [Header("控制设置")]
    public KeyCode rotateKey = KeyCode.Space; // 触发旋转的按键
    public bool rotateOnStart = false; // 是否在开始时自动旋转

    [Header("状态")]
    [SerializeField] private bool isRotating = false;
    [SerializeField] private float currentAngle = 0f;
    [SerializeField] private float targetAngle = 0f;

    // 旋转前的初始状态
    private Quaternion initialRotation;
    private Vector3 initialPosition;
    private Vector3 worldAxisPosition;

    void Start()
    {
        // 保存初始状态
        initialRotation = transform.rotation;
        initialPosition = transform.position;

        // 计算世界坐标系下的轴位置
        worldAxisPosition = transform.TransformPoint(axisPosition);

        if (rotateOnStart)
        {
            StartRotation();
        }
    }

    void Update()
    {
        // 按键触发旋转
        if (Input.GetKeyDown(rotateKey) && !isRotating)
        {
            StartRotation();
        }

        // 执行旋转
        if (isRotating)
        {
            PerformRotation();
        }
    }

    void StartRotation()
    {
        isRotating = true;
        currentAngle = 0f;
        targetAngle = rotationAngle;

        // 更新世界坐标系下的轴位置
        worldAxisPosition = transform.TransformPoint(axisPosition);

        // 保存初始状态
        initialRotation = transform.rotation;
        initialPosition = transform.position;
    }

    void PerformRotation()
    {
        // 计算这一帧应该旋转的角度
        float step = rotationSpeed * Time.deltaTime;
        float remainingAngle = targetAngle - currentAngle;

        // 如果剩余角度小于步长，直接旋转到目标角度
        if (Mathf.Abs(remainingAngle) <= step)
        {
            currentAngle = targetAngle;
            isRotating = false;
        }
        else
        {
            // 否则旋转一个步长
            currentAngle += Mathf.Sign(remainingAngle) * step;
        }

        // 应用旋转
        ApplyRotation(currentAngle);
    }

    void ApplyRotation(float angle)
    {
        // 计算旋转四元数
        Quaternion rotation = Quaternion.AngleAxis(angle, rotationAxis.normalized);

        // 计算相对于轴的位置偏移
        Vector3 offsetFromAxis = initialPosition - worldAxisPosition;

        // 应用旋转到偏移量
        Vector3 rotatedOffset = rotation * offsetFromAxis;

        // 更新位置和旋转
        transform.position = worldAxisPosition + rotatedOffset;
        transform.rotation = initialRotation * rotation;
    }

    // 公共方法供其他脚本调用
    public void StartRotation(float angle)
    {
        rotationAngle = angle;
        StartRotation();
    }

    public void StartRotation(Vector3 axis, float angle)
    {
        rotationAxis = axis;
        rotationAngle = angle;
        StartRotation();
    }

    public void StartRotation(Vector3 axis, Vector3 position, float angle)
    {
        rotationAxis = axis;
        axisPosition = position;
        rotationAngle = angle;
        StartRotation();
    }

    public void StopRotation()
    {
        isRotating = false;
    }

    public void ResetRotation()
    {
        isRotating = false;
        transform.rotation = initialRotation;
        transform.position = initialPosition;
        currentAngle = 0f;
    }

    // Inspector便捷按钮
    [ContextMenu("开始旋转")]
    public void StartRotationFromInspector()
    {
        StartRotation();
    }

    [ContextMenu("停止旋转")]
    public void StopRotationFromInspector()
    {
        StopRotation();
    }

    [ContextMenu("重置旋转")]
    public void ResetRotationFromInspector()
    {
        ResetRotation();
    }

    // 在Scene视图中绘制旋转轴和相关信息
    void OnDrawGizmosSelected()
    {
        // 计算世界坐标系下的轴位置
        Vector3 worldAxisPos = Application.isPlaying ?
            worldAxisPosition : transform.TransformPoint(axisPosition);

        // 绘制旋转轴
        Gizmos.color = Color.red;
        Vector3 axisDirection = rotationAxis.normalized;
        float axisLength = 2f;

        // 绘制轴线段
        Gizmos.DrawLine(
            worldAxisPos - axisDirection * axisLength,
            worldAxisPos + axisDirection * axisLength
        );

        // 绘制轴端点
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(worldAxisPos - axisDirection * axisLength, 0.1f);
        Gizmos.DrawWireSphere(worldAxisPos + axisDirection * axisLength, 0.1f);

        // 绘制轴位置点
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(worldAxisPos, 0.15f);

        // 绘制旋转轨迹
        if (!Application.isPlaying || isRotating)
        {
            DrawRotationTrajectory(worldAxisPos);
        }

        // 显示旋转信息
#if UNITY_EDITOR
        string info = $"旋转轴: {rotationAxis}\n";
        info += $"轴位置: {axisPosition}\n";
        info += $"旋转角度: {rotationAngle}°\n";
        info += $"旋转速度: {rotationSpeed}°/s\n";
        if (Application.isPlaying)
        {
            info += $"当前角度: {currentAngle:F1}°\n";
            info += isRotating ? "状态: 旋转中" : "状态: 静止";
        }

        UnityEditor.Handles.Label(worldAxisPos + Vector3.up * 0.3f, info);
#endif
    }

    void DrawRotationTrajectory(Vector3 axisWorldPos)
    {
        // 计算物体相对于轴的位置
        Vector3 objectOffset = transform.position - axisWorldPos;

        // 如果物体在轴上，不绘制轨迹
        if (objectOffset.magnitude < 0.01f) return;

        // 计算旋转平面法线（即旋转轴）
        Vector3 planeNormal = rotationAxis.normalized;

        // 计算物体在旋转平面上的投影
        Vector3 projectedOffset = objectOffset - Vector3.Dot(objectOffset, planeNormal) * planeNormal;

        if (projectedOffset.magnitude < 0.01f) return;

        // 计算旋转半径
        float radius = projectedOffset.magnitude;

        // 绘制旋转轨迹圆
        Gizmos.color = new Color(0, 1, 1, 0.5f);

        int segments = 36;
        Vector3 prevPoint = axisWorldPos + projectedOffset.normalized * radius;

        for (int i = 1; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2;
            Vector3 dir = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, planeNormal) * projectedOffset.normalized;
            Vector3 newPoint = axisWorldPos + dir * radius;

            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }

        // 绘制当前角度指示
        if (Application.isPlaying && isRotating)
        {
            Gizmos.color = Color.yellow;
            Vector3 currentDir = Quaternion.AngleAxis(currentAngle, planeNormal) * projectedOffset.normalized;
            Gizmos.DrawLine(axisWorldPos, axisWorldPos + currentDir * radius);

            // 绘制目标角度指示
            Gizmos.color = Color.green;
            Vector3 targetDir = Quaternion.AngleAxis(targetAngle, planeNormal) * projectedOffset.normalized;
            Gizmos.DrawLine(axisWorldPos, axisWorldPos + targetDir * radius);
        }
    }

    // 当Inspector值改变时更新
    void OnValidate()
    {
        // 确保旋转轴不为零
        if (rotationAxis.magnitude < 0.01f)
        {
            rotationAxis = Vector3.up;
        }

        // 限制旋转速度为正数
        rotationSpeed = Mathf.Max(0.1f, rotationSpeed);
    }
}
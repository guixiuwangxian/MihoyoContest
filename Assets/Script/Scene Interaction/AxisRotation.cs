using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisRotation : MonoBehaviour
{
    [Header("��ת����")]
    public Vector3 rotationAxis = Vector3.up; // ��ת�᷽��
    public Vector3 axisPosition = Vector3.zero; // ��ת��λ�ã��ֲ����꣩
    public float rotationAngle = 90f; // ��ת�Ƕ�
    public float rotationSpeed = 90f; // ��ת�ٶȣ���/�룩

    [Header("��������")]
    public KeyCode rotateKey = KeyCode.Space; // ������ת�İ���
    public bool rotateOnStart = false; // �Ƿ��ڿ�ʼʱ�Զ���ת

    [Header("״̬")]
    [SerializeField] private bool isRotating = false;
    [SerializeField] private float currentAngle = 0f;
    [SerializeField] private float targetAngle = 0f;

    // ��תǰ�ĳ�ʼ״̬
    private Quaternion initialRotation;
    private Vector3 initialPosition;
    private Vector3 worldAxisPosition;

    void Start()
    {
        // �����ʼ״̬
        initialRotation = transform.rotation;
        initialPosition = transform.position;

        // ������������ϵ�µ���λ��
        worldAxisPosition = transform.TransformPoint(axisPosition);

        if (rotateOnStart)
        {
            StartRotation();
        }
    }

    void Update()
    {
        // ����������ת
        if (Input.GetKeyDown(rotateKey) && !isRotating)
        {
            StartRotation();
        }

        // ִ����ת
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

        // ������������ϵ�µ���λ��
        worldAxisPosition = transform.TransformPoint(axisPosition);

        // �����ʼ״̬
        initialRotation = transform.rotation;
        initialPosition = transform.position;
    }

    void PerformRotation()
    {
        // ������һ֡Ӧ����ת�ĽǶ�
        float step = rotationSpeed * Time.deltaTime;
        float remainingAngle = targetAngle - currentAngle;

        // ���ʣ��Ƕ�С�ڲ�����ֱ����ת��Ŀ��Ƕ�
        if (Mathf.Abs(remainingAngle) <= step)
        {
            currentAngle = targetAngle;
            isRotating = false;
        }
        else
        {
            // ������תһ������
            currentAngle += Mathf.Sign(remainingAngle) * step;
        }

        // Ӧ����ת
        ApplyRotation(currentAngle);
    }

    void ApplyRotation(float angle)
    {
        // ������ת��Ԫ��
        Quaternion rotation = Quaternion.AngleAxis(angle, rotationAxis.normalized);

        // ������������λ��ƫ��
        Vector3 offsetFromAxis = initialPosition - worldAxisPosition;

        // Ӧ����ת��ƫ����
        Vector3 rotatedOffset = rotation * offsetFromAxis;

        // ����λ�ú���ת
        transform.position = worldAxisPosition + rotatedOffset;
        transform.rotation = initialRotation * rotation;
    }

    // ���������������ű�����
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

    // Inspector��ݰ�ť
    [ContextMenu("��ʼ��ת")]
    public void StartRotationFromInspector()
    {
        StartRotation();
    }

    [ContextMenu("ֹͣ��ת")]
    public void StopRotationFromInspector()
    {
        StopRotation();
    }

    [ContextMenu("������ת")]
    public void ResetRotationFromInspector()
    {
        ResetRotation();
    }

    // ��Scene��ͼ�л�����ת��������Ϣ
    void OnDrawGizmosSelected()
    {
        // ������������ϵ�µ���λ��
        Vector3 worldAxisPos = Application.isPlaying ?
            worldAxisPosition : transform.TransformPoint(axisPosition);

        // ������ת��
        Gizmos.color = Color.red;
        Vector3 axisDirection = rotationAxis.normalized;
        float axisLength = 2f;

        // �������߶�
        Gizmos.DrawLine(
            worldAxisPos - axisDirection * axisLength,
            worldAxisPos + axisDirection * axisLength
        );

        // ������˵�
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(worldAxisPos - axisDirection * axisLength, 0.1f);
        Gizmos.DrawWireSphere(worldAxisPos + axisDirection * axisLength, 0.1f);

        // ������λ�õ�
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(worldAxisPos, 0.15f);

        // ������ת�켣
        if (!Application.isPlaying || isRotating)
        {
            DrawRotationTrajectory(worldAxisPos);
        }

        // ��ʾ��ת��Ϣ
#if UNITY_EDITOR
        string info = $"��ת��: {rotationAxis}\n";
        info += $"��λ��: {axisPosition}\n";
        info += $"��ת�Ƕ�: {rotationAngle}��\n";
        info += $"��ת�ٶ�: {rotationSpeed}��/s\n";
        if (Application.isPlaying)
        {
            info += $"��ǰ�Ƕ�: {currentAngle:F1}��\n";
            info += isRotating ? "״̬: ��ת��" : "״̬: ��ֹ";
        }

        UnityEditor.Handles.Label(worldAxisPos + Vector3.up * 0.3f, info);
#endif
    }

    void DrawRotationTrajectory(Vector3 axisWorldPos)
    {
        // ����������������λ��
        Vector3 objectOffset = transform.position - axisWorldPos;

        // ������������ϣ������ƹ켣
        if (objectOffset.magnitude < 0.01f) return;

        // ������תƽ�淨�ߣ�����ת�ᣩ
        Vector3 planeNormal = rotationAxis.normalized;

        // ������������תƽ���ϵ�ͶӰ
        Vector3 projectedOffset = objectOffset - Vector3.Dot(objectOffset, planeNormal) * planeNormal;

        if (projectedOffset.magnitude < 0.01f) return;

        // ������ת�뾶
        float radius = projectedOffset.magnitude;

        // ������ת�켣Բ
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

        // ���Ƶ�ǰ�Ƕ�ָʾ
        if (Application.isPlaying && isRotating)
        {
            Gizmos.color = Color.yellow;
            Vector3 currentDir = Quaternion.AngleAxis(currentAngle, planeNormal) * projectedOffset.normalized;
            Gizmos.DrawLine(axisWorldPos, axisWorldPos + currentDir * radius);

            // ����Ŀ��Ƕ�ָʾ
            Gizmos.color = Color.green;
            Vector3 targetDir = Quaternion.AngleAxis(targetAngle, planeNormal) * projectedOffset.normalized;
            Gizmos.DrawLine(axisWorldPos, axisWorldPos + targetDir * radius);
        }
    }

    // ��Inspectorֵ�ı�ʱ����
    void OnValidate()
    {
        // ȷ����ת�᲻Ϊ��
        if (rotationAxis.magnitude < 0.01f)
        {
            rotationAxis = Vector3.up;
        }

        // ������ת�ٶ�Ϊ����
        rotationSpeed = Mathf.Max(0.1f, rotationSpeed);
    }
}
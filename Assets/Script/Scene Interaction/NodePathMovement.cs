using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodePathMovement : MonoBehaviour
{
    [System.Serializable]
    public class PathNode
    {
        public Vector3 position;
        public string name = "�ڵ�";

        public PathNode(Vector3 pos)
        {
            position = pos;
            name = "�ڵ�";
        }
    }

    [Header("·���ڵ�����")]
    public List<PathNode> nodes = new List<PathNode>();
    public float movementSpeed = 2f;

    [Header("����������")]
    public MouseButton forwardButton = MouseButton.Left;
    public MouseButton backwardButton = MouseButton.Right;

    public enum MouseButton { Left = 0, Right = 1, Middle = 2 }

    [Header("״̬")]
    [SerializeField] private int currentSegmentIndex = 0;
    [SerializeField] private float segmentProgress = 0f;
    [SerializeField] private bool isMoving = false;
    [SerializeField] private MovementDirection currentDirection = MovementDirection.Forward;

    private enum MovementDirection { Forward, Backward, None }

    void Start()
    {
        // ȷ�������������ڵ�
        if (nodes.Count < 2)
        {
            Debug.LogWarning("·����Ҫ���������ڵ㣡");
            return;
        }

        // ���ó�ʼλ�õ���һ���ڵ�
        transform.position = nodes[0].position;
        currentSegmentIndex = 0;
        segmentProgress = 0f;
    }

    void Update()
    {
        if (nodes.Count < 2) return;

        HandleInput();
        UpdateMovement();
    }

    void HandleInput()
    {
        bool forwardPressed = Input.GetMouseButton((int)forwardButton);
        bool backwardPressed = Input.GetMouseButton((int)backwardButton);

        if (forwardPressed && !backwardPressed)
        {
            // �����ƶ�
            currentDirection = MovementDirection.Forward;
            isMoving = true;
        }
        else if (backwardPressed && !forwardPressed)
        {
            // �����ƶ�
            currentDirection = MovementDirection.Backward;
            isMoving = true;
        }
        else
        {
            // ֹͣ�ƶ�
            isMoving = false;
        }
    }

    void UpdateMovement()
    {
        if (!isMoving) return;

        float speed = movementSpeed * Time.deltaTime;
        float segmentLength = GetCurrentSegmentLength();

        if (currentDirection == MovementDirection.Forward)
        {
            segmentProgress += speed / segmentLength;

            // ����Ƿ񵽴ﵱǰ�߶ε��յ�
            if (segmentProgress >= 1f)
            {
                // �ƶ�����һ���߶�
                if (currentSegmentIndex < nodes.Count - 2)
                {
                    currentSegmentIndex++;
                    segmentProgress = 0f;
                }
                else
                {
                    // �ѵ���·���յ�
                    segmentProgress = 1f;
                    isMoving = false;
                }
            }
        }
        else if (currentDirection == MovementDirection.Backward)
        {
            segmentProgress -= speed / segmentLength;

            // ����Ƿ񵽴ﵱǰ�߶ε����
            if (segmentProgress <= 0f)
            {
                // �ƶ�����һ���߶�
                if (currentSegmentIndex > 0)
                {
                    currentSegmentIndex--;
                    segmentProgress = 1f;
                }
                else
                {
                    // �ѵ���·�����
                    segmentProgress = 0f;
                    isMoving = false;
                }
            }
        }

        // ����λ��
        UpdatePosition();
    }

    void UpdatePosition()
    {
        if (nodes.Count < 2) return;

        Vector3 start = nodes[currentSegmentIndex].position;
        Vector3 end = nodes[currentSegmentIndex + 1].position;

        transform.position = Vector3.Lerp(start, end, segmentProgress);
    }

    float GetCurrentSegmentLength()
    {
        if (currentSegmentIndex >= nodes.Count - 1) return 0f;

        return Vector3.Distance(nodes[currentSegmentIndex].position, nodes[currentSegmentIndex + 1].position);
    }

    // ��Inspector����ӽڵ�
    [ContextMenu("��ӽڵ�")]
    public void AddNode()
    {
        Vector3 newPos = nodes.Count > 0 ? nodes[nodes.Count - 1].position + Vector3.forward : transform.position;
        nodes.Add(new PathNode(newPos));
    }

    [ContextMenu("��ĩβ��ӵ�ǰ�ڵ�")]
    public void AddCurrentPositionAsNode()
    {
        nodes.Add(new PathNode(transform.position));
    }

    [ContextMenu("���õ����")]
    public void ResetToStart()
    {
        if (nodes.Count > 0)
        {
            transform.position = nodes[0].position;
            currentSegmentIndex = 0;
            segmentProgress = 0f;
            isMoving = false;
        }
    }

    [ContextMenu("��ת���յ�")]
    public void JumpToEnd()
    {
        if (nodes.Count > 0)
        {
            transform.position = nodes[nodes.Count - 1].position;
            currentSegmentIndex = Mathf.Max(0, nodes.Count - 2);
            segmentProgress = 1f;
            isMoving = false;
        }
    }

    // ��Scene��ͼ�л���·���ͽڵ�
    void OnDrawGizmosSelected()
    {
        if (nodes == null || nodes.Count < 2) return;

        // ����·����
        Gizmos.color = Color.white;
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            if (nodes[i] != null && nodes[i + 1] != null)
            {
                Gizmos.DrawLine(nodes[i].position, nodes[i + 1].position);
            }
        }

        // ���ƽڵ�
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] != null)
            {
                // �����յ��ò�ͬ��ɫ
                if (i == 0)
                    Gizmos.color = Color.green; // ��� - ��ɫ
                else if (i == nodes.Count - 1)
                    Gizmos.color = Color.red;   // �յ� - ��ɫ
                else
                    Gizmos.color = Color.blue;  // �м�ڵ� - ��ɫ

                Gizmos.DrawSphere(nodes[i].position, 0.2f);

                // ���ƽڵ��ǩ
#if UNITY_EDITOR
                UnityEditor.Handles.Label(nodes[i].position + Vector3.up * 0.3f, $"{nodes[i].name} ({i})");
#endif
            }
        }

        // ���Ƶ�ǰλ�úͷ���ָʾ��
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.25f);

            // �����ƶ�����ָʾ
            if (isMoving)
            {
                Vector3 direction = currentDirection == MovementDirection.Forward ?
                    (nodes[currentSegmentIndex + 1].position - nodes[currentSegmentIndex].position).normalized :
                    (nodes[currentSegmentIndex].position - nodes[currentSegmentIndex + 1].position).normalized;

                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(transform.position, direction * 0.5f);
            }
        }
    }

    // ��Inspector����ʾ���õ���Ϣ
    void OnValidate()
    {
        // ȷ���ڵ��б�Ϊ��
        if (nodes == null)
            nodes = new List<PathNode>();

        // ȷ�������������ڵ�
        if (nodes.Count == 0)
        {
            nodes.Add(new PathNode(transform.position));
            nodes.Add(new PathNode(transform.position + Vector3.forward));
        }
        else if (nodes.Count == 1)
        {
            nodes.Add(new PathNode(nodes[0].position + Vector3.forward));
        }
    }
}
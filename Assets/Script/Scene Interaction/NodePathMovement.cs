using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodePathMovement : MonoBehaviour
{
    [System.Serializable]
    public class PathNode
    {
        public Vector3 position;
        public string name = "节点";

        public PathNode(Vector3 pos)
        {
            position = pos;
            name = "节点";
        }
    }

    [Header("路径节点设置")]
    public List<PathNode> nodes = new List<PathNode>();
    public float movementSpeed = 2f;

    [Header("鼠标控制设置")]
    public MouseButton forwardButton = MouseButton.Left;
    public MouseButton backwardButton = MouseButton.Right;

    public enum MouseButton { Left = 0, Right = 1, Middle = 2 }

    [Header("状态")]
    [SerializeField] private int currentSegmentIndex = 0;
    [SerializeField] private float segmentProgress = 0f;
    [SerializeField] private bool isMoving = false;
    [SerializeField] private MovementDirection currentDirection = MovementDirection.Forward;

    private enum MovementDirection { Forward, Backward, None }

    void Start()
    {
        // 确保至少有两个节点
        if (nodes.Count < 2)
        {
            Debug.LogWarning("路径需要至少两个节点！");
            return;
        }

        // 设置初始位置到第一个节点
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
            // 正向移动
            currentDirection = MovementDirection.Forward;
            isMoving = true;
        }
        else if (backwardPressed && !forwardPressed)
        {
            // 反向移动
            currentDirection = MovementDirection.Backward;
            isMoving = true;
        }
        else
        {
            // 停止移动
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

            // 检查是否到达当前线段的终点
            if (segmentProgress >= 1f)
            {
                // 移动到下一个线段
                if (currentSegmentIndex < nodes.Count - 2)
                {
                    currentSegmentIndex++;
                    segmentProgress = 0f;
                }
                else
                {
                    // 已到达路径终点
                    segmentProgress = 1f;
                    isMoving = false;
                }
            }
        }
        else if (currentDirection == MovementDirection.Backward)
        {
            segmentProgress -= speed / segmentLength;

            // 检查是否到达当前线段的起点
            if (segmentProgress <= 0f)
            {
                // 移动到上一个线段
                if (currentSegmentIndex > 0)
                {
                    currentSegmentIndex--;
                    segmentProgress = 1f;
                }
                else
                {
                    // 已到达路径起点
                    segmentProgress = 0f;
                    isMoving = false;
                }
            }
        }

        // 更新位置
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

    // 在Inspector中添加节点
    [ContextMenu("添加节点")]
    public void AddNode()
    {
        Vector3 newPos = nodes.Count > 0 ? nodes[nodes.Count - 1].position + Vector3.forward : transform.position;
        nodes.Add(new PathNode(newPos));
    }

    [ContextMenu("在末尾添加当前节点")]
    public void AddCurrentPositionAsNode()
    {
        nodes.Add(new PathNode(transform.position));
    }

    [ContextMenu("重置到起点")]
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

    [ContextMenu("跳转到终点")]
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

    // 在Scene视图中绘制路径和节点
    void OnDrawGizmosSelected()
    {
        if (nodes == null || nodes.Count < 2) return;

        // 绘制路径线
        Gizmos.color = Color.white;
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            if (nodes[i] != null && nodes[i + 1] != null)
            {
                Gizmos.DrawLine(nodes[i].position, nodes[i + 1].position);
            }
        }

        // 绘制节点
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] != null)
            {
                // 起点和终点用不同颜色
                if (i == 0)
                    Gizmos.color = Color.green; // 起点 - 绿色
                else if (i == nodes.Count - 1)
                    Gizmos.color = Color.red;   // 终点 - 红色
                else
                    Gizmos.color = Color.blue;  // 中间节点 - 蓝色

                Gizmos.DrawSphere(nodes[i].position, 0.2f);

                // 绘制节点标签
#if UNITY_EDITOR
                UnityEditor.Handles.Label(nodes[i].position + Vector3.up * 0.3f, $"{nodes[i].name} ({i})");
#endif
            }
        }

        // 绘制当前位置和方向指示器
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.25f);

            // 绘制移动方向指示
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

    // 在Inspector中显示有用的信息
    void OnValidate()
    {
        // 确保节点列表不为空
        if (nodes == null)
            nodes = new List<PathNode>();

        // 确保至少有两个节点
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
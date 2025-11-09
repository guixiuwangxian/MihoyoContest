using UnityEngine;

public class TranslateBlock : MonoBehaviour
{
    [Header("平移设置")]
    public Vector3 moveDirection = Vector3.forward;
    public float moveSpeed = 2f;
    public float maxMoveDistance = 5f;

    private Vector3 startPosition;
    private bool isMoving = false;
    private Vector3 moveVelocity = Vector3.zero;
    private float currentMoveDistance = 0f;
    private float speedMultiplier = 1f;

    void Start()
    {
        startPosition = transform.position;
    }

    void OnEnable()
    {
        // 订阅螺旋号角事件
        EventManager.Subscribe("Horn_RotateStructure", OnHornActivated);
    }

    void OnDisable()
    {
        EventManager.Unsubscribe("Horn_RotateStructure", OnHornActivated);
    }

    void Update()
    {
        if (isMoving)
        {
            HandleMovement();
        }
    }

    void OnHornActivated()
    {
        // 左键前进，右键返回
        if (Input.GetMouseButton(0))
        {
            StartMoving(1f); // 前进
        }
        else if (Input.GetMouseButton(1))
        {
            StartMoving(-1f); // 返回
        }

        // 鼠标松开时停止
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            StopMoving();
        }
    }

    void StartMoving(float direction)
    {
        isMoving = true;
        moveVelocity = moveDirection.normalized * moveSpeed * speedMultiplier * direction;
    }

    void HandleMovement()
    {
        Vector3 movement = moveVelocity * Time.deltaTime;
        float potentialDistance = currentMoveDistance + movement.magnitude * Mathf.Sign(moveVelocity.x + moveVelocity.y + moveVelocity.z);

        // 检查移动范围限制
        if (Mathf.Abs(potentialDistance) <= maxMoveDistance)
        {
            transform.position += movement;
            currentMoveDistance = potentialDistance;
        }
        else
        {
            // 到达边界，停止移动
            StopMoving();
        }
    }

    void StopMoving()
    {
        isMoving = false;
        moveVelocity = Vector3.zero;
    }

    // 供节奏之心调用提升速度
    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    void OnDrawGizmosSelected()
    {
        // 可视化移动范围
        Gizmos.color = Color.blue;
        Vector3 endPoint = transform.position + moveDirection.normalized * maxMoveDistance;
        Gizmos.DrawLine(transform.position, endPoint);
        Gizmos.DrawWireCube(endPoint, Vector3.one * 0.5f);
    }
}
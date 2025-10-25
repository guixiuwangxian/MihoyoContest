using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMove : MonoBehaviour
{
    Animator anim;
    private CharacterController characterController;
    public float RotateSpeed = 10;
    public float MoveSpeed = 5;

    // 状态变量
    private bool StartWalk { get; set; }
    private bool Walk { get; set; }
    private bool Run { get; set; }
    private bool IsJumping { get; set; }
    private bool IsGrounded { get; set; }

    // 跳跃相关
    public float JumpHeight = 2.0f;
    public float Gravity = -9.81f;
    public float GroundCheckDistance = 0.1f;
    public LayerMask GroundLayer = 1;

    // 内部变量
    private bool wasMoving;
    private bool isMoving;
    private bool runKey;
    private float verticalVelocity;
    private Vector3 moveDirection;

    void Start()
    {
        anim = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        // 如果没有角色控制器，添加一个
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
            characterController.center = new Vector3(0, 1, 0);
            characterController.height = 2.0f;
        }

        if (GroundLayer.value == 0)
            GroundLayer = LayerMask.GetMask("Default");
    }

    void UpdateAnim()
    {
        if (anim != null)
        {
            // 触发型参数
            if (StartWalk)
            {
                anim.SetTrigger("StartWalk");
                Debug.Log("触发 StartWalk 动画");
            }

            // 布尔型参数
            anim.SetBool("Walk", Walk);
            anim.SetBool("Run", Run);
            anim.SetBool("IsGrounded", IsGrounded);

            // 浮点型参数
            anim.SetFloat("VerticalVelocity", verticalVelocity);

            // 跳跃触发器在跳跃开始时设置一次
            if (IsJumping)
            {
                anim.SetTrigger("Jump");
                Debug.Log("触发 Jump 动画");
            }
        }
    }

    void Update()
    {
        HandleInput();
        HandleGravityAndJump();
        HandleMovement();
        UpdateAnim();

        wasMoving = isMoving;
    }

    void HandleInput()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // 移动状态判定
        isMoving = Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f;
        runKey = Input.GetKey(KeyCode.LeftShift);

        StartWalk = !wasMoving && isMoving && IsGrounded && !IsJumping;
        Run = isMoving && runKey && IsGrounded && !IsJumping;
        Walk = isMoving && !runKey && IsGrounded && !IsJumping;

        // 跳跃输入
        if (Input.GetButtonDown("Jump") && IsGrounded && !IsJumping)
        {
            IsJumping = true;
            verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
            Debug.Log("开始跳跃");
        }
    }

    void HandleGravityAndJump()
    {
        // 地面检测
        bool wasGrounded = IsGrounded;
        IsGrounded = characterController.isGrounded || CheckGroundWithRaycast();

        // 落地处理
        if (IsGrounded && !wasGrounded && IsJumping)
        {
            IsJumping = false;
            Debug.Log("落地");
        }

        // 处理重力
        if (IsGrounded)
        {
            if (verticalVelocity < 0)
            {
                verticalVelocity = -2f;
            }
        }
        else
        {
            verticalVelocity += Gravity * Time.deltaTime;
            verticalVelocity = Mathf.Max(verticalVelocity, -20f);
        }
    }

    bool CheckGroundWithRaycast()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        if (Physics.Raycast(rayStart, Vector3.down, out hit, GroundCheckDistance + 0.1f, GroundLayer))
        {
            return true;
        }
        return false;
    }

    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // 计算移动方向（基于世界坐标系）
        Vector3 inputDirection = new Vector3(h, 0, v);

        if (inputDirection.magnitude > 0.1f)
        {
            // 计算目标朝向
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotateSpeed * Time.deltaTime);
        }

        // 计算移动速度
        float currentSpeed = 0f;
        if (isMoving && IsGrounded && !IsJumping)
        {
            currentSpeed = Run ? MoveSpeed * 2f : MoveSpeed;
        }

        // 计算最终移动向量
        Vector3 move = transform.forward * currentSpeed * Time.deltaTime;
        move.y = verticalVelocity * Time.deltaTime; // 应用重力和跳跃

        // 应用移动
        if (characterController != null && characterController.enabled)
        {
            characterController.Move(move);
        }

        // 调试信息
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log($"移动输入: H={h}, V={v}, 速度={currentSpeed}, 移动向量={move}");
        }
    }

    // 可视化调试
    void OnDrawGizmosSelected()
    {
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.1f, Vector3.down * (GroundCheckDistance + 0.1f));
    }
}
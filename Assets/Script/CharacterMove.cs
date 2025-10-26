using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCharacterController : MonoBehaviour
{
    [Header("组件引用")]
    public Animator anim;
    private CharacterController controller;
    public Camera playerCamera; // 第一人称摄像头

    [Header("移动设置")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float mouseSensitivity = 2f;

    [Header("跳跃与重力设置")]
    public float jumpHeight = 1.5f;
    public float gravity = -15f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer = 1;

    // 状态变量
    private bool isMoving;
    private bool isRunning;
    private bool isJumping;
    private bool isGrounded;
    private bool wasGrounded;
    private bool jumpTriggered;

    // 物理变量
    private float verticalVelocity;

    // 视角控制
    private float cameraPitch = 0f;

    // 动画参数名称
    private const string WALK_PARAM = "Walk";
    private const string RUN_PARAM = "Run";
    private const string JUMP_PARAM = "Jump";
    private const string GROUNDED_PARAM = "IsGrounded";

    void Start()
    {
        // 获取组件
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        // 如果没有指定摄像头，使用主摄像头
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // 确保摄像头是角色的子对象
        if (playerCamera.transform.parent != transform)
        {
            playerCamera.transform.SetParent(transform);
            playerCamera.transform.localPosition = new Vector3(0, 1.6f, 0); // 眼睛高度
        }

        // 如果没有CharacterController，自动添加
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
            controller.center = new Vector3(0, 1, 0);
            controller.height = 2.0f;
            controller.radius = 0.3f;
        }

        // 设置地面层级
        if (groundLayer.value == 0)
            groundLayer = LayerMask.GetMask("Default");

        // 锁定光标到屏幕中心
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // 处理鼠标视角
        HandleMouseLook();

        // 处理输入
        HandleInput();

        // 处理物理
        HandlePhysics();

        // 处理移动
        HandleMovement();

        // 更新动画
        UpdateAnimations();

        // 保存上一帧的地面状态
        wasGrounded = isGrounded;
    }

    void HandleMouseLook()
    {
        // 获取鼠标输入
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 左右旋转角色
        transform.Rotate(Vector3.up * mouseX);

        // 上下旋转摄像头
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);

        if (playerCamera != null)
        {
            playerCamera.transform.localEulerAngles = Vector3.right * cameraPitch;
        }
    }

    void HandleInput()
    {
        // 获取输入
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 判断是否在移动
        isMoving = Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f;

        // 在地面上时，根据Shift键设置跑步状态
        if (isGrounded && !isJumping)
        {
            isRunning = Input.GetKey(KeyCode.LeftShift) && isMoving;
        }

        // 跳跃输入
        if (Input.GetButtonDown("Jump") && isGrounded && !isJumping)
        {
            jumpTriggered = true;
        }
    }

    void HandlePhysics()
    {
        // 地面检测
        wasGrounded = isGrounded;
        isGrounded = controller.isGrounded;

        // 使用射线检测作为辅助地面检测
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance + 0.1f, groundLayer))
        {
            isGrounded = true;
        }

        // 处理跳跃
        if (jumpTriggered)
        {
            ExecuteJump();
            jumpTriggered = false;
        }

        // 落地处理
        if (isGrounded && !wasGrounded && isJumping)
        {
            OnLand();
        }

        // 重力处理
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
            verticalVelocity = Mathf.Max(verticalVelocity, -20f);
        }
    }

    void HandleMovement()
    {
        // 获取输入
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 第一人称移动：基于角色当前朝向
        Vector3 moveDirection = (transform.forward * vertical) + (transform.right * horizontal);
        moveDirection.y = 0;
        moveDirection.Normalize();

        // 计算移动速度
        float currentSpeed = 0f;
        if (isMoving)
        {
            if (isGrounded && !isJumping)
            {
                currentSpeed = isRunning ? runSpeed : walkSpeed;
            }
            else
            {
                currentSpeed = (isRunning ? runSpeed : walkSpeed) * 0.5f;
            }
        }

        // 计算移动向量
        Vector3 move = moveDirection * currentSpeed * Time.deltaTime;
        move.y = verticalVelocity * Time.deltaTime;

        // 应用移动
        if (controller != null && controller.enabled)
        {
            controller.Move(move);
        }
    }

    void UpdateAnimations()
    {
        if (anim == null) return;

        // 根据状态机条件设置动画参数
        // 设置行走状态 - 当有移动输入且不在跑步状态时
        bool shouldWalk = isMoving && !isRunning && isGrounded && !isJumping;
        anim.SetBool(WALK_PARAM, shouldWalk);

        // 设置跑步状态 - 当有移动输入且在跑步状态时
        bool shouldRun = isMoving && isRunning && isGrounded && !isJumping;
        anim.SetBool(RUN_PARAM, shouldRun);

        // 设置地面状态
        anim.SetBool(GROUNDED_PARAM, isGrounded);

        // 触发跳跃动画 - 只在跳跃开始时触发一次
        if (jumpTriggered || (isJumping && !wasGrounded))
        {
            anim.SetTrigger(JUMP_PARAM);
        }

        // 重置跳跃触发器，防止重复触发
        if (isGrounded && anim.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
        {
            anim.ResetTrigger(JUMP_PARAM);
        }

        // 特殊处理：当在Jump状态且落地时，根据移动状态决定过渡到Walk或Idle
        if (isGrounded && wasGrounded == false && isJumping)
        {
            // 落地后如果正在移动，则过渡到Walk状态
            if (isMoving)
            {
                anim.SetBool(WALK_PARAM, true);
            }
            // 否则过渡到Idle状态
            else
            {
                anim.SetBool(WALK_PARAM, false);
                anim.SetBool(RUN_PARAM, false);
            }
        }

        // 特殊处理：从Jump到Run的条件 - 当落地后且满足跑步条件时
        if (isGrounded && wasGrounded == false && isJumping && isMoving && isRunning)
        {
            anim.SetBool(RUN_PARAM, true);
            anim.SetBool(WALK_PARAM, false);
        }
    }

    void ExecuteJump()
    {
        isJumping = true;
        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    void OnLand()
    {
        isJumping = false;
    }

    // 退出时解锁光标
    void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMove : MonoBehaviour
{
    [Header("组件引用")]
    public Animator anim;
    private CharacterController controller;
    public Camera playerCamera;

    [Header("移动设置")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;

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

    // 相机相关
    private Vector3 baseCameraLocalPosition;
    private float currentCameraForwardOffset;

    // 动画参数名称
    private const string WALK_PARAM = "Walk";
    private const string RUN_PARAM = "Run";
    private const string JUMP_PARAM = "Jump";
    private const string GROUNDED_PARAM = "IsGrounded";

    void Start()
    {
        // 获取组件
        controller = GetComponent<CharacterController>();

        // 使用主摄像头
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // 设置相机基础位置
        if (playerCamera != null)
        {
            baseCameraLocalPosition = new Vector3(0, 1.7f, 0);
            playerCamera.transform.localPosition = baseCameraLocalPosition;
        }

        // 如果没有CharacterController，自动添加
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
            SetupCharacterController();
        }

        // 设置地面层级
        if (groundLayer.value == 0)
            groundLayer = LayerMask.GetMask("Default");

        // 检查Animator组件
        if (anim == null)
        {
            anim = GetComponentInChildren<Animator>();
            if (anim == null)
            {
                Debug.LogError("没有找到 Animator 组件！");
            }
        }

        // 确保动画不应用根运动
        if (anim != null)
        {
            anim.applyRootMotion = false;
        }

        currentCameraForwardOffset = 0f;

        // 锁定光标到屏幕中心
        Cursor.lockState = CursorLockMode.Locked;
    }
    //设置角色控制器的物理碰撞属性
    void SetupCharacterController()
    {
        controller.center = new Vector3(0, 1, 0);//人物的中心点位置
        controller.height = 2.0f;//人物的高度
        controller.radius = 0.3f;//人物的半径
        controller.stepOffset = 0.3f;//人物能自动走的楼梯高度
        controller.slopeLimit = 45f;//人物能行走的最大坡度
    }

    void Update()
    {
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

        // 只有在有输入时才标准化，避免停止时的滑动
        if (moveDirection.magnitude > 0.1f)
        {
            moveDirection.Normalize();
        }

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
                // 空中移动速度减半
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

        AnimatorStateInfo currentState = anim.GetCurrentAnimatorStateInfo(0);
        bool shouldWalk = isMoving && !isRunning;
        bool shouldRun = isMoving && isRunning;

        // 跳跃动画结束处理
        if (currentState.IsName("Jump") && currentState.normalizedTime >= 0.95f && isGrounded)
        {

            anim.SetBool(WALK_PARAM, shouldWalk);
            anim.SetBool(RUN_PARAM, shouldRun);
            anim.ResetTrigger(JUMP_PARAM);
            isJumping = false;
        }

        // 常规参数更新
        anim.SetBool(WALK_PARAM, shouldWalk && isGrounded && !isJumping);
        anim.SetBool(RUN_PARAM, shouldRun && isGrounded && !isJumping);
        anim.SetBool(GROUNDED_PARAM, isGrounded);

        // 跳跃触发
        if (jumpTriggered && isGrounded && !currentState.IsName("Jump"))
        {
            anim.SetTrigger(JUMP_PARAM);
            isJumping = true;
            jumpTriggered = false;
        }

        // 落地瞬间同步
        if (!wasGrounded && isGrounded)
        {
            anim.SetBool(WALK_PARAM, shouldWalk);
            anim.SetBool(RUN_PARAM, shouldRun);
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

    // 公开方法供其他脚本调用
    public bool IsGrounded() => isGrounded;
    public bool IsMoving() => isMoving;
    public bool IsRunning() => isRunning;

    // 退出时解锁光标
    void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    // 调试GUI
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("角色移动状态:");
        GUILayout.Label($"移动: {isMoving}");
        GUILayout.Label($"跑步: {isRunning}");
        GUILayout.Label($"跳跃: {isJumping}");
        GUILayout.Label($"地面: {isGrounded}");
        GUILayout.Label($"垂直速度: {verticalVelocity:F2}");
        GUILayout.Label($"相机前移: {currentCameraForwardOffset:F2}");
        GUILayout.EndArea();
    }
}
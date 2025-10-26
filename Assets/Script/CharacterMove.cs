using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCharacterController : MonoBehaviour
{
    [Header("组件引用")]
    public Animator anim;
    private CharacterController controller;
    public Camera playerCamera;

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
        controller = GetComponent<CharacterController>();
        // 使用主摄像头(无其他摄像头时）
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // 确保摄像头是角色的子对象
        if (playerCamera != null && playerCamera.transform.parent != transform)
        {
            playerCamera.transform.SetParent(transform);
            playerCamera.transform.localPosition = new Vector3(0, 1.6f, 0);
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

        // 检查Animator组件
        if (anim == null)
        {
            anim = GetComponentInChildren<Animator>();
            if (anim == null)
            {
                Debug.LogError("没有找到 Animator 组件！");
            }
            else
            {
                Debug.Log("找到 Animator 组件: " + anim.name);
            }
        }
        else
        {
            Debug.Log("Animator 已赋值: " + anim.name);
        }
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

        // 调试鼠标输入
        if (Mathf.Abs(mouseX) > 0.01f || Mathf.Abs(mouseY) > 0.01f)
        {
            Debug.Log($"鼠标输入 - X: {mouseX}, Y: {mouseY}");
        }

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

        // 调试输入
        if (Mathf.Abs(horizontal) > 0.01f || Mathf.Abs(vertical) > 0.01f)
        {
            Debug.Log($"键盘输入 - 水平: {horizontal}, 垂直: {vertical}");
        }

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
            Debug.Log("跳跃按键被按下");
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

        AnimatorStateInfo currentState = anim.GetCurrentAnimatorStateInfo(0);
        bool shouldWalk = isMoving && !isRunning;
        bool shouldRun = isMoving && isRunning;

        // 跳跃动画结束处理
        if (currentState.IsName("Jump"))
        {
            // 在动画播放95%时强制同步状态
            if (currentState.normalizedTime >= 0.95f && isGrounded)
            {
                anim.SetBool(WALK_PARAM, shouldWalk);
                anim.SetBool(RUN_PARAM, shouldRun);
                anim.ResetTrigger(JUMP_PARAM);
                isJumping = false;

                // 立即更新状态机
                anim.Update(0f);
                Debug.Log("跳跃动画结束，强制状态同步");
            }
        }

        // 常规参数更新（带状态保护）
        anim.SetBool(WALK_PARAM, shouldWalk && isGrounded && !isJumping);
        anim.SetBool(RUN_PARAM, shouldRun && isGrounded && !isJumping);
        anim.SetBool(GROUNDED_PARAM, isGrounded);

        // 跳跃触发（带二次验证）
        if (jumpTriggered && isGrounded && !currentState.IsName("Jump"))
        {
            anim.SetTrigger(JUMP_PARAM);
            isJumping = true;
            jumpTriggered = false;
            Debug.Log("触发跳跃动画");
        }
        // 落地瞬间同步
        if (!wasGrounded && isGrounded)
        {
            anim.SetBool(WALK_PARAM, shouldWalk);
            anim.SetBool(RUN_PARAM, shouldRun);
            Debug.Log("着陆状态同步");
        }
    }

    void ExecuteJump()
    {
        isJumping = true;
        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        Debug.Log("执行跳跃，垂直速度: " + verticalVelocity);
    }

    void OnLand()
    {
        isJumping = false;
        Debug.Log("角色落地");
    }

    // 退出时解锁光标
    void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    // 添加一个简单的GUI来显示状态
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("第一人称控制器状态:");
        GUILayout.Label($"移动: {isMoving}");
        GUILayout.Label($"跑步: {isRunning}");
        GUILayout.Label($"跳跃: {isJumping}");
        GUILayout.Label($"地面: {isGrounded}");
        GUILayout.Label($"垂直速度: {verticalVelocity:F2}");
        GUILayout.EndArea();
    }
}
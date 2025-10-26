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

    [Header("相机设置")]
    public float cameraHeight = 1.7f; // 眼睛高度 - 直接设置
    public float cameraSmoothness = 10f;

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

    // 相机相关
    private Vector3 targetCameraLocalPosition;

    void Start()
    {
        // 获取组件
        controller = GetComponent<CharacterController>();

        // 使用主摄像头
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // 确保摄像头是角色的子对象
        if (playerCamera != null && playerCamera.transform.parent != transform)
        {
            playerCamera.transform.SetParent(transform);
        }

        // 如果没有CharacterController，自动添加并设置
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
            // 设置合理的控制器尺寸
            controller.height = 2.0f;
            controller.center = new Vector3(0, 1, 0);
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
        }

        // 确保动画不应用根运动
        if (anim != null)
        {
            anim.applyRootMotion = false;
        }

        // 初始化相机位置 - 这是关键！
        InitializeCameraPosition();
    }

    void InitializeCameraPosition()
    {
        if (playerCamera == null) return;

        // 关键：直接设置相机本地位置为眼睛高度
        // 忽略所有其他计算，直接设置
        targetCameraLocalPosition = new Vector3(0, cameraHeight, 0);
        playerCamera.transform.localPosition = targetCameraLocalPosition;
        playerCamera.transform.localRotation = Quaternion.identity;

        Debug.Log($"初始化相机位置: {targetCameraLocalPosition}");
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

    void LateUpdate()
    {
        // 在LateUpdate中确保相机位置正确
        UpdateCameraPosition();
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

    void UpdateCameraPosition()
    {
        if (playerCamera == null) return;

        // 直接强制设置相机位置，不使用任何平滑或偏移
        // 这是最简单的解决方案，确保相机始终在正确位置
        playerCamera.transform.localPosition = targetCameraLocalPosition;
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
            if (currentState.normalizedTime >= 0.95f && isGrounded)
            {
                anim.SetBool(WALK_PARAM, shouldWalk);
                anim.SetBool(RUN_PARAM, shouldRun);
                anim.ResetTrigger(JUMP_PARAM);
                isJumping = false;
            }
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
        GUILayout.Label($"相机高度: {cameraHeight:F2}");
        if (playerCamera != null)
        {
            GUILayout.Label($"相机实际位置: {playerCamera.transform.localPosition}");
        }
        GUILayout.EndArea();
    }

    // 在编辑器中可视化相机位置
    void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerCamera.transform.position, 0.1f);
        }
    }
}
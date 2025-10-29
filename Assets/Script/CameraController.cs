using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("相机设置")]
    public Transform player;              // 玩家角色
    public float mouseSensitivity = 100f; // 鼠标灵敏度
    public float cameraHeight = 1.7f;     // 相机高度
    public float normalForwardOffset=0.8f;
    [Header("跑步前移设置")]
    public float runForwardOffset = 0.3f;     // 跑步时的前移距离
    public float forwardTransitionSpeed = 8f; // 前移过渡速度

    private float xRotation = 0f;         // X轴旋转角度
    private Vector3 baseLocalPosition;    // 基础本地位置
    private float currentForwardOffset;   // 当前前移偏移
    private CharacterMove characterMove;  // 引用角色移动脚本

    void Start()
    {
        // 确保相机是玩家的子对象
        if (transform.parent != player)
        {
            transform.SetParent(player);
        }

        // 获取角色移动脚本引用
        characterMove = player.GetComponent<CharacterMove>();
        if (characterMove == null)
        {
            Debug.LogError("在玩家对象上找不到 CharacterMove 组件！");
        }

        // 设置基础位置
        baseLocalPosition = new Vector3(0, cameraHeight, normalForwardOffset);
        currentForwardOffset = 0f;
        UpdateCameraPosition();

        // 锁定光标
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // 获取鼠标输入
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 上下视角限制
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // 应用旋转
        player.Rotate(Vector3.up * mouseX);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 跑步前移
        HandleRunForwardOffset();

        // 更新相机位置
        UpdateCameraPosition();
    }

    void HandleRunForwardOffset()
    {
        float targetOffset = 0f;

        // 根据角色移动状态决定目标偏移
        if (characterMove != null)
        {
            if (characterMove.IsRunning() && characterMove.IsMoving())
            {
                targetOffset = runForwardOffset;
            }
            else
            {
                targetOffset = 0f;
            }
        }

        // 过渡到目标偏移
        currentForwardOffset = Mathf.Lerp(
            currentForwardOffset,
            targetOffset,
            forwardTransitionSpeed * Time.deltaTime
        );
    }

    void UpdateCameraPosition()
    {
        // 应用基础高度和前移偏移
        Vector3 targetPosition = baseLocalPosition + Vector3.forward * currentForwardOffset;
        transform.localPosition = targetPosition;
    }

    //设置相机的高度
    public void SetCameraHeight(float height)
    {
        cameraHeight = height;
        baseLocalPosition = new Vector3(0, cameraHeight, 0);
        UpdateCameraPosition();
    }
    //设置相机正常状态的位置
    public void SetNormalForwardOffset(float offset)
    {
        normalForwardOffset = offset;
        baseLocalPosition= new Vector3(0, cameraHeight,normalForwardOffset);
    }
    //设置相机跑步时的位置
    public void SetRunForwardOffset(float offset)
    {
        runForwardOffset = offset;
    }
}
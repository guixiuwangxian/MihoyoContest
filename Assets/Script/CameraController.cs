using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
 public Transform player;
    //获取鼠标移动值
    private float mouseX, mouseY;
    //鼠标灵敏度
    public float mouseSensitivity;
    //角色的旋转角度
    public float xRotation;

    // Update is called once per frame
    void Update()
    {
        //获取鼠标移动值
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        //限制角色的旋转角度
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -70f, 70f);

        //旋转角色
        player.Rotate(Vector3.up * mouseX);
        transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
    }
}

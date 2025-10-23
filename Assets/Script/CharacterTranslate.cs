using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CharacterTranslate : MonoBehaviour
{
    private CharacterController characterController;//角色控制器组件
    public float RotateSpeed = 10;//旋转速度
    public float MoveSpeed = 5;//移动速度

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();//获取角色控制器
    }

    // Update is called once per frame
    void Update()
    {
        float h = Input.GetAxis("Horizontal");//获取横向输入
        float v = Input.GetAxis("Vertical");//获取垂直输入

        if (h != 0 || v != 0)
        {

            Vector3 dir = new Vector3(h, 0, v);//获取向量

            Quaternion targetQ = Quaternion.LookRotation(dir, Vector3.up);//获取目标旋转角度
            targetQ = Quaternion.Lerp(transform.rotation, targetQ, Time.deltaTime * RotateSpeed);//平滑旋转
            transform.rotation = targetQ;//人物面向方向
            //向量的移动方法：transform.position=transform.position+transform.forward*Time.deltaTime*MoveSpeed;
            transform.Translate(transform.forward * Time.deltaTime * MoveSpeed, Space.World);//角色控制器移动
        }
    }


}
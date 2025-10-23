using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

public class CharacterTranslate : MonoBehaviour
{
    Animator anim;
    private CharacterController characterController;//角色控制器组件
    public float RotateSpeed = 10;//旋转速度
    public float MoveSpeed = 5;//移动速度
    public bool StartWalk;
    public bool Walk;
    public bool Run;
    private bool wasMoving; // 上一帧是否在移动
    private bool isMoving;//判断是否在移动
    private bool runKey;//获取run的触发
    void Start()
    {
        anim = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();//获取角色控制器
    }

    void UpdateAnim()
    {
        if (anim != null)
        {
            if (StartWalk) anim.SetTrigger("StartWalk");
            anim.SetBool("Walk", Walk);
            anim.SetBool("Run", Run);
        }
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal");//获取横向输入
        float v = Input.GetAxis("Vertical");//获取垂直输入

        // 状态判定
        isMoving = Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f;
        runKey = Input.GetKey(KeyCode.LeftShift);

        StartWalk = !wasMoving && isMoving; // 刚开始移动的那一帧为 true
        Run = isMoving && runKey;
        Walk = isMoving && !runKey;

        UpdateAnim();//播放动画

        if (isMoving)
        {
            Vector3 dir = new Vector3(h, 0, v);//获取向量
            Quaternion targetQ = Quaternion.LookRotation(dir, Vector3.up);//获取目标旋转角度
            targetQ = Quaternion.Lerp(transform.rotation, targetQ, Time.deltaTime * RotateSpeed);//平滑旋转
            transform.rotation = targetQ;//人物面向方向

            float speed = MoveSpeed * (Run ? 2f : 1f); // 跑步倍率为 2
            Vector3 move = transform.forward * speed * Time.deltaTime;
            if (characterController != null) characterController.Move(move);
            else transform.Translate(move, Space.World);
        }

        wasMoving = isMoving;
    }
}
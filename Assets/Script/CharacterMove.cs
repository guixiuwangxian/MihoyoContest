using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

public class CharacterMove : MonoBehaviour
{
    Animator anim;
    private CharacterController characterController;//��ɫ���������
    public float RotateSpeed = 10;//��ת�ٶ�
    public float MoveSpeed = 5;//�ƶ��ٶ�
    public bool StartWalk;
    public bool Walk;
    public bool Run;
    private bool wasMoving; // ��һ֡�Ƿ����ƶ�
    private bool isMoving;//�ж��Ƿ����ƶ�
    private bool runKey;//��ȡrun�Ĵ���
    void Start()
    {
        anim = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();//��ȡ��ɫ������
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
        float h = Input.GetAxis("Horizontal");//��ȡ��������
        float v = Input.GetAxis("Vertical");//��ȡ��ֱ����

        // ״̬�ж�
        isMoving = Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f;
        runKey = Input.GetKey(KeyCode.LeftShift);

        StartWalk = !wasMoving && isMoving; // �տ�ʼ�ƶ�����һ֡Ϊ true
        Run = isMoving && runKey;
        Walk = isMoving && !runKey;

        UpdateAnim();//���Ŷ���

        if (isMoving)
        {
            Vector3 dir = new Vector3(h, 0, v);//��ȡ����
            Quaternion targetQ = Quaternion.LookRotation(dir, Vector3.up);//��ȡĿ����ת�Ƕ�
            targetQ = Quaternion.Lerp(transform.rotation, targetQ, Time.deltaTime * RotateSpeed);//ƽ����ת
            transform.rotation = targetQ;//����������

            float speed = MoveSpeed * (Run ? 2f : 1f); // �ܲ�����Ϊ 2
            Vector3 move = transform.forward * speed * Time.deltaTime;
            if (characterController != null) characterController.Move(move);
            else transform.Translate(move, Space.World);
        }

        wasMoving = isMoving;
    }


}
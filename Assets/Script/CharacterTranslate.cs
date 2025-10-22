using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CharacterTranslate: MonoBehaviour
{
    public float RotateSpeed=10;
    public float MoveSpeed = 5;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (h != 0 || v != 0)
        {

            Vector3 dir = new Vector3(h, 0, v);
            
            Quaternion targetQ = Quaternion.LookRotation(dir, Vector3.up);
            targetQ = Quaternion.Lerp(transform.rotation, targetQ, Time.deltaTime*RotateSpeed);
            transform.rotation = targetQ;
            //向量的移动方法：transform.position=transform.position+transform.forward*Time.deltaTime*MoveSpeed;
            transform.Translate(transform.forward * Time.deltaTime * MoveSpeed, Space.World);
        }
    }
    
    
}

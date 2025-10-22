using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public Transform target;
    private Vector3 m_v3ReleativePos;
    
    // Start is called before the first frame update
    void Start()
    {
        m_v3ReleativePos = transform.position-target.position;

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = target.position + m_v3ReleativePos;
    }
}

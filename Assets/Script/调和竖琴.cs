using UnityEngine;

public class CalmUnstableObject : MonoBehaviour
{
    public ParticleSystem unstableEffect; // 不和谐状态特效
    public ParticleSystem calmEffect; // 平静状态特效
    public bool isCalm = false; // 是否已平静

    void OnEnable()
    {
        // 订阅竖琴的平静信号
        EventManager.Subscribe("Harp_CalmUnstable", CalmObject);
    }

    void OnDisable()
    {
        EventManager.Unsubscribe("Harp_CalmUnstable", CalmObject);
    }

    void Start()
    {
        // 初始播放不和谐特效
        if (unstableEffect != null)
        {
            unstableEffect.Play();
        }
    }

    // 修正不和谐音，使其平静
    void CalmObject()
    {
        if (!isCalm)
        {
            isCalm = true;
            unstableEffect.Stop();
            if (calmEffect != null)
            {
                calmEffect.Play();
            }
            // 可添加额外逻辑：解锁路径、停止物体抖动等
            Debug.Log($"{gameObject.name} 已平静");
        }
    }
}
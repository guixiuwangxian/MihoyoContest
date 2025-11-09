using UnityEngine;

public class RhythmHeart : MonoBehaviour
{
    [Header("基础设置")]
    public float floatHeight = 0.5f;
    public float floatSpeed = 2f;
    public float rotationSpeed = 90f;

    private Vector3 startPosition;
    private bool isCollected = false;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (!isCollected)
        {
            // 浮动动画
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // 旋转动画
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isCollected && other.CompareTag("Player"))
        {
            Collect();
        }
    }

    void Collect()
    {
        isCollected = true;

        // 查找收集器并通知收集
        RhythmHeartCollector collector = FindObjectOfType<RhythmHeartCollector>();
        if (collector != null)
        {
            collector.OnHeartCollected();
        }

        Debug.Log("节奏之心被收集!");
        Destroy(gameObject);
    }
}
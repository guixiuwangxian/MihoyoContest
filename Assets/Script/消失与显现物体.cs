using UnityEngine;

public class DisappearAppearBlock : MonoBehaviour
{
    [Header("消失显现设置")]
    public bool startVisible = true;
    public float fadeDuration = 0.5f;

    private Renderer blockRenderer;
    private Collider blockCollider;
    private bool isVisible;
    private bool isPlayerOverlapping = false;

    void Start()
    {
        blockRenderer = GetComponent<Renderer>();
        blockCollider = GetComponent<Collider>();
        isVisible = startVisible;
        UpdateBlockState();
    }

    void OnEnable()
    {
        // 订阅指挥棒的基础交互事件
        EventManager.Subscribe("Conductor_BasicInteract", ToggleState);
    }

    void OnDisable()
    {
        EventManager.Unsubscribe("Conductor_BasicInteract", ToggleState);
    }

    public void ToggleState()
    {
        // 检查玩家是否重叠且要显现，如果是则禁止
        if (isPlayerOverlapping && !isVisible)
        {
            Debug.Log("玩家与方块重叠，禁止显现");
            return;
        }

        isVisible = !isVisible;
        UpdateBlockState();

        // 播放音效
        if (TryGetComponent<AudioSource>(out AudioSource audio))
        {
            audio.Play();
        }
    }

    void UpdateBlockState()
    {
        if (blockRenderer != null)
        {
            Color color = blockRenderer.material.color;
            color.a = isVisible ? 1f : 0f;
            blockRenderer.material.color = color;
        }

        if (blockCollider != null)
        {
            blockCollider.enabled = isVisible;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerOverlapping = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerOverlapping = false;
        }
    }
}
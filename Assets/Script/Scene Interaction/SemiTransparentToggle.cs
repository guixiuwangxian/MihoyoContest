using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SemiTransparentToggle : MonoBehaviour
{
    [Header("透明度设置")]
    [Range(0.1f, 0.9f)]
    public float semiTransparentAlpha = 0.3f; // 半透明状态的透明度

    [Header("视觉反馈")]
    public Color tintColor = Color.cyan; // 半透明时的色调
    public bool useTintColor = true; // 是否使用色调区分

    [Header("碰撞体设置")]
    public bool disableColliderWhenSemiTransparent = true;

    [Header("状态显示")]
    [SerializeField] private bool isSemiTransparent = false;

    // 内部变量
    private Material[] originalMaterials;
    private Material[] semiTransparentMaterials;
    private Collider[] objectColliders;
    private bool leftButtonPressed = false;
    private bool rightButtonPressed = false;

    void Start()
    {
        InitializeComponents();
        CreateSemiTransparentMaterials();

        // 初始状态为不透明
        isSemiTransparent = false;
        ApplyOriginalMaterials();
        SetCollidersEnabled(true);
    }

    void InitializeComponents()
    {
        // 获取所有渲染器
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length];
        semiTransparentMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].material;
        }

        // 获取所有碰撞体
        objectColliders = GetComponentsInChildren<Collider>();
    }

    void CreateSemiTransparentMaterials()
    {
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            // 创建半透明材质副本
            semiTransparentMaterials[i] = new Material(originalMaterials[i]);

            // 设置透明渲染模式
            SetupMaterialForTransparency(semiTransparentMaterials[i]);

            // 应用半透明设置
            ApplySemiTransparentSettings(semiTransparentMaterials[i]);
        }
    }

    void SetupMaterialForTransparency(Material material)
    {
        // 设置标准渲染管线的透明材质属性
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

    void ApplySemiTransparentSettings(Material material)
    {
        Color color = material.color;

        if (useTintColor)
        {
            // 应用色调并设置透明度
            color = Color.Lerp(color, tintColor, 0.5f);
        }

        color.a = semiTransparentAlpha;
        material.color = color;
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // 左键按下 - 设置为半透明（只触发一次）
        if (Input.GetMouseButtonDown(0) && !leftButtonPressed)
        {
            leftButtonPressed = true;
            if (!isSemiTransparent)
            {
                SetSemiTransparent(true);
            }
        }

        // 左键释放
        if (Input.GetMouseButtonUp(0))
        {
            leftButtonPressed = false;
        }

        // 右键按下 - 恢复不透明（只触发一次）
        if (Input.GetMouseButtonDown(1) && !rightButtonPressed)
        {
            rightButtonPressed = true;
            if (isSemiTransparent)
            {
                SetSemiTransparent(false);
            }
        }

        // 右键释放
        if (Input.GetMouseButtonUp(1))
        {
            rightButtonPressed = false;
        }
    }

    void SetSemiTransparent(bool semiTransparent)
    {
        isSemiTransparent = semiTransparent;

        if (semiTransparent)
        {
            // 切换到半透明状态
            ApplySemiTransparentMaterials();

            // 禁用碰撞体
            if (disableColliderWhenSemiTransparent)
            {
                SetCollidersEnabled(false);
            }
        }
        else
        {
            // 切换到不透明状态
            ApplyOriginalMaterials();

            // 启用碰撞体
            if (disableColliderWhenSemiTransparent)
            {
                SetCollidersEnabled(true);
            }
        }
    }

    void ApplySemiTransparentMaterials()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            if (i < semiTransparentMaterials.Length)
            {
                renderers[i].material = semiTransparentMaterials[i];
            }
        }
    }

    void ApplyOriginalMaterials()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            if (i < originalMaterials.Length)
            {
                renderers[i].material = originalMaterials[i];
            }
        }
    }

    void SetCollidersEnabled(bool enabled)
    {
        foreach (Collider collider in objectColliders)
        {
            if (collider != null)
            {
                collider.enabled = enabled;
            }
        }
    }

    // 公共方法供其他脚本调用
    public void MakeSemiTransparent()
    {
        if (!isSemiTransparent)
        {
            SetSemiTransparent(true);
        }
    }

    public void MakeOpaque()
    {
        if (isSemiTransparent)
        {
            SetSemiTransparent(false);
        }
    }

    public void ToggleTransparency()
    {
        SetSemiTransparent(!isSemiTransparent);
    }

    // 在Inspector中更新材质（当参数改变时）
    void OnValidate()
    {
        if (Application.isPlaying && semiTransparentMaterials != null)
        {
            // 更新半透明材质设置
            for (int i = 0; i < semiTransparentMaterials.Length; i++)
            {
                if (semiTransparentMaterials[i] != null)
                {
                    ApplySemiTransparentSettings(semiTransparentMaterials[i]);
                }
            }

            // 如果当前是半透明状态，重新应用材质
            if (isSemiTransparent)
            {
                ApplySemiTransparentMaterials();
            }
        }
    }

    // Inspector便捷按钮
    [ContextMenu("设置为半透明")]
    public void SetSemiTransparentFromInspector()
    {
        SetSemiTransparent(true);
    }

    [ContextMenu("设置为不透明")]
    public void SetOpaqueFromInspector()
    {
        SetSemiTransparent(false);
    }

    [ContextMenu("切换状态")]
    public void ToggleFromInspector()
    {
        ToggleTransparency();
    }

    // 在Scene视图中绘制状态指示器
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // 根据状态显示不同颜色的边框
        Gizmos.color = isSemiTransparent ? new Color(0, 1, 1, 0.5f) : Color.green;
        Gizmos.DrawWireCube(transform.position, GetComponent<Renderer>().bounds.size * 1.1f);

        // 如果是半透明状态，显示额外的视觉提示
        if (isSemiTransparent)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, GetComponent<Renderer>().bounds.size * 1.2f);
        }
    }
}
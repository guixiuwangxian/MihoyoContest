using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SemiTransparentToggle : MonoBehaviour
{
    [Header("͸��������")]
    [Range(0.1f, 0.9f)]
    public float semiTransparentAlpha = 0.3f; // ��͸��״̬��͸����

    [Header("�Ӿ�����")]
    public Color tintColor = Color.cyan; // ��͸��ʱ��ɫ��
    public bool useTintColor = true; // �Ƿ�ʹ��ɫ������

    [Header("��ײ������")]
    public bool disableColliderWhenSemiTransparent = true;

    [Header("״̬��ʾ")]
    [SerializeField] private bool isSemiTransparent = false;

    // �ڲ�����
    private Material[] originalMaterials;
    private Material[] semiTransparentMaterials;
    private Collider[] objectColliders;
    private bool leftButtonPressed = false;
    private bool rightButtonPressed = false;

    void Start()
    {
        InitializeComponents();
        CreateSemiTransparentMaterials();

        // ��ʼ״̬Ϊ��͸��
        isSemiTransparent = false;
        ApplyOriginalMaterials();
        SetCollidersEnabled(true);
    }

    void InitializeComponents()
    {
        // ��ȡ������Ⱦ��
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length];
        semiTransparentMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].material;
        }

        // ��ȡ������ײ��
        objectColliders = GetComponentsInChildren<Collider>();
    }

    void CreateSemiTransparentMaterials()
    {
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            // ������͸�����ʸ���
            semiTransparentMaterials[i] = new Material(originalMaterials[i]);

            // ����͸����Ⱦģʽ
            SetupMaterialForTransparency(semiTransparentMaterials[i]);

            // Ӧ�ð�͸������
            ApplySemiTransparentSettings(semiTransparentMaterials[i]);
        }
    }

    void SetupMaterialForTransparency(Material material)
    {
        // ���ñ�׼��Ⱦ���ߵ�͸����������
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
            // Ӧ��ɫ��������͸����
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
        // ������� - ����Ϊ��͸����ֻ����һ�Σ�
        if (Input.GetMouseButtonDown(0) && !leftButtonPressed)
        {
            leftButtonPressed = true;
            if (!isSemiTransparent)
            {
                SetSemiTransparent(true);
            }
        }

        // ����ͷ�
        if (Input.GetMouseButtonUp(0))
        {
            leftButtonPressed = false;
        }

        // �Ҽ����� - �ָ���͸����ֻ����һ�Σ�
        if (Input.GetMouseButtonDown(1) && !rightButtonPressed)
        {
            rightButtonPressed = true;
            if (isSemiTransparent)
            {
                SetSemiTransparent(false);
            }
        }

        // �Ҽ��ͷ�
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
            // �л�����͸��״̬
            ApplySemiTransparentMaterials();

            // ������ײ��
            if (disableColliderWhenSemiTransparent)
            {
                SetCollidersEnabled(false);
            }
        }
        else
        {
            // �л�����͸��״̬
            ApplyOriginalMaterials();

            // ������ײ��
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

    // ���������������ű�����
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

    // ��Inspector�и��²��ʣ��������ı�ʱ��
    void OnValidate()
    {
        if (Application.isPlaying && semiTransparentMaterials != null)
        {
            // ���°�͸����������
            for (int i = 0; i < semiTransparentMaterials.Length; i++)
            {
                if (semiTransparentMaterials[i] != null)
                {
                    ApplySemiTransparentSettings(semiTransparentMaterials[i]);
                }
            }

            // �����ǰ�ǰ�͸��״̬������Ӧ�ò���
            if (isSemiTransparent)
            {
                ApplySemiTransparentMaterials();
            }
        }
    }

    // Inspector��ݰ�ť
    [ContextMenu("����Ϊ��͸��")]
    public void SetSemiTransparentFromInspector()
    {
        SetSemiTransparent(true);
    }

    [ContextMenu("����Ϊ��͸��")]
    public void SetOpaqueFromInspector()
    {
        SetSemiTransparent(false);
    }

    [ContextMenu("�л�״̬")]
    public void ToggleFromInspector()
    {
        ToggleTransparency();
    }

    // ��Scene��ͼ�л���״ָ̬ʾ��
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // ����״̬��ʾ��ͬ��ɫ�ı߿�
        Gizmos.color = isSemiTransparent ? new Color(0, 1, 1, 0.5f) : Color.green;
        Gizmos.DrawWireCube(transform.position, GetComponent<Renderer>().bounds.size * 1.1f);

        // ����ǰ�͸��״̬����ʾ������Ӿ���ʾ
        if (isSemiTransparent)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, GetComponent<Renderer>().bounds.size * 1.2f);
        }
    }
}
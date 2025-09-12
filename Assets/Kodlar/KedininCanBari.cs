using UnityEngine;
using UnityEngine.UI;

public class KedininCanBari : MonoBehaviour
{
    [Header("Data")]
    public KedininCani kediCan;        // Cat's health component
    public Image fillImage;            // Image -> Type: Filled, Method: Horizontal

    [Header("Optional")]
    public Gradient renkGradyan;       // Optional gradient (green->red etc.)

    int maxCan;

    void Awake()
    {
        if (!fillImage) fillImage = GetComponent<Image>();
    }

    void OnEnable()
    {
        if (fillImage) fillImage.enabled = true;
    }

    void Start()
    {
        // Auto-find by tag "Kedi" if not assigned
        if (!kediCan)
        {
            var kediGo = GameObject.FindGameObjectWithTag("Kedi");
            if (kediGo) kediCan = kediGo.GetComponent<KedininCani>();
        }

        if (kediCan) maxCan = Mathf.Max(1, kediCan.can);

        PrepareImage();
        UpdateBar();
    }

    void Update()
    {
        UpdateBar();
    }

    void PrepareImage()
    {
        if (!fillImage) return;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
    }

    void UpdateBar()
    {
        if (!kediCan || !fillImage || maxCan <= 0) return;

        float t = Mathf.Clamp01((float)kediCan.can / maxCan);
        fillImage.fillAmount = t;

        if (renkGradyan != null)
            fillImage.color = renkGradyan.Evaluate(t);

        // If you want to hide at zero, uncomment:
        fillImage.enabled = t > 0f;
    }
}

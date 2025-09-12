using UnityEngine;
using UnityEngine.UI;

public class KafaCanBar : MonoBehaviour
{
    [Header("Veri")]
    public Can can;                         // Martýnýn/kiminse Can scripti
    public Image fillImage;                 // Filled Horizontal image
    public Vector3 worldOffset = new Vector3(0f, 1.0f, 0f);

    [Header("Davranýþ")]
    public bool kamerayaBak = true;         // Billboard
    public bool tamDoluykenGizle = false;   // Full can’da gizle
    public bool sifirkenGizle = true;       // 0’da gizle

    [Header("Renk (opsiyonel)")]
    public Gradient renkGradyan;

    Transform hedef;        // can’ýn sahibi
    Camera cam;
    int maxCan;

    void Awake()
    {
        if (!fillImage) fillImage = GetComponent<Image>();
        cam = Camera.main;
    }

    void OnEnable()
    {
        if (fillImage) fillImage.enabled = true;
    }

    void Start()
    {
        if (!can) can = GetComponentInParent<Can>();
        if (can)
        {
            maxCan = Mathf.Max(1, can.canPuani);
            hedef = can.transform;
        }
        HazirlaImage();
        GuncelleBar();
        Konumla();
    }

    void LateUpdate()
    {
        GuncelleBar();
        Konumla();
    }

    void HazirlaImage()
    {
        if (!fillImage) return;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
    }

    void GuncelleBar()
    {
        if (!can || !fillImage || maxCan <= 0) return;

        float t = Mathf.Clamp01((float)can.canPuani / maxCan);
        fillImage.fillAmount = t;

        if (renkGradyan != null)
            fillImage.color = renkGradyan.Evaluate(t);

        bool gizle = (tamDoluykenGizle && t >= 0.999f) || (sifirkenGizle && t <= 0.001f);
        fillImage.enabled = !gizle;
    }

    void Konumla()
    {
        if (!hedef) return;

        // Dünya konumu
        Vector3 p = hedef.position + worldOffset;
        transform.position = p;

        if (kamerayaBak && cam)
        {
            // Billboard (sadece Z’i kameraya baktýr)
            Vector3 dir = transform.position - cam.transform.position;
            dir.y = 0f; // 2D’de düz tut
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        }
    }

    // UÇUÞTA GÝZLEMEYÝ buradan da tetikleyebilesin
    public void KacistaGizle()
    {
        if (fillImage) fillImage.enabled = false;
        enabled = false; // scripti sustur
    }
}

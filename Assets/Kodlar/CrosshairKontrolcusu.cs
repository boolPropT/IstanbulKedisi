using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairKontrolcusu : MonoBehaviour
{
    public enum Mode { UI, WorldSpace2D }
    [Header("Mod")] public Mode mode = Mode.UI;

    [Header("Görseller")]
    public Sprite normalSprite;
    public Sprite hitSprite;
    public float hitFlashDuration = 0.12f;

    [Header("Ýmleç")]
    public bool hideSystemCursor = true;
    public Vector2 screenOffset = Vector2.zero;

    [Header("Vuruþ Algýlama (opsiyonel)")]
    public bool changeOnClickIfHits = true;
    public LayerMask hittableMask = ~0;
    public string requiredTag = "";
    public float hitScalePulse = 1.15f;
    public float pulseReturnSpeed = 18f;

    // Dahili
    Camera cam;
    Image uiImage;
    SpriteRenderer sr;
    RectTransform rt;
    Vector3 baseScale;
    bool flashing;

    // YENÝ: Canvas referanslarý (UI modunda þart)
    Canvas canvas;
    RectTransform canvasRect;

    void Awake()
    {
        // Canvas'ý ve kamerayý doðru kaynaktan al
        canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvasRect = canvas.GetComponent<RectTransform>();
            cam = canvas.worldCamera;                 // ÖNEMLÝ
        }
        if (cam == null) cam = Camera.main;          // yedek

        uiImage = GetComponent<Image>();
        sr = GetComponent<SpriteRenderer>();
        rt = GetComponent<RectTransform>();
        baseScale = transform.localScale;

        SetSprite(normalSprite);
    }

    void OnEnable() { if (hideSystemCursor) Cursor.visible = false; }
    void OnDisable() { if (hideSystemCursor) Cursor.visible = true; }

    void Update()
    {
        FollowMouse();

        if (changeOnClickIfHits && Input.GetMouseButtonDown(0))
        {
            if (Check2DHitUnderMouse()) HitConfirm();
        }

        if (!flashing && transform.localScale != baseScale)
            transform.localScale = Vector3.Lerp(transform.localScale, baseScale, Time.unscaledDeltaTime * pulseReturnSpeed);
    }

    void FollowMouse()
    {
        if (mode == Mode.UI)
        {
            if (rt == null || canvasRect == null) return;

            // Ekran -> Canvas local (DOÐRU YÖNTEM)
            Vector2 local;
            var sp = (Vector2)Input.mousePosition + screenOffset;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, sp, cam, out local))
            {
                rt.anchoredPosition = local;
            }
        }
        else
        {
            if (!cam) cam = Camera.main;
            Vector3 mp = Input.mousePosition;
            mp.z = 10f; // sahnene göre ayarla
            Vector3 world = cam.ScreenToWorldPoint(mp);
            world += (Vector3)(screenOffset * 0.01f);
            world.z = 0f;
            transform.position = world;
        }
    }

    bool Check2DHitUnderMouse()
    {
        if (!cam) cam = Camera.main;
        Vector3 mp = Input.mousePosition;
        Vector3 world = cam.ScreenToWorldPoint(mp);
        Vector2 p = new Vector2(world.x, world.y);

        Collider2D col = Physics2D.OverlapPoint(p, hittableMask);
        if (col == null) return false;
        if (!string.IsNullOrEmpty(requiredTag) && !col.CompareTag(requiredTag)) return false;
        return true;
    }

    void SetSprite(Sprite s)
    {
        if (mode == Mode.UI)
        {
            if (uiImage != null) uiImage.sprite = s;
        }
        else
        {
            if (sr != null) sr.sprite = s;
        }
    }

    IEnumerator FlashHit()
    {
        flashing = true;
        SetSprite(hitSprite ? hitSprite : normalSprite);
        transform.localScale = baseScale * hitScalePulse;

        float t = 0f;
        while (t < hitFlashDuration)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        SetSprite(normalSprite);
        flashing = false;
    }

    public void HitConfirm()
    {
        if (hitSprite == null) { SetSprite(normalSprite); return; }
        StopAllCoroutines();
        StartCoroutine(FlashHit());
    }
}

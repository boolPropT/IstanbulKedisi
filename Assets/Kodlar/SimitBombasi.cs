using UnityEngine;
using System.Collections;
using TMPro;

public class SimitBombasi : MonoBehaviour
{
    [Header("Kullanim")]
    public KeyCode tus = KeyCode.Space;
    public int superCharges = 0;

    [Header("Etkisi")]
    public float yaricap = 7.5f;
    public int hasar = 50;
    public LayerMask dusmanKatmani;
    public bool ucanlariUcur = true;

    [Header("VFX / SFX / UI")]
    // Transparent simit sprite (e.g., 512x512)
    public Sprite simitHalkaSprite;
    public Color simitRenk = new Color(1f, 1f, 1f, 0.5f);

    // Effect duration (seconds)
    public float vfxSure = 0.35f;

    // Scale/alpha curves (0..1). If null, defaults are used.
    public AnimationCurve vfxOlcekEgrisi = AnimationCurve.EaseInOut(0, 0.2f, 1, 1.4f);
    public AnimationCurve vfxAlfaEgrisi = AnimationCurve.EaseInOut(0, 0.8f, 1, 0f);

    [Header("VFX Sorting (World-space SpriteRenderer)")]
    // Sorting Layer name (e.g., Default, UI, VFX)
    public string vfxSortingLayer = "Default";
    // Order in Layer. Larger = in front.
    public int vfxSortingOrder = 500;
    // Optional material (e.g., Particles/Additive)
    public Material vfxMaterial;
    // Animate with unscaled time (works even when Time.timeScale == 0)
    public bool vfxUnscaledTime = true;

    [Header("Ses")]
    public AudioClip sfxPatlama;     // short "puff" sound
    public float sfxSes = 0.8f;
    AudioSource asrc;

    [Header("Ipucu (bottom hint text)")]
    public TextMeshProUGUI ipucuText;          // bottom-of-screen hint
    public string ipucuMetni = "Bosluk: Simit Bombasi";
    public bool ipucuUnscaled = true;

    void Awake()
    {
        asrc = GetComponent<AudioSource>();
        if (!asrc) asrc = gameObject.AddComponent<AudioSource>();
        asrc.playOnAwake = false;
        asrc.spatialBlend = 0f;

        if (ipucuText) ipucuText.gameObject.SetActive(false);
    }

    void Update()
    {
        // Show/hide bottom hint
        if (ipucuText)
        {
            bool goster = superCharges > 0;
            if (ipucuText.gameObject.activeSelf != goster)
                ipucuText.gameObject.SetActive(goster);

            if (goster) ipucuText.text = ipucuMetni;
        }

        if (superCharges > 0 && Input.GetKeyDown(tus))
        {
            superCharges--;
            Patlat();
            StartCoroutine(VFXSimitHalkasi());
            PlaySFX();
        }
    }

    void Patlat()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, yaricap, dusmanKatmani);
        foreach (var h in hits)
        {
            var can = h.GetComponentInParent<Can>();
            if (can != null) can.canPuani = Mathf.Max(0, can.canPuani - hasar);

            if (ucanlariUcur)
            {
                var kac = h.GetComponentInParent<MartiUcuslaYokOl>();
                if (kac != null) kac.UcuslaAyril();
            }
        }
    }

    IEnumerator VFXSimitHalkasi()
    {
        if (simitHalkaSprite == null) yield break;

        // World-space sprite object
        var go = new GameObject("SimitVFX");
        go.transform.position = transform.position;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = simitHalkaSprite;
        sr.color = simitRenk;

        // Sorting Layer / Order
        if (!string.IsNullOrEmpty(vfxSortingLayer))
            sr.sortingLayerID = SortingLayer.NameToID(vfxSortingLayer);
        sr.sortingOrder = vfxSortingOrder;

        if (vfxMaterial) sr.sharedMaterial = vfxMaterial;

        // Curves (fallback defaults if null)
        var olcekEgri = vfxOlcekEgrisi != null ? vfxOlcekEgrisi : AnimationCurve.EaseInOut(0, 0.2f, 1, 1.4f);
        var alfaEgri = vfxAlfaEgrisi != null ? vfxAlfaEgrisi : AnimationCurve.EaseInOut(0, 1f, 1, 0f);

        float t = 0f;
        while (t < vfxSure)
        {
            float u = t / vfxSure;
            float scale = olcekEgri.Evaluate(u);
            float alpha = alfaEgri.Evaluate(u);

            // Scale roughly relative to radius
            go.transform.localScale = Vector3.one * scale * (yaricap / 2.5f);

            var c = sr.color; c.a = alpha; sr.color = c;

            t += vfxUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }

        Destroy(go);
    }

    void PlaySFX()
    {
        if (sfxPatlama && asrc)
            asrc.PlayOneShot(sfxPatlama, sfxSes);
    }
}

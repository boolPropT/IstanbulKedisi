using UnityEngine;

public class MartiUcuslaYokOl : MonoBehaviour
{
    [Header("Uçuþ Ayarlarý")]
    public Sprite ucusSprite;              // Martýnýn uçarken kullanacaðý sprite
    public float ucusHizi = 6f;            // Uçuþ hýzý
    public Vector2 ucusYonu = new(1f, 1.2f); // Saða-yukarý kaçýþ (normalize edilecek)
    public bool sagaBakarkenSagaKacsin = true; // Yönü yüz bakýþýna göre ayarla

    [Header("Ýsteðe Baðlý")]
    public bool colliderlariKapat = true;  // Çarpýþmalarý kapat
    public bool tumDavranislariDurdur = true; // AI/Follow scriptlerini kilitle
    public string[] korunacakBilesenler = { nameof(MartiUcuslaYokOl) }; // Kendisini koru

    [Header("Ekran Dýþý Ýmha")]
    public Camera hedefKamera;   // Boþ býrakýrsan otomatik Camera.main alýnýr
    public float yokEtmeDisMarj = 0.05f; // Bir týk tampon

    [Header("Ses")]
    public AudioClip sfxUcuslaYokOl;     // short "puff" sound
    public float sfxSes = 0.8f;
    AudioSource asrc;

    private bool kaciyor;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D[] cols;
    private MonoBehaviour[] behaviours;
    private Vector2 gercekUcusYonu;

    public bool Kaciyor { get; private set; }

    void Awake()
    {
        asrc = GetComponent<AudioSource>();
        if (!asrc) asrc = gameObject.AddComponent<AudioSource>();
        asrc.playOnAwake = false;
        asrc.spatialBlend = 0f;

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        cols = GetComponentsInChildren<Collider2D>(includeInactive: true);
        behaviours = GetComponents<MonoBehaviour>();
    }

    /// 
    /// Martý “öldüðünde” bunu çaðýr.
    /// 
    public void UcuslaAyril()
    {
        Kaciyor = true;

        // Can yazýlarýný kökten sustur
        var canMetinleri = GetComponentsInChildren<MartininCanMetni>(true);
        for (int i = 0; i < canMetinleri.Length; i++)
        {
            var m = canMetinleri[i];
            if (m == null) continue;

            // Yazýyý kapat
            if (m.text != null) m.text.gameObject.SetActive(false);

            // Bu scripti devre dýþý býrak ki LateUpdate tekrar açamasýn
            m.enabled = false;
        }

        if (kaciyor) return;
        kaciyor = true;

        // Görseli uçuþ spritiyle deðiþtir
        if (ucusSprite != null) sr.sprite = ucusSprite;

        // Fizik: yerçekimi ve itiþleri kapat, düz sür
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.gravityScale = 0f;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Çarpýþmalarý kapat
        if (colliderlariKapat)
        {
            foreach (var c in cols) c.enabled = false;
        }

        // Diðer davranýþ/AI scriptlerini durdur
        if (tumDavranislariDurdur)
        {
            foreach (var b in behaviours)
            {
                if (b == null) continue;
                var tipAdi = b.GetType().Name;
                bool koru = false;
                foreach (var k in korunacakBilesenler)
                    if (tipAdi == k) { koru = true; break; }
                if (!koru) b.enabled = false;
            }
            // Son olarak kendimizi yine açýk býrakýyoruz
            enabled = true;
        }

        // Yön hesapla
        gercekUcusYonu = ucusYonu.normalized;
        if (sr != null && sagaBakarkenSagaKacsin)
        {
            // Sprite'ýn scale.x ile yüz yönünü yakalayýp kaçýþ yönünü çevir
            bool sagaBakiyor = transform.localScale.x >= 0f;
            if (!sagaBakiyor) gercekUcusYonu.x *= -1f;
            // Görsel flip ayarý (sprite’ý doðru yöne baksýn istiyorsan)
            sr.flipX = !sagaBakiyor;
        }

        // Sabit hýzda kaçmaya baþla
        if (rb != null) rb.velocity = gercekUcusYonu * ucusHizi;

        PlaySFX();
    }

    void Update()
    {
        if (!kaciyor || rb == null) return;

        // Hafif yukarý ivme vererek “uçup uzaklaþma” hissi
        rb.velocity = Vector2.Lerp(rb.velocity, gercekUcusYonu * ucusHizi, 0.04f);
    }


    void LateUpdate()
    {
        if (!kaciyor) return;

        var cam = hedefKamera != null ? hedefKamera : Camera.main;
        if (cam == null) return; // sahnede kamera yoksa sus

        Vector3 vp = cam.WorldToViewportPoint(transform.position);

        // Z>0 önde, Z<0 kameranýn arkasýnda (bu da yok etme sebebi)
        bool arkada = vp.z < 0f;
        bool solda = vp.x < -yokEtmeDisMarj;
        bool sagda = vp.x > 1f + yokEtmeDisMarj;
        bool asagida = vp.y < -yokEtmeDisMarj;
        bool yukarida = vp.y > 1f + yokEtmeDisMarj;

        if (arkada || solda || sagda || asagida || yukarida)
        {
            Destroy(gameObject);
        }
    }

    void PlaySFX()
    {
        if (sfxUcuslaYokOl && asrc)
            asrc.PlayOneShot(sfxUcuslaYokOl, sfxSes);
    }
}
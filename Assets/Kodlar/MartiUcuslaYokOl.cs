using UnityEngine;

public class MartiUcuslaYokOl : MonoBehaviour
{
    [Header("U�u� Ayarlar�")]
    public Sprite ucusSprite;              // Mart�n�n u�arken kullanaca�� sprite
    public float ucusHizi = 6f;            // U�u� h�z�
    public Vector2 ucusYonu = new(1f, 1.2f); // Sa�a-yukar� ka��� (normalize edilecek)
    public bool sagaBakarkenSagaKacsin = true; // Y�n� y�z bak���na g�re ayarla

    [Header("�ste�e Ba�l�")]
    public bool colliderlariKapat = true;  // �arp��malar� kapat
    public bool tumDavranislariDurdur = true; // AI/Follow scriptlerini kilitle
    public string[] korunacakBilesenler = { nameof(MartiUcuslaYokOl) }; // Kendisini koru

    [Header("Ekran D��� �mha")]
    public Camera hedefKamera;   // Bo� b�rak�rsan otomatik Camera.main al�n�r
    public float yokEtmeDisMarj = 0.05f; // Bir t�k tampon

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
    /// Mart� ��ld���nde� bunu �a��r.
    /// 
    public void UcuslaAyril()
    {
        Kaciyor = true;

        // Can yaz�lar�n� k�kten sustur
        var canMetinleri = GetComponentsInChildren<MartininCanMetni>(true);
        for (int i = 0; i < canMetinleri.Length; i++)
        {
            var m = canMetinleri[i];
            if (m == null) continue;

            // Yaz�y� kapat
            if (m.text != null) m.text.gameObject.SetActive(false);

            // Bu scripti devre d��� b�rak ki LateUpdate tekrar a�amas�n
            m.enabled = false;
        }

        if (kaciyor) return;
        kaciyor = true;

        // G�rseli u�u� spritiyle de�i�tir
        if (ucusSprite != null) sr.sprite = ucusSprite;

        // Fizik: yer�ekimi ve iti�leri kapat, d�z s�r
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.gravityScale = 0f;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // �arp��malar� kapat
        if (colliderlariKapat)
        {
            foreach (var c in cols) c.enabled = false;
        }

        // Di�er davran��/AI scriptlerini durdur
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
            // Son olarak kendimizi yine a��k b�rak�yoruz
            enabled = true;
        }

        // Y�n hesapla
        gercekUcusYonu = ucusYonu.normalized;
        if (sr != null && sagaBakarkenSagaKacsin)
        {
            // Sprite'�n scale.x ile y�z y�n�n� yakalay�p ka��� y�n�n� �evir
            bool sagaBakiyor = transform.localScale.x >= 0f;
            if (!sagaBakiyor) gercekUcusYonu.x *= -1f;
            // G�rsel flip ayar� (sprite�� do�ru y�ne baks�n istiyorsan)
            sr.flipX = !sagaBakiyor;
        }

        // Sabit h�zda ka�maya ba�la
        if (rb != null) rb.velocity = gercekUcusYonu * ucusHizi;

        PlaySFX();
    }

    void Update()
    {
        if (!kaciyor || rb == null) return;

        // Hafif yukar� ivme vererek �u�up uzakla�ma� hissi
        rb.velocity = Vector2.Lerp(rb.velocity, gercekUcusYonu * ucusHizi, 0.04f);
    }


    void LateUpdate()
    {
        if (!kaciyor) return;

        var cam = hedefKamera != null ? hedefKamera : Camera.main;
        if (cam == null) return; // sahnede kamera yoksa sus

        Vector3 vp = cam.WorldToViewportPoint(transform.position);

        // Z>0 �nde, Z<0 kameran�n arkas�nda (bu da yok etme sebebi)
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
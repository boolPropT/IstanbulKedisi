using UnityEngine;

public class UcanMartiGecis : MonoBehaviour
{
    // ------------------------------------------------
    // HAREKET
    // ------------------------------------------------
    [Header("Hareket")]
    public float hiz = 6f;
    public bool solaDogru = true;

    // (Müze eseri) Eski viewport kesmesi için kullanýlýyordu; dursun ama kullanýlmýyor.
    public float ekranDisMarj = 0.08f;

    // ------------------------------------------------
    // DROP
    // ------------------------------------------------
    [Header("Drop")]
    public Transform hedefKedi;              // Boþsa Start'ta "Kedi" tag’ýndan bulunur
    public GameObject kakaYayPrefab;         // Kaka prefab’ý (KakaDusTopDown)
    public Transform dropNoktasi;            // Martýnýn alt/orta noktasý
    public float hizEsigiBirak = 0.4f;       // Kedinin X hizasýna yaklaþma eþiði
    public bool sadeceBirKereBirak = true;   // Bir kez býraksýn mý

    // ------------------------------------------------
    // GÖRSEL
    // ------------------------------------------------
    [Header("Görsel Yön")]
    public bool spriteSagPozitifScale = true; // Sprite saða bakarken Scale.x > 0 mý?

    // ------------------------------------------------
    // YOK ETME (KAMERA DÜNYA SINIRINA GÖRE)
    // ------------------------------------------------
    [Header("Yok Etme (Kamera Dünya Sýnýrýna Göre)")]
    public Camera hedefKamera;                  // Inspector’dan ver; boþsa Awake’te Camera.main
    public float killMarginWorld = 2f;          // “dýþarý” saymak için dünya tamponu
    public float disaridaykenYokEtGecikmesi = 0.15f; // dýþarýda bu kadar kalýrsa öldür
    public bool killIfExitedWithoutDrop = true; // drop yapmasa da dýþarý çýkýnca öldür

    [Header("Emniyet")]
    public float azamiYasam = 20f;              // En fazla bu kadar yaþasýn

    [Header("Ses")]
    public AudioClip sfxKakaBirak;     // short "puff" sound
    public float sfxSes = 0.8f;
    AudioSource asrc;

    // ------------------------------------------------
    // Ýç durum
    // ------------------------------------------------
    bool birakti;              // Bu yaþam döngüsünde drop tetikledi mi
    bool dropYapti;            // En az bir kez kaka býraktý mý
    float yasamSayaci;         // Emniyet sayaç
    bool onceInside;           // Ekrana en az bir kere girdi mi
    float outsideTimer;        // Dýþarýda ne kadar kaldý

    Camera cam;
    SpriteRenderer sr;

    void Awake()
    {
        asrc = GetComponent<AudioSource>();
        if (!asrc) asrc = gameObject.AddComponent<AudioSource>();
        asrc.playOnAwake = false;
        asrc.spatialBlend = 0f;

        cam = Camera.main;
        if (!hedefKamera) hedefKamera = Camera.main;

        sr = GetComponent<SpriteRenderer>();
        if (!sr) sr = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        // Hedef kediyi bul
        if (!hedefKedi)
        {
            var k = GameObject.FindGameObjectWithTag("Kedi");
            if (k) hedefKedi = k.transform;
        }

        // Yönü scale ile kilitle
        if (sr)
        {
            var s = transform.localScale;
            float yon = solaDogru ? -1f : 1f;
            s.x = Mathf.Abs(s.x) * (spriteSagPozitifScale ? yon : -yon);
            transform.localScale = s;
        }
    }

    void Update()
    {
        // Hareket
        float xdir = solaDogru ? -1f : 1f;
        transform.position += new Vector3(xdir * hiz, 0f, 0f) * Time.deltaTime;

        // Drop koþulu
        if (!birakti && hedefKedi != null)
        {
            float dx = Mathf.Abs(transform.position.x - hedefKedi.position.x);
            if (dx <= hizEsigiBirak)
            {
                BirakKaka();
                if (sadeceBirKereBirak) birakti = true;
            }
        }

        // Kamera dünya sýnýrý tabanlý “içeride miyim” kontrolü
        if (hedefKamera)
        {
            Vector3 camPos = hedefKamera.transform.position;
            float halfH = hedefKamera.orthographicSize;
            float halfW = halfH * hedefKamera.aspect;

            float left = camPos.x - halfW;
            float right = camPos.x + halfW;
            float bottom = camPos.y - halfH;
            float top = camPos.y + halfH;

            Vector3 p = transform.position;

            bool insideNow =
                p.x >= left && p.x <= right &&
                p.y >= bottom && p.y <= top;

            if (insideNow)
            {
                onceInside = true;
                outsideTimer = 0f;
            }
            else if (onceInside)
            {
                // ekran dikdörtgeninden çýktýk; killMarginWorld kadar dýþarý taþýnca ve
                // yeterince bekleyince öldür
                float leftKill = left - killMarginWorld;
                float rightKill = right + killMarginWorld;
                float bottomKill = bottom - killMarginWorld;
                float topKill = top + killMarginWorld;

                bool outOfBounds =
                    p.x < leftKill || p.x > rightKill ||
                    p.y < bottomKill || p.y > topKill;

                if (outOfBounds && (dropYapti || killIfExitedWithoutDrop))
                {
                    outsideTimer += Time.deltaTime;
                    if (outsideTimer >= disaridaykenYokEtGecikmesi)
                    {
                        Destroy(gameObject);
                        return;
                    }
                }
            }
        }

        // Emniyet: maksimum yaþam
        if (azamiYasam > 0f)
        {
            yasamSayaci += Time.deltaTime;
            if (yasamSayaci >= azamiYasam)
            {
                Destroy(gameObject);
                return;
            }
        }
    }

    void BirakKaka()
    {
        if (!kakaYayPrefab) return;

        Vector3 pos = (dropNoktasi ? dropNoktasi.position : transform.position) + Vector3.down * 0.1f;

        var go = Instantiate(kakaYayPrefab, pos, Quaternion.identity, null);
        go.transform.SetParent(null, true);

        // en az bir kez drop iþaretini koy
        dropYapti = true;

        PlaySFX();

        // KakaDusTopDown kullanýyorsun; ayarlarý prefab’tan geliyor.
        // Gerekirse drift vb. burada ince ayar yapýlabilir.
    }

    void PlaySFX()
    {
        if (sfxKakaBirak && asrc)
            asrc.PlayOneShot(sfxKakaBirak, sfxSes);
    }
}

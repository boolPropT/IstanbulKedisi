using UnityEngine;

public class UcanMartiGecis : MonoBehaviour
{
    // ------------------------------------------------
    // HAREKET
    // ------------------------------------------------
    [Header("Hareket")]
    public float hiz = 6f;
    public bool solaDogru = true;

    // (M�ze eseri) Eski viewport kesmesi i�in kullan�l�yordu; dursun ama kullan�lm�yor.
    public float ekranDisMarj = 0.08f;

    // ------------------------------------------------
    // DROP
    // ------------------------------------------------
    [Header("Drop")]
    public Transform hedefKedi;              // Bo�sa Start'ta "Kedi" tag��ndan bulunur
    public GameObject kakaYayPrefab;         // Kaka prefab�� (KakaDusTopDown)
    public Transform dropNoktasi;            // Mart�n�n alt/orta noktas�
    public float hizEsigiBirak = 0.4f;       // Kedinin X hizas�na yakla�ma e�i�i
    public bool sadeceBirKereBirak = true;   // Bir kez b�raks�n m�

    // ------------------------------------------------
    // G�RSEL
    // ------------------------------------------------
    [Header("G�rsel Y�n")]
    public bool spriteSagPozitifScale = true; // Sprite sa�a bakarken Scale.x > 0 m�?

    // ------------------------------------------------
    // YOK ETME (KAMERA D�NYA SINIRINA G�RE)
    // ------------------------------------------------
    [Header("Yok Etme (Kamera D�nya S�n�r�na G�re)")]
    public Camera hedefKamera;                  // Inspector�dan ver; bo�sa Awake�te Camera.main
    public float killMarginWorld = 2f;          // �d��ar�� saymak i�in d�nya tamponu
    public float disaridaykenYokEtGecikmesi = 0.15f; // d��ar�da bu kadar kal�rsa �ld�r
    public bool killIfExitedWithoutDrop = true; // drop yapmasa da d��ar� ��k�nca �ld�r

    [Header("Emniyet")]
    public float azamiYasam = 20f;              // En fazla bu kadar ya�as�n

    [Header("Ses")]
    public AudioClip sfxKakaBirak;     // short "puff" sound
    public float sfxSes = 0.8f;
    AudioSource asrc;

    // ------------------------------------------------
    // �� durum
    // ------------------------------------------------
    bool birakti;              // Bu ya�am d�ng�s�nde drop tetikledi mi
    bool dropYapti;            // En az bir kez kaka b�rakt� m�
    float yasamSayaci;         // Emniyet saya�
    bool onceInside;           // Ekrana en az bir kere girdi mi
    float outsideTimer;        // D��ar�da ne kadar kald�

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

        // Y�n� scale ile kilitle
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

        // Drop ko�ulu
        if (!birakti && hedefKedi != null)
        {
            float dx = Mathf.Abs(transform.position.x - hedefKedi.position.x);
            if (dx <= hizEsigiBirak)
            {
                BirakKaka();
                if (sadeceBirKereBirak) birakti = true;
            }
        }

        // Kamera d�nya s�n�r� tabanl� �i�eride miyim� kontrol�
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
                // ekran dikd�rtgeninden ��kt�k; killMarginWorld kadar d��ar� ta��nca ve
                // yeterince bekleyince �ld�r
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

        // Emniyet: maksimum ya�am
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

        // en az bir kez drop i�aretini koy
        dropYapti = true;

        PlaySFX();

        // KakaDusTopDown kullan�yorsun; ayarlar� prefab�tan geliyor.
        // Gerekirse drift vb. burada ince ayar yap�labilir.
    }

    void PlaySFX()
    {
        if (sfxKakaBirak && asrc)
            asrc.PlayOneShot(sfxKakaBirak, sfxSes);
    }
}

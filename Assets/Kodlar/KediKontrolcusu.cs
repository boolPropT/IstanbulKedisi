using UnityEngine;

public class KediKontrolcusu : MonoBehaviour
{
    public float hiz = 5f;
   
    public Transform atisNoktasi;
    [SerializeField] float atisCikisMesafesi = 0.35f;
    public GameObject mermiPrefab;

    [Header("Ses")]
    public AudioClip sfxAtesEt;     // short "puff" sound
    public float sfxSes = 0.8f;
    AudioSource asrc;

    Rigidbody2D rb;
    Vector2 hareket;
    Camera cam;
    SpriteRenderer sr;

    void Awake()
    {
        asrc = GetComponent<AudioSource>();
        if (!asrc) asrc = gameObject.AddComponent<AudioSource>();
        asrc.playOnAwake = false;
        asrc.spatialBlend = 0f;

        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        sr = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        //wasd veya ok tuþlarý ile karakteri kontrol
        var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        hareket = (input.sqrMagnitude > 1f) ? input.normalized : input;

        //fare ile niþan alma
        Vector3 fareDunyaKonumu = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 yon = (fareDunyaKonumu - atisNoktasi.position).normalized;
        atisNoktasi.right = yon;

        if (Input.GetMouseButtonDown(0))
        {
            AtesEt();
            PlaySFX();
        }

        //kediyi sola saða döndürme
        if (yon.x < 0)
        {
            sr.flipX = true; //sola dön
        }
        else
        {
            sr.flipX = false; //saða dönük kal
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + hiz * Time.fixedDeltaTime * hareket);
    }

    void AtesEt()
    {
        if (!mermiPrefab) return;

        Vector2 dir = atisNoktasi.right.normalized;
        Vector3 pos = atisNoktasi.position + (Vector3)dir * atisCikisMesafesi;
        var go = Instantiate(mermiPrefab, pos, Quaternion.identity);

        if (go.TryGetComponent(out Mermi m)) m.Firlat(dir);

    }

    void PlaySFX()
    {
        if (sfxAtesEt && asrc)
            asrc.PlayOneShot(sfxAtesEt, sfxSes);
    }
}

using UnityEngine;

public class Marti : MonoBehaviour
{
    [Header("Hareket")]
    public float hiz = 2.8f;

    [Header("Saldýrý")]
    public int verilenHasar = 1;        // bir vuruþta kaç can alacak
    public float vurBekle = 0.7f;       // Vurduktan sonra bekleyeceði süre, saniye bazýnda. Çok kolay gelirse 0.5 civarý tut.
    public float geriZipla = 7f;      // geri tepme hýzý (birim/sn)
    public float temasEsigi = 0.15f;    // collider mesafe eþiði 

    [SerializeField] float geriZiplaSuresi = 0.12f; // geri tepmenin süresi

    Rigidbody2D rb;
    SpriteRenderer sr;

    Transform oyuncu;
    KedininCani kedininCani;
    Collider2D kediCol;
    Collider2D martiCol;

    float saldiriSogumasi;          // gerçek sayaç (geri sayým)
    Vector2 takip;                  // ileri yürüyüþ yönü

    // knockback
    float geriZiplaSayac;
    Vector2 geriZiplaVektor;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        martiCol = GetComponent<Collider2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Start()
    {
        var k = GameObject.FindGameObjectWithTag("Kedi");
        if (k) BaglaKedi(k.transform);
    }

    void BaglaKedi(Transform t)
    {
        oyuncu = t;

        // KedininCani
        if (t.TryGetComponent(out KedininCani kc))
            kedininCani = kc;
        else
        {
            kc = t.GetComponentInChildren<KedininCani>();
            if (kc) kedininCani = kc;
            else
            {
                kc = t.GetComponentInParent<KedininCani>();
                kedininCani = kc;
            }
        }

        // Collider2D
        if (t.TryGetComponent(out Collider2D col))
            kediCol = col;
        else
        {
            col = t.GetComponentInChildren<Collider2D>();
            if (col) kediCol = col;
            else
            {
                col = t.GetComponentInParent<Collider2D>();
                kediCol = col;
            }
        }
    }


    void Update()
    {
        if (dondur)
        {
            return;
        }

        if (saldiriSogumasi > 0f) saldiriSogumasi -= Time.deltaTime;
        if (geriZiplaSayac > 0f) geriZiplaSayac -= Time.deltaTime;

        if (!oyuncu)
        {
            takip = Vector2.zero;
            var k = GameObject.FindGameObjectWithTag("Kedi");
            if (k) BaglaKedi(k.transform);
            return;
        }

        Vector2 delta = (Vector2)(oyuncu.position - transform.position);

        if (sr && Mathf.Abs(delta.x) > 0.001f) sr.flipX = (delta.x < 0f);

        bool temas;
        if (martiCol && kediCol)
        {
            var d = Physics2D.Distance(martiCol, kediCol);
            temas = d.distance <= temasEsigi; // <=0: overlap
        }
        else
        {
            temas = delta.magnitude <= 0.7f; // yedek
        }

        if (temas)
        {
            takip = Vector2.zero;
            VurmayiDene(delta);
        }
        else
        {
            takip = delta.normalized;
        }

        
    }

    void FixedUpdate()
    {
        if (dondur) return;

        Vector2 hareket = (geriZiplaSayac > 0f)
            ? geriZiplaVektor                   // sadece knockback
            : takip * hiz;                      // yoksa takip

        if (hareket.sqrMagnitude > 0.0001f)
            rb.MovePosition(rb.position + hareket * Time.fixedDeltaTime);
    }


    void VurmayiDene(Vector2 deltaOyuncuya)
    {

        if (saldiriSogumasi > 0f) return;
        if (kedininCani == null) return;


        if (kedininCani.Vurulabilir())
        {
            kedininCani.HasarAl(verilenHasar);  // hasar ver
            saldiriSogumasi = vurBekle;

            BaslatGeriTepki(deltaOyuncuya);

            // çarpýþmayý kýsa süre kapat
            if (martiCol && kediCol) StartCoroutine(GeciciCarpismaKapat(0.1f));
        }
    }

    void BaslatGeriTepki(Vector2 deltaOyuncuya)
    {
        Vector2 itisYon = (-deltaOyuncuya).normalized;
        geriZiplaVektor = itisYon * geriZipla; // bu "hýz" vektörü (unit/sn)
        geriZiplaSayac = geriZiplaSuresi;
    }

    System.Collections.IEnumerator GeciciCarpismaKapat(float sure)
    {
        Physics2D.IgnoreCollision(martiCol, kediCol, true);
        yield return new WaitForSeconds(sure);
        if (martiCol && kediCol) Physics2D.IgnoreCollision(martiCol, kediCol, false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.7f);
    }

    //MARTIYI DONDURMA BÖLÜMÜ

    bool dondur = false;

    private void OnEnable()
    {
        OyunDurumu.KediOldu += Dondur;
    }
    private void OnDisable()
    {
        OyunDurumu.KediOldu -= Dondur;
    }

    void Dondur()
    {
        dondur = true;
        takip = Vector2.zero;
        geriZiplaSayac = 0f;
        geriZiplaVektor = Vector2.zero;


        if (rb)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;

            //Fiziði durdur.
            rb.simulated = false;

            /* Alternatif: Simülasyon kapatmaktan vazgeçersem.
            
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
            
             */
        }

        StopAllCoroutines();
    }
}

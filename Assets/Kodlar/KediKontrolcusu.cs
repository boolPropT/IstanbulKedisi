using UnityEngine;

public class KediKontrolcusu : MonoBehaviour
{
    public float hiz = 5f;
   
    public Transform atisNoktasi;
    [SerializeField] float atisCikisMesafesi = 0.35f;
    public GameObject mermiPrefab;
    
    Rigidbody2D rb;
    Vector2 hareket;
    Camera cam;
    SpriteRenderer sr;

    void Awake()
    {
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
}

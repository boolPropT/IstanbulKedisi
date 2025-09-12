using UnityEngine;

public class Mermi : MonoBehaviour
{
    public float hiz = 18f;
    public float omur = 3f;
    public int hasar = 1;
    public LayerMask vurulacakKatman;
    [SerializeField] bool kameradanCikincaYokEt = true;
    [SerializeField] float tasmaPayi = 0.02f;


    Rigidbody2D rb;
    Camera cam;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    //kedi çaðýracak...
    public void Firlat(Vector2 yon, float hizCarpani = 1f)
    {
        yon = yon.sqrMagnitude > 0 ? yon.normalized : Vector2.right;
        rb.velocity = hiz * hizCarpani * yon;
        transform.right = yon;
        Destroy(gameObject, omur);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((vurulacakKatman.value & (1 << other.gameObject.layer)) == 0)
            return;
        //Sadece vurulacakKatman'dakilere vur.


        if (other.TryGetComponent<Can>(out var can))
        {
            can.HasarAl(hasar);
            Destroy(gameObject);
            return;
        }
    }

    
    private void Update()
    {
        if (!kameradanCikincaYokEt || cam == null) return;
        Vector3 v = cam.WorldToViewportPoint(transform.position);
        bool mermiKameraDisinda = v.z > 0f && (v.x < -tasmaPayi || v.x > 1f + tasmaPayi || v.y < -tasmaPayi || v.y > 1f + tasmaPayi);
        if (mermiKameraDisinda) Destroy(gameObject);
    }

}

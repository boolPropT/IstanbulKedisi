using UnityEngine;

public class KakaParcasi : MonoBehaviour
{
    public Vector2 hiz = Vector2.zero;
    public float omur = 1.6f;
    public int zarar = 6;
    public LayerMask hedefKatman;   // Kedi layer’ý
    public string hedefTag = "Kedi";
    public float carpismaYaricapi = 0.22f;

    void Start()
    {
        if (omur > 0f) Destroy(gameObject, omur);
    }

    void Update()
    {
        transform.position += (Vector3)(hiz * Time.deltaTime);

        Collider2D col = hedefKatman.value != 0
            ? Physics2D.OverlapCircle(transform.position, carpismaYaricapi, hedefKatman)
            : Physics2D.OverlapCircle(transform.position, carpismaYaricapi);

        if (col)
        {
            if (!string.IsNullOrEmpty(hedefTag) && !col.CompareTag(hedefTag))
                return;

            var can = col.GetComponentInParent<KedininCani>();
            if (can != null)
            {
                can.HasarAl(zarar);
                Destroy(gameObject);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, carpismaYaricapi);
    }
}

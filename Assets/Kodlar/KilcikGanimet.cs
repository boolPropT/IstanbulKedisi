using UnityEngine;

public class KilcikGanimet : MonoBehaviour
{
    [Header("Puan")]
    public int puan = 5;

    [Header("Davranis")]
    public float omur = 12f;        // yerde kalacagi en fazla sure
    public float bobGenlik = 0.05f; // yukari-asagi salinim
    public float bobHiz = 2.5f;     // salinim hizi
    public float donmeHizi = 45f;   // derece/sn


    Vector3 baslangicPos;

    void Awake()
    {
        baslangicPos = transform.position;
        if (omur > 0) Destroy(gameObject, omur);
    }

    void Update()
    {
        float y = Mathf.Sin(Time.time * bobHiz) * bobGenlik;
        transform.position = new Vector3(baslangicPos.x, baslangicPos.y + y, baslangicPos.z);
        if (donmeHizi != 0) transform.Rotate(0, 0, donmeHizi * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Kedi")) return;

        
        SkorYoneticisi.Instance?.Ekle(puan);
        Destroy(gameObject);
        
    }

    
}
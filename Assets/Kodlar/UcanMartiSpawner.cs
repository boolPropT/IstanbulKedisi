using UnityEngine;

public class UcanMartiSpawner : MonoBehaviour
{
    public GameObject martiPrefab;
    public float sureMin = 3f;
    public float sureMax = 6f;

    [Tooltip("X ekseninde soldan/saðdan iç pay (dünya birimi)")]
    public float padXWorld = 0.25f;

    [Tooltip("Üst kenardan iç pay (dünya birimi)")]
    public float padYWorld = 0.10f;

    Camera cam;
    float sayac;

    void Awake()
    {
        cam = Camera.main;
        if (!cam) Debug.LogError("[Spawner] MainCamera tag’lý kamera yok.");
        if (!martiPrefab) Debug.LogError("[Spawner] martiPrefab atanmadý.");
    }

    void Start()
    {
        sayac = Random.Range(sureMin, sureMax);
    }

    void Update()
    {
        sayac -= Time.deltaTime;
        if (sayac <= 0f)
        {
            Spawn();
            sayac = Random.Range(sureMin, sureMax);
        }
    }

    void Spawn()
    {
        if (!martiPrefab) return;
        if (!cam) cam = Camera.main;
        if (!cam) return;

        // Kamera dünya sýnýrlarý
        Vector3 camPos = cam.transform.position;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        float leftX = camPos.x - halfW;
        float rightX = camPos.x + halfW;
        float topY = camPos.y + halfH;
        float bottomY = camPos.y - halfH;

        // Kenarlardan iç pay (dünya birimi)
        const float padXWorld = 0.25f;  // sol/sað güvenlik payý
        const float padYWorld = 0.10f;  // üst kenardan güvenlik payý

        // 1) Kediye göre KENAR seç: Kedi saðdaysa soldan doð, kedi soldaysa saðdan
        Transform kedi = null;
        var k = GameObject.FindGameObjectWithTag("Kedi");
        if (k) kedi = k.transform;

        bool spawnLeft, martiLeftToRight;
        if (kedi)
        {
            bool kediSagda = kedi.position.x >= camPos.x;
            spawnLeft = kediSagda;                  // kedi saðda -> soldan doð
            martiLeftToRight = kediSagda;          // kedi saðda -> saða uç
        }
        else
        {
            spawnLeft = Random.value < 0.5f;
            martiLeftToRight = spawnLeft;          // kenardan içeri
        }

        // 2) X: SADECE kenar (tam içeri klamplayacaðýz)
        float spawnX = spawnLeft ? leftX : rightX;

        // 3) Y: üst kenarýn hemen içinde (sprite yüksekliðini birazdan hesaplayýp netleyeceðiz)
        float spawnY = topY - padYWorld;

        // 4) Geçici konumda doður
        Vector3 world = new Vector3(spawnX, spawnY, 0f);
        var go = Instantiate(martiPrefab, world, Quaternion.identity);

        // 5) Sprite’ýn GERÇEK boyuna göre tamamen ÝÇERÝ al
        var renderers = go.GetComponentsInChildren<SpriteRenderer>();
        if (renderers != null && renderers.Length > 0)
        {
            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);

            float sprHalfW = b.extents.x;
            float sprHalfH = b.extents.y;

            Vector3 p = go.transform.position;

            // X: kenardan içeri, kafa/kanat taþmayacak
            p.x = spawnLeft
                ? leftX + sprHalfW + padXWorld
                : rightX - sprHalfW - padXWorld;

            // Y: DAÝMA ÜST KENARIN içinde; ortalara asla düþmesin
            p.y = topY - sprHalfH - padYWorld;

            p.z = 0f;
            go.transform.position = p;
        }
        else
        {
            go.transform.position = new Vector3(world.x, world.y, 0f);
        }

        // 6) Yön ve hedef
        var u = go.GetComponent<UcanMartiGecis>();
        if (u != null)
        {
            // solaDogru: true -> sola, false -> saða
            u.solaDogru = !martiLeftToRight ? true : false;

            if (!u.hedefKedi && kedi) u.hedefKedi = kedi;
            if (!u.hedefKamera) u.hedefKamera = cam;

            // Ekrandan çýkýnca yok etme kurallarý (drop yapsa da dýþarý çýkarsa temizle)
            u.killMarginWorld = 2f;
            u.disaridaykenYokEtGecikmesi = 0.15f;
            u.killIfExitedWithoutDrop = true;
        }
    }

}

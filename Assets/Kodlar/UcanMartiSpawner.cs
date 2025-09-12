using UnityEngine;

public class UcanMartiSpawner : MonoBehaviour
{
    public GameObject martiPrefab;
    public float sureMin = 3f;
    public float sureMax = 6f;

    [Tooltip("X ekseninde soldan/sa�dan i� pay (d�nya birimi)")]
    public float padXWorld = 0.25f;

    [Tooltip("�st kenardan i� pay (d�nya birimi)")]
    public float padYWorld = 0.10f;

    Camera cam;
    float sayac;

    void Awake()
    {
        cam = Camera.main;
        if (!cam) Debug.LogError("[Spawner] MainCamera tag�l� kamera yok.");
        if (!martiPrefab) Debug.LogError("[Spawner] martiPrefab atanmad�.");
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

        // Kamera d�nya s�n�rlar�
        Vector3 camPos = cam.transform.position;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        float leftX = camPos.x - halfW;
        float rightX = camPos.x + halfW;
        float topY = camPos.y + halfH;
        float bottomY = camPos.y - halfH;

        // Kenarlardan i� pay (d�nya birimi)
        const float padXWorld = 0.25f;  // sol/sa� g�venlik pay�
        const float padYWorld = 0.10f;  // �st kenardan g�venlik pay�

        // 1) Kediye g�re KENAR se�: Kedi sa�daysa soldan do�, kedi soldaysa sa�dan
        Transform kedi = null;
        var k = GameObject.FindGameObjectWithTag("Kedi");
        if (k) kedi = k.transform;

        bool spawnLeft, martiLeftToRight;
        if (kedi)
        {
            bool kediSagda = kedi.position.x >= camPos.x;
            spawnLeft = kediSagda;                  // kedi sa�da -> soldan do�
            martiLeftToRight = kediSagda;          // kedi sa�da -> sa�a u�
        }
        else
        {
            spawnLeft = Random.value < 0.5f;
            martiLeftToRight = spawnLeft;          // kenardan i�eri
        }

        // 2) X: SADECE kenar (tam i�eri klamplayaca��z)
        float spawnX = spawnLeft ? leftX : rightX;

        // 3) Y: �st kenar�n hemen i�inde (sprite y�ksekli�ini birazdan hesaplay�p netleyece�iz)
        float spawnY = topY - padYWorld;

        // 4) Ge�ici konumda do�ur
        Vector3 world = new Vector3(spawnX, spawnY, 0f);
        var go = Instantiate(martiPrefab, world, Quaternion.identity);

        // 5) Sprite��n GER�EK boyuna g�re tamamen ��ER� al
        var renderers = go.GetComponentsInChildren<SpriteRenderer>();
        if (renderers != null && renderers.Length > 0)
        {
            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);

            float sprHalfW = b.extents.x;
            float sprHalfH = b.extents.y;

            Vector3 p = go.transform.position;

            // X: kenardan i�eri, kafa/kanat ta�mayacak
            p.x = spawnLeft
                ? leftX + sprHalfW + padXWorld
                : rightX - sprHalfW - padXWorld;

            // Y: DA�MA �ST KENARIN i�inde; ortalara asla d��mesin
            p.y = topY - sprHalfH - padYWorld;

            p.z = 0f;
            go.transform.position = p;
        }
        else
        {
            go.transform.position = new Vector3(world.x, world.y, 0f);
        }

        // 6) Y�n ve hedef
        var u = go.GetComponent<UcanMartiGecis>();
        if (u != null)
        {
            // solaDogru: true -> sola, false -> sa�a
            u.solaDogru = !martiLeftToRight ? true : false;

            if (!u.hedefKedi && kedi) u.hedefKedi = kedi;
            if (!u.hedefKamera) u.hedefKamera = cam;

            // Ekrandan ��k�nca yok etme kurallar� (drop yapsa da d��ar� ��karsa temizle)
            u.killMarginWorld = 2f;
            u.disaridaykenYokEtGecikmesi = 0.15f;
            u.killIfExitedWithoutDrop = true;
        }
    }

}

using UnityEngine;
using System.Collections;

public class MartiSpawner : MonoBehaviour
{
    [Header("Genel")]
    public GameObject martiPrefab;
    public BoxCollider2D spawnAlani;   // haritanin DIS siniri (dunya icini kapsasin)
    public Transform hedef;            // kedi

    [Header("Offscreen Spawn")]
    public float kameraKenarPayi = 0.2f;   // ekran disina ne kadar tasma (dunya icinde)
    public float minKediMesafesi = 3.5f;   // kedinin dibine dogmasin
    public LayerMask engelKatmani;         // duvar/sahil vb. dogdugu anda icine girmesin

    [Header("Zorluk")]
    public float baslangicAralik = 2.5f;
    public float minimumAralik = 1.0f;
    public float azaltmaAdimi = 0.1f;
    public int maksimumAktifDusman = 30;

    int aktifDusman;
    float anlikAralik;

    public bool autoStart = true;       // Inspector’dan kapatýlabilir
    Coroutine cr;                       // aktif coroutine

    void Start()
    {
        anlikAralik = baslangicAralik;
        if (autoStart)
            StartSpawning();            // ESKÝ: StartCoroutine(Akisi()); yerine
    }

    public void StartSpawning()
    {
        if (cr != null) StopCoroutine(cr);
        cr = StartCoroutine(Akisi());
    }

    public void StopSpawning()
    {
        if (cr != null)
        {
            StopCoroutine(cr);
            cr = null;
        }
    }

    //WaveManager dýþarýdan “x adet” istemek isterse
    public void Spawn(int adet = 1)
    {
        for (int i = 0; i < adet; i++) Spawnla();
    }

    IEnumerator Akisi()
    {
        while (true)
        {
            if (OyunDurumu.kediHayatta == false) yield break;

            if (aktifDusman < maksimumAktifDusman)
                Spawnla();

            yield return new WaitForSeconds(anlikAralik);
            anlikAralik = Mathf.Max(minimumAralik, anlikAralik - azaltmaAdimi);
        }
    }

    bool Spawnla()
    {
        if (!martiPrefab || !spawnAlani) return false;

        if (TryGetOffscreenPoint(out Vector2 p))
        {
            var go = Instantiate(martiPrefab, p, Quaternion.identity);
            aktifDusman++;
            var can = go.GetComponent<Can>();
            if (can) can.Oldu += _ => aktifDusman--;
            return true;
        }
        return false;
    }

    bool TryGetOffscreenPoint(out Vector2 point)
    {
        point = Vector2.zero;
        var cam = Camera.main;
        if (!cam) return false;

        // 1) Kamera gorunumunu dunya koordinatina cevir
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        Vector2 camCenter = cam.transform.position;

        Rect camRect = new(
            camCenter.x - halfW, camCenter.y - halfH,
            2f * halfW, 2f * halfH
        );

        // 2) Spawn alaninin sinirlari
        Bounds wb = spawnAlani.bounds;

        // 3) Kameranin disinda ama dunya icinde olacak bir nokta ara
        const int MAX_TRY = 40;
        for (int i = 0; i < MAX_TRY; i++)
        {
            // Ekranin disindaki 4 kenardan birini sec
            int kenar = Random.Range(0, 4); // 0=sol, 1=sag, 2=alt, 3=ust
            float x, y;

            switch (kenar)
            {
                case 0: // sol
                    x = camRect.xMin - kameraKenarPayi;
                    y = Random.Range(camRect.yMin, camRect.yMax);
                    break;
                case 1: // sag
                    x = camRect.xMax + kameraKenarPayi;
                    y = Random.Range(camRect.yMin, camRect.yMax);
                    break;
                case 2: // alt
                    y = camRect.yMin - kameraKenarPayi;
                    x = Random.Range(camRect.xMin, camRect.xMax);
                    break;
                default: // ust
                    y = camRect.yMax + kameraKenarPayi;
                    x = Random.Range(camRect.xMin, camRect.xMax);
                    break;
            }

            // Dunya sinirlari icine clamp et (haritanin disina tasma)
            x = Mathf.Clamp(x, wb.min.x + 0.1f, wb.max.x - 0.1f);
            y = Mathf.Clamp(y, wb.min.y + 0.1f, wb.max.y - 0.1f);

            Vector2 aday = new(x, y);

            // Kediye cok yakin degil
            if (hedef && Vector2.Distance(aday, hedef.position) < minKediMesafesi)
                continue;

            // Engellerin icine dogma (sahil duvarlari, binalar vs.)
            bool icindeEngel = Physics2D.OverlapCircle(aday, 0.2f, engelKatmani);
            if (icindeEngel) continue;

            // Opsiyonel: Dogrudan gorus hatta engel var mi? (patikasi kapaliysa spawn etme)
            if (hedef)
            {
                var hit = Physics2D.Linecast(aday, hedef.position, engelKatmani);
                if (hit.collider != null) continue; // arada duvar var, vazgec
            }

            point = aday;
            return true;
        }

        return false; // yer bulamadi
    }
}
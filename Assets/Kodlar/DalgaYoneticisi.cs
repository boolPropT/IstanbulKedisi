using System.Collections;
using UnityEngine;
using TMPro;

public class DalgaYoneticisi : MonoBehaviour
{
    [Header("Banner")]
    public DalgaBanner banner;

    [Header("Shop")]
    public OdulEkrani shopUI;
    public bool herDalgaArasiShop = true; // son dalga hari�

    [System.Serializable]
    public class WaveDef
    {
        public string ad = "DALGA";
        public float sure = 60f;

        [Header("Y�r�yen Mart�lar")]
        public MartiSpawner yuruyenSpawner;
        public bool yuruyenAcik = true;
        public float yuruyenAralik = 1.2f;  // sadece bilgi ama�l�; MartiSpawner kendi i� i�ini yapar
        public int yuruyenPaket = 2;        // dalga d�ng�s�nde elle tetiklemek isterseniz

        [Header("U�an Mart�lar")]
        public UcanMartiSpawner ucanSpawner;
        public bool ucanAcik = false;
        public float ucanAralik = 3.5f;     // bilgi ama�l�
        public int ucanPaket = 1;

        [Header("Boss (opsiyonel)")]
        public bool bossVar = false;
        public GameObject bossPrefab;
        public Vector3 bossSpawn = Vector3.zero;
    }

    [Header("Dalgalar")]
    public WaveDef[] dalgalar;

    [Header("UI")]
    public TMP_Text dalgaMetni;        // �rn: "DALGA 1" veya WaveDef.ad
    public TMP_Text saya�Metni;        // "Kalan: 00:59"
    public GameObject bitisPaneli;     // istersen GameOverPanel�i ba�la

    [Header("Demo")]
    public float demoToplamSure = 180f; // 3 dk

    float demoGeri;
    bool durdu;
    Coroutine ak��;

    void OnEnable()
    {
        OyunDurumu.Sifirla();  // temiz ba�lang��
        OyunDurumu.KediOldu += KediOldu;
    }

    void OnDisable()
    {
        OyunDurumu.KediOldu -= KediOldu;
    }

    void Start()
    {
        demoGeri = demoToplamSure;
        ak�� = StartCoroutine(DalgaAkisi());
    }

    IEnumerator DalgaAkisi()
    {
        for (int i = 0; i < dalgalar.Length; i++)
        {
            if (durdu) yield break;

            var w = dalgalar[i];

            if (banner) banner.Show(w.ad.ToUpper());   // �rn: "DALGA 2"

            // UI
            if (dalgaMetni) dalgaMetni.text = $"{w.ad}";

            // Spawner�lar� ba�lat
            if (w.yuruyenSpawner)
            {
                w.yuruyenSpawner.autoStart = false; // bizim kontrol�m�zde
                w.yuruyenSpawner.StartSpawning();
            }
            if (w.ucanSpawner && w.ucanAcik)
            {
                w.ucanSpawner.StartSpawning();
            }

            // Boss tek sefer
            if (w.bossVar && w.bossPrefab)
            {
                Instantiate(w.bossPrefab, w.bossSpawn, Quaternion.identity);
            }

            float t = w.sure;
            while (t > 0f && demoGeri > 0f && !durdu)
            {
                t -= Time.deltaTime;
                demoGeri -= Time.deltaTime;

                if (saya�Metni)
                {
                    int s = Mathf.CeilToInt(t);
                    saya�Metni.text = $"Kalan: {FormatSure(s)}";
                }

                yield return null;
            }

            // Dalga bitti -> spawner kapat
            if (w.yuruyenSpawner) w.yuruyenSpawner.StopSpawning();
            if (w.ucanSpawner) w.ucanSpawner.StopSpawning();
            
            if (herDalgaArasiShop && i < dalgalar.Length - 1 && shopUI != null && !durdu && demoGeri > 0f)
            {
                shopUI.Open();
                // shop kapanana kadar bekle
                while (shopUI.IsOpen() && !durdu)
                    yield return null;
            }

            if (demoGeri <= 0f || durdu) break;

            // �stersen nefes:
            // yield return new WaitForSeconds(1f);

            
        }

        Biti�();
    }

    void KediOldu()
    {
        if (durdu) return;
        durdu = true;
        Biti�();
    }

    void Biti�()
    {
        if (ak�� != null) StopCoroutine(ak��);
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (saya�Metni) saya�Metni.text = "Kalan: 00:00";
        if (bitisPaneli) bitisPaneli.SetActive(true);
    }

    string FormatSure(int saniye)
    {
        int m = saniye / 60;
        int s = saniye % 60;
        return $"{m:00}:{s:00}";
    }
}

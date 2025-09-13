using System.Collections;
using UnityEngine;
using TMPro;

public class DalgaYoneticisi : MonoBehaviour
{
    [Header("Banner")]
    public DalgaBanner banner;

    [Header("Shop")]
    public OdulEkrani shopUI;
    public bool herDalgaArasiShop = true; // son dalga hariç

    [System.Serializable]
    public class WaveDef
    {
        public string ad = "DALGA";
        public float sure = 60f;

        [Header("Yürüyen Martýlar")]
        public MartiSpawner yuruyenSpawner;
        public bool yuruyenAcik = true;
        public float yuruyenAralik = 1.2f;  // sadece bilgi amaçlý; MartiSpawner kendi iç iþini yapar
        public int yuruyenPaket = 2;        // dalga döngüsünde elle tetiklemek isterseniz

        [Header("Uçan Martýlar")]
        public UcanMartiSpawner ucanSpawner;
        public bool ucanAcik = false;
        public float ucanAralik = 3.5f;     // bilgi amaçlý
        public int ucanPaket = 1;

        [Header("Boss (opsiyonel)")]
        public bool bossVar = false;
        public GameObject bossPrefab;
        public Vector3 bossSpawn = Vector3.zero;
    }

    [Header("Dalgalar")]
    public WaveDef[] dalgalar;

    [Header("UI")]
    public TMP_Text dalgaMetni;        // örn: "DALGA 1" veya WaveDef.ad
    public TMP_Text sayaçMetni;        // "Kalan: 00:59"
    public GameObject bitisPaneli;     // istersen GameOverPanel’i baðla

    [Header("Demo")]
    public float demoToplamSure = 180f; // 3 dk

    float demoGeri;
    bool durdu;
    Coroutine akýþ;

    void OnEnable()
    {
        OyunDurumu.Sifirla();  // temiz baþlangýç
        OyunDurumu.KediOldu += KediOldu;
    }

    void OnDisable()
    {
        OyunDurumu.KediOldu -= KediOldu;
    }

    void Start()
    {
        demoGeri = demoToplamSure;
        akýþ = StartCoroutine(DalgaAkisi());
    }

    IEnumerator DalgaAkisi()
    {
        for (int i = 0; i < dalgalar.Length; i++)
        {
            if (durdu) yield break;

            var w = dalgalar[i];

            if (banner) banner.Show(w.ad.ToUpper());   // örn: "DALGA 2"

            // UI
            if (dalgaMetni) dalgaMetni.text = $"{w.ad}";

            // Spawner’larý baþlat
            if (w.yuruyenSpawner)
            {
                w.yuruyenSpawner.autoStart = false; // bizim kontrolümüzde
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

                if (sayaçMetni)
                {
                    int s = Mathf.CeilToInt(t);
                    sayaçMetni.text = $"Kalan: {FormatSure(s)}";
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

            // Ýstersen nefes:
            // yield return new WaitForSeconds(1f);

            
        }

        Bitiþ();
    }

    void KediOldu()
    {
        if (durdu) return;
        durdu = true;
        Bitiþ();
    }

    void Bitiþ()
    {
        if (akýþ != null) StopCoroutine(akýþ);
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (sayaçMetni) sayaçMetni.text = "Kalan: 00:00";
        if (bitisPaneli) bitisPaneli.SetActive(true);
    }

    string FormatSure(int saniye)
    {
        int m = saniye / 60;
        int s = saniye % 60;
        return $"{m:00}:{s:00}";
    }
}

using System.Collections;
using UnityEngine;
using TMPro;

public class DalgaYoneticisi : MonoBehaviour
{
    [Header("Paneller")]
    public OyunBittiPaneli oyunBittiPaneli;        // Ölümde açýlacak
    public OyunSonuSkorPaneli skorPaneli;          // Süre/dalga bitince açýlacak

    [Header("Banner")]
    public DalgaBanner banner;

    [Header("Shop")]
    public OdulEkrani shopUI;
    public bool herDalgaArasiShop = true; // son dalga haric

    // EKLENDI: Muzik ayarlari
    [Header("Müzik")]
    public AudioSource musicSource;        // loop çalacak kaynak (2D)
    public AudioClip[] waveLoops;          // her dalga için 1 loop (index = dalga indexi)
    public AudioClip skorLoop;             // dalgalar bitince/score ekranýnda çalýnacak
    public float fadeTime = 0.5f;          // klip geçiþlerinde fade süresi
    public float musicVolume = 0.8f;       // ana hedef ses seviyesi
    public bool stopMusicOnDeath = true;   // kedi ölürse müziði kes

    // EKLENDI: Ilk dalga gecikmesi ve opsiyonel banner/sayac
    [Header("Baslangic")]
    public float ilkDalgaGecikme = 10f;              // oyundan sonra ilk dalga bekleme suresi
    public bool baslangicSayaciGoster = true;        // geri sayimi sayacta goster
    public bool baslangictaBanner = true;            // hazirlan banneri goster
    public string baslangicBannerMetni = "HAZIRLAN!";

    [System.Serializable]
    public class WaveDef
    {
        public string ad = "DALGA";
        public float sure = 60f;

        [Header("Yuruyen Martilar")]
        public MartiSpawner yuruyenSpawner;
        public bool yuruyenAcik = true;
        public float yuruyenAralik = 1.2f;
        public int yuruyenPaket = 2;

        [Header("Ucan Martilar")]
        public UcanMartiSpawner ucanSpawner;
        public bool ucanAcik = false;
        public float ucanAralik = 3.5f;
        public int ucanPaket = 1;

        [Header("Boss (opsiyonel)")]
        public bool bossVar = false;
        public GameObject bossPrefab;
        public Vector3 bossSpawn = Vector3.zero;
    }

    [Header("Dalgalar")]
    public WaveDef[] dalgalar;

    [Header("UI")]
    public TMP_Text dalgaMetni;
    public TMP_Text sayacMetni;
    public GameObject bitisPaneli; // burada OyunSonuSkorPaneli component'i olacak

    [Header("Demo")]
    public float demoToplamSure = 180f;

    float demoGeri;
    bool durdu;
    Coroutine akis;

    // EKLENDI: muzik icin aktif fade coroutine referansi
    Coroutine musicCR;

    // Kedi referansi (can kaybini hesaplamak icin)
    KedininCani kedi;

    void OnEnable()
    {
        OyunDurumu.Sifirla();
        OyunDurumu.KediOldu += KediOldu;
    }

    void OnDisable()
    {
        OyunDurumu.KediOldu -= KediOldu;
    }

    void Start()
    {
        kedi = FindObjectOfType<KedininCani>(true);
        demoGeri = demoToplamSure;

        // EKLENDI: baslangicta tum spawnerlari emniyete al (otomatigi calismasin)
        for (int i = 0; i < dalgalar.Length; i++)
        {
            var w = dalgalar[i];
            if (w.yuruyenSpawner)
            {
                w.yuruyenSpawner.autoStart = false; // kendi Start'inda calismasin
                w.yuruyenSpawner.StopSpawning();    // her ihtimale karsi sustur
            }
            if (w.ucanSpawner)
            {
                w.ucanSpawner.StopSpawning();
            }
        }

        // EKLENDI: baslangicta muzik sesi sifirla
        if (musicSource)
        {
            musicSource.loop = true;
            musicSource.volume = 0f;
            musicSource.Stop();
            musicSource.clip = null;
        }

        akis = StartCoroutine(DalgaAkisi());
    }

    IEnumerator DalgaAkisi()
    {
        // EKLENDI: ilk dalga gecikmesi
        if (ilkDalgaGecikme > 0f && !durdu)
        {
            if (baslangictaBanner && banner)
                banner.Show(baslangicBannerMetni);

            float t0 = ilkDalgaGecikme;
            while (t0 > 0f && demoGeri > 0f && !durdu)
            {
                t0 -= Time.deltaTime;
                demoGeri -= Time.deltaTime; // toplam demo suresi icine bu bekleme de dahil

                if (baslangicSayaciGoster && sayacMetni)
                {
                    int s0 = Mathf.CeilToInt(t0);
                    sayacMetni.text = "Hazýrlan! " + FormatSure(s0);
                }
                yield return null;
            }
        }

        for (int i = 0; i < dalgalar.Length; i++)
        {
            if (durdu) yield break;

            var w = dalgalar[i];

            // EKLENDI: Dalga muzigi
            PlayWaveMusic(i);

            if (banner) banner.Show(w.ad.ToUpper());
            if (dalgaMetni) dalgaMetni.text = "" + w.ad;

            if (w.yuruyenSpawner)
            {
                w.yuruyenSpawner.autoStart = false;
                w.yuruyenSpawner.StartSpawning();
            }
            if (w.ucanSpawner && w.ucanAcik)
            {
                w.ucanSpawner.StartSpawning();
            }

            if (w.bossVar && w.bossPrefab)
            {
                Instantiate(w.bossPrefab, w.bossSpawn, Quaternion.identity);
            }

            float t = w.sure;
            while (t > 0f && demoGeri > 0f && !durdu)
            {
                t -= Time.deltaTime;
                demoGeri -= Time.deltaTime;

                if (sayacMetni)
                {
                    int s = Mathf.CeilToInt(t);
                    sayacMetni.text = "Kalan: " + FormatSure(s);
                }

                yield return null;
            }

            if (w.yuruyenSpawner) w.yuruyenSpawner.StopSpawning();
            if (w.ucanSpawner) w.ucanSpawner.StopSpawning();

            if (herDalgaArasiShop && i < dalgalar.Length - 1 && shopUI != null && !durdu && demoGeri > 0f)
            {
                shopUI.Open();
                while (shopUI.IsOpen() && !durdu)
                    yield return null;
            }

            if (demoGeri <= 0f || durdu) break;
        }

        Bitis();
    }

    void KediOldu()
    {
        if (durdu) return;
        durdu = true;

        // EKLENDI: ölümde muzik davranisi
        if (stopMusicOnDeath) StopMusicImmediate();

        // Ölümde direkt oyun bitti panelini aç
        if (akis != null) StopCoroutine(akis);

        if (oyunBittiPaneli != null)
        {
            oyunBittiPaneli.Show();
            return;
        }

        // Emniyet kemeri
        Bitis();
    }

    void Bitis()
    {
        if (akis != null) StopCoroutine(akis);

        // Skor hesapla
        int kalanKilcik = 0;
        if (SkorYoneticisi.Instance != null)
            kalanKilcik = SkorYoneticisi.Instance.SkorSayisi;

        int maxCan = 0;
        int sonCan = 0;
        if (kedi != null)
        {
            maxCan = kedi.maxCan;
            sonCan = kedi ? kedi.can : 0;
        }

        // EKLENDI: Skor muzigi
        PlayScoreMusic();

        // Skor paneli varsa onu göster, yoksa eski davranýþ
        if (skorPaneli != null)
        {
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (sayacMetni) sayacMetni.text = "Kalan: 00:00";
            skorPaneli.gameObject.SetActive(true);
            skorPaneli.GosterSonuc(kalanKilcik, maxCan, sonCan);
            return;
        }

        // Eski davranýþ
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (sayacMetni) sayacMetni.text = "Kalan: 00:00";
        if (bitisPaneli)
        {
            bitisPaneli.SetActive(true);
            var panel = bitisPaneli.GetComponent<OyunSonuSkorPaneli>();
            if (panel != null)
                panel.GosterSonuc(kalanKilcik, maxCan, sonCan);
        }
    }

    string FormatSure(int saniye)
    {
        int m = saniye / 60;
        int s = saniye % 60;
        return string.Format("{0:00}:{1:00}", m, s);
    }

    // =======================
    // EKLENDI: Muzik Yardimcilar
    // =======================
    void PlayWaveMusic(int waveIndex)
    {
        if (!musicSource) return;

        AudioClip hedef = null;
        if (waveLoops != null && waveIndex >= 0 && waveIndex < waveLoops.Length)
            hedef = waveLoops[waveIndex];

        if (hedef == null) return; // Bu dalga için müzik yoksa dokunma

        // Ayný klip zaten çalýyorsa sadece hedef volume’e fade et
        if (musicSource.clip == hedef && musicSource.isPlaying)
        {
            StartMusicFade(musicSource.volume, musicVolume);
            return;
        }

        // Yeni klibe fade
        StartMusicChange(hedef, musicVolume);
    }

    void PlayScoreMusic()
    {
        if (!musicSource || skorLoop == null) return;
        StartMusicChange(skorLoop, musicVolume);
    }

    void StopMusicImmediate()
    {
        if (!musicSource) return;
        if (musicCR != null) StopCoroutine(musicCR);
        musicSource.Stop();
        musicSource.clip = null;
        musicSource.volume = 0f;
    }

    void StartMusicChange(AudioClip yeniKlip, float hedefVol)
    {
        if (musicCR != null) StopCoroutine(musicCR);
        musicCR = StartCoroutine(FadeToClip(musicSource, yeniKlip, hedefVol, fadeTime));
    }

    void StartMusicFade(float from, float to)
    {
        if (musicCR != null) StopCoroutine(musicCR);
        musicCR = StartCoroutine(FadeVolume(musicSource, from, to, fadeTime));
    }

    IEnumerator FadeToClip(AudioSource src, AudioClip clip, float targetVol, float time)
    {
        if (!src) yield break;

        float t = 0f;
        float startVol = src.volume;

        // Fade out eski
        while (t < time)
        {
            t += Time.unscaledDeltaTime;  // TimeScale 0 olsa da çalýþsýn
            float k = Mathf.Clamp01(t / time);
            src.volume = Mathf.Lerp(startVol, 0f, k);
            yield return null;
        }

        src.Stop();
        src.clip = clip;
        src.loop = true;
        src.Play();

        // Fade in yeni
        t = 0f;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / time);
            src.volume = Mathf.Lerp(0f, targetVol, k);
            yield return null;
        }
        src.volume = targetVol;
        musicCR = null;
    }

    IEnumerator FadeVolume(AudioSource src, float from, float to, float time)
    {
        if (!src) yield break;
        float t = 0f;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / time);
            src.volume = Mathf.Lerp(from, to, k);
            yield return null;
        }
        src.volume = to;
        musicCR = null;
    }
}

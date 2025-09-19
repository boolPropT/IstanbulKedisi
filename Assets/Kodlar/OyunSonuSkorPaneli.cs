using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class OyunSonuSkorPaneli : MonoBehaviour
{
    [Header("UI Refs")]
    public TMP_Text baslikText;           // opsiyonel "Oyun Bitti" vs.
    public TMP_Text kilcikSatir;          // "Toplanan kilcik: X  =>  X * 100 = Y"
    public TMP_Text canKaybiSatir;        // "Kaybedilen can: Z   =>  Z * 50 = W"
    public TMP_Text toplamSatir;          // "Toplam Skor: T"
    public Button anaMenuButton;

    [Header("Puan Katsayilari")]
    public int puanKilcik = 100;
    public int cezaCanKaybi = 50;

    [Header("Ana Menu")]
    public string anaMenuSahneAdi = "MainMenu"; // sende neyse onu yaz
    public bool kullanSahneAdi = true;          // false ise build index 0 yuklenir

    void Awake()
    {
        gameObject.SetActive(false);
        if (anaMenuButton != null) anaMenuButton.onClick.AddListener(AnaMenuyeDon);
    }

    public void GosterSonuc(int kalanKilcik, int kediMaxCan, int kediSonCan)
    {
        int kaybedilen = Mathf.Clamp(kediMaxCan - Mathf.Max(0, kediSonCan), 0, kediMaxCan);
        int puanKilcikToplam = kalanKilcik * puanKilcik;   // kalan bakiyeye odul
        int cezaToplam = kaybedilen * cezaCanKaybi;
        int final = puanKilcikToplam - cezaToplam;

        if (kilcikSatir)
            kilcikSatir.text = string.Format("Kýlçýk: {2}", kalanKilcik, puanKilcik, puanKilcikToplam);

        if (canKaybiSatir)
            canKaybiSatir.text = string.Format("Hasar: -{2}", kaybedilen, cezaCanKaybi, cezaToplam);

        if (toplamSatir)
            toplamSatir.text = string.Format("Toplam Skor: {0}", final);

        gameObject.SetActive(true);
    }

    void AnaMenuyeDon()
    {
        Time.timeScale = 1f; // olur da donmus kaldiysan
        if (kullanSahneAdi && !string.IsNullOrEmpty(anaMenuSahneAdi))
            SceneManager.LoadScene(anaMenuSahneAdi);
        else
            SceneManager.LoadScene(0);
    }
}

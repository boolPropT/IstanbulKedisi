using UnityEngine;
using TMPro;

public class SkorYoneticisi : MonoBehaviour
{
    public static SkorYoneticisi Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI skorText;
    public string format = "{0}";

    [Header("Ses")]
    public AudioClip sfxKilcikTopla;     // short "puff" sound
    public float sfxSes = 0.8f;
    AudioSource asrc;

    int skor;                      // harcanabilen bakiye
    int toplamToplananKilcik;      // final icin kumulatif sayac

    void Awake()
    {
        asrc = GetComponent<AudioSource>();
        if (!asrc) asrc = gameObject.AddComponent<AudioSource>();
        asrc.playOnAwake = false;
        asrc.spatialBlend = 0f;

        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        GuncelleUI();
    }

    public void Ekle(int miktar)
    {
        skor += miktar;
        toplamToplananKilcik += miktar;   // <<< EK
        GuncelleUI();
        PlaySFX();
    }

    // Disariya yalnizca okuma
    public int SkorSayisi { get { return skor; } }
    public int ToplamToplananKilcik { get { return toplamToplananKilcik; } }  // <<< EK

    // Shop icin harcama
    public bool Harca(int miktar)
    {
        if (miktar <= 0) return true;
        if (skor < miktar) return false;
        skor -= miktar;
        GuncelleUI();
        return true;
    }

    void GuncelleUI()
    {
        if (skorText) skorText.text = string.Format(format, skor);
    }

    void PlaySFX()
    {
        if (sfxKilcikTopla && asrc)
            asrc.PlayOneShot(sfxKilcikTopla, sfxSes);
    }
}

// KedininCani.cs
using System;
using UnityEngine;

public class KedininCani : MonoBehaviour
{
    [Header("Can")]
    public int can = 10;
    public int maxCan = 10;                //  EKLE: üst sýnýr

    [Header("Dokunulmazlýk")]
    public float dokunulmazlikSuresi = 0.5f;
    float tekrarVurulabilirZaman;

    [Header("Ses")]
    public AudioClip sfxHasarAl;     // short "puff" sound
    public float sfxSes = 0.8f;
    AudioSource asrc;

    void Awake()
    {
        asrc = GetComponent<AudioSource>();
        if (!asrc) asrc = gameObject.AddComponent<AudioSource>();
        asrc.playOnAwake = false;
        asrc.spatialBlend = 0f;

        if (maxCan < can) maxCan = can;    // EK: sahnede can > max ise senkron
    }

    public bool Vurulabilir()
    {
        return Time.time >= tekrarVurulabilirZaman;
    }

    public void HasarAl(int miktar)
    {
        if (!Vurulabilir()) return;

        can -= miktar;
        PlaySFX();
        tekrarVurulabilirZaman = Time.time + dokunulmazlikSuresi;

        Debug.Log($"Kedinin caný: {can}");

        if (can <= 0)
        {
            OyunDurumu.KediOlumunuIsaretle();
            Destroy(gameObject);
            FindObjectOfType<OyunBittiPaneli>()?.Show();
        }
    }

    // EKLE: Shop buradan iyileþtirsin
    public void Iyilestir(int miktar)
    {
        if (miktar <= 0) return;
        can = Mathf.Min(maxCan, can + miktar);
        Debug.Log($"[KedininCani] Ýyileþti (+{miktar}). Yeni can: {can}/{maxCan}");
    }

    void PlaySFX()
    {
        if (sfxHasarAl && asrc)
            asrc.PlayOneShot(sfxHasarAl, sfxSes);
    }
}

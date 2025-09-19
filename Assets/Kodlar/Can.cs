using UnityEngine;
using System;

public class Can : MonoBehaviour
{
    [Header("Ses")]
    public AudioClip sfxCanAl;     // short "puff" sound
    public float sfxSes = 0.8f;
    AudioSource asrc;

    public int canPuani = 3;

    public Action<Can> Oldu;

    void Awake()
    {
        asrc = GetComponent<AudioSource>();
        if (!asrc) asrc = gameObject.AddComponent<AudioSource>();
        asrc.playOnAwake = false;
        asrc.spatialBlend = 0f;
    }

    public void HasarAl(int miktar)
    {
        canPuani -= miktar; //can azalýyor...
        PlaySFX();
        if (canPuani <= 0) 
        {
            Oldu?.Invoke(this);
            GetComponent<MartiUcuslaYokOl>()?.UcuslaAyril();
        }
    }

    void PlaySFX()
    {
        if (sfxCanAl && asrc)
            asrc.PlayOneShot(sfxCanAl, sfxSes);
    }
}
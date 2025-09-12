using System;
using UnityEngine;

public class KedininCani : MonoBehaviour
{
    [Header("Can")]
    public int can = 10;

    [Header("Dokunulmazl�k")]
    //Vurulduktan sonra bu kadar s�re tekrar vurulamaz.
    //�ok kolay gelirse dokunulmazlikSuresini 0.35�0.4�a indir,  
    //�ok zor gelirse dokunulmazlikSuresini 0.6�0.8�e ��kar.
    public float dokunulmazlikSuresi = 0.5f;
    float tekrarVurulabilirZaman;

    public bool Vurulabilir()
    {
        return Time.time >= tekrarVurulabilirZaman;
    }

    public void HasarAl(int miktar)
    {
        if (!Vurulabilir()) return;

        can -= miktar;
        tekrarVurulabilirZaman = Time.time + dokunulmazlikSuresi;
        
        Debug.Log($"Kedinin can�: {can}");

        if (can <= 0)
        {
            //�l�m
            OyunDurumu.KediOlumunuIsaretle();
            Destroy(gameObject);
            FindObjectOfType<OyunBittiPaneli>()?.Show();
        }
    }


}

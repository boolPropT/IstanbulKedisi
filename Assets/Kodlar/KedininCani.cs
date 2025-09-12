using System;
using UnityEngine;

public class KedininCani : MonoBehaviour
{
    [Header("Can")]
    public int can = 10;

    [Header("Dokunulmazlýk")]
    //Vurulduktan sonra bu kadar süre tekrar vurulamaz.
    //Çok kolay gelirse dokunulmazlikSuresini 0.35–0.4’a indir,  
    //Çok zor gelirse dokunulmazlikSuresini 0.6–0.8’e çýkar.
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
        
        Debug.Log($"Kedinin caný: {can}");

        if (can <= 0)
        {
            //Ölüm
            OyunDurumu.KediOlumunuIsaretle();
            Destroy(gameObject);
            FindObjectOfType<OyunBittiPaneli>()?.Show();
        }
    }


}

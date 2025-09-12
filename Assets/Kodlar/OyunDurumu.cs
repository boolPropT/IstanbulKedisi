using UnityEngine;

public static class OyunDurumu
{
    public static bool kediHayatta = true;
    public static System.Action KediOldu;

    public static void KediOlumunuIsaretle()
    {
        if (!kediHayatta) return;
        kediHayatta = false;
        KediOldu?.Invoke();
    }

    //her yeni oyun/Retry için temiz baþlangýç
    public static void Sifirla()
    {
        kediHayatta = true;
        KediOldu = null; // eski dinleyicileri temizle
    }
}

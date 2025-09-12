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

    //her yeni oyun/Retry i�in temiz ba�lang��
    public static void Sifirla()
    {
        kediHayatta = true;
        KediOldu = null; // eski dinleyicileri temizle
    }
}

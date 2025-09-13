using UnityEngine;

public class KilcikYoneticisi : MonoBehaviour
{
    public static KilcikYoneticisi I;
    public int toplamKilcik;

    void Awake()
    {
        if (I == null) I = this; else Destroy(gameObject);
    }

    public void KilcikTopla(int adet = 1)
    {
        toplamKilcik += Mathf.Max(1, adet);
    }

    public bool Harca(int adet)
    {
        if (toplamKilcik < adet) return false;
        toplamKilcik -= adet;
        return true;
    }
}

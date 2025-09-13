using UnityEngine;

public class SimitBombasi : MonoBehaviour
{
    [Header("Kullan�m")]
    public KeyCode tus = KeyCode.Space;   // Patlatma tu�u
    [Tooltip("Shop�tan al�nan tek kullan�ml�k �arj say�s�.")]
    public int superCharges = 0;          // Shop bunu +1 yapar

    [Header("Etkisi")]
    public float yaricap = 7.5f;
    public int hasar = 50;
    public LayerMask dusmanKatmani;       // Mart� layer�lar�n� se�
    public bool ucanlariUcur = true;      // U�anlara �ka� tetiklemesi

    [Header("VFX (opsiyonel)")]
    public GameObject vfx;

    void Update()
    {
        if (superCharges > 0 && Input.GetKeyDown(tus))
        {
            superCharges--;
            Patlat();
        }
    }

    void Patlat()
    {
        if (vfx) Instantiate(vfx, transform.position, Quaternion.identity);

        var hits = Physics2D.OverlapCircleAll(transform.position, yaricap, dusmanKatmani);
        foreach (var h in hits)
        {
            // Can�� varsa hasar uygula
            var can = h.GetComponentInParent<Can>();
            if (can != null) can.canPuani = Mathf.Max(0, can.canPuani - hasar);

            // U�an ge�i� mart�lar� g�rsel olarak da��ls�n istiyorsan
            if (ucanlariUcur)
            {
                var kac = h.GetComponentInParent<MartiUcuslaYokOl>();
                if (kac != null) kac.UcuslaAyril();
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, yaricap);
    }
}

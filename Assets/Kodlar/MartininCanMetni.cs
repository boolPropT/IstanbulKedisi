using UnityEngine;
using TMPro;

public class MartininCanMetni : MonoBehaviour
{
    public Can can;             // Martýnýn can kodu.
    public TextMeshPro text;    // Martýnýn childdaki TMP text.
    public Vector3 offset = new Vector3(0f, 0.9f, 0f);

    int baslangicCan;

    // Havuzdan/respawn dönüþünde yanlýþlýkla kapalý kaldýysa tekrar aç
    private void OnEnable()
    {
        if (text != null) text.gameObject.SetActive(true);
    }

    private void Start()
    {
        if (!can) can = GetComponent<Can>();
        if (!text) text = GetComponentInChildren<TextMeshPro>();
        if (can) baslangicCan = can.canPuani;
    }

    private void LateUpdate()
    {
        if (!can || !text) return;

        // Konumu kafasýnýn üstüne sabitle
        text.transform.position = transform.position + offset;

        // Can puanýný yaz
        text.text = $"[ {can.canPuani} / {baslangicCan} ]";

        // Sýfýrýn altýnda yazýyý gizle
        if (can.canPuani <= 0)
            text.gameObject.SetActive(false);
    }
}
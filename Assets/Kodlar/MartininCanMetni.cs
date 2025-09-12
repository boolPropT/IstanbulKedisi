using UnityEngine;
using TMPro;

public class MartininCanMetni : MonoBehaviour
{
    public Can can;             // Mart�n�n can kodu.
    public TextMeshPro text;    // Mart�n�n childdaki TMP text.
    public Vector3 offset = new Vector3(0f, 0.9f, 0f);

    int baslangicCan;

    // Havuzdan/respawn d�n���nde yanl��l�kla kapal� kald�ysa tekrar a�
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

        // Konumu kafas�n�n �st�ne sabitle
        text.transform.position = transform.position + offset;

        // Can puan�n� yaz
        text.text = $"[ {can.canPuani} / {baslangicCan} ]";

        // S�f�r�n alt�nda yaz�y� gizle
        if (can.canPuani <= 0)
            text.gameObject.SetActive(false);
    }
}
using UnityEngine;
using TMPro;

public class SkorYoneticisi : MonoBehaviour
{
    public static SkorYoneticisi Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI skorText;   // Canvas icindeki TMP-Text (UGUI)
    public string format = "{0}";

    int skor;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        GuncelleUI();
    }

    public void Ekle(int miktar)
    {
        skor += miktar;
        GuncelleUI();
    }

    void GuncelleUI()
    {
        if (skorText) skorText.text = string.Format(format, skor);
    }
}
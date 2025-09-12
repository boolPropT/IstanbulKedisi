using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
    public KedininCani kediCan;
    public TextMeshProUGUI kediText;

    void Update()
    {
        if (!kediCan || !kediText) return;
        kediText.text = $"{kediCan.can}";
    }
}

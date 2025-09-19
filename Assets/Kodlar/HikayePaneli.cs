using UnityEngine;
using UnityEngine.SceneManagement;



public class HikayePaneli : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private string oyunSahnesiIsmi = "Oyun";

    public void HikayedenOyunaGiris()
    {
        SceneManager.LoadScene(oyunSahnesiIsmi);
    }
}

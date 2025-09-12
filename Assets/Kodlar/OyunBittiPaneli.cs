using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OyunBittiPaneli : MonoBehaviour
{
    [Header("References")]
    public GameObject gameOverPanel;   // Karartma + butonlar�n k�k�
    public Button retryButton;
    public Button quitButton;

    bool shown;

    void Awake()
    {
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (retryButton) retryButton.onClick.AddListener(OnRetry);
        if (quitButton) quitButton.onClick.AddListener(OnQuit);
    }

    public void Show()
    {
        if (shown) return;
        shown = true;

        if (gameOverPanel) gameOverPanel.SetActive(true);

        // Oyunu durdur, imleci g�r�n�r yap
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void OnRetry()
    {
        // Zaman� normale al, sahneyi yeniden y�kle
        Time.timeScale = 1f;
        OyunDurumu.Sifirla(); //kritik
        var idx = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        UnityEngine.SceneManagement.SceneManager.LoadScene(idx);
    }

    void OnQuit()
    {
        Time.timeScale = 1f;
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

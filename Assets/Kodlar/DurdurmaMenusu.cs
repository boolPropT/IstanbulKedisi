using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class DurdurmaMenusu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject optionsPanel;

    [Header("UI Focus")]
    [SerializeField] private Button firstPauseFocus;     // ResumeButton
    [SerializeField] private Button firstOptionsFocus;   // BackButton veya ilk slider

    [Header("Audio (Optional)")]
    [SerializeField] private Slider masterVolumeSlider;  // 0..1
    private const string PrefsMasterVol = "master_volume";

    [Header("Quit/Ana Menü")]
    [SerializeField] private bool quitToMainMenu = false;         // true ise sahne yükler
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Misc")]
    [SerializeField] private bool pauseOnFocusLoss = true;        // Alt+Tab olunca auto-pause

    private bool isPaused;
    private EventSystem es;

    private void Awake()
    {
        es = EventSystem.current;
        if (pausePanel) pausePanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(false);

        // Ses tercihini yükle
        float vol = PlayerPrefs.GetFloat(PrefsMasterVol, 1f);
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = vol;
            masterVolumeSlider.onValueChanged.AddListener(v =>
            {
                AudioListener.volume = Mathf.Clamp01(v);
                PlayerPrefs.SetFloat(PrefsMasterVol, v);
            });
            AudioListener.volume = vol;
        }
    }

    private void Update()
    {
        // Eski Input System (basit ve yeterli)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused) Pause();
            else
            {
                // Options açýksa önce options'tan çýk, deðilse resume et
                if (optionsPanel != null && optionsPanel.activeSelf) CloseOptions();
                else Resume();
            }
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (pauseOnFocusLoss && !hasFocus && !isPaused)
            Pause();
    }

    // --- Public UI Events ---
    public void OnResumePressed() { Resume(); }
    public void OnOptionsPressed() { OpenOptions(); }
    public void OnBackFromOptions() { CloseOptions(); }
    public void OnQuitPressed()
    {
        if (quitToMainMenu && !string.IsNullOrEmpty(mainMenuSceneName))
        {
            // TimeScale'i geri alýp ana menüye dön
            Time.timeScale = 1f;
            AudioListener.pause = false;
            SceneManager.LoadScene(mainMenuSceneName);
            return;
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // --- Core ---
    private void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;          // oyunu dondur
        AudioListener.pause = true;   // tüm AudioSource'larý duraklat
        if (pausePanel) pausePanel.SetActive(true);
        if (optionsPanel) optionsPanel.SetActive(false);

        ShowCursor(true);

        // Klavye/gamepad odak
        if (firstPauseFocus != null) Select(firstPauseFocus.gameObject);
    }

    private void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (pausePanel) pausePanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(false);

        ShowCursor(false);
        ClearSelection();
    }

    private void OpenOptions()
    {
        if (optionsPanel) optionsPanel.SetActive(true);
        // Options açýlýnca odak
        if (firstOptionsFocus != null) Select(firstOptionsFocus.gameObject);
    }

    private void CloseOptions()
    {
        if (optionsPanel) optionsPanel.SetActive(false);
        if (firstPauseFocus != null) Select(firstPauseFocus.gameObject);
    }

    // --- Helpers ---
    private void ShowCursor(bool show)
    {
        Cursor.visible = show;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Select(GameObject go)
    {
        if (es == null) es = EventSystem.current;
        if (es == null) return;
        es.SetSelectedGameObject(null);
        es.SetSelectedGameObject(go);
    }

    private void ClearSelection()
    {
        if (es == null) es = EventSystem.current;
        if (es == null) return;
        es.SetSelectedGameObject(null);
    }
}

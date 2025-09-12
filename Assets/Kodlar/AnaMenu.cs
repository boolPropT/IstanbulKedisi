using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class AnaMenu : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string gameplaySceneName = "Game"; // Oyun sahnenin adý

    [Header("Panels")]
    [SerializeField] private GameObject optionsPanel;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource uiClickSfx;
    [SerializeField] private Slider masterVolumeSlider; // 0..1
    private const string PrefsMasterVol = "master_volume";

    private void Start()
    {
        // Options panel kapalý baþlasýn
        if (optionsPanel != null) optionsPanel.SetActive(false);

        // Volume yükle
        float vol = PlayerPrefs.GetFloat(PrefsMasterVol, 1f);
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = vol;
            ApplyMasterVolume(vol);
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
    }

    public void OnPlayPressed()
    {
        Click();
        // Ýstersen fade/async loader ekleyebilirsin (aþaðýda örnek var)
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OnOptionsPressed()
    {
        Click();
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void OnBackFromOptions()
    {
        Click();
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    public void OnQuitPressed()
    {
        Click();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void Click()
    {
        if (uiClickSfx != null) uiClickSfx.Play();
    }

    private void OnMasterVolumeChanged(float v)
    {
        ApplyMasterVolume(v);
        PlayerPrefs.SetFloat(PrefsMasterVol, v);
    }

    private void ApplyMasterVolume(float v)
    {
        AudioListener.volume = Mathf.Clamp01(v);
    }
}


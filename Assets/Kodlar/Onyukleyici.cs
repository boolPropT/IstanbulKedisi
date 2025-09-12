using UnityEngine;

[DefaultExecutionOrder(-10000)]

public class Onyukleyici : MonoBehaviour
{
    [SerializeField] int hedefFps = 60;
    [SerializeField] bool vsyncKapat = true;

    void Awake()
    {
        if (vsyncKapat)
            QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = hedefFps;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            QualitySettings.vSyncCount = (QualitySettings.vSyncCount == 0) ? 1 : 0;
            Debug.Log($"VSync: {(QualitySettings.vSyncCount == 0 ? "KAPALI" : "AÇIK")}");
        }
    }
}

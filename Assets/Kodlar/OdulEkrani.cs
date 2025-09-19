using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OdulEkrani : MonoBehaviour
{
    [Header("Refs")]
    public GameObject rootPanel;
    public TMP_Text kilcikText;
    public Button btnHeal5;
    public Button btnBigBomb;
    public Button btnContinue;

    [Header("Economy")]
    public int costHeal5 = 10;
    public int costBigBomb = 20;

    [Header("Targets")]
    public KedininCani kediCan;     // kedinin can scripti
    public SimitBombasi simit;      // tek kullan�ml�k bomba

    [Header("Health Cap")]
    public int kediMaxCanGuess = 0;

    bool open;
    float prevTimeScale = 1f;
    int kediMaxCanCache = 0;

    SkorYoneticisi skor;            // <<< do�rudan referans

    void Awake()
    {
        if (!rootPanel) rootPanel = gameObject;
        rootPanel.SetActive(false);

        if (btnHeal5) btnHeal5.onClick.AddListener(BuyHeal5);
        if (btnBigBomb) btnBigBomb.onClick.AddListener(BuyBigBomb);
        if (btnContinue) btnContinue.onClick.AddListener(ContinueGame);
    }

    void OnEnable()
    {
        // Oto-ba�la: Inspector bo�sa sahneden bul
        if (!kediCan) kediCan = FindObjectOfType<KedininCani>(true);
        skor = SkorYoneticisi.Instance ?? FindObjectOfType<SkorYoneticisi>(true);
        Refresh();
    }

    // +5 Can sat�n al�nca G�ZLE g�r�l�r teyit i�in k���k bir flash tetikleyelim
    IEnumerator CanFlash()
    {
        // Bar/metin sistemini bilmiyorum; ama kedinin �st�nde k�sa bir �+5� popup istersen burada �retirsin.
        // �imdilik sadece bir log:
        Debug.Log("[Shop] +5 Can uyguland�. Yeni can: " + kediCan.can);
        yield break;
    }

    void Update()
    {
        if (!open) return;

        // Eski input k�sayollar�
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            ContinueGame();
        if (Input.GetKeyDown(KeyCode.Alpha1)) BuyHeal5();
        if (Input.GetKeyDown(KeyCode.Alpha2)) BuyBigBomb();

        if (kilcikText) kilcikText.text = $"K�l��k: {GetKilcik()}";
    }

    public void Open()
    {
        if (open) return;
        open = true;

        if (kediCan != null && kediMaxCanCache == 0)
            kediMaxCanCache = (kediMaxCanGuess > 0) ? kediMaxCanGuess : Mathf.Max(kediCan.can, 1);

        EnsureUsableCanvas();
        Refresh();
        rootPanel.SetActive(true);

        prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Varsay�lan se�ili buton
        if (btnHeal5 && btnHeal5.interactable) btnHeal5.Select();
        else if (btnBigBomb && btnBigBomb.interactable) btnBigBomb.Select();
        else if (btnContinue) btnContinue.Select();
    }

    public void Close()
    {
        if (!open) return;
        open = false;
        rootPanel.SetActive(false);
        Time.timeScale = prevTimeScale;
    }

    public bool IsOpen() => open;

    void Refresh()
    {
        int elde = GetKilcik();
        if (kilcikText) kilcikText.text = $"K�l��k: {elde}";

        bool kediYasiyor = kediCan != null && kediCan.can > 0;
        if (btnHeal5) btnHeal5.interactable = kediYasiyor && (elde >= costHeal5);
        if (btnBigBomb) btnBigBomb.interactable = (elde >= costBigBomb) && (simit != null);
    }

    int GetKilcik() => skor ? skor.SkorSayisi : 0;

    bool TrySpend(int miktar) => skor && skor.Harca(miktar);

    void BuyHeal5()
    {
        if (!open || kediCan == null || kediCan.can <= 0) return;
        if (!TrySpend(costHeal5)) return;

        kediCan.Iyilestir(5);    // direkt alan set etmeyi b�rak
        Refresh();
    }

    IEnumerator DelayWriteCan(int hedef)
    {
        yield return null; // bir frame bekle
        if (kediCan) kediCan.can = hedef;
    }

    void BuyBigBomb()
    {
        if (!open || simit == null) return;
        if (!TrySpend(costBigBomb)) return;

        simit.superCharges += 1; // tek kullan�ml�k
        Refresh();
    }

    void ContinueGame() => Close();

    // Sadece g�r�n�r/t�klan�r hale getir; EventSystem�a dokunma
    void EnsureUsableCanvas()
    {
        var t = rootPanel.transform;
        while (t != null)
        {
            if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
            t = t.parent;
        }

        var cv = rootPanel.GetComponentInParent<Canvas>(true);
        if (cv != null)
        {
            cv.enabled = true;
            if (cv.renderMode == RenderMode.ScreenSpaceCamera && cv.worldCamera == null)
                cv.worldCamera = Camera.main;
        }

        var cg = rootPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = rootPanel.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.blocksRaycasts = true;
        cg.interactable = true;
    }
}

// OdulEkrani.cs (v3 - SkorYoneticisi entegrasyonu + sade UI)
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Reflection;

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
    public KedininCani kediCan;
    public SimitBombasi simit;

    [Header("Health Cap")]
    public int kediMaxCanGuess = 0;

    bool open;
    float prevTimeScale = 1f;
    int kediMaxCanCache = 0;

    // SkorYoneticisi yumu�ak ba� alanlar�
    object skorSingle; Type skorType;
    MemberInfo kilcikMember;     // alan ya da property
    MethodInfo spendMethod;      // Harca/Spend t�revleri

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
        BindSkorYoneticisi();
        Refresh();
    }

    void Update()
    {
        if (!open) return;

        // K�sayollar (UI bozulsa bile ��k/sat�n al)
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            ContinueGame();
        if (Input.GetKeyDown(KeyCode.Alpha1)) BuyHeal5();
        if (Input.GetKeyDown(KeyCode.Alpha2)) BuyBigBomb();

        if (kilcikText) kilcikText.text = $"K�l��k: {GetKilcik()}";
    }

    // --- Open(): Yeni EventSystem ekleme yok, sadece paneli g�r�n�r/etkile�imli yap ---
    public void Open()
    {
        if (open) return;
        open = true;

        if (kediCan != null && kediMaxCanCache == 0)
            kediMaxCanCache = (kediMaxCanGuess > 0) ? kediMaxCanGuess : Mathf.Max(kediCan.can, 1);

        EnsureUsableCanvas();   // sadece Canvas/CanvasGroup ayar�, EventSystem�a dokunmuyor
        Refresh();
        rootPanel.SetActive(true);

        prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;    // oyunu dondur
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

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

    // ---------- UI / Economy ----------
    void Refresh()
    {
        int elde = GetKilcik();
        if (kilcikText) kilcikText.text = $"K�l��k: {elde}";

        bool kediYasiyor = kediCan != null && kediCan.can > 0;
        if (btnHeal5) btnHeal5.interactable = kediYasiyor && (elde >= costHeal5);
        if (btnBigBomb) btnBigBomb.interactable = (elde >= costBigBomb) && (simit != null);
    }

    void BuyHeal5()
    {
        if (!open || kediCan == null || kediCan.can <= 0) return;
        if (!TrySpend(costHeal5)) return;

        int max = (kediMaxCanCache > 0) ? kediMaxCanCache : int.MaxValue;
        kediCan.can = Mathf.Min(max, kediCan.can + 5);
        Refresh();
    }

    void BuyBigBomb()
    {
        if (!open || simit == null) return;
        if (!TrySpend(costBigBomb)) return;

        simit.superCharges += 1;
        Refresh();
    }

    void ContinueGame() => Close();

    // ---------- SkorYoneticisi entegrasyonu ----------
    void BindSkorYoneticisi()
    {
        skorSingle = null; skorType = null; kilcikMember = null; spendMethod = null;

        // T�r� bul
        skorType = Type.GetType("SkorYoneticisi") ?? Type.GetType("SkorYonetici");
        if (skorType == null)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                skorType = asm.GetType("SkorYoneticisi") ?? asm.GetType("SkorYonetici");
                if (skorType != null) break;
            }
        }
        if (skorType == null) return;

        // Singleton/instance
        var pI = skorType.GetProperty("I", BindingFlags.Public | BindingFlags.Static);
        var pInstance = skorType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
        skorSingle = pI?.GetValue(null) ?? pInstance?.GetValue(null);
        if (skorSingle == null)
        {
            // Sahnedeki �rne�i bul
            var find = typeof(UnityEngine.Object).GetMethod("FindObjectOfType", 1, Type.EmptyTypes);
            var gen = find.MakeGenericMethod(skorType);
            skorSingle = gen.Invoke(null, null);
        }
        if (skorSingle == null) return;

        // K�l��k alan/�zellik adaylar�
        string[] kilcikNames = { "kilcik", "kilcikSayisi", "kilcikToplam", "toplamKilcik", "skor", "score", "toplamSkor" };
        foreach (var name in kilcikNames)
        {
            var pi = skorType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (pi != null && (pi.PropertyType == typeof(int))) { kilcikMember = pi; break; }
            var fi = skorType.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (fi != null && (fi.FieldType == typeof(int))) { kilcikMember = fi; break; }
        }

        // Harcama metodu adaylar�
        string[] spendNames = { "HarcaKilcik", "Harca", "Spend", "SpendKilcik" };
        foreach (var name in spendNames)
        {
            var mi = skorType.GetMethod(name, BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(int) }, null);
            if (mi != null) { spendMethod = mi; break; }
        }
    }

    int GetKilcik()
    {
        if (skorSingle == null || kilcikMember == null) return 0;

        try
        {
            if (kilcikMember is PropertyInfo pi) return (int)pi.GetValue(skorSingle);
            if (kilcikMember is FieldInfo fi) return (int)fi.GetValue(skorSingle);
        }
        catch { }
        return 0;
    }

    bool TrySpend(int miktar)
    {
        if (skorSingle == null) return false;

        // Tercihen metot �a��r
        if (spendMethod != null)
        {
            try
            {
                var ok = spendMethod.Invoke(skorSingle, new object[] { miktar });
                if (ok is bool b) return b;
            }
            catch { }
        }

        // Fallback: sayac� d���r (yaz�labilir alan/propery ise)
        int mevcut = GetKilcik();
        if (mevcut < miktar) return false;

        try
        {
            if (kilcikMember is PropertyInfo pi && pi.CanWrite)
            {
                pi.SetValue(skorSingle, mevcut - miktar);
                return true;
            }
            if (kilcikMember is FieldInfo fi && !fi.IsInitOnly)
            {
                fi.SetValue(skorSingle, mevcut - miktar);
                return true;
            }
        }
        catch { }

        return false;
    }

    // ---------- UI yard�mc�lar� (yumu�ak) ----------
    // --- EnsureUsableCanvas(): sadece g�r�n�rl�k ve raycast, input mod�l�ne KAR�AMAYIZ ---
    void EnsureUsableCanvas()
    {
        if (!rootPanel) rootPanel = gameObject;

        // 1) Parent zincirini a�
        var t = rootPanel.transform;
        while (t != null)
        {
            if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
            t = t.parent;
        }

        // 2) Canvas ayar� (Overlay �nerilir; Camera/WorldSpace ise kamera ata)
        var cv = rootPanel.GetComponentInParent<Canvas>(true);
        if (cv != null)
        {
            cv.enabled = true;
            if (cv.renderMode == RenderMode.ScreenSpaceCamera && cv.worldCamera == null)
                cv.worldCamera = Camera.main;
            // sortingOrder�� zorla 9999 yapm�yoruz; mevcut UI hiyerar�in bozulmas�n.
        }

        // 3) CanvasGroup ayar�: g�r�n�r ve t�klan�r olsun
        var cg = rootPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = rootPanel.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.blocksRaycasts = true;
        cg.interactable = true;

        // 4) Arka plan� engel yapma:
        // Shop arkas� karartma imaj�nda "Raycast Target" a��k kalabilir,
        // ama butonlar�n �zerinde ikinci, g�r�nmez bir imaj varsa Raycast Target'�n� kapat.
    }
}

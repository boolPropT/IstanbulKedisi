using System.Collections;
using UnityEngine;
using TMPro;

public class DalgaBanner : MonoBehaviour
{
    [Header("Refs")]
    public TMP_Text label;          // Label objesi
    public CanvasGroup group;       // CanvasGroup (alpha)
    public AudioSource sfx;         // Davul vuruþu

    [Header("Süreler")]
    public float fadeIn = 0.18f;
    public float hold = 1.0f;
    public float fadeOut = 0.5f;

    [Header("Animasyon")]
    public float scaleIn = 1.35f;    // ilk girerken biraz büyükten gelsin
    public float scaleOut = 0.98f;  // çýkarken hafif küçülme
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    Coroutine cr;

    void Reset()
    {
        group = GetComponent<CanvasGroup>();
        if (!sfx) sfx = GetComponent<AudioSource>();
        label = GetComponentInChildren<TMP_Text>();
    }

    public void Show(string text)
    {
        if (cr != null) StopCoroutine(cr);
        cr = StartCoroutine(ShowRoutine(text));
    }

    IEnumerator ShowRoutine(string text)
    {
        if (!label || !group) yield break;

        label.text = text;
        group.alpha = 0f;
        transform.localScale = Vector3.one * scaleIn;

        // ses
        if (sfx && sfx.clip) sfx.Play();

        // Fade In + scale to 1
        float t = 0f;
        while (t < fadeIn)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeIn);
            float e = ease.Evaluate(k);
            group.alpha = e;
            transform.localScale = Vector3.one * Mathf.Lerp(scaleIn, 1f, e);
            yield return null;
        }
        group.alpha = 1f;
        transform.localScale = Vector3.one;

        // Hold
        t = 0f;
        while (t < hold)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // Fade Out + slight scale
        t = 0f;
        while (t < fadeOut)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeOut);
            float e = ease.Evaluate(k);
            group.alpha = 1f - e;
            transform.localScale = Vector3.one * Mathf.Lerp(1f, scaleOut, e);
            yield return null;
        }
        group.alpha = 0f;
        transform.localScale = Vector3.one;

        cr = null;
    }
}

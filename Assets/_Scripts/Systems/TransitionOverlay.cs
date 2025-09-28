using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TransitionOverlay : MonoBehaviour
{
    public static TransitionOverlay Instance { get; private set; }
    [SerializeField] private Image fadeImage;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator FadeOut(float dur = 0.5f)
    {
        if (!fadeImage) yield break;
        fadeImage.gameObject.SetActive(true);

        float t = 0f;
        Color c = fadeImage.color;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / dur);
            fadeImage.color = c;
            yield return null;
        }
        c.a = 1f;
        fadeImage.color = c;
    }

    public IEnumerator FadeIn(float dur = 0.5f)
    {
        if (!fadeImage) yield break;
        float t = 0f;
        Color c = fadeImage.color;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / dur);
            fadeImage.color = c;
            yield return null;
        }
        c.a = 0f;
        fadeImage.color = c;
        fadeImage.gameObject.SetActive(false);
    }
}

using UnityEngine;
using UnityEngine.UI;

public class ScreenFadder : MonoBehaviour
{
    public static ScreenFadder Instance { get; private set; }

    [SerializeField] float defaultFadeOut = 0.4f;
    [SerializeField] float defaultFadeIn = 0.4f;

    CanvasGroup cg;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // build overlay
        var canvas = new GameObject("ScreenFaderCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas.transform.SetParent(transform, false);
        var c = canvas.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 10000;
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        var panel = new GameObject("Black", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        panel.transform.SetParent(canvas.transform, false);
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var img = panel.GetComponent<Image>(); img.color = Color.black;
        cg = panel.GetComponent<CanvasGroup>(); cg.alpha = 0f; cg.blocksRaycasts = false;
    }

    public System.Collections.IEnumerator FadeOut(float? dur = null) => FadeTo(1f, dur ?? defaultFadeOut);
    public System.Collections.IEnumerator FadeIn(float? dur = null) => FadeTo(0f, dur ?? defaultFadeIn);

    System.Collections.IEnumerator FadeTo(float target, float time)
    {
        float start = cg.alpha;
        if (Mathf.Approximately(time, 0f)) { cg.alpha = target; yield break; }
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / time;
            cg.alpha = Mathf.Lerp(start, target, t);
            yield return null;
        }
        cg.alpha = target;
    }
}

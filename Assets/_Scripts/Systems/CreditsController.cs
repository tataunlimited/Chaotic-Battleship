using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static SceneTypes;
using UnityEditor;
using static Unity.Burst.Intrinsics.X86.Avx;

public class CreditsController : MonoBehaviour
{
    [Header("Text source")]
    public TextAsset creditsFile;

    [Header("When to exit")]
    public float exitAfterSeconds = 180f;

    [Header("Scroll")]
    public float scrollSpeed = 70f;      
    public float startOffset = 20f;      
    public float endMargin = 40f;        
    public float endHoldSeconds = 0.0f;  

    [Header("Next scene")]
    public SceneType menuScene = SceneType.MainMenu;

    [Header("Auto-created if left empty")]
    public RectTransform viewport;
    public RectTransform content;

    RectTransform canvasRT;
    TextMeshProUGUI body;
    float textHeight;
    float distanceToTravel;
    bool initialized, finished;

    void Awake()
    {
        Time.timeScale = 1f;

        // canvas 
        var go = new GameObject("CreditsCanvas",
                                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasRT = go.transform as RectTransform;

        // viewport
        if (!viewport)
        {
            var vpGO = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            vpGO.transform.SetParent(canvasRT, false);
            viewport = vpGO.GetComponent<RectTransform>();
            viewport.anchorMin = new Vector2(0.1f, 0.1f);
            viewport.anchorMax = new Vector2(0.9f, 0.85f);
            viewport.offsetMin = viewport.offsetMax = Vector2.zero;
            vpGO.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        }

        // content
        if (!content)
        {
            var cGO = new GameObject("Content", typeof(RectTransform));
            cGO.transform.SetParent(viewport, false);
            content = cGO.GetComponent<RectTransform>();
            content.pivot = new Vector2(0.5f, 0f);
            content.anchorMin = new Vector2(0.5f, 0f);
            content.anchorMax = new Vector2(0.5f, 0f);
        }

        // text
        var bodyGO = new GameObject("CreditsText", typeof(TextMeshProUGUI));
        bodyGO.transform.SetParent(content, false);
        body = bodyGO.GetComponent<TextMeshProUGUI>();
        body.alignment = TextAlignmentOptions.Center;
        body.fontSize = 28;
        body.overflowMode = TextOverflowModes.Overflow;
        body.rectTransform.sizeDelta = new Vector2(900f, 0f);
        body.text = creditsFile ? creditsFile.text : "No credits file assigned.";
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(InitCredits());
        StartCoroutine(AutoExitAfterTimer());
    }

    System.Collections.IEnumerator InitCredits()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();

        body.ForceMeshUpdate(true, true);
        textHeight = Mathf.Max(body.preferredHeight, 1f);

        content.sizeDelta = new Vector2(0f, textHeight);

        float startY = -viewport.rect.height - startOffset;
        content.anchoredPosition = new Vector2(0f, startY);

        distanceToTravel = startOffset + viewport.rect.height + textHeight + endMargin;

        initialized = true;
    }
    System.Collections.IEnumerator AutoExitAfterTimer()
    {
        yield return new WaitForSecondsRealtime(exitAfterSeconds);
        if (!finished) StartCoroutine(ExitToMenu());
    }

    // Update is called once per frame
    void Update()
    {
        if (!initialized || finished) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(ExitToMenu());
            return;
        }

        var pos = content.anchoredPosition;
        pos.y += scrollSpeed * Time.unscaledDeltaTime;
        content.anchoredPosition = pos;

        if (pos.y >= distanceToTravel)
            StartCoroutine(ExitToMenu());
    }

    System.Collections.IEnumerator ExitToMenu()
    {
        finished = true;

        if (endHoldSeconds > 0f)
            yield return new WaitForSecondsRealtime(endHoldSeconds);

        Time.timeScale = 1f;

        if (SceneManager.Instance != null)
            SceneManager.Instance.LoadScene(menuScene);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(menuScene.ToString());
    }
}
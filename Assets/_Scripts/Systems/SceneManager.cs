using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using static SceneTypes;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance { get; private set; }

    [System.Serializable]
    public struct SceneMap { public SceneType type; public string sceneName; }

    [Header("Scene Name Mapping")]
    [SerializeField]
    private SceneMap[] scenes =
    {
        new SceneMap{ type = SceneType.MainMenu,  sceneName = "MainMenu" },
        new SceneMap{ type = SceneType.Game,      sceneName = "Game" },
        new SceneMap{ type = SceneType.Credits,   sceneName = "Credits" },
        new SceneMap{ type = SceneType.Harbor,    sceneName = "Harbor" },
    };

    [Header("Transition UI")]
    [SerializeField] private RectTransform transitionImage;
    [SerializeField] private float slideDuration = 2.0f;
    [SerializeField] private SlideFrom slideFrom = SlideFrom.Right;

    private Vector2 offscreenPos;
    private readonly Vector2 centerPos = Vector2.zero;

    public event Action<SceneType> OnSceneLoaded;
    public enum SlideFrom { Left, Right, Top, Bottom }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupImage();
    }

    void SetupImage()
    {
        if (!transitionImage) { Debug.LogError("[SceneManager] Transition image missing!"); return; }

        var parentRT = transitionImage.parent as RectTransform;
        var pr = parentRT ? parentRT.rect : new Rect(0, 0, Screen.width, Screen.height);
        transitionImage.anchorMin = transitionImage.anchorMax = new Vector2(0.5f, 0.5f);
        transitionImage.pivot = new Vector2(0.5f, 0.5f);
        transitionImage.sizeDelta = new Vector2(pr.width, pr.height);

        float w = pr.width, h = pr.height;
        offscreenPos = slideFrom switch
        {
            SlideFrom.Left => new Vector2(-w, 0),
            SlideFrom.Right => new Vector2(w, 0),
            SlideFrom.Top => new Vector2(0, h),
            _ => new Vector2(0, -h),
        };

        transitionImage.anchoredPosition = offscreenPos;
    }
    public void LoadScene(SceneType type)
    {
        string target = GetSceneName(type);
        if (string.IsNullOrEmpty(target))
        {
            Debug.LogError($"[SceneManager] No mapping for {type}");
            return;
        }

        string current = USceneManager.GetActiveScene().name;

        if (ShouldBeInstant(current, target))
        {
            Debug.Log($"[SceneManager] Instant load: {current} ? {target}");
            USceneManager.LoadScene(target, LoadSceneMode.Single);
            return;
        }

        StartCoroutine(LoadRoutine(target));
    }
    bool ShouldBeInstant(string from, string to)
    {
        if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
            return false;

        bool fromMain = from == "MainMenu";
        bool toMain = to == "MainMenu";

        if ((fromMain && to == "Options") || (toMain && from == "Options"))
            return true;

        if ((fromMain && to == "Credits") || (toMain && from == "Credits"))
            return true;

        return false;
    }

    string GetSceneName(SceneType type)
    {
        for (int i = 0; i < scenes.Length; i++)
            if (scenes[i].type == type) return scenes[i].sceneName;

        Debug.LogError($"[SceneManager] No mapping found for {type}");
        return null;
    }

    IEnumerator LoadRoutine(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneManager] Scene name was null/empty.");
            yield break;
        }

        yield return Slide(transitionImage.anchoredPosition, centerPos, slideDuration);

        var op = USceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        op.allowSceneActivation = true;

        while (!op.isDone)
            yield return null;

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        yield return new WaitForSecondsRealtime(3f);

        yield return Slide(centerPos, offscreenPos, slideDuration);
    }

    IEnumerator Slide(Vector2 from, Vector2 to, float dur)
    {
        float t = 0f;
        transitionImage.anchoredPosition = from;

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / dur);
            transitionImage.anchoredPosition = Vector2.LerpUnclamped(from, to, u);
            yield return null;
        }

        transitionImage.anchoredPosition = to;
    }

    public void ReloadActiveScene()
    {
        var activeName = USceneManager.GetActiveScene().name;
        StartCoroutine(LoadRoutine(activeName));
    }
}
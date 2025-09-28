using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using static SceneTypes;

public class SceneManager : MonoBehaviour
{
    
    public static SceneManager Instance { get; private set; }

    [System.Serializable]
    public struct SceneMap
    {
        public SceneType type;
        public string sceneName;
    }

    [Header("Scene Name Mapping")] [SerializeField]
    private SceneMap[] scenes =
    {
        new SceneMap { type = SceneType.MainMenu, sceneName = "MainMenu" },
        new SceneMap { type = SceneType.Game, sceneName = "Game" },
        new SceneMap { type = SceneType.Credits, sceneName = "Credits" },
        new SceneMap { type = SceneType.Harbor, sceneName = "Harbor" },
    };

    [Header("Transition UI")] [SerializeField]
    private RectTransform leftDoor;

    [SerializeField] private RectTransform rightDoor;
    [SerializeField] private float slideDuration = 2.0f;
    [SerializeField] private SlideFrom slideFrom = SlideFrom.Right;
    

    private readonly Vector2 _leftDoorOpenPos = new(-960f, 0f);
    private readonly Vector2 _leftDoorClosePos = new(0, 0f);
    private readonly Vector2 _rightDoorOpenPos = new(0, 0f);
    private readonly Vector2 _rightDoorClosePos = new(-960f, 0f);


    public event Action<SceneType> OnSceneLoaded;

    public enum SlideFrom
    {
        Left,
        Right,
        Top,
        Bottom
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ToggleDoors(true);
    }

    void Start()
    {

        if (OnSceneLoaded != null)
            OnSceneLoaded(GetCurrentScene());
    }
    public SceneType GetCurrentScene()
    {
        var sceneName = USceneManager.GetActiveScene().name;
        return GetSceneType(sceneName);
    }
    private SceneType GetSceneType(string sceneName)
    {
        foreach (var scene in scenes)
        {
            if (scene.sceneName == sceneName)
            {
                return scene.type;
            }
        }
        return SceneType.MainMenu;
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
            OnSceneLoaded?.Invoke(GetSceneType(target));

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
            if (scenes[i].type == type)
                return scenes[i].sceneName;

        Debug.LogError($"[SceneManager] No mapping found for {type}");
        return null;
    }

    private void ToggleDoors(bool open)
    {
        leftDoor.DOAnchorPos(open? _leftDoorOpenPos : _leftDoorClosePos, slideDuration);
        rightDoor.DOAnchorPos(open? _rightDoorOpenPos : _rightDoorClosePos, slideDuration);
    }
    IEnumerator LoadRoutine(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneManager] Scene name was null/empty.");
            yield break;
        }

        ToggleDoors(false);
        yield return new WaitForSecondsRealtime(slideDuration);

        var op = USceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        op.allowSceneActivation = true;

        while (!op.isDone)
            yield return null;

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        OnSceneLoaded?.Invoke(GetSceneType(sceneName));

        yield return new WaitForSecondsRealtime(2f);
        ToggleDoors(true);
        yield return new WaitForSecondsRealtime(slideDuration);
    }
    

    public void ReloadActiveScene()
    {
        var activeName = USceneManager.GetActiveScene().name;
        StartCoroutine(LoadRoutine(activeName));
    }
}
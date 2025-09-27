using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using static SceneTypes;

public class SceneManager : MonoBehaviour
{
    // singleton
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
        new SceneMap{ type = SceneType.Harbor, sceneName = "Harbor" },
    };

    [Header("Transition UI")]
    
    [SerializeField] private Image fadeImage; 
    [SerializeField] private Slider progressBar;   
    [SerializeField] private float fadeDuration = 0.5f; 
    
    public event Action<SceneType> OnSceneLoaded;
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (ScreenFader.Instance)
            StartCoroutine(ScreenFader.Instance.FadeIn());
    }
    
    void Start()
    {
        if (OnSceneLoaded != null)
            OnSceneLoaded(GetCurrentScene());
    }

    // public API
    public void LoadScene(SceneType scene) => StartCoroutine(LoadRoutine(GetSceneName(scene)));

    string GetSceneName(SceneType type)
    {
        for (int i = 0; i < scenes.Length; i++)
            if (scenes[i].type == type) return scenes[i].sceneName;

        Debug.LogError($"[SceneManager] No mapping found for {type}");
        return null;
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

    IEnumerator LoadRoutine(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneManager] Scene name was null/empty.");
            yield break;
        }

        if (ScreenFader.Instance)
            yield return ScreenFader.Instance.FadeOut();

        var op = USceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            if (progressBar) progressBar.value = op.progress / 0.9f;
            yield return null;
        }
        if (progressBar) progressBar.value = 1f;

        yield return new WaitForSecondsRealtime(0.05f);

        op.allowSceneActivation = true;

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        if (ScreenFader.Instance)
            yield return ScreenFader.Instance.FadeIn();

        if (progressBar) progressBar.value = 0f;
        
        OnSceneLoaded?.Invoke(GetSceneType(sceneName));
    }

    public void ReloadActiveScene()
    {
        var activeName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log($"[SceneManager] ReloadActiveScene -> {activeName}");
        StartCoroutine(LoadRoutine(activeName));
    }
}
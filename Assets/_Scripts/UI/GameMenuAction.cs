using System.Collections;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class GameMenuActions : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject pauseMenuRoot;                  // Pause menu container
    [SerializeField] private GameObject giveUpConfirmPanel;             // Give Up modal container

    [Header("Auto-Resolve (fallback by name if refs are not assigned)")]
    [SerializeField] private string pauseMenuRootName = "PauseMenu";    // Sibling under HUD_Pause
    [SerializeField] private string giveUpPanelName   = "GiveUpConfirm";// Sibling under HUD_Pause

    [Header("Navigation")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";     // If empty, build index 0 is used

    [Header("Optional Fanfare")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip defeatFanfare;
    [SerializeField] private float fanfareDelay = 0.25f;

    void Awake()
    {
        AutoResolveRefs();
        // Ensure modal starts hidden; Pause menu visible state is controlled elsewhere.
        if (giveUpConfirmPanel) giveUpConfirmPanel.SetActive(false);
    }

    void AutoResolveRefs()
    {
        if (pauseMenuRoot == null)
        {
            var candidate = FindDeep(pauseMenuRootName);
            if (candidate != null) pauseMenuRoot = candidate;
        }
        if (giveUpConfirmPanel == null)
        {
            var candidate = FindDeep(giveUpPanelName);
            if (candidate != null) giveUpConfirmPanel = candidate;
        }
    }

    GameObject FindDeep(string targetName)
    {
        if (string.IsNullOrWhiteSpace(targetName)) return null;
        var all = Resources.FindObjectsOfTypeAll<Transform>();
        for (int i = 0; i < all.Length; i++)
        {
            var t = all[i];
            if (t != null && t.name == targetName && t.gameObject.scene.IsValid())
                return t.gameObject;
        }
        return null;
    }

    // Open Give Up confirmation: hide pause menu, show modal, remain paused.
    public void ShowGiveUpConfirm()
    {
        Time.timeScale = 0f;
        if (pauseMenuRoot)      pauseMenuRoot.SetActive(false);
        if (giveUpConfirmPanel) giveUpConfirmPanel.SetActive(true);
    }

    // Cancel Give Up: hide modal, restore pause menu, remain paused.
    public void OnGiveUpNo()
    {
        if (giveUpConfirmPanel) giveUpConfirmPanel.SetActive(false);
        if (pauseMenuRoot)      pauseMenuRoot.SetActive(true);
        Time.timeScale = 0f;
    }

    // Confirm Give Up: unpause, wipe data, optionally play sting, then load Main Menu.
    public void OnGiveUpYes()
    {
        Time.timeScale = 1f;
        SaveManager.ResetAllData();

        if (sfxSource != null && defeatFanfare != null)
            StartCoroutine(GiveUpRoutine());
        else
            LoadMainMenu();
    }

    IEnumerator GiveUpRoutine()
    {
        sfxSource.PlayOneShot(defeatFanfare);
        yield return new WaitForSeconds(fanfareDelay);
        LoadMainMenu();
    }

    void LoadMainMenu()
    {
        if (!string.IsNullOrWhiteSpace(mainMenuSceneName))
            UnitySceneManager.LoadScene(mainMenuSceneName);
        else
            UnitySceneManager.LoadScene(0);
    }

    // Restart current scene from baseline score; wave number persists. Clears mid-wave snapshot.
    public void RestartWave()
    {
        Time.timeScale = 1f;

        var pd = PlayerData.Instance;
        if (pd != null)
        {
            int before = pd.currentScore;
            pd.currentScore = pd.scoreAtWaveStart;
            Debug.Log($"[Menu] RestartWave: score {before} → {pd.currentScore}. Wave={pd.waveNumber}.");
        }
        else
        {
            Debug.LogWarning("[Menu] RestartWave: PlayerData.Instance is null.");
        }

        SaveManager.ClearBoardState();
        SaveManager.SaveGame();
        UnitySceneManager.LoadScene(UnitySceneManager.GetActiveScene().buildIndex);
    }

    // Full wipe then reload current scene.
    public void ResetProgress()
    {
        Time.timeScale = 1f;
        SaveManager.ResetAllData();
        UnitySceneManager.LoadScene(UnitySceneManager.GetActiveScene().buildIndex);
    }
}

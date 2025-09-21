using UnityEngine;

public class FadderBoot : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Ensure()
    {
        if (ScreenFader.Instance == null)
        {
            new GameObject("ScreenFadder").AddComponent<ScreenFader>();
            Debug.Log("[FadderBoot] Spawned ScreenFadder");
        }

        if (SceneManager.Instance == null)
        {
            var SceneManager = Resources.Load<SceneManager>("SceneManager");
            Instantiate(SceneManager);
        }
    }
}

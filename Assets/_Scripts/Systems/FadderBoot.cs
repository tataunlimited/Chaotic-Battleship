using UnityEngine;

public class FadderBoot : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Ensure()
    {
        if (ScreenFadder.Instance == null)
        {
            new GameObject("ScreenFadder").AddComponent<ScreenFadder>();
            Debug.Log("[FadderBoot] Spawned ScreenFadder");
        }
    }
}

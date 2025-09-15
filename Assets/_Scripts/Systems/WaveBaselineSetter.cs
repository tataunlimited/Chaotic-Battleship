using UnityEngine;

// Optional helper you can add to any always-present object in the scene.
// On scene load, if there is NO mid-wave snapshot, this captures the current score
// as the baseline for the wave. This avoids having to modify GameManager.
public class WaveBaselineSetter : MonoBehaviour
{
    private void Start()
    {
        // If there is already a board snapshot, we assume we're resuming mid-wave
        // and therefore we keep the previously stored baseline.
        if (!SaveManager.HasBoardState() && PlayerData.Instance != null)
        {
            PlayerData.Instance.scoreAtWaveStart = PlayerData.Instance.currentScore;
            SaveManager.SaveGame();
            Debug.Log("[WaveBaselineSetter] Captured wave baseline: " + PlayerData.Instance.scoreAtWaveStart);
        }
    }
}

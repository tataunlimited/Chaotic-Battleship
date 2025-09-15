using Core.Ship;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DevSaveHUD : MonoBehaviour
{
    [Header("Labels")]
    [SerializeField] private TextMeshProUGUI waveLabel;
    [SerializeField] private TextMeshProUGUI scoreLabel;

    [Header("Buttons")]
    [SerializeField] private Button addScoreBtn;
    [SerializeField] private Button addWaveBtn;
    [SerializeField] private Button saveBtn;
    [SerializeField] private Button resetBtn;

    private void Awake()
    {
        if (addScoreBtn) addScoreBtn.onClick.AddListener(() => { PlayerData.Instance.currentScore += 100; Refresh(); });
        if (addWaveBtn)  addWaveBtn.onClick.AddListener(() => { PlayerData.Instance.waveNumber    += 1;   Refresh(); });
        if (saveBtn)     saveBtn.onClick.AddListener(() => SaveManager.SaveGame());
        if (resetBtn)    resetBtn.onClick.AddListener(() => { SaveManager.ResetAllData(); Refresh(); });
    }

    private void OnEnable() => Refresh();

    private void Refresh()
    {
        if (PlayerData.Instance == null) return;
        if (waveLabel)  waveLabel.text  = $"Wave: {PlayerData.Instance.waveNumber}";
        if (scoreLabel) scoreLabel.text = $"Score: {PlayerData.Instance.currentScore:N0}";
    }
}

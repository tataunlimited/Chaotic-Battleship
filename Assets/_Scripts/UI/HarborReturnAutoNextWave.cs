using System.Reflection;
using UnityEngine;

public class HarborReturnAutoNextWave : MonoBehaviour
{
    [SerializeField] private MonoBehaviour nextWaveBehaviour; // drag component that has StartNextWave()
    [SerializeField] private string startMethodName = "StartNextWave";
    [SerializeField] private float delay = 0.25f;

    private const string FlagKey = "harbor_pending_nextwave";

    private void Awake()
    {
        if (PlayerPrefs.GetInt(FlagKey, 0) == 1)
        {
            PlayerPrefs.SetInt(FlagKey, 0);
            PlayerPrefs.Save();
            Invoke(nameof(InvokeStart), delay);
        }
    }

    private void InvokeStart()
    {
        if (nextWaveBehaviour == null) return;
        var mi = nextWaveBehaviour.GetType().GetMethod(
            startMethodName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        mi?.Invoke(nextWaveBehaviour, null);
    }
}

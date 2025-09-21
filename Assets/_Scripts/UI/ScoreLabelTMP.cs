using UnityEngine;
using TMPro;

public class ScoreLabelTMP : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private string prefix = "POINTS: ";
    [SerializeField] private bool thousandsSeparator = true;
    [SerializeField] private float updateInterval = 0.2f; // seconds

    private PlayerData pd;
    private float timer;

    private void Reset()
    {
        if (!label) label = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        pd = PlayerData.Instance;
        timer = 0f;
        Refresh();
    }

    private void Update()
    {
        timer += Time.unscaledDeltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            Refresh();
        }
    }

    private void Refresh()
    {
        if (!label) return;
        int score = (pd != null) ? pd.currentScore : 0;
        label.text = prefix + (thousandsSeparator ? score.ToString("N0") : score.ToString());
    }
}

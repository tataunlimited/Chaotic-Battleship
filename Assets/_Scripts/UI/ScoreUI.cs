using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    private void OnEnable()
    {
        // Subscribe to score updates from GameManagerScore
        GameManagerScore.OnScoreChanged += HandleScoreChanged;
    }

    private void OnDisable()
    {
        // Clean up subscription when disabled/destroyed
        GameManagerScore.OnScoreChanged -= HandleScoreChanged;
    }

    private void HandleScoreChanged(int value)
    {
        if (scoreText != null)
        {
            scoreText.text = value.ToString("N0");
        }
    }
}

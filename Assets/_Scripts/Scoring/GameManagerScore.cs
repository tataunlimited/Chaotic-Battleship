using System;
using Core.Ship;
using UnityEngine;
using TMPro;

public class GameManagerScore : MonoBehaviour
{
    // 🔔 New: broadcast score changes so any UI (or SFX, etc.) can react
    public static event Action<int> OnScoreChanged;

    [SerializeField] private ScoreConfig config;

    // (Optional legacy UI reference; safe to keep during transition)
    [SerializeField] private TextMeshProUGUI scoreText;

    public int Score {
        get => PlayerData.Instance.currentScore;
        private set => PlayerData.Instance.currentScore = value;
    }

    public int TurnsThisWave { get; private set; }

    private void OnEnable()
    {
        GameEvents.OnPlayerHitSegment     += HandleHitSegment;
        GameEvents.OnPlayerDestroyedShip  += HandleDestroyedShip;
        GameEvents.OnWaveCleared          += HandleWaveCleared;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerHitSegment     -= HandleHitSegment;
        GameEvents.OnPlayerDestroyedShip  -= HandleDestroyedShip;
        GameEvents.OnWaveCleared          -= HandleWaveCleared;
    }

    private void Start()
    {
        // Emit initial value so UI shows the correct score on scene load
        EmitScoreChanged();
    }

    public void RegisterPlayerTurn() => TurnsThisWave++;

    private void HandleHitSegment(ShipType ship)
    {
        Score += config ? config.segmentHit : 25;
        EmitScoreChanged();
    }

    private void HandleDestroyedShip(ShipType ship)
    {
        int add = 0;
        if (config) add = config.DestroyBonus(ship);
        else {
            switch (ship)
            {
                case ShipType.Submarine:  add = 50;  break;
                case ShipType.Destroyer:  add = 100; break;
                case ShipType.Battleship: add = 200; break;
            }
        }
        Score += add;
        EmitScoreChanged();
    }

    private void HandleWaveCleared()
    {
        int baseBonus = config ? config.waveClear   : 1000;
        int maxSpeed  = config ? config.speedMax    : 1000;
        int perTurn   = config ? config.speedPerTurn: 50;

        Score += baseBonus;
        Score += Mathf.Max(maxSpeed - perTurn * TurnsThisWave, 0);

        TurnsThisWave = 0;
        EmitScoreChanged();
    }

    // 🔔 Single place to update legacy text AND fire the decoupled event
    private void EmitScoreChanged()
    {
        if (scoreText) scoreText.text = Score.ToString("N0"); // legacy path (safe to remove later)
        OnScoreChanged?.Invoke(Score);                        // decoupled signal
    }
}

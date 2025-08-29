using System;
using Core.Ship;
using UnityEngine;
using TMPro;

public class GameManagerScore : MonoBehaviour
{
    [SerializeField] private ScoreConfig config;
    [SerializeField] private TextMeshProUGUI scoreText;

    public int Score { 
        get => PlayerData.Instance.currentScore;
        private set => PlayerData.Instance.currentScore = value;
    }
    public int TurnsThisWave { get; private set; }

    private void OnEnable()
    {
        GameEvents.OnPlayerHitSegment += HandleHitSegment;
        GameEvents.OnPlayerDestroyedShip += HandleDestroyedShip;
        GameEvents.OnWaveCleared += HandleWaveCleared;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerHitSegment -= HandleHitSegment;
        GameEvents.OnPlayerDestroyedShip -= HandleDestroyedShip;
        GameEvents.OnWaveCleared -= HandleWaveCleared;
    }

    private void Start()
    {
        UpdateUI();
    }

    public void RegisterPlayerTurn() => TurnsThisWave++;

    private void HandleHitSegment(ShipType ship)
    {
        Score += config ? config.segmentHit : 25;
        UpdateUI();
    }

    private void HandleDestroyedShip(ShipType ship)
    {
        int add = 0;
        if (config) add = config.DestroyBonus(ship);
        else {
            switch (ship)
            {
                case ShipType.Submarine: add = 50; break;
                case ShipType.Destroyer: add = 100; break;
                case ShipType.Battleship: add = 200; break;
            }
        }
        Score += add;
        UpdateUI();
    }

    private void HandleWaveCleared()
    {
        int baseBonus = config ? config.waveClear : 1000;
        int maxSpeed = config ? config.speedMax : 1000;
        int perTurn = config ? config.speedPerTurn : 50;

        Score += baseBonus;
        Score += Mathf.Max(maxSpeed - perTurn * TurnsThisWave, 0);

        UpdateUI();
        TurnsThisWave = 0;
    }

    private void UpdateUI()
    {
        if (scoreText) scoreText.text = Score.ToString("N0");
    }
}

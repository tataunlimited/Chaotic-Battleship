using System;
using System.Collections.Generic;
using Core.Ship;
using UnityEngine;

public static class SaveManager
{
    private const string KEY_WAVE              = "Wave";
    private const string KEY_SCORE             = "Score";
    private const string KEY_NUM_SUBS_IN_DOCK  = "NumSubsInDock";
    private const string KEY_NUM_DESTROYERS_IN_DOCK  = "NumDestroyersInDock";
    private const string KEY_NUM_CRUISERS_IN_DOCK  = "NumCruisersInDock";
    private const string KEY_NUM_BATTLESHIPS_IN_DOCK  = "NumBattleshipsInDock";
    private const string KEY_SCORE_WAVE_START = "ScoreWaveStart";   // baseline at start of wave
    private const string KEY_GAMESTATE         = "GameStateJson";    // mid-wave snapshot

    // ===== Meta Progress (wave, score, upgrades) =====
    public static void SaveGame()
    {
        var pd = PlayerData.Instance;
        if (pd == null)
        {
            Debug.LogWarning("[SaveManager] SaveGame() — PlayerData.Instance is null.");
            return;
        }

        PlayerPrefs.SetInt(KEY_WAVE,             pd.waveNumber);
        PlayerPrefs.SetInt(KEY_NUM_SUBS_IN_DOCK,  pd.numberSubsInDock);
        PlayerPrefs.SetInt(KEY_NUM_DESTROYERS_IN_DOCK,  pd.numberDestroyersInDock);
        PlayerPrefs.SetInt(KEY_NUM_CRUISERS_IN_DOCK,  pd.numberCruisersInDock);
        PlayerPrefs.SetInt(KEY_NUM_BATTLESHIPS_IN_DOCK,  pd.numberBattleshipsInDock);
        PlayerPrefs.SetInt(KEY_SCORE,            pd.currentScore);
        PlayerPrefs.SetInt(KEY_SCORE_WAVE_START, pd.scoreAtWaveStart);

        // Save per-ship upgrades
        pd.EnsureUpgradeDefaults();
        foreach (var ship in pd.AllShipTypesForUpgrades())
        {
            int Get(UpgradeType t) => pd.GetUpgrade(ship, t);
            PlayerPrefs.SetInt(UpgradeKey(ship, UpgradeType.SpecialAttack), Get(UpgradeType.SpecialAttack));
            PlayerPrefs.SetInt(UpgradeKey(ship, UpgradeType.Movement),      Get(UpgradeType.Movement));
            PlayerPrefs.SetInt(UpgradeKey(ship, UpgradeType.AttackPattern), Get(UpgradeType.AttackPattern));
            PlayerPrefs.SetInt(UpgradeKey(ship, UpgradeType.Armor),         Get(UpgradeType.Armor));
        }
        Debug.Log("SAVING GAME: " + pd.waveNumber + " " + pd.currentScore + " " + pd.scoreAtWaveStart);
        PlayerPrefs.Save();
        Debug.Log($"[SaveManager] SaveGame() — wave={pd.waveNumber}, score={pd.currentScore}, baseline={pd.scoreAtWaveStart}");
    }

    public static void LoadGame()
    {
        var pd = PlayerData.Instance;
        if (pd == null)
        {
            Debug.LogWarning("[SaveManager] LoadGame() — PlayerData.Instance is null.");
            return;
        }

        if (PlayerPrefs.HasKey(KEY_WAVE))
            pd.waveNumber = PlayerPrefs.GetInt(KEY_WAVE, pd.waveNumber);

        if (PlayerPrefs.HasKey(KEY_SCORE))
            pd.currentScore = PlayerPrefs.GetInt(KEY_SCORE, pd.currentScore);

        if (PlayerPrefs.HasKey(KEY_SCORE_WAVE_START))
            pd.scoreAtWaveStart = PlayerPrefs.GetInt(KEY_SCORE_WAVE_START, pd.scoreAtWaveStart);
        if (PlayerPrefs.HasKey(KEY_NUM_SUBS_IN_DOCK))
            pd.numberSubsInDock = PlayerPrefs.GetInt(KEY_NUM_SUBS_IN_DOCK, pd.numberSubsInDock);
        if (PlayerPrefs.HasKey(KEY_NUM_DESTROYERS_IN_DOCK))
            pd.numberDestroyersInDock = PlayerPrefs.GetInt(KEY_NUM_DESTROYERS_IN_DOCK, pd.numberDestroyersInDock);
        if (PlayerPrefs.HasKey(KEY_NUM_CRUISERS_IN_DOCK))
            pd.numberCruisersInDock = PlayerPrefs.GetInt(KEY_NUM_CRUISERS_IN_DOCK, pd.numberCruisersInDock);
        if (PlayerPrefs.HasKey(KEY_NUM_BATTLESHIPS_IN_DOCK))
            pd.numberBattleshipsInDock = PlayerPrefs.GetInt(KEY_NUM_BATTLESHIPS_IN_DOCK, pd.numberBattleshipsInDock);

        // Load per-ship upgrades
        pd.EnsureUpgradeDefaults();
        foreach (var ship in pd.AllShipTypesForUpgrades())
        {
            int Get(string k) => PlayerPrefs.GetInt(k, 0);
            pd.SetUpgrade(ship, UpgradeType.SpecialAttack, Get(UpgradeKey(ship, UpgradeType.SpecialAttack)));
            pd.SetUpgrade(ship, UpgradeType.Movement,      Get(UpgradeKey(ship, UpgradeType.Movement)));
            pd.SetUpgrade(ship, UpgradeType.AttackPattern, Get(UpgradeKey(ship, UpgradeType.AttackPattern)));
            pd.SetUpgrade(ship, UpgradeType.Armor,         Get(UpgradeKey(ship, UpgradeType.Armor)));
        }

        Debug.Log($"[SaveManager] LoadGame() — wave={pd.waveNumber}, score={pd.currentScore}, baseline={pd.scoreAtWaveStart}");
    }

    // ===== Board Snapshots (mid-wave) — JSON CORE =====
    public static void SaveBoardState(string json)
    {
        PlayerPrefs.SetString(KEY_GAMESTATE, json);
        PlayerPrefs.Save();
        Debug.Log($"[SaveManager] SaveBoardState(json) — bytes={json?.Length ?? 0}");
    }

    public static bool TryLoadBoardState(out string json)
    {
        if (PlayerPrefs.HasKey(KEY_GAMESTATE))
        {
            json = PlayerPrefs.GetString(KEY_GAMESTATE, string.Empty);
            return !string.IsNullOrEmpty(json);
        }
        json = null;
        return false;
    }

    public static void ClearBoardState()
    {
        if (PlayerPrefs.HasKey(KEY_GAMESTATE))
        {
            PlayerPrefs.DeleteKey(KEY_GAMESTATE);
            PlayerPrefs.Save();
            Debug.Log("[SaveManager] ClearBoardState()");
        }
    }

    public static bool HasBoardState()
    {
        return PlayerPrefs.HasKey(KEY_GAMESTATE);
    }

    // ===== Board Snapshots — OVERLOADS for GameState =====
    // These match your GameManager calls: SaveBoardState(GameState) and TryLoadBoardState(out GameState)

    // NOTE: We rely on UnityEngine.JsonUtility for (de)serialization.
    // Replace with your own serializer if you have a custom one.
    public static void SaveBoardState(GameState state)
    {
        if (state == null)
        {
            Debug.LogWarning("[SaveManager] SaveBoardState(GameState) — state was null; clearing snapshot.");
            ClearBoardState();
            return;
        }

        try
        {
            string json = JsonUtility.ToJson(state);
            SaveBoardState(json); // delegate to JSON core
        }
        catch (Exception ex)
        {
            Debug.LogError("[SaveManager] SaveBoardState(GameState) — serialization failed: " + ex);
        }
    }

    public static bool TryLoadBoardState(out GameState state)
    {
        state = null;

        if (!TryLoadBoardState(out string json))
            return false;

        try
        {
            // If GameState is a class/struct you own, JsonUtility should be fine.
            // If it's polymorphic or uses non-Unity types, consider a different serializer.
            state = JsonUtility.FromJson<GameState>(json);
            if (state == null)
            {
                Debug.LogWarning("[SaveManager] TryLoadBoardState(GameState) — deserialized null, clearing snapshot.");
                ClearBoardState();
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError("[SaveManager] TryLoadBoardState(GameState) — deserialization failed: " + ex);
            return false;
        }
    }

    // ===== Full Reset =====
    public static void ResetAllData()
    {
        const string PREF_MASTER = "opt_audio_master";
        const string PREF_SFX    = "opt_audio_sfx";
        const string PREF_BGM    = "opt_audio_bgm";

        bool hasM = PlayerPrefs.HasKey(PREF_MASTER);
        bool hasS = PlayerPrefs.HasKey(PREF_SFX);
        bool hasB = PlayerPrefs.HasKey(PREF_BGM);

        float vM = PlayerPrefs.GetFloat(PREF_MASTER, 1f);
        float vS = PlayerPrefs.GetFloat(PREF_SFX,    1f);
        float vB = PlayerPrefs.GetFloat(PREF_BGM,    1f);

        PlayerPrefs.DeleteAll();

        if (hasM) PlayerPrefs.SetFloat(PREF_MASTER, vM);
        if (hasS) PlayerPrefs.SetFloat(PREF_SFX,    vS);
        if (hasB) PlayerPrefs.SetFloat(PREF_BGM,    vB);
        PlayerPrefs.Save();

        var pd = PlayerData.Instance;
        if (pd != null)
        {
            pd.waveNumber = 1;
            pd.currentScore = 0;
            pd.scoreAtWaveStart = 0;

            pd.EnsureUpgradeDefaults();
            foreach (var ship in pd.AllShipTypesForUpgrades())
            {
                pd.SetUpgrade(ship, UpgradeType.SpecialAttack, 0);
                pd.SetUpgrade(ship, UpgradeType.Movement, 0);
                pd.SetUpgrade(ship, UpgradeType.AttackPattern, 0);
                pd.SetUpgrade(ship, UpgradeType.Armor, 0);
            }
        }

        Debug.Log("[SaveManager] ResetAllData()");
    }


    // ===== Helpers =====
    private static string UpgradeKey(ShipType ship, UpgradeType type) => $"Upgrade_{ship}_{type}";
}

public enum UpgradeType { SpecialAttack, Movement, AttackPattern, Armor }

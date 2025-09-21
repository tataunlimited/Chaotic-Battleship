using System.Collections.Generic;
using Core.Board;
using Core.Ship;
using UnityEngine;

[AddComponentMenu("Game/Enemy Wave Spawner")]
public class EnemyWaveSpawner : MonoBehaviour
{
    [Header("Use a WaveDefinition (preferred)")]

    [Header("OR: Counts (fallback if no WaveDefinition)")]
    [Min(0)] public int numSubmarines = 0;
    [Min(0)] public int numDestroyers = 0;
    [Min(0)] public int numBattleships = 0;
    [Min(0)] public int numCruisers = 0;

    [Header("References")]
    public BoardController controller;   // optional; will try BoardController.Instance if null
    public BoardView enemyBoard;         // optional; will use controller.enemyView if null

    [Header("AI Settings (used if no override in WaveDefinition)")]
    [Range(0, 3)] public int intelligenceLevel = 0;
    public bool revealOnSpawn = false;

    [SerializeField] private EnemyWaveManager waveManager = new EnemyWaveManager();
    public static EnemyWaveSpawner Instance;

    public List<WaveDefinition> waves = new();

    void OnValidate()
    {
        waveManager ??= new EnemyWaveManager();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("EnemyWaveSpawner: Multiple instances detected. Destroying duplicate.");
            Destroy(this);
            return;
        }
        Instance = this;
    }

    [ContextMenu("Spawn Wave Now")]
    public void SpawnWave()
    {
        // choose wave definition based on PlayerData current wave.
        WaveDefinition current_wave = GetCurrentWaveDefinition();

        if (controller == null) controller = BoardController.Instance;
        if (controller == null)
        {
            Debug.LogError("EnemyWaveSpawner: BoardController.Instance not found.");
            return;
        }

        if (enemyBoard == null) enemyBoard = controller.enemyView;
        if (enemyBoard == null)
        {
            Debug.LogError("EnemyWaveSpawner: enemyBoard reference is missing.");
            return;
        }

        // Build the ship models list
        List<ShipModel> ships = (current_wave != null) ? BuildWaveFromDefinition(current_wave)
                                                         : BuildWaveFromCounts();

        if (ships.Count == 0)
        {
            Debug.LogWarning("EnemyWaveSpawner: Nothing to spawn (0 ships).");
            return;
        }

        // If the asset overrides AI, apply it
        if (current_wave != null && current_wave.overrideIntelligence)
            waveManager.intelligenceLevel = Mathf.Clamp(current_wave.intelligenceLevel, 0, 3);

        // Place ships in valid positions/orientations
        bool placedAll = waveManager.RandomlySetShipsLocations(enemyBoard, ships);
        if (!placedAll)
        {
            Debug.LogWarning("EnemyWaveSpawner: Not all ships could be placed on the board.");
        }
                

        // Reveal flag: asset value OR local flag
        bool reveal = (current_wave != null && current_wave.revealOnSpawn) || revealOnSpawn;

        // Convert models to live ShipViews
        controller.SpawnEnemyShipsFromModels(ships, reveal);
    }

    private List<ShipModel> BuildWaveFromDefinition(WaveDefinition def)
    {
        var list = new List<ShipModel>(def.TotalShips);
        foreach (var entry in def.ships)
            AddCopies(list, entry.type, entry.count);
        return list;
    }

    private List<ShipModel> BuildWaveFromCounts()
    {
        var list = new List<ShipModel>(numSubmarines + numDestroyers + numBattleships + numCruisers);
        AddCopies(list, ShipType.Submarine, numSubmarines);
        AddCopies(list, ShipType.Destroyer, numDestroyers);
        AddCopies(list, ShipType.Battleship, numBattleships);
        AddCopies(list, ShipType.Cruiser, numCruisers);
        return list;
    }

    private static void AddCopies(List<ShipModel> list, ShipType type, int count)
    {
        if (count <= 0) return;

        if (!ShipDatabase.DefaultShips.TryGetValue(type, out var model))
        {
            Debug.LogError($"EnemyWaveSpawner: Default ship not found for {type}");
            return;
        }

        for (int i = 0; i < count; i++)
            list.Add(model.Copy());
    }
    
    // Get the current wave definition based on PlayerData wave number, if it exists
    public WaveDefinition GetCurrentWaveDefinition()
    {
        if (waves == null || waves.Count == 0) return null;
        int index = Mathf.Clamp(PlayerData.Instance.waveNumber - 1, 0, waves.Count - 1);
        return waves[index];
    }
}

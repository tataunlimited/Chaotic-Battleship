using System.Collections.Generic;
using System.Linq;
using Core.Board;
using Core.Ship;
using UnityEngine;

[AddComponentMenu("Game/Enemy Wave Spawner")]
public class EnemyWaveSpawner : MonoBehaviour
{
    [Header("Use a WaveDefinition (preferred)")]
    public List<WaveDefinition> waves = new();

    [Header("OR: Counts (fallback if no WaveDefinition)")]
    [Min(0)] public int numSubmarines = 0;
    [Min(0)] public int numDestroyers = 0;
    [Min(0)] public int numBattleships = 0;
    [Min(0)] public int numCruisers = 0;

    [Header("References")]
    public BoardController controller;   // optional; will try BoardController.Instance if null
    public BoardView enemyBoard;         // optional; will use controller.enemyView if null

    [Header("AI Settings (used if no WaveDefinition)")]
    [Range(0, 3)] public int intelligenceLevel = 0;
    public bool revealOnSpawn = false;

    [SerializeField] private EnemyWaveManager waveManager = new EnemyWaveManager();
    public static EnemyWaveSpawner Instance;

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
        var def = GetCurrentWaveDefinition();

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

        // --- Build models ---
        List<ShipModel> ships = (def != null) ? BuildWaveFromDefinition(def)
                                              : BuildWaveFromCounts();
        if (ships.Count == 0)
        {
            Debug.LogWarning("EnemyWaveSpawner: Nothing to spawn (0 ships).");
            return;
        }
        //set the intelligence level based on the wave definition or the default intelligence level
        waveManager.intelligenceLevel = (def != null && def.overrideIntelligence)
            ? Mathf.Clamp(def.intelligenceLevel, 0, 3)
            : Mathf.Clamp(intelligenceLevel, 0, 3);

        // --- Set the wave default AI level ---
        waveManager.intelligenceLevel = (def != null && def.overrideIntelligence)
            ? Mathf.Clamp(def.intelligenceLevel, 0, 3)
            : Mathf.Clamp(intelligenceLevel, 0, 3);

        // --- Place ships ---
        bool placedAll = waveManager.RandomlySetShipsLocations(enemyBoard, ships);
        if (!placedAll)
            Debug.LogWarning("EnemyWaveSpawner: Not all ships could be placed on the board.");

        bool reveal = (def != null && def.revealOnSpawn) || revealOnSpawn;

        // Track which ships are added by this spawn so we can bind per-ship AI
        var preExisting = new HashSet<ShipView>(enemyBoard.SpawnedShips.Values);
        controller.SpawnEnemyShipsFromModels(ships, false);
        var justSpawned = enemyBoard.SpawnedShips.Values.Where(sv => !preExisting.Contains(sv)).ToList();
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
        AddCopies(list, ShipType.Submarine,  numSubmarines);
        AddCopies(list, ShipType.Destroyer,  numDestroyers);
        AddCopies(list, ShipType.Battleship, numBattleships);
        AddCopies(list, ShipType.Cruiser,    numCruisers);
        return list;
    }

    private static void AddCopies(List<ShipModel> list, ShipType type, int count)
    {
        if (count <= 0) return;

        // if (!ShipFactory.DefaultShips.TryGetValue(type, out var model))
        // {
        //     Debug.LogError($"EnemyWaveSpawner: Default ship not found for {type}");
        //     return;
        // }
        var model = ShipFactory.CreateShipModel(type); // force creation (if not already created)
        for (int i = 0; i < count; i++)
            list.Add(model.Copy());
    }

    public WaveDefinition GetCurrentWaveDefinition()
    {
        if (waves == null || waves.Count == 0) return null;
        int index = Mathf.Clamp(PlayerData.Instance.waveNumber - 1, 0, waves.Count - 1);
        return waves[index];
    }
}

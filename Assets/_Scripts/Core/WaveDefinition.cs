using System;
using System.Collections.Generic;
using Core.Ship;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveDefinition", menuName = "Game/Waves/Wave Definition", order = 0)]
public class WaveDefinition : ScriptableObject
{
    [Serializable]
    public class ShipEntry
    {
        public ShipType type;
        [Min(0)] public int count = 0;
    }

    [Serializable]
    public class TypeAIOverride
    {
        public ShipType type;
        [Range(0,3)] public int intelligenceLevel = 0;
    }

    [Serializable]
    public class PerShipAIOverride
    {
        public ShipType type;
        [Min(0)] public int count = 0;          // assign to any 'count' ships of this type spawned for this wave
        [Range(0,3)] public int intelligenceLevel = 0;
    }

    [Header("Ships in this wave")]
    public List<ShipEntry> ships = new();

    [Header("Wave default AI (optional)")]
    public bool overrideIntelligence = false;
    [Range(0,3)] public int intelligenceLevel = 0;

    [Header("Reveal on spawn")]
    public bool revealOnSpawn = false;

    [Header("Overrides")]
    [Tooltip("Set a default AI level for every ship of a type in this wave.")]
    public List<TypeAIOverride> typeAI = new();

    [Tooltip("Set AI on a subset of ships of a given type (first N found after spawning).")]
    public List<PerShipAIOverride> perShipAI = new();

    public int TotalShips
    {
        get
        {
            int total = 0;
            foreach (var s in ships) total += Mathf.Max(0, s.count);
            return total;
        }
    }
}

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

    [Header("Ships in this wave")]
    public List<ShipEntry> ships = new();

    [Header("Wave default AI (optional)")]
    public bool overrideIntelligence = false;
    [Range(0,3)] public int intelligenceLevel = 0;

    [Header("Reveal on spawn")]
    public bool revealOnSpawn = false;

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

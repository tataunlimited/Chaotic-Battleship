using System.Collections.Generic;
using Core.Ship;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveDefinition", menuName = "Game/Waves/Wave Definition", order = 0)]
public class WaveDefinition : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public ShipType type;
        [Min(0)] public int count;
    }

    [Header("Meta")]
    public string displayName;
    [TextArea] public string notes;

    [Header("Composition")]
    public List<Entry> ships = new();

    [Header("AI Overrides (optional)")]
    public bool overrideIntelligence;
    [Range(0, 3)] public int intelligenceLevel = 0;

    [Tooltip("Reveal enemy ships after spawn (useful for testing).")]
    public bool revealOnSpawn = false;

    public int TotalShips
    {
        get
        {
            int t = 0;
            foreach (var e in ships) t += Mathf.Max(0, e.count);
            return t;
        }
    }
}

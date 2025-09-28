// Assets/Editor/UpgradeIconLookupBuilder.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Rebuilds an UpgradeIconLookupSO from a folder of sprites.
// File name examples expected by this tool (case-insensitive):
//   sub_move00.png, submarine_move01.png, destroyer_move02.png, cruiser_move03.png, battleship_move01.png
public static class UpgradeIconLookupBuilder
{
    // Adjust to where you want the asset saved (must be under Assets/)
    private const string ASSET_PATH = "Assets/_ScriptableObjects/ShipUpgrades/UpgradeIconLookup.asset";

    // Default source folder for Movement icons
    private const string DEFAULT_SRC_FOLDER = "Assets/_Art/UI/Harbor/UpgradeIcons/Movement";

    // Tokens used to match file names to ship types. Add/remove tokens as needed.
    private static readonly Dictionary<Core.Ship.ShipType, string[]> ShipTokens =
        new Dictionary<Core.Ship.ShipType, string[]>
        {
            { Core.Ship.ShipType.Submarine,  new[] { "submarine", "sub_" , "sub-" , "sub " , "sub" } },
            { Core.Ship.ShipType.Destroyer,  new[] { "destroyer", "dest", "destroy" } },
            { Core.Ship.ShipType.Cruiser,    new[] { "cruiser" } },
            { Core.Ship.ShipType.Battleship, new[] { "battleship", "battle", "bb", "bship" } },
        };

    [MenuItem("Tools/Upgrades/Rebuild Movement Icons (Default Path)")]
    public static void RebuildFromDefault()
    {
        RebuildFromFolder(DEFAULT_SRC_FOLDER);
    }

    [MenuItem("Tools/Upgrades/Rebuild Movement Icons From Folder...")]
    public static void RebuildFromFolderMenu()
    {
        var src = EditorUtility.OpenFolderPanel("Select Movement Icons Folder", "Assets", "");
        if (string.IsNullOrEmpty(src)) return;

        if (src.StartsWith(Application.dataPath))
            src = "Assets" + src.Substring(Application.dataPath.Length).Replace("\\", "/");

        RebuildFromFolder(src);
    }

    public static void RebuildFromFolder(string assetFolder)
    {
        if (string.IsNullOrEmpty(assetFolder) || !AssetDatabase.IsValidFolder(assetFolder))
        {
            EditorUtility.DisplayDialog("Upgrade Icon Builder", $"Folder not found:\n{assetFolder}", "OK");
            return;
        }

        EnsureFolderChain(Path.GetDirectoryName(ASSET_PATH)?.Replace("\\", "/"));

        var lookup = AssetDatabase.LoadAssetAtPath<UpgradeIconLookupSO>(ASSET_PATH);
        if (lookup == null)
        {
            lookup = ScriptableObject.CreateInstance<UpgradeIconLookupSO>();
            AssetDatabase.CreateAsset(lookup, ASSET_PATH);
            AssetDatabase.SaveAssets();
        }

        var sprites = AssetDatabase.FindAssets("t:Sprite", new[] { assetFolder })
                                   .Select(g => AssetDatabase.GUIDToAssetPath(g))
                                   .Select(p => AssetDatabase.LoadAssetAtPath<Sprite>(p))
                                   .Where(s => s != null)
                                   .ToList();

        // Build one row per ship for UpgradeType.Movement
        var rows = new List<UpgradeIconLookupSO.IconRow>();
        foreach (Core.Ship.ShipType ship in Enum.GetValues(typeof(Core.Ship.ShipType)))
        {
            // Skip "None" if present
            if (ship.ToString().Equals("None", StringComparison.OrdinalIgnoreCase))
                continue;

            var row = new UpgradeIconLookupSO.IconRow
            {
                ship = ship,
                upgradeType = UpgradeType.Movement,
                iconsByLevel = new List<Sprite>()
            };

            var tokens = ShipTokens.TryGetValue(ship, out var tks) ? tks : new[] { ship.ToString().ToLowerInvariant() };

            // Match by tokens (filename contains any token)
            var shipSprites = sprites
                .Where(s =>
                {
                    var n = s.name.ToLowerInvariant();
                    return tokens.Any(tok => n.Contains(tok));
                })
                .OrderBy(s => ExtractLevelIndex(s.name)) // expects trailing 00..99
                .ToList();

            int maxIndex = shipSprites.Select(s => ExtractLevelIndex(s.name)).DefaultIfEmpty(-1).Max();
            for (int i = 0; i <= maxIndex; i++)
            {
                var sprite = shipSprites.FirstOrDefault(s => ExtractLevelIndex(s.name) == i);
                row.iconsByLevel.Add(sprite);
            }

            rows.Add(row);
        }

        // Write serialized data (use intValue for enums to avoid index issues)
        var so = new SerializedObject(lookup);
        var rowsProp = so.FindProperty("rows");
        rowsProp.arraySize = rows.Count;

        for (int i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            var elem = rowsProp.GetArrayElementAtIndex(i);

            elem.FindPropertyRelative("ship").intValue        = (int)r.ship;
            elem.FindPropertyRelative("upgradeType").intValue = (int)r.upgradeType;

            var listProp = elem.FindPropertyRelative("iconsByLevel");
            listProp.arraySize = r.iconsByLevel.Count;
            for (int j = 0; j < r.iconsByLevel.Count; j++)
            {
                listProp.GetArrayElementAtIndex(j).objectReferenceValue = r.iconsByLevel[j];
            }
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(lookup);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[UpgradeIconLookupBuilder] Rebuilt from {assetFolder} → {ASSET_PATH}");
    }

    private static int ExtractLevelIndex(string name)
    {
        // Match trailing two digits first (…00, …01, …12)
        for (int i = name.Length - 1; i >= 1; i--)
            if (char.IsDigit(name[i]) && char.IsDigit(name[i - 1]) &&
                int.TryParse(name.Substring(i - 1, 2), out int idx)) return idx;

        // Fallback: any last single digit
        for (int i = name.Length - 1; i >= 0; i--)
            if (char.IsDigit(name[i])) return (int)char.GetNumericValue(name[i]);

        return 0;
    }

    private static void EnsureFolderChain(string assetFolder)
    {
        if (string.IsNullOrEmpty(assetFolder)) return;

        var parts = assetFolder.Split('/').Where(p => !string.IsNullOrEmpty(p)).ToArray();
        if (parts.Length == 0 || parts[0] != "Assets")
            throw new Exception($"Folder must be under Assets: {assetFolder}");

        string parent = "Assets";
        for (int i = 1; i < parts.Length; i++)
        {
            string current = $"{parent}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(current))
                AssetDatabase.CreateFolder(parent, parts[i]);
            parent = current;
        }
    }
}

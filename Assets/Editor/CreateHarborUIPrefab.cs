#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI;

public static class CreateHarborUIPrefab
{
    private const string PrefabDir = "Assets/_Prefabs/Harbor";
    private const string PrefabPath = PrefabDir + "/PFB_HarborUI.prefab";

    [MenuItem("Tools/Harbor/Create Harbor UI Prefab From Selection")]
    private static void CreateFromSelection()
    {
        var sel = Selection.activeGameObject;
        if (sel == null)
        {
            EditorUtility.DisplayDialog("Create Harbor UI Prefab",
                "Select the Harbor UI root object in the scene (e.g., 'HarborUI_Root') then run this.",
                "OK");
            return;
        }

        // Ensure target folder exists
        if (!AssetDatabase.IsValidFolder(PrefabDir))
        {
            var parent = "Assets/_Prefabs";
            if (!AssetDatabase.IsValidFolder(parent)) AssetDatabase.CreateFolder("Assets", "_Prefabs");
            AssetDatabase.CreateFolder(parent, "Harbor");
        }

        // Ensure HarborSceneUIManager exists on root
        var mgr = sel.GetComponent<HarborSceneUIManager>();
        if (!mgr) mgr = sel.AddComponent<HarborSceneUIManager>();

        // Auto-bind serialized fields by common paths under the selected root
        AutoBindFields(sel, mgr);

        // Wire the four Armor buttons (if present)
        WireArmorButtons(sel, mgr);

        // Save as prefab
        var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(sel, PrefabPath, InteractionMode.UserAction);
        if (prefab)
        {
            Debug.Log($"[Harbor] Prefab created: {PrefabPath}");
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(prefab);
        }
        else
        {
            Debug.LogError("[Harbor] Failed to create prefab. Check Console for errors.");
        }
    }

    private static void AutoBindFields(GameObject root, HarborSceneUIManager mgr)
    {
        var so = new SerializedObject(mgr);

        // Lookups by relative path (adjusts only if null to preserve manual bindings)
        SetIfNullGO (so, "armorPanelRoot",  FindGO(root, "GROUP_UpgradeInfo"));
        SetIfNullTMP(so, "currentLevelLabel",  FindTMP(root, "GROUP_UpgradeInfo/TXT_CurrentLevel"));
        SetIfNullTMP(so, "currentStatsLabel",  FindTMP(root, "GROUP_UpgradeInfo/TXT_CurrentStats"));
        SetIfNullTMP(so, "nextNameLabel",      FindTMP(root, "GROUP_UpgradeInfo/TXT_NextName"));
        SetIfNullTMP(so, "nextDescLabel",      FindTMP(root, "GROUP_UpgradeInfo/LABEL_UpgradeDescription/TXT_NextDescription"));
        SetIfNullTMP(so, "nextCostLabel",      FindTMP(root, "GROUP_UpgradeInfo/TXT_NextCost"));
        SetIfNullTMP(so, "nextGainsLabel",     FindTMP(root, "GROUP_UpgradeInfo/TXT_NextGains"));
        SetIfNullBtn(so, "upgradeButton",      FindBtn(root, "GROUP_UpgradeInfo/BUTTON_Upgrade"));

        so.ApplyModifiedProperties();
    }

    private static void WireArmorButtons(GameObject root, HarborSceneUIManager mgr)
    {
        // Tries to locate four Armor buttons under GROUP_Left by name contains
        var left = FindGO(root, "GROUP_Left");
        if (!left) return;

        TryBindArmor(left, mgr, "Submarine", mgr.SubmarineArmorUpgradePressed);
        TryBindArmor(left, mgr, "Destroyer", mgr.DestroyerArmorUpgradePressed);
        TryBindArmor(left, mgr, "Cruiser",   mgr.CruiserArmorUpgradePressed);
        TryBindArmor(left, mgr, "Battleship",mgr.BattleshipArmorUpgradePressed);
    }

    private static void TryBindArmor(GameObject left, HarborSceneUIManager mgr, string shipKey, UnityEngine.Events.UnityAction action)
    {
        // Find a Button whose hierarchy name contains both shipKey and "Armor"
        var btn = left.GetComponentsInChildren<Button>(true)
                      .FirstOrDefault(b =>
                      {
                          var path = GetPath(b.transform, left.transform);
                          var n = path.ToLowerInvariant();
                          return n.Contains(shipKey.ToLowerInvariant()) && n.Contains("armor");
                      });

        if (btn == null) return;

        // Avoid duplicates
        var onClick = btn.onClick;
        bool already = false;
        for (int i = 0; i < onClick.GetPersistentEventCount(); i++)
        {
            var target = onClick.GetPersistentTarget(i) as Object;
            var method = onClick.GetPersistentMethodName(i);
            if (target == mgr && method == action.Method.Name) { already = true; break; }
        }
        if (!already)
        {
            UnityEventTools.AddPersistentListener(onClick, action);
            EditorUtility.SetDirty(btn);
        }
    }

    // --- Helpers to find objects/components under root ---

    private static GameObject FindGO(GameObject root, string relPath)
    {
        var t = root.transform.Find(relPath);
        return t ? t.gameObject : null;
    }
    private static TMP_Text FindTMP(GameObject root, string relPath)
    {
        var go = FindGO(root, relPath);
        return go ? go.GetComponent<TMP_Text>() : null;
    }
    private static Button FindBtn(GameObject root, string relPath)
    {
        var go = FindGO(root, relPath);
        return go ? go.GetComponent<Button>() : null;
    }

    // --- Serialized setters (set only if property is null) ---

    private static void SetIfNullGO(SerializedObject so, string propName, GameObject go)
    {
        if (!go) return;
        var p = so.FindProperty(propName);
        if (p != null && p.objectReferenceValue == null) p.objectReferenceValue = go;
    }
    private static void SetIfNullTMP(SerializedObject so, string propName, TMP_Text t)
    {
        if (!t) return;
        var p = so.FindProperty(propName);
        if (p != null && p.objectReferenceValue == null) p.objectReferenceValue = t;
    }
    private static void SetIfNullBtn(SerializedObject so, string propName, Button b)
    {
        if (!b) return;
        var p = so.FindProperty(propName);
        if (p != null && p.objectReferenceValue == null) p.objectReferenceValue = b;
    }

    // Debug helper: get relative path from root
    private static string GetPath(Transform t, Transform stopAt)
    {
        var path = t.name;
        while (t.parent && t.parent != stopAt)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
#endif

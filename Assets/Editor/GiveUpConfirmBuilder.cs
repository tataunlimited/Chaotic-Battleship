#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;

// TextMeshPro
using TMPro;

public static class GiveUpConfirmBuilder
{
    private const string PrefabPath = "Assets/Prefabs/UI/GiveUpConfirm.prefab";

    [MenuItem("Tools/Give Up Modal/Create Prefab Asset", priority = 10)]
    public static void CreatePrefabAsset()
    {
        EnsureFolders("Assets/Prefabs", "Assets/Prefabs/UI");

        var root = BuildModalGO();
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        if (prefab != null)
        {
            Debug.Log($"[GiveUpConfirm] Prefab created at {PrefabPath}");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        else
            Debug.LogError("[GiveUpConfirm] Failed to create prefab.");
    }

    [MenuItem("Tools/Give Up Modal/Place In Scene", priority = 11)]
    public static void PlaceInScene()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            Debug.LogWarning("[GiveUpConfirm] Prefab not found. Creating it now…");
            CreatePrefabAsset();
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null) { Debug.LogError("[GiveUpConfirm] Could not create/load prefab."); return; }
        }

        var hudPause = GameObject.Find("HUD_Pause");
        if (hudPause == null)
        {
            Debug.LogError("[GiveUpConfirm] Could not find 'HUD_Pause' in the scene.");
            return;
        }

        // Avoid duplicates
        var existing = hudPause.transform
            .GetComponentsInChildren<Transform>(true)
            .FirstOrDefault(t => t.name == "GiveUpConfirm");
        if (existing != null)
        {
            Debug.Log("[GiveUpConfirm] Instance already exists. Selecting it.");
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = "GiveUpConfirm";
        instance.transform.SetParent(hudPause.transform, false);
        instance.transform.SetAsLastSibling();
        instance.SetActive(false);

        AutoWire(instance);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = instance;
        Debug.Log("[GiveUpConfirm] Placed in scene and wired.");
    }

    // ------------ builders ------------

    private static GameObject BuildModalGO()
    {
        var root = new GameObject("GiveUpConfirm");
        var rect = root.AddComponent<RectTransform>();
        StretchFull(rect);

        var canvas = root.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;
        root.AddComponent<GraphicRaycaster>();
        root.AddComponent<CanvasGroup>();

        // Scrim
        var scrim = CreateUIObject("Scrim", root.transform, out RectTransform scrimRT);
        var imgScrim = scrim.AddComponent<Image>();
        imgScrim.color = new Color(0f, 0f, 0f, 0.62f);
        imgScrim.raycastTarget = true;
        StretchFull(scrimRT);

        // Panel
        var panel = CreateUIObject("Panel", root.transform, out RectTransform panelRT);
        var imgPanel = panel.AddComponent<Image>();
        imgPanel.color = new Color(1f, 1f, 1f, 0.06f);
        panelRT.anchorMin = panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.sizeDelta = new Vector2(520, 300);
        panelRT.anchoredPosition = Vector2.zero;

        var vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.padding = new RectOffset(24, 24, 24, 24);
        vlg.spacing = 12f;
        var fitter = panel.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Title
        CreateTMPText("Title", panel.transform,
            "Give up and return to Main Menu?", 28, FontWeight.SemiBold, TextAlignmentOptions.Center);

        // Body
        CreateTMPText("Body", panel.transform,
            "This will erase progress for this run.", 20, FontWeight.Regular, TextAlignmentOptions.Center);

        // Buttons row
        var row = CreateUIObject("Row", panel.transform, out RectTransform rowRT);
        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 16f; hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;

        // Yes
        var yes = CreateButton("YesButton", row.transform, "Yes, give up");
        SetLayoutPreferred(yes.GetComponent<RectTransform>(), 220, 56);

        // No
        var no = CreateButton("NoButton", row.transform, "Cancel");
        SetLayoutPreferred(no.GetComponent<RectTransform>(), 220, 56);

        return root;
    }

    private static GameObject CreateUIObject(string name, Transform parent, out RectTransform rt)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        rt = go.AddComponent<RectTransform>();
        return go;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
    }

    private static GameObject CreateTMPText(string name, Transform parent, string text,
                                            int size, FontWeight weight, TextAlignmentOptions align)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontWeight = weight;                     // <-- fix: use FontWeight, not FontStyles.SemiBold
        tmp.alignment = align;
        tmp.textWrappingMode = TextWrappingModes.Normal; // <-- fix: replaces enableWordWrapping
        return go;
    }

    private static GameObject CreateButton(string name, Transform parent, string label)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.12f);

        var btn = go.AddComponent<Button>();
        btn.transition = Selectable.Transition.ColorTint;

        // Label
        var txtGO = new GameObject("Text (TMP)");
        txtGO.transform.SetParent(go.transform, false);
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;

        // Center the label
        var t = tmp.rectTransform;
        t.anchorMin = t.anchorMax = new Vector2(0.5f, 0.5f);
        t.anchoredPosition = Vector2.zero;

        return go;
    }

    private static void SetLayoutPreferred(RectTransform rt, float w, float h)
    {
        var le = rt.GetComponent<LayoutElement>();
        if (le == null) le = rt.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = w; le.preferredHeight = h;
    }

    private static void AutoWire(GameObject giveUpInstance)
    {
        // Find GameMenuActions (2022.2+). Fallback keeps older LTS happy.
        GameMenuActions gma = null;
#if UNITY_2022_2_OR_NEWER
        gma = Object.FindFirstObjectByType<GameMenuActions>();
#else
        gma = Object.FindObjectOfType<GameMenuActions>();
#endif
        if (gma == null)
        {
            Debug.LogWarning("[GiveUpConfirm] GameMenuActions not found. Assign 'giveUpConfirmPanel' manually.");
            return;
        }

        // Assign panel reference
        var so = new SerializedObject(gma);
        var panelProp = so.FindProperty("giveUpConfirmPanel");
        if (panelProp != null)
        {
            panelProp.objectReferenceValue = giveUpInstance;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Wire buttons (persistent UnityActions)
        var yes = giveUpInstance.transform.Find("Panel/Row/YesButton")?.GetComponent<Button>();
        var no  = giveUpInstance.transform.Find("Panel/Row/NoButton")?.GetComponent<Button>();

        if (yes != null)
        {
            yes.onClick.RemoveAllListeners();
            UnityAction yesAction = gma.OnGiveUpYes;                 // <-- correct signature
            UnityEventTools.AddPersistentListener(yes.onClick, yesAction);
            EditorUtility.SetDirty(yes);
        }

        if (no != null)
        {
            no.onClick.RemoveAllListeners();
            UnityAction noAction = gma.OnGiveUpNo;                   // <-- correct signature
            UnityEventTools.AddPersistentListener(no.onClick, noAction);
            EditorUtility.SetDirty(no);
        }
    }

    private static void EnsureFolders(params string[] paths)
    {
        foreach (var path in paths)
        {
            if (AssetDatabase.IsValidFolder(path)) continue;
            var parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            var leaf = System.IO.Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif

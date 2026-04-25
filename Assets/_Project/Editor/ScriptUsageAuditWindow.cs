using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class ScriptUsageAuditWindow : EditorWindow
{
    private const string ProjectScriptsRoot = "Assets/_Project/Scripts";
    private const string PrefabSearchRoot = "Assets/_Project/Prefabs";
    private const string MenuPath = "Tools/Project/Script Usage Audit";

    private readonly List<ScriptUsageRow> rows = new List<ScriptUsageRow>();
    private Vector2 scrollPosition;
    private string activeScenePath;
    private string report = "Click Scan to audit project scripts.";

    [MenuItem(MenuPath)]
    public static void Open()
    {
        GetWindow<ScriptUsageAuditWindow>("Script Usage Audit");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Script Usage Audit", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Scans project scripts and reports whether component scripts are referenced by any project prefab or the active scene. ScriptableObject and utility/base scripts are categorized separately.",
            MessageType.Info);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Scan", GUILayout.Width(120f)))
                Scan();

            GUI.enabled = rows.Count > 0;
            if (GUILayout.Button("Copy Report", GUILayout.Width(120f)))
                EditorGUIUtility.systemCopyBuffer = report;
            GUI.enabled = true;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Active Scene", activeScenePath ?? SceneManager.GetActiveScene().path);
        EditorGUILayout.LabelField("Prefab Root", PrefabSearchRoot);
        EditorGUILayout.LabelField("Script Root", ProjectScriptsRoot);
        EditorGUILayout.Space();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        EditorGUILayout.TextArea(report, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    private void Scan()
    {
        rows.Clear();

        Scene activeScene = SceneManager.GetActiveScene();
        activeScenePath = activeScene.path;

        if (string.IsNullOrWhiteSpace(activeScenePath))
        {
            report = "The active scene has not been saved yet, so it cannot be scanned from disk.";
            Repaint();
            return;
        }

        if (activeScene.isDirty &&
            !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            report = "Scan canceled. Save the active scene first so the audit reads current serialized references.";
            Repaint();
            return;
        }

        Dictionary<string, List<string>> referencesByGuid = BuildReferenceIndex(activeScenePath);
        string[] scriptGuids = AssetDatabase.FindAssets("t:MonoScript", new[] { ProjectScriptsRoot });

        foreach (string guid in scriptGuids)
        {
            string scriptPath = AssetDatabase.GUIDToAssetPath(guid);
            MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);

            if (monoScript == null)
                continue;

            Type scriptType = monoScript.GetClass();
            ScriptKind kind = GetScriptKind(scriptType);
            referencesByGuid.TryGetValue(guid, out List<string> locations);

            rows.Add(new ScriptUsageRow
            {
                Name = Path.GetFileNameWithoutExtension(scriptPath),
                Path = scriptPath,
                Kind = kind,
                Locations = locations ?? new List<string>()
            });
        }

        rows.Sort((left, right) =>
        {
            int kindCompare = left.Kind.CompareTo(right.Kind);
            return kindCompare != 0
                ? kindCompare
                : string.Compare(left.Path, right.Path, StringComparison.Ordinal);
        });

        report = BuildReport();
        Repaint();
    }

    private static Dictionary<string, List<string>> BuildReferenceIndex(string activeScenePath)
    {
        Dictionary<string, List<string>> referencesByGuid = new Dictionary<string, List<string>>();
        List<string> scannedAssetPaths = new List<string> { activeScenePath };

        scannedAssetPaths.AddRange(
            AssetDatabase.FindAssets("t:Prefab", new[] { PrefabSearchRoot })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => !string.IsNullOrWhiteSpace(path)));

        foreach (string assetPath in scannedAssetPaths.Distinct())
        {
            if (!File.Exists(assetPath))
                continue;

            foreach (string guid in ExtractScriptGuids(File.ReadAllText(assetPath)))
            {
                if (!referencesByGuid.TryGetValue(guid, out List<string> locations))
                {
                    locations = new List<string>();
                    referencesByGuid.Add(guid, locations);
                }

                locations.Add(assetPath);
            }
        }

        return referencesByGuid;
    }

    private static IEnumerable<string> ExtractScriptGuids(string serializedAsset)
    {
        const string token = "m_Script: {fileID: 11500000, guid: ";
        int searchIndex = 0;

        while (searchIndex < serializedAsset.Length)
        {
            int tokenIndex = serializedAsset.IndexOf(token, searchIndex, StringComparison.Ordinal);
            if (tokenIndex < 0)
                yield break;

            int guidStart = tokenIndex + token.Length;
            if (guidStart + 32 <= serializedAsset.Length)
                yield return serializedAsset.Substring(guidStart, 32);

            searchIndex = guidStart + 32;
        }
    }

    private static ScriptKind GetScriptKind(Type scriptType)
    {
        if (scriptType == null)
            return ScriptKind.CompileIssueOrNoClass;

        if (scriptType.IsAbstract || scriptType.IsInterface)
            return ScriptKind.UtilityOrBase;

        if (typeof(MonoBehaviour).IsAssignableFrom(scriptType))
            return ScriptKind.Component;

        if (typeof(ScriptableObject).IsAssignableFrom(scriptType))
            return ScriptKind.ScriptableObject;

        return ScriptKind.UtilityOrBase;
    }

    private string BuildReport()
    {
        StringBuilder builder = new StringBuilder();
        IReadOnlyList<ScriptUsageRow> unusedComponents = rows
            .Where(row => row.Kind == ScriptKind.Component && row.Locations.Count == 0)
            .ToArray();

        IReadOnlyList<ScriptUsageRow> usedComponents = rows
            .Where(row => row.Kind == ScriptKind.Component && row.Locations.Count > 0)
            .ToArray();

        builder.AppendLine("SCRIPT USAGE AUDIT");
        builder.AppendLine($"Active scene: {activeScenePath}");
        builder.AppendLine($"Prefabs scanned under: {PrefabSearchRoot}");
        builder.AppendLine($"Scripts scanned under: {ProjectScriptsRoot}");
        builder.AppendLine();
        builder.AppendLine($"Component scripts used: {usedComponents.Count}");
        builder.AppendLine($"Component scripts not found in scanned prefabs/active scene: {unusedComponents.Count}");
        builder.AppendLine();

        AppendSection(builder, "COMPONENT SCRIPTS NOT FOUND", unusedComponents, includeLocations: false);
        AppendSection(builder, "COMPONENT SCRIPTS FOUND", usedComponents, includeLocations: true);
        AppendSection(builder, "SCRIPTABLEOBJECT SCRIPTS", rows.Where(row => row.Kind == ScriptKind.ScriptableObject), includeLocations: false);
        AppendSection(builder, "UTILITY / BASE SCRIPTS", rows.Where(row => row.Kind == ScriptKind.UtilityOrBase), includeLocations: false);
        AppendSection(builder, "COMPILE ISSUE OR NO CLASS", rows.Where(row => row.Kind == ScriptKind.CompileIssueOrNoClass), includeLocations: false);

        return builder.ToString();
    }

    private static void AppendSection(
        StringBuilder builder,
        string title,
        IEnumerable<ScriptUsageRow> sectionRows,
        bool includeLocations)
    {
        ScriptUsageRow[] materializedRows = sectionRows.ToArray();
        builder.AppendLine(title);

        if (materializedRows.Length == 0)
        {
            builder.AppendLine("  None");
            builder.AppendLine();
            return;
        }

        foreach (ScriptUsageRow row in materializedRows)
        {
            builder.AppendLine($"  - {row.Name}");
            builder.AppendLine($"    {row.Path}");

            if (!includeLocations)
                continue;

            foreach (string location in row.Locations.Distinct().OrderBy(path => path))
                builder.AppendLine($"    used by: {location}");
        }

        builder.AppendLine();
    }

    private enum ScriptKind
    {
        Component,
        ScriptableObject,
        UtilityOrBase,
        CompileIssueOrNoClass
    }

    private sealed class ScriptUsageRow
    {
        public string Name;
        public string Path;
        public ScriptKind Kind;
        public List<string> Locations;
    }
}

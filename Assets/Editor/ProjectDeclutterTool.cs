using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class ProjectDeclutterTool : EditorWindow
{
    private List<SceneAsset> rootScenes = new List<SceneAsset>();
    private string legacyFolderPath = "Assets/Legacy_Assets";
    private string undoFilePath = "Assets/Legacy_Assets/Undo_Declutter.json";

    private List<string> excludedFolders = new List<string>()
    {
        "Assets/TextMesh Pro",
        "Assets/Jose Arriagada",
        "Assets/Standard Assets",
        "Packages/",
        "ProjectSettings/",
        "Assets/AfectiveIntegration"
    };

    private Vector2 scrollPos;

    [MenuItem("Tools/Declutter Project")]
    public static void ShowWindow()
    {
        GetWindow<ProjectDeclutterTool>("Declutter Project");
    }

    private void OnEnable()
    {
        RefreshScenes();
    }

    private void RefreshScenes()
    {
        rootScenes.Clear();
        // Auto-populate with all scenes, excluding Legacy folder and Excluded Folders
        string[] allScenes = AssetDatabase.FindAssets("t:SceneAsset");
        foreach (string guid in allScenes)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            bool isExcluded = path.StartsWith(legacyFolderPath);
            foreach (string excl in excludedFolders)
            {
                if (path.StartsWith(excl)) { isExcluded = true; break; }
            }
            if (isExcluded) continue;

            SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            if (scene != null && !rootScenes.Contains(scene))
            {
                rootScenes.Add(scene);
            }
        }
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        GUILayout.Label("Project Declutter Tool", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This tool finds assets that are not dependencies of the listed Root Scenes and moves them to the Output folder.", MessageType.Info);

        GUILayout.Space(10);
        GUILayout.Label("Settings", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox("Output folder must start with 'Assets/'. If you want it outside of the project entirely, you must manually move it from your File Explorer after decluttering.", MessageType.Warning);
        legacyFolderPath = EditorGUILayout.TextField("Output Folder", legacyFolderPath);
        undoFilePath = legacyFolderPath + "/Undo_Declutter.json";

        GUILayout.Space(5);
        GUILayout.Label("Excluded Folders (Will not be moved):", EditorStyles.boldLabel);
        for (int i = 0; i < excludedFolders.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            excludedFolders[i] = EditorGUILayout.TextField(excludedFolders[i]);
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                excludedFolders.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }
        if (GUILayout.Button("Add Excluded Folder"))
        {
            excludedFolders.Add("Assets/NewFolder");
        }

        GUILayout.Space(10);
        GUILayout.Label("Root Scenes (Dependencies of these will NOT be moved):", EditorStyles.boldLabel);
        for (int i = 0; i < rootScenes.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            rootScenes[i] = (SceneAsset)EditorGUILayout.ObjectField(rootScenes[i], typeof(SceneAsset), false);
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                rootScenes.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add Scene"))
        {
            rootScenes.Add(null);
        }
        if (GUILayout.Button("Refresh Scenes List"))
        {
            RefreshScenes();
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Declutter Unused Assets", GUILayout.Height(30)))
        {
            Declutter();
        }

        GUILayout.Space(10);

        if (File.Exists(undoFilePath))
        {
            if (GUILayout.Button("Revert Declutter (Undo)", GUILayout.Height(30)))
            {
                RevertDeclutter();
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void Declutter()
    {
        if (rootScenes.Count == 0 || rootScenes.All(s => s == null))
        {
            EditorUtility.DisplayDialog("Error", "Please add at least one root scene.", "OK");
            return;
        }

        legacyFolderPath = legacyFolderPath.Replace("\\", "/").Trim();
        if (!legacyFolderPath.StartsWith("Assets/"))
        {
            legacyFolderPath = "Assets/" + legacyFolderPath.TrimStart('/');
        }

        if (legacyFolderPath == "Assets" || legacyFolderPath == "Assets/")
        {
            EditorUtility.DisplayDialog("Error", "Invalid output folder. It must be a subfolder, e.g., 'Assets/Legacy_Assets'", "OK");
            return;
        }

        List<string> rootPaths = rootScenes.Where(s => s != null).Select(s => AssetDatabase.GetAssetPath(s)).ToList();

        // Collect all dependencies
        string[] dependencies = AssetDatabase.GetDependencies(rootPaths.ToArray(), true);
        HashSet<string> usedAssets = new HashSet<string>(dependencies);

        // Find all assets
        string[] allAssetGUIDs = AssetDatabase.FindAssets("");
        List<string> assetsToMove = new List<string>();

        foreach (string guid in allAssetGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // Skip folders
            if (AssetDatabase.IsValidFolder(path)) continue;

            // Safe exclusions: Scripts, Editor folders, Resources, StreamingAssets
            if (path.EndsWith(".cs") ||
                path.Contains("/Editor/") ||
                path.Contains("/Resources/") ||
                path.Contains("/StreamingAssets/") ||
                path.StartsWith(legacyFolderPath))
            {
                continue;
            }

            // Check dynamic excluded folders
            bool isExcluded = false;
            foreach (string excl in excludedFolders)
            {
                if (path.StartsWith(excl)) { isExcluded = true; break; }
            }
            if (isExcluded) continue;

            if (!usedAssets.Contains(path))
            {
                assetsToMove.Add(path);
            }
        }

        if (assetsToMove.Count == 0)
        {
            EditorUtility.DisplayDialog("Result", "No unused assets found.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Confirm", $"Found {assetsToMove.Count} unused assets. Move them to {legacyFolderPath}?", "Yes", "Cancel"))
        {
            return;
        }

        if (!AssetDatabase.IsValidFolder(legacyFolderPath))
        {
            string parent = Path.GetDirectoryName(legacyFolderPath).Replace("\\", "/");
            string folderName = Path.GetFileName(legacyFolderPath);
            if (string.IsNullOrEmpty(parent)) parent = "Assets";
            AssetDatabase.CreateFolder("Assets", folderName);
        }

        Dictionary<string, string> moveRecords = new Dictionary<string, string>();

        int count = 0;
        foreach (string path in assetsToMove)
        {
            EditorUtility.DisplayProgressBar("Moving Assets", path, (float)count / assetsToMove.Count);

            // Maintain folder structure inside Legacy
            string relativePath = path.Substring("Assets/".Length);
            string newPath = legacyFolderPath + "/" + relativePath;

            string newDir = Path.GetDirectoryName(newPath);
            if (!Directory.Exists(newDir))
            {
                Directory.CreateDirectory(newDir);
                AssetDatabase.Refresh();
            }

            string error = AssetDatabase.MoveAsset(path, newPath);
            if (string.IsNullOrEmpty(error))
            {
                moveRecords.Add(newPath, path);
            }
            else
            {
                Debug.LogWarning($"Failed to move {path}: {error}");
            }
            count++;
        }

        EditorUtility.ClearProgressBar();

        // Cleanup empty folders left behind
        CleanupEmptyFolders("Assets");

        // Save undo instructions
        string json = JsonUtility.ToJson(new Serialization<string, string>(moveRecords), true);
        File.WriteAllText(undoFilePath, json);
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Complete", $"Moved {moveRecords.Count} assets to {legacyFolderPath}.", "OK");
    }

    private void CleanupEmptyFolders(string startPath)
    {
        if (!Directory.Exists(startPath)) return;
        string[] allFolders = Directory.GetDirectories(startPath, "*", SearchOption.AllDirectories);
        var sortedDirs = allFolders.OrderByDescending(d => d.Length).ToList();

        foreach (string dir in sortedDirs)
        {
            string assetPath = dir.Replace("\\", "/");

            bool isExcluded = false;
            foreach (string excl in excludedFolders)
            {
                if (assetPath.StartsWith(excl)) { isExcluded = true; break; }
            }
            if (isExcluded) continue;

            if (Directory.GetFiles(dir).Length == 0 && Directory.GetDirectories(dir).Length == 0)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }
        }
    }

    private void RevertDeclutter()
    {
        if (!File.Exists(undoFilePath)) return;

        string json = File.ReadAllText(undoFilePath);
        Serialization<string, string> moveRecords = JsonUtility.FromJson<Serialization<string, string>>(json);

        if (moveRecords == null || moveRecords.targetKeys == null)
        {
            EditorUtility.DisplayDialog("Error", "Undo file is corrupted.", "OK");
            return;
        }

        int count = 0;
        for (int i = 0; i < moveRecords.targetKeys.Count; i++)
        {
            string currentPath = moveRecords.targetKeys[i];
            string originalPath = moveRecords.targetValues[i];

            EditorUtility.DisplayProgressBar("Reverting Assets", originalPath, (float)i / moveRecords.targetKeys.Count);

            string originalDir = Path.GetDirectoryName(originalPath);
            if (!Directory.Exists(originalDir))
            {
                Directory.CreateDirectory(originalDir);
                AssetDatabase.Refresh();
            }

            string error = AssetDatabase.MoveAsset(currentPath, originalPath);
            if (string.IsNullOrEmpty(error))
            {
                count++;
            }
            else
            {
                Debug.LogWarning($"Failed to revert {currentPath}: {error}");
            }
        }

        EditorUtility.ClearProgressBar();

        // Delete undo file
        File.Delete(undoFilePath);
        if (Directory.Exists(legacyFolderPath) && Directory.GetFiles(legacyFolderPath, "*", SearchOption.AllDirectories).Length == 0)
        {
            AssetDatabase.DeleteAsset(legacyFolderPath);
        }

        CleanupEmptyFolders("Assets");

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Complete", $"Reverted {count} assets to their original locations.", "OK");
    }

    // Helper for Dictionary serialization
    [System.Serializable]
    public class Serialization<TKey, TValue> : ISerializationCallbackReceiver
    {
        [SerializeField]
        public List<TKey> targetKeys;
        [SerializeField]
        public List<TValue> targetValues;

        private Dictionary<TKey, TValue> target;
        public Dictionary<TKey, TValue> ToDictionary() { return target; }

        public Serialization(Dictionary<TKey, TValue> target)
        {
            this.target = target;
            targetKeys = new List<TKey>(target.Keys);
            targetValues = new List<TValue>(target.Values);
        }

        public void OnBeforeSerialize()
        {
            targetKeys = new List<TKey>(target.Keys);
            targetValues = new List<TValue>(target.Values);
        }

        public void OnAfterDeserialize()
        {
            var count = Mathf.Min(targetKeys.Count, targetValues.Count);
            target = new Dictionary<TKey, TValue>(count);
            for (var i = 0; i < count; ++i)
            {
                target.Add(targetKeys[i], targetValues[i]);
            }
        }
    }
}

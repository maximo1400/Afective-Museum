#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;

public class AssetBundleCreator : MonoBehaviour
{
    [MenuItem("Assets/Build Asset Bundle")]
    static void BuildBundles()
    {
        string dir = "Assets/StreamingAssets";
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        
        // Build bundles for the current active platform (e.g. Standalone or WebGL)
        BuildPipeline.BuildAssetBundles(dir, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
        Debug.Log("AssetBundles successfully built to " + dir);
    }
}
#endif
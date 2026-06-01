using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class ForceDisableBakeOnLoad
{
    static ForceDisableBakeOnLoad()
    {
        EditorApplication.playModeStateChanged += (state) =>
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                Lightmapping.bakeOnSceneLoad = Lightmapping.BakeOnSceneLoadMode.Never;
#pragma warning disable 0618
                Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
#pragma warning restore 0618
            }
        };
    }

    [MenuItem("Tools/Force Disable All Auto-Baking")]
    public static void ForceDisable()
    {
        Lightmapping.bakeOnSceneLoad = Lightmapping.BakeOnSceneLoadMode.Never;
#pragma warning disable 0618
        Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
#pragma warning restore 0618
        
        Lightmapping.Cancel();
        Lightmapping.Clear();
        Lightmapping.ClearDiskCache();
        
        Debug.Log("<color=green><b>Nuked all Auto-Baking logic and cleared the queue globally!</b></color>");
    }
}

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[InitializeOnLoad]
public class UpgradeInputSystemFix
{
    static UpgradeInputSystemFix()
    {
        try
        {
            // Approach 1: Use SerializedObject to modify PlayerSettings (bypasses missing property API)
            var playerSettings = Resources.FindObjectsOfTypeAll<PlayerSettings>();
            if (playerSettings.Length > 0)
            {
                var so = new SerializedObject(playerSettings[0]);
                var activeInputHandlerProp = so.FindProperty("activeInputHandler");
                if (activeInputHandlerProp != null)
                {
                    if (activeInputHandlerProp.intValue != 1)
                    {
                        Debug.Log("Enabling new Input System via SerializedObject...");
                        activeInputHandlerProp.intValue = 1; // 1 = Input System Package
                        so.ApplyModifiedProperties();
                        AssetDatabase.SaveAssets();
                        Debug.Log("Successfully switched to the new Input System via SerializedObject.");
                    }
                    return; // Done
                }
            }

            // Approach 2: Use Reflection to access the internal EditorPlayerSettingHelpers
            Type helperType = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.GetName().Name == "Unity.InputSystem.Editor")
                {
                    helperType = asm.GetType("UnityEngine.InputSystem.Editor.EditorPlayerSettingHelpers");
                    break;
                }
            }

            if (helperType != null)
            {
                var newProp = helperType.GetProperty("newSystemBackendsEnabled", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                var oldProp = helperType.GetProperty("oldSystemBackendsEnabled", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                if (newProp != null && oldProp != null)
                {
                    bool newEnabled = (bool)newProp.GetValue(null);
                    bool oldEnabled = (bool)oldProp.GetValue(null);

                    if (!newEnabled || oldEnabled)
                    {
                        Debug.Log("Enabling new Input System via Reflection...");
                        newProp.SetValue(null, true);
                        oldProp.SetValue(null, false);
                        Debug.Log("Successfully switched to the new Input System via Reflection.");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to update Input System settings programmatically: " + e.Message);
        }
    }
}
#endif

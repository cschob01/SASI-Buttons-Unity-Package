#if UNITY_EDITOR
using System.Reflection;
using Unity.SASIButtons;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System;

[InitializeOnLoad]
public static class SASIButtons_Cleanup
{
    static SASIButtons_Cleanup()
    {
        EditorApplication.delayCall += Cleanup;
    }

    private static void Cleanup()
    {
        foreach (SASIButtons_ButtonData data in UnityEngine.Object.FindObjectsByType<SASIButtons_ButtonData>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None))
        {
            if (data.GetComponent<Button>() == null)
            {
                Debug.Log("[SASIButtons_Cleanup] Found ButtonData component with no button. Destroying...");
                UnityEngine.Object.DestroyImmediate(data);
            }

            // Validate calls
            foreach (SASIButtons_Call call in data.calls)
            {
                Type type = Type.GetType(call.typeName);

                if (type == null)
                {
                    LogMissingLink(data, call);
                    continue;
                }

                MethodInfo method = type.GetMethod(
                    call.methodName,
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Static |
                    BindingFlags.Instance
                );

                if (method == null)
                {
                    LogMissingLink(data, call);
                    continue;
                }
            }

            EditorUtility.SetDirty(data);
        }
    }

    private static void LogMissingLink(
    SASIButtons_ButtonData data,
    SASIButtons_Call call)
    {
        Debug.LogWarning(
            $"[SASIButtons_Cleanup] " +
            $"Lost link existing on button component of {data.gameObject.name}. Please fix.\n" +
            $"    {call.typeName}:{call.methodName}\n" +
            $"Please fix this reference.",
            data.gameObject
        );
    }
}
#endif
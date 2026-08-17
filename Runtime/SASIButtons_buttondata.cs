using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.SASIButtons
{
    [AddComponentMenu("")]
    public class SASIButtons_ButtonData : MonoBehaviour
    {
        public List<SASIButtons_Call> calls = new();

        private void OnEnable()
        {
            Button button = GetComponent<Button>();

            if (button != null)
                button.onClick.AddListener(InvokeCalls);
        }
        private void OnDisable()
        {
            Button button = GetComponent<Button>();

            if (button != null)
                button.onClick.RemoveListener(InvokeCalls);
        }

        private void InvokeCalls()
        {
            foreach (SASIButtons_Call call in calls)
            {
                Type type = Type.GetType(call.typeName);

                if (type == null)
                {
                    LogMissingLink(this, call);
                    continue;
                }

                MethodInfo method = type.GetMethod(
                    call.methodName,
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance
                );

                if (method == null)
                {
                    LogMissingLink(this, call);
                    continue;
                }

                ParameterInfo[] methodParameters = method.GetParameters();

                object[] parameters = null;
                if (methodParameters.Length > 1)
                {
                    Debug.LogWarning("[SASIButtons_ButtonData] Cannot call functions with more than one arguements. Skipping call...");
                    continue;
                }
                else if (methodParameters.Length == 1)
                {
                    if (call.param == null)
                    {
                        Debug.LogWarning(
                            "[SASIButtons_ButtonData] Parameter is missing for " +
                            type.Name + "." + method.Name + ". Skipping call..."
                        );
                        continue;
                    }

                    object value = call.param.GetValue();

                    if (value != null && value.GetType() != methodParameters[0].ParameterType)
                    {
                        Debug.LogWarning(
                            "[SASIButtons_ButtonData : DEVELOPER MISTAKE] Serialized parameter type [" +
                            value.GetType() +
                            "] is not what " +
                            type.Name + "." + method.Name +
                            " expects. Skipping call..."
                        );
                        continue;
                    }

                    parameters = new object[] { value };
                }

                if (call.isSingleton)
                {
                    PropertyInfo instanceProperty = type.GetProperty(
                        "Instance",
                        BindingFlags.Public | BindingFlags.Static);

                    if (instanceProperty == null) continue;

                    object instance = instanceProperty.GetValue(null);

                    if (instance is UnityEngine.Object unityObject && unityObject == null)
                    {
                        Debug.LogWarning("[SASIButtons_ButtonData] Singleton instance " + type.Name + "." + method.Name + " not found. Skipping call...");
                        continue;
                    }

                    method.Invoke(instance, parameters);
                }
                else
                {
                    method?.Invoke(null, parameters);
                }
            }
        }

        private static void LogMissingLink(
            SASIButtons_ButtonData data,
            SASIButtons_Call call)
        {
            Debug.LogWarning(
                $"[SASIButtons_ButtonData] " +
                $"Lost link on {data.gameObject.name}:\n" +
                $"    {call.typeName}:{call.methodName}\n" +
                $"Please fix this reference.",
                data.gameObject
            );
        }
    }
}
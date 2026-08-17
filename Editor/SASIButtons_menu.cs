using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections.Generic;
using System.Reflection;

using Unity.SASIButtons;

using UnityEditorInternal;

namespace Unity.SASIButtons.Editor
{
    [CustomEditor(typeof(Button))]

    public class SASIButtons_ButtonEditor : ButtonEditor
    {
        private ReorderableList sasi_List;
        private SASIButtons_ButtonData currentData;

        private void SetupReorderableList(Button button)
        {
            GetOrCreateData(button);

            SerializedObject serializedObject =
                new SerializedObject(currentData);

            SerializedProperty calls =
                serializedObject.FindProperty(nameof(SASIButtons_ButtonData.calls));

            sasi_List = new ReorderableList(
                            serializedObject,
                            calls,
                            true,   // draggable
                            true,   // header
                            true,   // add button
                            true    // remove button
                        );

            sasi_List.elementHeightCallback =
                (int index) =>
                {
                    SerializedProperty call =
                        calls.GetArrayElementAtIndex(index);

                    string typeName =
                        call.FindPropertyRelative(nameof(SASIButtons_Call.typeName)).stringValue;

                    string methodName =
                        call.FindPropertyRelative(nameof(SASIButtons_Call.methodName)).stringValue;

                    Type type = Type.GetType(typeName);

                    if (type == null)
                        return EditorGUIUtility.singleLineHeight * 2 + 6;

                    MethodInfo method = type.GetMethod(
                        methodName,
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Static |
                        BindingFlags.Instance
                    );

                    if (method == null)
                        return EditorGUIUtility.singleLineHeight * 2 + 6;

                    bool hasParam = method.GetParameters().Length == 1;

                    return hasParam
                        ? EditorGUIUtility.singleLineHeight * 2 + 6
                        : EditorGUIUtility.singleLineHeight + 4;
                };

            sasi_List.drawElementCallback =
                (Rect rect, int index, bool active, bool focused) =>
                {
                    SASIButtons_Call sasi_call = currentData.calls[index];

                    SerializedProperty call =
                        calls.GetArrayElementAtIndex(index);

                    string typeName =
                        call.FindPropertyRelative(nameof(SASIButtons_Call.typeName)).stringValue;

                    string methodName =
                        call.FindPropertyRelative(nameof(SASIButtons_Call.methodName)).stringValue;

                    Type type = Type.GetType(typeName);

                    MethodInfo method = null;

                    if (type != null)
                    {
                        method = type.GetMethod(
                            methodName,
                            BindingFlags.Public |
                            BindingFlags.NonPublic |
                            BindingFlags.Static |
                            BindingFlags.Instance
                        );
                    }

                    // Missing link
                    if (type == null || method == null)
                    {
                        rect.y += 2;

                        EditorGUI.LabelField(
                            new Rect(
                                rect.x,
                                rect.y,
                                rect.width,
                                EditorGUIUtility.singleLineHeight
                            ),
                            "Missing Link"
                        );

                        rect.y += EditorGUIUtility.singleLineHeight + 2;

                        EditorGUI.LabelField(
                            new Rect(
                                rect.x,
                                rect.y,
                                rect.width,
                                EditorGUIUtility.singleLineHeight
                            ),
                            $"{typeName} : {methodName}"
                        );

                        return;
                    }

                    bool hasParam =
                        method != null &&
                        method.GetParameters().Length == 1;

                    rect.y += 2;

                    float typeWidth = 150f;

                    // Method
                    EditorGUI.LabelField(
                        new Rect(
                            rect.x,
                            rect.y,
                            typeWidth,
                            EditorGUIUtility.singleLineHeight
                        ),
                        type.Name
                    );

                    EditorGUI.LabelField(
                        new Rect(
                            rect.x + typeWidth + 5,
                            rect.y,
                            rect.width - typeWidth - 5,
                            EditorGUIUtility.singleLineHeight
                        ),
                        methodName + "()"
                    );

                    // String Parameter
                    if (hasParam)
                    {
                        rect.y += EditorGUIUtility.singleLineHeight + 2;

                        SerializedProperty param =
                            call.FindPropertyRelative(nameof(SASIButtons_Call.param));

                        ParameterInfo parameter =
                            method.GetParameters()[0];

                        EditorGUI.LabelField(
                            new Rect(
                                rect.x,
                                rect.y,
                                typeWidth,
                                EditorGUIUtility.singleLineHeight
                            ),
                            parameter.ParameterType.Name
                        );

                        Type paramClass =
                            typeof(SASI_Parameter<>).MakeGenericType(parameter.ParameterType);

                        if (param.managedReferenceValue == null)
                        {
                            Debug.LogWarning("[SASIButtons_menu | DEVELOPER MISTAKE] Parameter not initialized. Fixing...");

                            param.managedReferenceValue =
                                Activator.CreateInstance(paramClass);

                            param.serializedObject.ApplyModifiedProperties();

                            return;
                        }

                        if (param.managedReferenceValue.GetType() != paramClass)
                        {
                            Debug.LogWarning(
                                "[SASIButtons_menu] " +
                                $"Parameter type mismatch. Expected {paramClass}, " +
                                $"found {param.managedReferenceValue.GetType()}. Fixing..."
                            );

                            param.managedReferenceValue =
                                Activator.CreateInstance(paramClass);

                            param.serializedObject.ApplyModifiedProperties();

                            return;
                        }

                        SerializedProperty value =
                            param.FindPropertyRelative(nameof(SASI_Parameter<int>.value));

                        bool canSerialize = value != null;
                        if (value != null && value.propertyType == SerializedPropertyType.ManagedReference)
                        {
                            canSerialize = value.managedReferenceValue != null;
                        }

                        if (canSerialize)
                        {
                            EditorGUI.PropertyField(
                                new Rect(
                                    rect.x + typeWidth + 5,
                                    rect.y,
                                    rect.width - typeWidth - 5,
                                    EditorGUIUtility.singleLineHeight
                                ),
                                value,
                                GUIContent.none
                            );
                        }
                        else
                        {
                            EditorGUI.LabelField(
                                new Rect(
                                    rect.x + typeWidth + 5,
                                    rect.y,
                                    rect.width - typeWidth - 5,
                                    EditorGUIUtility.singleLineHeight
                                ),
                                "Unable to serialize parameter, passing null..."
                            );
                        }
                    }
                };

            sasi_List.drawHeaderCallback =
                (Rect rect) =>
                {
                    EditorGUI.LabelField(
                        rect,
                        "[Static/Singleton] On Click ()"
                    );
                };

            sasi_List.onAddCallback =
                (ReorderableList list) =>
                {
                    ShowFunctionMenu(button);
                };

            sasi_List.onRemoveCallback =
                (ReorderableList list) =>
                {
                    Undo.RecordObject(
                        currentData,
                        "Remove SIPackage Call"
                    );

                    calls.DeleteArrayElementAtIndex(list.index);

                    serializedObject.ApplyModifiedProperties();

                    EditorUtility.SetDirty(currentData);
                };
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            Button button = (Button)target;

            GetOrCreateData(button);

            // Create the list once.
            if (sasi_List == null)
            {
                SetupReorderableList(button);
            }

            sasi_List.serializedProperty.serializedObject.Update();

            sasi_List.DoLayoutList();

            sasi_List.serializedProperty.serializedObject.ApplyModifiedProperties();
        }


        private void ShowFunctionMenu(Button button)
        {
            GenericMenu menu = new GenericMenu();

            // Find static methods
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;

                try { types = assembly.GetTypes(); }
                catch { continue; }

                foreach (Type type in types)
                {
                    AddStaticMethods(menu, button, type);
                    AddSingletonMethods(menu, button, type);
                }
            }

            if (menu.GetItemCount() == 0)
            {
                menu.AddDisabledItem(new GUIContent("No functions found"));
            }

            menu.AddSeparator("");

            menu.AddDisabledItem(
                new GUIContent(
                    "Only classes marked [SASIButtonsCallable] are shown"
                )
            );

            menu.ShowAsContext();
        }

        private void AddStaticMethods(
            GenericMenu menu,
            Button button,
            Type type)
        {
            if (type.GetCustomAttribute<SASIButtonsCallableAttribute>() == null)
                return;

            if (!type.IsClass) return;

            MethodInfo[] methods = type.GetMethods(
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.DeclaredOnly
            );

            foreach (MethodInfo method in methods)
            {
                if (method.IsSpecialName) continue;
                if (method.GetParameters().Length > 1) continue;

                string path = $"Static/{type.FullName}/{method.Name}()";

                menu.AddItem(
                    new GUIContent(path), 
                    false,
                    () => AddCall(button, type, method, false)
                );
            }
        }

        private void AddSingletonMethods(
            GenericMenu menu,
            Button button,
            Type type)
        {
            if (type.GetCustomAttribute<SASIButtonsCallableAttribute>() == null)
                return;

            if (!type.IsClass) return;

            // Look for a public static Instance property.
            PropertyInfo instanceProperty = type.GetProperty(
                "Instance",
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.FlattenHierarchy
            );

            // Or a public static Instance field.
            FieldInfo instanceField = type.GetField(
                "Instance",
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.FlattenHierarchy
            );

            if (instanceProperty == null && instanceField == null) return;

            MethodInfo[] methods = type.GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly
            );

            foreach (MethodInfo method in methods)
            {
                if (method.IsSpecialName)
                    continue;

                if (method.GetParameters().Length > 1)
                    continue;

                string path =
                    $"Singleton/{type.FullName}/{method.Name}()";

                menu.AddItem(
                    new GUIContent(path),
                    false,
                    () => AddCall(button, type, method, true)
                );
            }
        }

        private void AddCall(
            Button button,
            Type type,
            MethodInfo method,
            bool isSingleton)
        {
            GetOrCreateData(button);

            Undo.RecordObject(currentData, "Add SASIButtons Call");

            SASIButtons_Call call = new SASIButtons_Call
            {
                typeName = type.AssemblyQualifiedName,
                methodName = method.Name,
                isSingleton = isSingleton
            };

            ParameterInfo[] parameters = method.GetParameters();

            if (parameters.Length == 1)
            {
                Type parameterType = parameters[0].ParameterType;

                Type parameterClass =
                    typeof(SASI_Parameter<>).MakeGenericType(parameterType);

                call.param =
                    (SASI_Parameter)Activator.CreateInstance(parameterClass);
            }

            currentData.calls.Add(call);

            EditorUtility.SetDirty(currentData);
        }

        private SASIButtons_ButtonData GetOrCreateData(Button button)
        {
            if (currentData != null)
                return currentData;

            currentData = button.GetComponent<SASIButtons_ButtonData>();

            if (currentData == null)
            {
                currentData = Undo.AddComponent<SASIButtons_ButtonData>(
                    button.gameObject
                );

                currentData.hideFlags = HideFlags.HideInInspector;

                EditorUtility.SetDirty(currentData);
            }

            return currentData;
        }
    }
}


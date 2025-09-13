using UnityEditor;
using UnityEngine;
using System.IO;

namespace EnumCreator.Editor
{
    [CustomEditor(typeof(EnumCreator.EnumDefinition))]
    public class EnumDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var def = (EnumCreator.EnumDefinition)target;
            var enumNameProp = serializedObject.FindProperty("enumName");
            var nsProp = serializedObject.FindProperty("namespace");
            var valuesProp = serializedObject.FindProperty("values");
            var removedValuesProp = serializedObject.FindProperty("removedValues");
            var useFlagsProp = serializedObject.FindProperty("useFlags");

            EditorGUILayout.PropertyField(enumNameProp);
            EditorGUILayout.PropertyField(nsProp);
            EditorGUILayout.PropertyField(useFlagsProp, new GUIContent("Use as Flags"));

            EditorGUILayout.Space();

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Cannot edit enum values during Play Mode.", MessageType.Warning);
                GUI.enabled = false;
            }

            EditorGUILayout.LabelField("Values", EditorStyles.boldLabel);

            for (int i = 0; i < valuesProp.arraySize; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                var element = valuesProp.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(element, new GUIContent("Value"));

                // Ensure tooltips list is aligned
                if (def.MutableTooltips.Count <= i)
                    def.MutableTooltips.Add("");

                def.MutableTooltips[i] = EditorGUILayout.TextField("Tooltip", def.MutableTooltips[i]);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    var removedName = element.stringValue;
                    removedValuesProp.arraySize++;
                    removedValuesProp.GetArrayElementAtIndex(removedValuesProp.arraySize - 1).stringValue = removedName;
                    valuesProp.DeleteArrayElementAtIndex(i);
                    def.MutableTooltips.RemoveAt(i); // Remove corresponding tooltip
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }

            // Add button also adds empty tooltip
            if (GUILayout.Button("+ Add Value"))
            {
                valuesProp.InsertArrayElementAtIndex(valuesProp.arraySize);
                def.MutableTooltips.Add("");
            }

            GUI.enabled = true;

            EditorGUILayout.Space();

            // Apply Changes button with validation
            if (GUILayout.Button("Apply Changes"))
            {
                serializedObject.ApplyModifiedProperties();

                // Validate duplicates & empty
                for (int i = 0; i < valuesProp.arraySize; i++)
                {
                    var nameI = valuesProp.GetArrayElementAtIndex(i).stringValue;
                    if (string.IsNullOrWhiteSpace(nameI))
                    {
                        Debug.LogWarning($"Enum '{def.EnumName}': Empty value at index {i}, skipping generation.");
                        return;
                    }

                    for (int j = i + 1; j < valuesProp.arraySize; j++)
                    {
                        var nameJ = valuesProp.GetArrayElementAtIndex(j).stringValue;
                        if (nameI == nameJ)
                        {
                            Debug.LogWarning($"Enum '{def.EnumName}': Duplicate value '{nameI}', skipping generation.");
                            return;
                        }
                    }
                }

                EnumGenerator.Generate(def);
            }

            // Open Generated File button
            if (GUILayout.Button("Open Generated File"))
            {
                string path = Path.Combine("Assets/GeneratedEnums", def.EnumName + ".cs");
                if (File.Exists(path))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                    AssetDatabase.OpenAsset(asset);
                }
                else
                {
                    EditorUtility.DisplayDialog("EnumCreator", "Generated file not found. Apply changes first.", "OK");
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

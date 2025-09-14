using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

namespace EnumCreator.Editor
{
    [CustomEditor(typeof(EnumCreator.EnumDefinition))]
    public class EnumDefinitionEditor : UnityEditor.Editor
    {
        private static readonly Regex ValidIdentifierRegex = new Regex(@"^[_a-zA-Z][_a-zA-Z0-9]*$");

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

                // Record undo
                Undo.RecordObject(def, "Enum Value Change");

                EditorGUI.BeginChangeCheck();
                string oldValue = element.stringValue;
                string newValue = EditorGUILayout.TextField("Value", oldValue);

                // Sanitize input: remove invalid characters
                newValue = SanitizeIdentifier(newValue);

                // Check for duplicates
                bool isDuplicate = false;
                for (int j = 0; j < valuesProp.arraySize; j++)
                {
                    if (j != i && valuesProp.GetArrayElementAtIndex(j).stringValue == newValue)
                    {
                        isDuplicate = true;
                        break;
                    }
                }

                if (EditorGUI.EndChangeCheck())
                {
                    if (isDuplicate)
                    {
                        Debug.LogWarning($"Enum '{def.EnumName}': Duplicate value '{newValue}' not allowed.");
                    }
                    else if (string.IsNullOrWhiteSpace(newValue))
                    {
                        Debug.LogWarning($"Enum '{def.EnumName}': Value cannot be empty.");
                    }
                    else
                    {
                        element.stringValue = newValue;
                        EditorUtility.SetDirty(def);
                    }
                }

                // Ensure tooltips list is aligned
                if (def.MutableTooltips.Count <= i)
                    def.MutableTooltips.Add("");

                // Tooltip field
                Undo.RecordObject(def, "Enum Tooltip Change");
                EditorGUI.BeginChangeCheck();
                def.MutableTooltips[i] = EditorGUILayout.TextField("Tooltip", def.MutableTooltips[i]);
                if (EditorGUI.EndChangeCheck())
                    EditorUtility.SetDirty(def);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    Undo.RecordObject(def, "Enum Value Remove");

                    var removedName = element.stringValue;
                    removedValuesProp.arraySize++;
                    removedValuesProp.GetArrayElementAtIndex(removedValuesProp.arraySize - 1).stringValue = removedName;
                    valuesProp.DeleteArrayElementAtIndex(i);
                    def.MutableTooltips.RemoveAt(i);

                    EditorUtility.SetDirty(def);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }

            // Add button also adds empty tooltip
            if (GUILayout.Button("+ Add Value"))
            {
                Undo.RecordObject(def, "Enum Value Add");

                valuesProp.InsertArrayElementAtIndex(valuesProp.arraySize);
                def.MutableTooltips.Add("");

                EditorUtility.SetDirty(def);
            }

            GUI.enabled = true;

            EditorGUILayout.Space();

            // Apply Changes button with validation
            if (GUILayout.Button("Apply Changes"))
            {
                serializedObject.ApplyModifiedProperties();

                for (int i = 0; i < valuesProp.arraySize; i++)
                {
                    var nameI = valuesProp.GetArrayElementAtIndex(i).stringValue;

                    // Empty check
                    if (string.IsNullOrWhiteSpace(nameI))
                    {
                        Debug.LogWarning($"Enum '{def.EnumName}': Empty value at index {i}, skipping generation.");
                        return;
                    }

                    // Invalid identifier check
                    if (!ValidIdentifierRegex.IsMatch(nameI))
                    {
                        Debug.LogWarning($"Enum '{def.EnumName}': Value '{nameI}' is not a valid C# identifier, skipping generation.");
                        return;
                    }

                    // Duplicate check
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

        private string SanitizeIdentifier(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            // Replace spaces and invalid chars with _
            string sanitized = Regex.Replace(input, @"[^a-zA-Z0-9_]", "_");

            // If starts with a number, prepend _
            if (char.IsDigit(sanitized[0]))
                sanitized = "_" + sanitized;

            return sanitized;
        }
    }
}

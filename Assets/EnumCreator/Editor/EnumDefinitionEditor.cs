using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace EnumCreator.Editor
{
    [CustomEditor(typeof(EnumCreator.EnumDefinition))]
    public class EnumDefinitionEditor : UnityEditor.Editor
    {
        private static readonly Regex ValidIdentifierRegex = new Regex(@"^[_a-zA-Z][_a-zA-Z0-9]*$");
        private bool hasUnsavedChanges_local = false;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var def = (EnumCreator.EnumDefinition)target;
            var settings = EnumCreatorSettingsManager.GetOrCreateSettings();
            var enumNameProp = serializedObject.FindProperty("enumName");
            var nsProp = serializedObject.FindProperty("namespace");
            var valuesProp = serializedObject.FindProperty("values");
            var removedValuesProp = serializedObject.FindProperty("removedValues");
            var useFlagsProp = serializedObject.FindProperty("useFlags");
            

            // Open Generated File button at the very top right
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); // Push button to the right
            
            string generatedPath = settings?.GeneratedEnumsPath ?? "Assets/GeneratedEnums";
            string path = Path.Combine(generatedPath, def.EnumName + ".cs");
            bool fileExists = File.Exists(path);
            
            GUI.enabled = fileExists; // Only enable if file exists
            if (GUILayout.Button("Open Generated File", GUILayout.Width(150)))
            {
                var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                AssetDatabase.OpenAsset(asset);
            }
            GUI.enabled = true; // Re-enable GUI
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5); // Add space between button and enum name

            // Always show enum name as read-only (uses filename)
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Enum Name", "Uses filename as enum name"), GUILayout.Width(85));
            EditorGUILayout.LabelField(def.EnumName, EditorStyles.textField);
            EditorGUILayout.EndHorizontal();
            
            // Update enum name to match filename if it doesn't already
            string fileName = def.name;
            if (!string.IsNullOrEmpty(fileName) && def.EnumName != fileName)
            {
                var enumNameField = typeof(EnumCreator.EnumDefinition).GetField("enumName", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                enumNameField?.SetValue(def, fileName);
                EditorUtility.SetDirty(def);
            }
            
            // Namespace field with tight spacing
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Namespace", GUILayout.Width(85));
            nsProp.stringValue = EditorGUILayout.TextField(nsProp.stringValue);
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
                hasUnsavedChanges_local = true;
            
            // Use as Flags field with tight spacing
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Use as Flags", GUILayout.Width(85));
            useFlagsProp.boolValue = EditorGUILayout.Toggle(useFlagsProp.boolValue);
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
                hasUnsavedChanges_local = true;

            EditorGUILayout.Space(5);

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

                // Enable/Disable checkbox
                Undo.RecordObject(def, "Enum Value Toggle");
                EditorGUI.BeginChangeCheck();
                
                // Check if this value is currently disabled (in removed values)
                bool isEnabled = !def.MutableRemovedValues.Contains(element.stringValue);
                
                EditorGUILayout.BeginHorizontal();
                bool newEnabled = EditorGUILayout.Toggle(isEnabled, GUILayout.Width(20));
                EditorGUILayout.LabelField(isEnabled ? "✓ Enabled" : "✗ Disabled", GUILayout.Width(80));
                EditorGUILayout.EndHorizontal();
                
                if (EditorGUI.EndChangeCheck())
                {
                    if (isEnabled && !newEnabled)
                    {
                        // Disable: move to removed values
                        var valueName = element.stringValue;
                        int originalValue = def.UseFlags ? (1 << i) : i;
                        
                        Debug.Log($"Disabling '{valueName}' at index {i}, preserving value: {originalValue}");
                        
                        removedValuesProp.arraySize++;
                        removedValuesProp.GetArrayElementAtIndex(removedValuesProp.arraySize - 1).stringValue = valueName;
                        def.MutableRemovedValueNumbers.Add(originalValue);
                        
                    }
                    else if (!isEnabled && newEnabled)
                    {
                        // Enable: remove from removed values
                        var valueName = element.stringValue;
                        
                        Debug.Log($"Enabling '{valueName}' - removing from obsolete list");
                        
                        // Remove from removed values using mutable list
                        int removedIndex = def.MutableRemovedValues.IndexOf(valueName);
                        if (removedIndex >= 0)
                        {
                            def.MutableRemovedValues.RemoveAt(removedIndex);
                            def.MutableRemovedValueNumbers.RemoveAt(removedIndex);
                        }
                        
                    }
                    
                    EditorUtility.SetDirty(def);
                    hasUnsavedChanges_local = true;
                }

                // Record undo for value changes
                Undo.RecordObject(def, "Enum Value Change");

                EditorGUI.BeginChangeCheck();
                string oldValue = element.stringValue;
                
                // Check if this specific value should be protected from changes
                // Only protect if it exists in the generated file AND the setting is enabled
                bool preventChanges = false;
                if (settings?.PreventValueNameChanges == true)
                {
                    // Only protect values that exist in the generated enum file
                    var existingValues = EnumValueProtectionHelper.GetExistingEnumValues(def);
                    
                    // Check if this value exists in the generated file
                    bool existsInGeneratedFile = existingValues.Contains(oldValue);
                    
                    // Only protect if it exists in generated file
                    if (existsInGeneratedFile)
                    {
                        // Count how many times this value appears in the current editor
                        int currentCount = 0;
                        for (int j = 0; j < valuesProp.arraySize; j++)
                        {
                            if (valuesProp.GetArrayElementAtIndex(j).stringValue == oldValue)
                            {
                                currentCount++;
                            }
                        }
                        
                        // Find the index of the first occurrence of this value
                        int firstOccurrenceIndex = -1;
                        for (int j = 0; j < valuesProp.arraySize; j++)
                        {
                            if (valuesProp.GetArrayElementAtIndex(j).stringValue == oldValue)
                            {
                                firstOccurrenceIndex = j;
                                break;
                            }
                        }
                        
                        // Only protect the first occurrence (original value)
                        // Allow editing of duplicates (subsequent occurrences)
                        preventChanges = (i == firstOccurrenceIndex);
                    }
                }
                
                string newValue;
                
                if (preventChanges)
                {
                    // Show as label if changes are prevented for this value
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Value", GUILayout.Width(50));
                    EditorGUILayout.LabelField(oldValue, EditorStyles.textField);
                    EditorGUILayout.LabelField("🔒", GUILayout.Width(20)); // Lock icon to indicate protection
                    EditorGUILayout.EndHorizontal();
                    newValue = oldValue;
                }
                else
                {
                    // Allow editing with consistent width
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Value", GUILayout.Width(50));
                    newValue = EditorGUILayout.TextField(oldValue);
                    EditorGUILayout.EndHorizontal();
                    // Sanitize input: remove invalid characters
                    newValue = SanitizeIdentifier(newValue);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    element.stringValue = newValue;
                    EditorUtility.SetDirty(def);
                    hasUnsavedChanges_local = true;
                }

                // Only show tooltip field if IncludeTooltips is enabled
                if (settings?.IncludeTooltips == true)
                {
                    // Ensure tooltips list is aligned
                    if (def.MutableTooltips.Count <= i)
                        def.MutableTooltips.Add("");

                    // Tooltip field with tight spacing
                    Undo.RecordObject(def, "Enum Tooltip Change");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Tooltip", GUILayout.Width(50));
                    def.MutableTooltips[i] = EditorGUILayout.TextField(def.MutableTooltips[i]);
                    EditorGUILayout.EndHorizontal();
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorUtility.SetDirty(def);
                        hasUnsavedChanges_local = true;
                    }
                }

                EditorGUILayout.EndVertical();
            }

            // Apply Changes and Add Value buttons row
            EditorGUILayout.BeginHorizontal();
            
            // Apply Changes button - only show when there are changes
            if (hasUnsavedChanges_local)
            {
                Color originalColor = GUI.color;
                GUI.color = Color.yellow; // Highlight the button when there are unsaved changes
                
                if (GUILayout.Button("Apply Changes ⚠", GUILayout.Width(120)))
                {
                    serializedObject.ApplyModifiedProperties();

                    bool hasErrors = false;
                    bool hasWarnings = false;
                    string errorMessage = "";
                    string warningMessage = "";

                    // Check for empty values
                    for (int i = 0; i < valuesProp.arraySize; i++)
                    {
                        var nameI = valuesProp.GetArrayElementAtIndex(i).stringValue;

                        if (string.IsNullOrWhiteSpace(nameI))
                        {
                            hasErrors = true;
                            errorMessage += $"Empty value at index {i + 1}.\n";
                        }
                    }

                    // Check for invalid identifiers
                    for (int i = 0; i < valuesProp.arraySize; i++)
                    {
                        var nameI = valuesProp.GetArrayElementAtIndex(i).stringValue;
                        
                        if (!string.IsNullOrWhiteSpace(nameI) && !ValidIdentifierRegex.IsMatch(nameI))
                        {
                            hasErrors = true;
                            errorMessage += $"Value '{nameI}' is not a valid C# identifier.\n";
                        }
                    }

                    // Check for duplicates (warning, not error)
                    var duplicates = new HashSet<string>();
                    for (int i = 0; i < valuesProp.arraySize; i++)
                    {
                        var nameI = valuesProp.GetArrayElementAtIndex(i).stringValue;
                        
                        if (!string.IsNullOrWhiteSpace(nameI))
                        {
                            if (duplicates.Contains(nameI))
                            {
                                hasWarnings = true;
                                warningMessage += $"Duplicate value: '{nameI}'.\n";
                            }
                            else
                            {
                                duplicates.Add(nameI);
                            }
                        }
                    }

                    // Show error dialog if there are critical errors
                    if (hasErrors)
                    {
                        EditorUtility.DisplayDialog("Enum Creator - Errors", 
                            $"Cannot generate enum '{def.EnumName}' due to errors:\n\n{errorMessage.TrimEnd()}", "OK");
                        GUI.color = originalColor;
                        EditorGUILayout.EndHorizontal();
                        return;
                    }

                    // Show warning dialog if there are duplicates and prevent generation
                    if (hasWarnings)
                    {
                        EditorUtility.DisplayDialog("Enum Creator - Duplicates Found", 
                            $"Cannot apply changes due to duplicate values in '{def.EnumName}':\n\n{warningMessage.TrimEnd()}\n\nPlease fix the duplicates before applying changes.", 
                            "OK");
                        GUI.color = originalColor;
                        EditorGUILayout.EndHorizontal();
                        return;
                    }

                    // Generate the enum only if no errors or warnings
                    EnumGenerator.Generate(def);
                    hasUnsavedChanges_local = false; // Reset the flag after successful generation
                    
                    GUI.color = originalColor;
                }
            }
            
            GUILayout.FlexibleSpace(); // Push Add Value button to the right
            
            // Add button - only adds tooltip if IncludeTooltips is enabled
            Color addButtonOriginalColor = GUI.color; // Store original color
            GUI.color = Color.white; // Keep Add Value button white regardless of changes
            if (GUILayout.Button("Add Value", GUILayout.Width(100)))
            {
                Undo.RecordObject(def, "Enum Value Add");

                valuesProp.InsertArrayElementAtIndex(valuesProp.arraySize);
                // Set the new value to empty string instead of copying the previous one
                valuesProp.GetArrayElementAtIndex(valuesProp.arraySize - 1).stringValue = "";
                
                // Only add tooltip if IncludeTooltips is enabled
                if (settings?.IncludeTooltips == true)
                {
                    def.MutableTooltips.Add("");
                }

                EditorUtility.SetDirty(def);
                hasUnsavedChanges_local = true;
            }
            GUI.color = addButtonOriginalColor; // Restore original color
            EditorGUILayout.EndHorizontal();


            GUI.enabled = true;

            // Check if undo has restored the state to original (no changes)
            // This happens when Ctrl+Z is pressed and the state matches the saved state
            if (hasUnsavedChanges_local && !serializedObject.hasModifiedProperties && !EditorUtility.IsDirty(def))
            {
                hasUnsavedChanges_local = false;
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

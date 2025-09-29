using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EnumEditor.Editor
{
    [CustomPropertyDrawer(typeof(Enum), true)]
    public class EnumFieldDrawer : PropertyDrawer
    {
        private static readonly GUIContent AddButtonContent = new GUIContent("+", "Add new enum value");
        private static readonly GUIContent NewValueLabel = new GUIContent("New Value:");
        
        private string newValueName = "";
        private bool showAddField = false;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var settings = EnumEditorSettings.Instance;
            
            if (!settings.enableEnumEditor)
            {
                // Use default enum drawer
                EditorGUI.PropertyField(position, property, label);
                return;
            }
            
            // Draw the enum field
            var enumRect = new Rect(position.x, position.y, position.width - 25, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(enumRect, property, label);
            
            // Draw add button
            var buttonRect = new Rect(position.x + position.width - 25, position.y, 25, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(buttonRect, AddButtonContent))
            {
                showAddField = !showAddField;
                if (showAddField)
                {
                    newValueName = "";
                }
            }
            
            // Draw new value input field if expanded
            if (showAddField)
            {
                var newValueRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, 
                    position.width, EditorGUIUtility.singleLineHeight);
                
                EditorGUI.BeginChangeCheck();
                newValueName = EditorGUI.TextField(newValueRect, NewValueLabel, newValueName);
                
                if (EditorGUI.EndChangeCheck())
                {
                    // Let user type whatever they want - validate later
                }
                
                // Draw add button for the new value
                var addValueRect = new Rect(position.x, position.y + (EditorGUIUtility.singleLineHeight + 2) * 2, 
                    position.width, EditorGUIUtility.singleLineHeight);
                
                if (GUI.Button(addValueRect, "Add Value"))
                {
                    AddNewEnumValue(property, newValueName);
                    showAddField = false;
                    newValueName = "";
                }
            }
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var baseHeight = EditorGUIUtility.singleLineHeight;
            
            if (showAddField)
            {
                baseHeight += (EditorGUIUtility.singleLineHeight + 2) * 2; // New value field + add button
            }
            
            return baseHeight;
        }
        
        private void AddNewEnumValue(SerializedProperty property, string newValueName)
        {
            var settings = EnumEditorSettings.Instance;
            
            // Validate input
            if (string.IsNullOrWhiteSpace(newValueName))
            {
                EditorUtility.DisplayDialog("EnumEditor", "Please enter a valid enum value name.", "OK");
                return;
            }
            
            if (!EnumModifier.ValidateEnumValueName(newValueName))
            {
                EditorUtility.DisplayDialog("EnumEditor", 
                    $"'{newValueName}' is not a valid C# identifier. Please use only letters, numbers, and underscores.", "OK");
                return;
            }
            
            // Get the enum type
            var enumType = GetEnumType(property);
            if (enumType == null)
            {
                EditorUtility.DisplayDialog("EnumEditor", "Could not determine enum type.", "OK");
                return;
            }
            
            // Get the script object to determine current script path
            var scriptObject = property.serializedObject.targetObject;
            
            // Find the enum info using two-step search (current script first, then all folders)
            var enumInfo = EnumDetector.FindEnumByType(enumType, scriptObject);
            if (enumInfo == null)
            {
                EditorUtility.DisplayDialog("EnumEditor", 
                    $"Could not find enum definition for '{enumType.Name}'. Make sure the enum is defined in a C# script.", "OK");
                return;
            }
            
            // Check for duplicates
            if (enumInfo.HasValue(newValueName))
            {
                EditorUtility.DisplayDialog("EnumEditor", 
                    $"Value '{newValueName}' already exists in enum '{enumType.Name}'.", "OK");
                return;
            }
            
            // Show confirmation dialog if enabled
            if (settings.showConfirmationDialog)
            {
                string message = $"Are you sure you want to add '{newValueName}' to enum '{enumType.Name}'?\n\n" +
                               $"This will modify the source file: {enumInfo.filePath}";
                
                if (!EditorUtility.DisplayDialog("EnumEditor - Confirm Addition", message, "Yes", "Cancel"))
                {
                    return;
                }
            }
            
            // Add the new value
            bool success = EnumModifier.AddEnumValue(enumInfo.filePath, enumType.Name, newValueName);
            
            if (success)
            {
                Debug.Log($"EnumEditor: Successfully added '{newValueName}' to enum '{enumType.Name}'");
                
                // Refresh the property
                property.serializedObject.Update();
                
                // Try to set the new value
                try
                {
                    var newEnumValue = Enum.Parse(enumType, newValueName);
                    property.enumValueIndex = (int)Convert.ChangeType(newEnumValue, typeof(int));
                    property.serializedObject.ApplyModifiedProperties();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"EnumEditor: Could not set new enum value: {ex.Message}");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("EnumEditor", 
                    $"Failed to add '{newValueName}' to enum '{enumType.Name}'. Check the Console for details.", "OK");
            }
        }
        
        private Type GetEnumType(SerializedProperty property)
        {
            try
            {
                // Get the field info
                var fieldInfo = GetFieldInfo(property);
                if (fieldInfo != null)
                {
                    return fieldInfo.FieldType;
                }
                
                // Fallback: try to get from property
                var targetObject = property.serializedObject.targetObject;
                var targetType = targetObject.GetType();
                
                // Find the field in the target type
                var field = targetType.GetField(property.propertyPath, 
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (field != null)
                {
                    return field.FieldType;
                }
                
                // Try to get from property path
                var propertyPath = property.propertyPath;
                var pathParts = propertyPath.Split('.');
                
                Type currentType = targetType;
                FieldInfo currentField = null;
                
                foreach (var part in pathParts)
                {
                    currentField = currentType.GetField(part, 
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    
                    if (currentField == null) break;
                    
                    currentType = currentField.FieldType;
                }
                
                return currentField?.FieldType;
            }
            catch (Exception ex)
            {
                Debug.LogError($"EnumEditor: Failed to get enum type: {ex.Message}");
                return null;
            }
        }
        
        private FieldInfo GetFieldInfo(SerializedProperty property)
        {
            try
            {
                var targetObject = property.serializedObject.targetObject;
                var targetType = targetObject.GetType();
                
                // Find the field in the target type
                var field = targetType.GetField(property.propertyPath, 
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
                return field;
            }
            catch (Exception ex)
            {
                Debug.LogError($"EnumEditor: Failed to get field info: {ex.Message}");
                return null;
            }
        }
    }
}

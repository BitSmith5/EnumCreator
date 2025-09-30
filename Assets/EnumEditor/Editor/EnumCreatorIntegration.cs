using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace EnumEditor.Editor
{
    /// <summary>
    /// Integration between EnumEditor and EnumCreator to keep them synchronized
    /// </summary>
    public static class EnumCreatorIntegration
    {
        private static readonly Regex EnumValuePattern = new Regex(@"^\s*(\w+)\s*=\s*(\d+),?\s*$", 
            RegexOptions.Compiled | RegexOptions.Multiline);
        
        /// <summary>
        /// Checks if a file is managed by EnumCreator
        /// </summary>
        public static bool IsEnumCreatorManagedFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return false;
            
            // Check if file is in the generated enums folder
            var settings = GetEnumCreatorSettings();
            if (settings == null)
                return false;
                
            string generatedPath = settings.GeneratedEnumsPath;
            if (string.IsNullOrEmpty(generatedPath))
                generatedPath = "Assets/GeneratedEnums";
            
            // Convert to absolute path for comparison
            string absoluteGeneratedPath = Path.GetFullPath(generatedPath);
            string absoluteFilePath = Path.GetFullPath(filePath);
            
            return absoluteFilePath.StartsWith(absoluteGeneratedPath);
        }
        
        /// <summary>
        /// Gets the EnumDefinition asset for a given enum file
        /// </summary>
        public static EnumCreator.EnumDefinition GetEnumDefinitionForFile(string filePath)
        {
            if (!IsEnumCreatorManagedFile(filePath))
                return null;
            
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            return FindEnumDefinition(fileName);
        }
        
        /// <summary>
        /// Updates the EnumDefinition asset when EnumEditor adds a new value
        /// </summary>
        public static bool UpdateEnumDefinitionAfterValueAdded(string filePath, string enumName, string newValueName)
        {
            var enumDef = GetEnumDefinitionForFile(filePath);
            if (enumDef == null)
                return false;
            
            try
            {
                // Parse the updated file to get all current values
                string content = File.ReadAllText(filePath);
                var currentValues = ParseEnumValues(content);
                
                // Update the EnumDefinition's values list
                UpdateEnumDefinitionValues(enumDef, currentValues);
                
                // Mark the asset as dirty and save
                EditorUtility.SetDirty(enumDef);
                AssetDatabase.SaveAssets();
                
                Debug.Log($"EnumEditor: Updated EnumDefinition for '{enumName}' with new value '{newValueName}'");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"EnumEditor: Failed to update EnumDefinition for '{enumName}': {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Parses enum values from file content
        /// </summary>
        private static System.Collections.Generic.List<string> ParseEnumValues(string content)
        {
            var values = new System.Collections.Generic.List<string>();
            var lines = content.Split('\n');
            
            foreach (string line in lines)
            {
                var match = EnumValuePattern.Match(line);
                if (match.Success)
                {
                    string valueName = match.Groups[1].Value;
                    values.Add(valueName);
                }
            }
            
            return values;
        }
        
        /// <summary>
        /// Updates the EnumDefinition's values list with current values from file
        /// </summary>
        private static void UpdateEnumDefinitionValues(EnumCreator.EnumDefinition enumDef, System.Collections.Generic.List<string> currentValues)
        {
            // Use reflection to access the private values field
            var valuesField = typeof(EnumCreator.EnumDefinition).GetField("values", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (valuesField != null)
            {
                var valuesList = valuesField.GetValue(enumDef) as System.Collections.Generic.List<string>;
                if (valuesList != null)
                {
                    // Clear and repopulate with current values
                    valuesList.Clear();
                    valuesList.AddRange(currentValues);
                }
            }
            
            // Also update tooltips list to match the new values count
            var tooltipsField = typeof(EnumCreator.EnumDefinition).GetField("tooltips", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (tooltipsField != null)
            {
                var tooltipsList = tooltipsField.GetValue(enumDef) as System.Collections.Generic.List<string>;
                if (tooltipsList != null)
                {
                    // Ensure tooltips list has the same count as values
                    while (tooltipsList.Count < currentValues.Count)
                    {
                        tooltipsList.Add("");
                    }
                    
                    // Remove excess tooltips if values were removed
                    while (tooltipsList.Count > currentValues.Count)
                    {
                        tooltipsList.RemoveAt(tooltipsList.Count - 1);
                    }
                }
            }
        }
        
        /// <summary>
        /// Finds an EnumDefinition asset by enum name
        /// </summary>
        private static EnumCreator.EnumDefinition FindEnumDefinition(string enumName)
        {
            string[] guids = AssetDatabase.FindAssets("t:EnumDefinition");
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var enumDef = AssetDatabase.LoadAssetAtPath<EnumCreator.EnumDefinition>(path);
                
                if (enumDef != null && enumDef.EnumName == enumName)
                {
                    return enumDef;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Gets EnumCreator settings
        /// </summary>
        private static EnumCreator.EnumCreatorSettings GetEnumCreatorSettings()
        {
            try
            {
                // Try to get settings using reflection to avoid direct dependency
                var settingsManagerType = System.Type.GetType("EnumCreator.Editor.EnumCreatorSettingsManager, Assembly-CSharp-Editor");
                if (settingsManagerType != null)
                {
                    var getSettingsMethod = settingsManagerType.GetMethod("GetOrCreateSettings", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    
                    if (getSettingsMethod != null)
                    {
                        return getSettingsMethod.Invoke(null, null) as EnumCreator.EnumCreatorSettings;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"EnumEditor: Could not access EnumCreator settings: {ex.Message}");
            }
            
            return null;
        }
        
        /// <summary>
        /// Checks if EnumCreator is available in the project
        /// </summary>
        public static bool IsEnumCreatorAvailable()
        {
            return System.Type.GetType("EnumCreator.Editor.EnumCreatorSettingsManager, Assembly-CSharp-Editor") != null;
        }
    }
}

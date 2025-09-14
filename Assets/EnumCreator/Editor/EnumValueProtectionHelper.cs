using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace EnumCreator.Editor
{
    /// <summary>
    /// Helper class to determine if enum values should be protected from changes
    /// </summary>
    public static class EnumValueProtectionHelper
    {
        private static readonly Regex ValuePattern = new Regex(@"^\s*(\w+)\s*=\s*\d+,?\s*$", 
            RegexOptions.Compiled | RegexOptions.Multiline);
        /// <summary>
        /// Gets the list of values that exist in the generated enum file
        /// </summary>
        public static HashSet<string> GetExistingEnumValues(EnumCreator.EnumDefinition enumDef)
        {
            var existingValues = new HashSet<string>();
            
            try
            {
                var settings = EnumCreatorSettingsManager.GetOrCreateSettings();
                string generatedPath = settings?.GeneratedEnumsPath ?? "Assets/GeneratedEnums";
                string filePath = Path.Combine(generatedPath, $"{enumDef.EnumName}.cs");
                
                if (File.Exists(filePath))
                {
                    string content = File.ReadAllText(filePath);
                    
                    // Parse enum values from the generated file
                    // Match patterns like: "ValueName = 0," or "ValueName = 1,"
                    var lines = content.Split('\n');
                    
                    foreach (string line in lines)
                    {
                        var match = ValuePattern.Match(line);
                        if (match.Success)
                        {
                            string valueName = match.Groups[1].Value;
                            existingValues.Add(valueName);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to read existing enum values for {enumDef.EnumName}: {ex.Message}");
            }
            
            return existingValues;
        }
        
        /// <summary>
        /// Determines if a specific value should be protected from changes
        /// </summary>
        public static bool ShouldProtectValue(EnumCreator.EnumDefinition enumDef, string valueName, bool preventValueNameChanges)
        {
            // If the setting is disabled, never protect
            if (!preventValueNameChanges)
                return false;
                
            // Get existing values from generated file
            var existingValues = GetExistingEnumValues(enumDef);
            
            // Protect only if the value exists in the generated file
            return existingValues.Contains(valueName);
        }
    }
}

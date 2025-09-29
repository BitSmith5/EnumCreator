using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace EnumEditor.Editor
{
    public static class EnumDetector
    {
        private static readonly Regex EnumRegex = new Regex(
            @"(?:public\s+|internal\s+|private\s+)?enum\s+(\w+)(?:\s*:\s*\w+)?\s*\{([^}]+)\}",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.Singleline
        );
        
        private static readonly Regex FlagsAttributeRegex = new Regex(
            @"\[System\.Flags\]|\[Flags\]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        
        private static readonly Regex NamespaceRegex = new Regex(
            @"namespace\s+([\w\.]+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        
        private static readonly Regex EnumValueRegex = new Regex(
            @"(\w+)(?:\s*=\s*(\d+))?",
            RegexOptions.Compiled
        );
        
        public static List<EnumInfo> FindEnumsInProject()
        {
            var enums = new List<EnumInfo>();
            var settings = EnumEditorSettings.Instance;
            
            if (!settings.enableEnumEditor) return enums;
            
            // Get all C# files in the project
            var csFiles = Directory.GetFiles("Assets", "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Contains("Library") && !f.Contains("Temp"))
                .ToList();
            
            foreach (var filePath in csFiles)
            {
                try
                {
                    var fileEnums = FindEnumsInFile(filePath);
                    enums.AddRange(fileEnums);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"EnumEditor: Failed to parse file {filePath}: {ex.Message}");
                }
            }
            
            return enums;
        }
        
        public static List<EnumInfo> FindEnumsInFile(string filePath)
        {
            var enums = new List<EnumInfo>();
            
            if (!File.Exists(filePath)) return enums;
            
            var content = File.ReadAllText(filePath);
            var matches = EnumRegex.Matches(content);
            
            // Extract namespace
            var namespaceMatch = NamespaceRegex.Match(content);
            string namespaceName = namespaceMatch.Success ? namespaceMatch.Groups[1].Value : "";
            
            // Check if file has Flags attribute
            bool hasFlagsAttribute = FlagsAttributeRegex.IsMatch(content);
            
            foreach (Match match in matches)
            {
                string enumName = match.Groups[1].Value;
                string enumBody = match.Groups[2].Value;
                
                var enumInfo = new EnumInfo
                {
                    enumName = enumName,
                    namespaceName = namespaceName,
                    filePath = filePath,
                    isFlags = hasFlagsAttribute
                };
                
                // Parse enum values
                ParseEnumValues(enumInfo, enumBody);
                
                enums.Add(enumInfo);
            }
            
            return enums;
        }
        
        public static EnumInfo FindEnumByName(string enumName, string currentScriptPath = null)
        {
            // Step 1: Try to find in the current script first
            if (!string.IsNullOrEmpty(currentScriptPath) && File.Exists(currentScriptPath))
            {
                var enumsInCurrentScript = FindEnumsInFile(currentScriptPath);
                var foundEnum = enumsInCurrentScript.FirstOrDefault(e => 
                    e.enumName.Equals(enumName, StringComparison.OrdinalIgnoreCase));
                
                if (foundEnum != null) return foundEnum;
            }
            
            // Step 2: If not found in current script, search all scripts
            var allEnums = FindEnumsInProject();
            return allEnums.FirstOrDefault(e => 
                e.enumName.Equals(enumName, StringComparison.OrdinalIgnoreCase));
        }
        
        public static EnumInfo FindEnumByType(Type enumType, UnityEngine.Object scriptObject = null)
        {
            if (enumType == null || !enumType.IsEnum) return null;
            
            var enumName = enumType.Name;
            var namespaceName = enumType.Namespace ?? "";
            
            // Step 1: Try to find the enum in the current script first
            string currentScriptPath = null;
            if (scriptObject != null)
            {
                currentScriptPath = AssetDatabase.GetAssetPath(scriptObject);
                if (!string.IsNullOrEmpty(currentScriptPath))
                {
                    var currentScriptEnums = FindEnumsInFile(currentScriptPath);
                    var foundInCurrentScript = currentScriptEnums.FirstOrDefault(e => 
                        e.enumName.Equals(enumName, StringComparison.OrdinalIgnoreCase) &&
                        e.namespaceName.Equals(namespaceName, StringComparison.OrdinalIgnoreCase));
                    
                    if (foundInCurrentScript != null)
                    {
                        return foundInCurrentScript;
                    }
                }
            }
            
            // Step 2: If not found in current script, search all folders
            var allEnums = FindEnumsInProject();
            return allEnums.FirstOrDefault(e => 
                e.enumName.Equals(enumName, StringComparison.OrdinalIgnoreCase) &&
                e.namespaceName.Equals(namespaceName, StringComparison.OrdinalIgnoreCase));
        }
        
        private static void ParseEnumValues(EnumInfo enumInfo, string enumBody)
        {
            var lines = enumBody.Split('\n');
            
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("//")) continue;
                
                // Remove trailing comma
                if (trimmedLine.EndsWith(","))
                {
                    trimmedLine = trimmedLine.Substring(0, trimmedLine.Length - 1);
                }
                
                var match = EnumValueRegex.Match(trimmedLine);
                if (match.Success)
                {
                    string valueName = match.Groups[1].Value;
                    int value = 0;
                    
                    if (match.Groups[2].Success)
                    {
                        int.TryParse(match.Groups[2].Value, out value);
                    }
                    else
                    {
                        // Calculate value based on position and flags
                        value = enumInfo.isFlags ? (1 << enumInfo.values.Count) : enumInfo.values.Count;
                    }
                    
                    bool isObsolete = trimmedLine.Contains("[System.Obsolete") || trimmedLine.Contains("[Obsolete");
                    
                    enumInfo.values.Add(new EnumValueInfo(valueName, value, isObsolete));
                }
            }
        }
        
        public static bool IsValidEnumValueName(string valueName)
        {
            if (string.IsNullOrWhiteSpace(valueName)) return false;
            
            // Check C# identifier rules
            if (!char.IsLetter(valueName[0]) && valueName[0] != '_') return false;
            
            for (int i = 1; i < valueName.Length; i++)
            {
                if (!char.IsLetterOrDigit(valueName[i]) && valueName[i] != '_') return false;
            }
            
            return true;
        }
        
        public static string SanitizeEnumValueName(string valueName)
        {
            if (string.IsNullOrWhiteSpace(valueName)) return "";
            
            // Replace invalid characters with underscores
            var sanitized = Regex.Replace(valueName, @"[^a-zA-Z0-9_]", "_");
            
            // If starts with a number, prepend underscore
            if (char.IsDigit(sanitized[0]))
            {
                sanitized = "_" + sanitized;
            }
            
            return sanitized;
        }
    }
}

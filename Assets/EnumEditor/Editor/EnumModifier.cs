using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace EnumEditor.Editor
{
    public static class EnumModifier
    {
        private static readonly Regex EnumRegex = new Regex(
            @"(public\s+|internal\s+|private\s+)?enum\s+(\w+)(?:\s*:\s*\w+)?\s*\{([^}]+)\}",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.Singleline
        );
        
        private static readonly Regex EnumWithBraceRegex = new Regex(
            @"(public\s+|internal\s+|private\s+)?enum\s+(\w+)(?:\s*:\s*\w+)?\s*\{([^}]+)\}",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.Singleline
        );
        
        private static readonly Regex FlagsAttributeRegex = new Regex(
            @"\[System\.Flags\]|\[Flags\]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        
        public static bool AddEnumValue(string filePath, string enumName, string newValueName)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"EnumEditor: File not found: {filePath}");
                return false;
            }
            
            var content = File.ReadAllText(filePath);
            var modifiedContent = AddEnumValueToContent(content, enumName, newValueName);
            
            if (modifiedContent == content)
            {
                Debug.LogError($"EnumEditor: Could not find enum '{enumName}' in file '{filePath}'");
                return false;
            }
            
            try
            {
                File.WriteAllText(filePath, modifiedContent);
                
                // Force Unity to refresh and recompile the modified file
                try
                {
                    // Convert to Unity asset path
                    string assetPath = filePath.Replace(Application.dataPath, "Assets");
                    
                    // Use a delayed call to avoid immediate file system conflicts
                    EditorApplication.delayCall += () =>
                    {
                        try
                        {
                            if (File.Exists(filePath))
                            {
                                // Force Unity to reimport the specific asset
                                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                                
                                // Also refresh the asset database to ensure changes are visible
                                AssetDatabase.Refresh();
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogWarning($"EnumEditor: Asset refresh failed, but file was written: {ex.Message}");
                        }
                    };
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"EnumEditor: Could not schedule asset refresh: {ex.Message}");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"EnumEditor: Failed to write to file '{filePath}': {ex.Message}");
                return false;
            }
        }
        
        private static string AddEnumValueToContent(string content, string enumName, string newValueName)
        {
            var lines = content.Split('\n');
            var result = new List<string>();
            bool inTargetEnum = false;
            bool foundEnum = false;
            int braceCount = 0;
            bool enumStarted = false;
            
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                
                // Check if this line starts our target enum
                if (!foundEnum && line.Contains($"enum {enumName}"))
                {
                    inTargetEnum = true;
                    foundEnum = true;
                    result.Add(line);
                    continue;
                }
                
                if (inTargetEnum)
                {
                    // Count braces to know when enum ends
                    foreach (char c in line)
                    {
                        if (c == '{') 
                        {
                            braceCount++;
                            enumStarted = true;
                        }
                        else if (c == '}') 
                        {
                            braceCount--;
                        }
                    }
                    
                    // If we're at the closing brace of our enum
                    if (enumStarted && braceCount == 0)
                    {
                        // Check if value already exists
                        string enumContent = string.Join("\n", result.Where((l, idx) => idx >= result.Count - (i - result.Count + 1)));
                        if (EnumValueExists(enumContent, newValueName))
                        {
                            Debug.LogWarning($"EnumEditor: Value '{newValueName}' already exists in enum '{enumName}'");
                            return content;
                        }
                        
                        // Determine if this is a flags enum
                        bool isFlags = FlagsAttributeRegex.IsMatch(content);
                        
                        // Calculate the next value
                        int nextValue = CalculateNextValue(enumContent, isFlags);
                        
                        // Add the new value before the closing brace
                        string lastLine = result[result.Count - 1];
                        if (!lastLine.TrimEnd().EndsWith(",") && !string.IsNullOrWhiteSpace(lastLine.Trim()))
                        {
                            result[result.Count - 1] = lastLine.TrimEnd() + ",";
                        }
                        
                        result.Add($"        {newValueName} = {nextValue}");
                        result.Add("    }");
                        inTargetEnum = false;
                        continue;
                    }
                }
                
                result.Add(line);
            }
            
            return string.Join("\n", result);
        }
        
        private static bool EnumValueExists(string enumBody, string valueName)
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
                
                // Extract value name (before = sign)
                var valueMatch = Regex.Match(trimmedLine, @"(\w+)");
                if (valueMatch.Success)
                {
                    string existingValueName = valueMatch.Groups[1].Value;
                    if (existingValueName.Equals(valueName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        private static int CalculateNextValue(string enumBody, bool isFlags)
        {
            var lines = enumBody.Split('\n');
            var values = new List<int>();
            
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("//")) continue;
                
                // Remove trailing comma
                if (trimmedLine.EndsWith(","))
                {
                    trimmedLine = trimmedLine.Substring(0, trimmedLine.Length - 1);
                }
                
                // Extract numeric value
                var valueMatch = Regex.Match(trimmedLine, @"=\s*(\d+)");
                if (valueMatch.Success)
                {
                    if (int.TryParse(valueMatch.Groups[1].Value, out int value))
                    {
                        values.Add(value);
                    }
                }
            }
            
            if (isFlags)
            {
                // For flags, find the next power of 2
                if (values.Count == 0) return 1;
                
                int maxValue = values.Max();
                int nextPower = 1;
                while (nextPower <= maxValue)
                {
                    nextPower <<= 1;
                }
                
                return nextPower;
            }
            else
            {
                // For regular enums, use sequential numbering
                if (values.Count == 0) return 0;
                
                return values.Max() + 1;
            }
        }
        
        public static bool ValidateEnumValueName(string valueName)
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
        
        public static bool BackupFile(string filePath)
        {
            if (!File.Exists(filePath)) return false;
            
            try
            {
                string backupPath = filePath + ".backup";
                File.Copy(filePath, backupPath, true);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"EnumEditor: Failed to create backup for '{filePath}': {ex.Message}");
                return false;
            }
        }
        
        public static void RestoreBackup(string filePath)
        {
            string backupPath = filePath + ".backup";
            
            if (File.Exists(backupPath))
            {
                try
                {
                    File.Copy(backupPath, filePath, true);
                    AssetDatabase.Refresh();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"EnumEditor: Failed to restore backup for '{filePath}': {ex.Message}");
                }
            }
        }
    }
}

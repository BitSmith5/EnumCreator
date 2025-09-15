using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace EnumCreator.Editor
{
    [InitializeOnLoad]
    public static class EnumFileWatcher
    {
        static EnumFileWatcher()
        {
            EditorApplication.delayCall += CheckAllEnumFilesOnStartup;
        }

        private static void CheckAllEnumFilesOnStartup()
        {
            CheckAllEnumFiles();
        }

        private static void CheckAllEnumFiles()
        {
            string generatedPath = "Assets/GeneratedEnums";
            if (!Directory.Exists(generatedPath))
                return;

            string[] enumFiles = Directory.GetFiles(generatedPath, "*.cs", SearchOption.TopDirectoryOnly);
            
            foreach (string filePath in enumFiles)
            {
                ProcessEnumFile(filePath);
            }
        }

        private static void ProcessEnumFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return;

                string fileName = Path.GetFileNameWithoutExtension(filePath);
                var enumDef = FindEnumDefinition(fileName);
                
                if (enumDef == null)
                {
                    enumDef = CreateEnumDefinitionFromFile(filePath, fileName);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"EnumCreator: Failed to process enum file '{filePath}': {ex.Message}");
            }
        }

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

        private static EnumCreator.EnumDefinition CreateEnumDefinitionFromFile(string filePath, string fileName)
        {
            try
            {
                string content = File.ReadAllText(filePath);
                var parsedEnum = ParseEnumContent(content);
                
                // Allow empty enums - don't fail if no values are found
                if (parsedEnum == null)
                {
                    Debug.LogWarning($"EnumCreator: Could not parse enum from file '{filePath}' - invalid enum format.");
                    return null;
                }

                var settings = EnumCreatorSettingsManager.GetOrCreateSettings();
                var enumDef = ScriptableObject.CreateInstance<EnumCreator.EnumDefinition>();
                
                var enumNameField = typeof(EnumCreator.EnumDefinition).GetField("enumName", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                enumNameField?.SetValue(enumDef, fileName);

                var namespaceField = typeof(EnumCreator.EnumDefinition).GetField("@namespace", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                namespaceField?.SetValue(enumDef, !string.IsNullOrEmpty(parsedEnum.Namespace) ? parsedEnum.Namespace : settings?.DefaultNamespace ?? "Game.Enums");

                var useFlagsField = typeof(EnumCreator.EnumDefinition).GetField("useFlags", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                useFlagsField?.SetValue(enumDef, parsedEnum.UseFlags);

                string enumDefPath = $"Assets/EnumCreator/Definitions/{fileName}.asset";
                
                string directory = Path.GetDirectoryName(enumDefPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                AssetDatabase.CreateAsset(enumDef, enumDefPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                return enumDef;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"EnumCreator: Failed to create enum definition from file '{filePath}': {ex.Message}");
                return null;
            }
        }

        private static ParsedEnum ParseEnumContent(string content)
        {
            try
            {
                var parsedEnum = new ParsedEnum();

                // Extract namespace
                var namespaceMatch = Regex.Match(content, @"namespace\s+([^\s{]+)");
                if (namespaceMatch.Success)
                {
                    parsedEnum.Namespace = namespaceMatch.Groups[1].Value;
                }

                // Check for [System.Flags] attribute
                parsedEnum.UseFlags = content.Contains("[System.Flags]");

                // Extract enum values - allow empty enums
                var valuePattern = @"(\s*\[UnityEngine\.Tooltip\([^)]+\)\])?(\s*\[System\.Obsolete\([^)]+\)\])?\s*(\w+)\s*=\s*(\d+),?";
                var matches = Regex.Matches(content, valuePattern, RegexOptions.Multiline);

                foreach (Match match in matches)
                {
                    bool hasTooltip = match.Groups[1].Success;
                    bool isObsolete = match.Groups[2].Success;
                    string valueName = match.Groups[3].Value;
                    int numericValue = int.Parse(match.Groups[4].Value);

                    string tooltip = "";
                    if (hasTooltip)
                    {
                        var tooltipMatch = Regex.Match(match.Groups[1].Value, @"\[UnityEngine\.Tooltip\([""']([^""']+)[""']\)\]");
                        if (tooltipMatch.Success)
                        {
                            tooltip = tooltipMatch.Groups[1].Value;
                        }
                    }

                    parsedEnum.Values.Add(new ParsedEnumValue
                    {
                        Name = valueName,
                        NumericValue = numericValue,
                        IsObsolete = isObsolete,
                        Tooltip = tooltip
                    });
                }

                return parsedEnum;
            }
            catch (System.Exception)
            {
                return null;
            }
        }
    }

    public class ParsedEnum
    {
        public string Namespace { get; set; } = "";
        public bool UseFlags { get; set; } = false;
        public System.Collections.Generic.List<ParsedEnumValue> Values { get; set; } = new System.Collections.Generic.List<ParsedEnumValue>();
    }

    public class ParsedEnumValue
    {
        public string Name { get; set; }
        public int NumericValue { get; set; }
        public bool IsObsolete { get; set; } = false;
        public string Tooltip { get; set; } = "";
    }
}
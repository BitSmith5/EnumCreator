using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace EnumCreator.Editor
{
    /// <summary>
    /// Watches generated enum files for changes and updates the corresponding enum definitions
    /// </summary>
    [InitializeOnLoad]
    public static class EnumFileWatcher
    {
        private static FileSystemWatcher fileWatcher;
        private static readonly string GeneratedPath = "Assets/GeneratedEnums";
        private static System.Collections.Generic.HashSet<string> modifiedFiles = new System.Collections.Generic.HashSet<string>();
        private static System.Collections.Generic.Dictionary<string, System.DateTime> lastProcessedTimes = new System.Collections.Generic.Dictionary<string, System.DateTime>();

        static EnumFileWatcher()
        {
            SetupFileWatcher();
            SetupCompilationCallbacks();
            
            // Check for any enum files that might have been modified since last Unity session
            // This handles the case where someone gets latest from repo and opens Unity
            EditorApplication.delayCall += CheckAllEnumFilesOnStartup;
        }

        private static void SetupFileWatcher()
        {
            if (fileWatcher != null)
            {
                fileWatcher.Dispose();
            }

            if (!Directory.Exists(GeneratedPath))
                return;

            fileWatcher = new FileSystemWatcher(GeneratedPath, "*.cs")
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = false
            };

            fileWatcher.Changed += OnEnumFileChanged;
            fileWatcher.Created += OnEnumFileChanged;
        }

        private static void OnEnumFileChanged(object sender, FileSystemEventArgs e)
        {
            // Track modified files but don't process immediately
            // We'll process them after compilation
            if (File.Exists(e.FullPath))
            {
                string relativePath = e.FullPath.Replace(Application.dataPath, "Assets");
                modifiedFiles.Add(relativePath);
            }
        }

        private static void SetupCompilationCallbacks()
        {
            // Hook into Unity's compilation pipeline
            CompilationPipeline.compilationStarted += OnCompilationStarted;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
            
            // Hook into asset import pipeline to detect when files are imported from version control
            AssetDatabase.importPackageCompleted += OnImportPackageCompleted;
            
        }

        private static void OnCompilationStarted(object obj)
        {
            // Compilation started - nothing to do yet
        }

        private static void OnCompilationFinished(object obj)
        {
            // Process all modified files after compilation
            foreach (string filePath in modifiedFiles)
            {
                if (File.Exists(filePath))
                {
                    ProcessEnumFile(filePath);
                }
            }
            
            // Clear the list after processing
            modifiedFiles.Clear();
        }

        private static void OnImportPackageCompleted(string packageName)
        {
            // Trigger a check for any enum files that might have been modified
            CheckAllEnumFiles();
        }

        private static void OnAssetsImported(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            // Check if any of the imported assets are enum files in our GeneratedEnums folder
            foreach (string assetPath in importedAssets)
            {
                if (assetPath.StartsWith(GeneratedPath) && assetPath.EndsWith(".cs"))
                {
                    // This is an enum file that was imported (likely from version control)
                    // Process it to sync with the corresponding enum definition
                    ProcessEnumFile(assetPath);
                }
            }
        }

        private static void CheckAllEnumFiles()
        {
            if (!Directory.Exists(GeneratedPath))
                return;

            string[] enumFiles = Directory.GetFiles(GeneratedPath, "*.cs", SearchOption.TopDirectoryOnly);
            
            foreach (string filePath in enumFiles)
            {
                if (IsManuallyEdited(filePath))
                {
                    ProcessEnumFile(filePath);
                }
            }
        }

        private static void CheckAllEnumFilesOnStartup()
        {
            // Check for enum files that were modified since last Unity session
            // This handles the case where someone gets latest from repo and opens Unity
            CheckAllEnumFiles();
        }

        private static void ProcessEnumFile(string filePath)
        {
            try
            {
                // Check if this is a manually edited file (not auto-generated)
                if (!IsManuallyEdited(filePath))
                    return;

                // Check if we've already processed this file recently
                var fileInfo = new FileInfo(filePath);
                if (lastProcessedTimes.ContainsKey(filePath) && 
                    lastProcessedTimes[filePath] >= fileInfo.LastWriteTime)
                {
                    return; // Already processed this version
                }

                string fileName = Path.GetFileNameWithoutExtension(filePath);

                // Find corresponding enum definition
                var enumDef = FindEnumDefinition(fileName);
                if (enumDef == null)
                {
                    return;
                }

                // Parse the enum file and update the definition
                ParseAndUpdateEnumDefinition(filePath, enumDef);

                // Only mark as dirty and save if there were actual changes
                EditorUtility.SetDirty(enumDef);
                AssetDatabase.SaveAssetIfDirty(enumDef);
                
                // Track when we last processed this file
                lastProcessedTimes[filePath] = fileInfo.LastWriteTime;
            }
            catch (System.Exception)
            {
                // Silently handle any parsing errors
            }
        }

        private static bool IsManuallyEdited(string filePath)
        {
            try
            {
                string content = File.ReadAllText(filePath);
                
                // Always process files in the GeneratedEnums folder
                // We'll let the user decide if they want to sync changes
                return true;
            }
            catch
            {
                return false;
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

        private static void ParseAndUpdateEnumDefinition(string filePath, EnumCreator.EnumDefinition enumDef)
        {
            string content = File.ReadAllText(filePath);
            
            // Parse enum content
            var parsedEnum = ParseEnumContent(content);
            
            if (parsedEnum == null)
            {
                return;
            }

            // Update enum definition
            UpdateEnumDefinition(enumDef, parsedEnum);
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

                // Extract enum values
                var valuePattern = @"(\s*\[System\.Obsolete\([^)]+\)\])?\s*(\w+)\s*=\s*(\d+),?";
                var matches = Regex.Matches(content, valuePattern, RegexOptions.Multiline);

                foreach (Match match in matches)
                {
                    bool isObsolete = match.Groups[1].Success;
                    string valueName = match.Groups[2].Value;
                    int numericValue = int.Parse(match.Groups[3].Value);

                    parsedEnum.Values.Add(new ParsedEnumValue
                    {
                        Name = valueName,
                        NumericValue = numericValue,
                        IsObsolete = isObsolete
                    });
                }

                return parsedEnum;
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        private static void UpdateEnumDefinition(EnumCreator.EnumDefinition enumDef, ParsedEnum parsedEnum)
        {
            // Update namespace if changed
            if (parsedEnum.Namespace != enumDef.Namespace)
            {
                var field = typeof(EnumCreator.EnumDefinition).GetField("@namespace", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(enumDef, parsedEnum.Namespace);
            }

            // Update useFlags if changed
            if (parsedEnum.UseFlags != enumDef.UseFlags)
            {
                var field = typeof(EnumCreator.EnumDefinition).GetField("useFlags", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(enumDef, parsedEnum.UseFlags);
            }

            // Ensure lists are properly synchronized (they should already be initialized)
            
            // Ensure tooltips list is same size as values list
            while (enumDef.MutableTooltips.Count < enumDef.MutableValues.Count)
            {
                enumDef.MutableTooltips.Add("");
            }
            
            // Ensure removed value numbers list is same size as removed values list
            while (enumDef.MutableRemovedValueNumbers.Count < enumDef.MutableRemovedValues.Count)
            {
                enumDef.MutableRemovedValueNumbers.Add(0);
            }

            // Smart update: preserve existing values and only modify what's necessary
            var existingActiveValues = new System.Collections.Generic.HashSet<string>(enumDef.MutableValues);
            var existingRemovedValues = new System.Collections.Generic.HashSet<string>(enumDef.MutableRemovedValues);
            
            var newActiveValues = new System.Collections.Generic.List<string>();
            var newRemovedValues = new System.Collections.Generic.List<string>();
            var newRemovedValueNumbers = new System.Collections.Generic.List<int>();
            var newTooltips = new System.Collections.Generic.List<string>();

            // Process parsed values - keep all values in active list, just track which are disabled
            foreach (var parsedValue in parsedEnum.Values)
            {
                // Always add to active values (they should stay visible in inspector)
                if (!existingActiveValues.Contains(parsedValue.Name))
                {
                    // New value - add it
                    newActiveValues.Add(parsedValue.Name);
                    newTooltips.Add(""); // Default empty tooltip
                }
                else
                {
                    // Already exists - keep it and preserve tooltip
                    newActiveValues.Add(parsedValue.Name);
                    int tooltipIndex = enumDef.MutableValues.IndexOf(parsedValue.Name);
                    if (tooltipIndex >= 0 && tooltipIndex < enumDef.MutableTooltips.Count)
                    {
                        newTooltips.Add(enumDef.MutableTooltips[tooltipIndex]);
                    }
                    else
                    {
                        newTooltips.Add(""); // Default empty tooltip
                    }
                }
                
                // Handle disabled state
                if (parsedValue.IsObsolete)
                {
                    // Value is obsolete in the file
                    if (!existingRemovedValues.Contains(parsedValue.Name))
                    {
                        // New obsolete value - add it to removed list
                        newRemovedValues.Add(parsedValue.Name);
                        newRemovedValueNumbers.Add(parsedValue.NumericValue);
                    }
                    else
                    {
                        // Already obsolete - keep it
                        newRemovedValues.Add(parsedValue.Name);
                        // Find and preserve the original numeric value
                        int originalIndex = enumDef.MutableRemovedValues.IndexOf(parsedValue.Name);
                        if (originalIndex >= 0 && originalIndex < enumDef.MutableRemovedValueNumbers.Count)
                        {
                            newRemovedValueNumbers.Add(enumDef.MutableRemovedValueNumbers[originalIndex]);
                        }
                        else
                        {
                            newRemovedValueNumbers.Add(parsedValue.NumericValue);
                        }
                    }
                }
                else
                {
                    // Value is active - remove from removed list if it was there
                    if (existingRemovedValues.Contains(parsedValue.Name))
                    {
                        // Was obsolete, now active - just don't add to removed list
                    }
                }
            }

            // Update the enum definition with the new values
            enumDef.MutableValues.Clear();
            enumDef.MutableValues.AddRange(newActiveValues);
            
            enumDef.MutableTooltips.Clear();
            enumDef.MutableTooltips.AddRange(newTooltips);
            
            enumDef.MutableRemovedValues.Clear();
            enumDef.MutableRemovedValues.AddRange(newRemovedValues);
            
            enumDef.MutableRemovedValueNumbers.Clear();
            enumDef.MutableRemovedValueNumbers.AddRange(newRemovedValueNumbers);
        }

        private class ParsedEnum
        {
            public string Namespace { get; set; } = "";
            public bool UseFlags { get; set; }
            public System.Collections.Generic.List<ParsedEnumValue> Values { get; set; } = new System.Collections.Generic.List<ParsedEnumValue>();
        }

        private class ParsedEnumValue
        {
            public string Name { get; set; } = "";
            public int NumericValue { get; set; }
            public bool IsObsolete { get; set; }
        }

        [MenuItem("Tools/Enum Creator/Setup File Watcher")]
        public static void SetupWatcher()
        {
            SetupFileWatcher();
            SetupCompilationCallbacks();
        }

        [MenuItem("Tools/Enum Creator/Force Sync All Enum Files")]
        public static void ForceSyncAll()
        {
            CheckAllEnumFiles();
        }

        [MenuItem("Tools/Enum Creator/Trigger Compilation Sync")]
        public static void TriggerCompilationSync()
        {
            OnCompilationFinished(null);
        }
    }
}

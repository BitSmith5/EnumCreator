using UnityEditor;
using UnityEngine;

namespace EnumCreator.Editor
{
    /// <summary>
    /// Handles applying default settings to newly created EnumDefinition assets
    /// </summary>
    public class EnumDefinitionAssetProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string assetPath in importedAssets)
            {
                if (assetPath.EndsWith(".asset"))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<EnumCreator.EnumDefinition>(assetPath);
                    if (asset != null)
                    {
                        // Apply default settings to newly created enum definitions
                        EnumDefinitionInitializer.ApplyDefaultSettingsToAsset(asset);
                    }
                }
            }
        }
    }

    /// <summary>
    /// InitializeOnLoad class to handle settings application on editor startup
    /// </summary>
    [InitializeOnLoad]
    public static class EnumDefinitionInitializer    {
        static EnumDefinitionInitializer()
        {
            // Hook into the asset creation process
            EditorApplication.projectChanged += OnProjectChanged;
        }

        private static void OnProjectChanged()
        {
            // Check all EnumDefinition assets and apply settings if needed
            string[] guids = AssetDatabase.FindAssets("t:EnumDefinition");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<EnumCreator.EnumDefinition>(path);
                if (asset != null)
                {
                    ApplyDefaultSettingsToAsset(asset);
                }
            }
        }

        public static void ApplyDefaultSettingsToAsset(EnumCreator.EnumDefinition asset)
        {
            // Only apply settings if this is a new asset with default values
            if (IsDefaultAsset(asset))
            {
                var settings = EnumCreatorSettingsManager.GetOrCreateSettings();
                if (settings != null)
                {
                    // Apply default namespace and flags setting
                    EnumCreatorSettingsManager.ApplyDefaultSettings(asset);
                    
                    // Always use filename as enum name
                    string fileName = asset.name; // ScriptableObject name is based on the asset file name
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        var enumNameField = typeof(EnumCreator.EnumDefinition).GetField("enumName", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        enumNameField?.SetValue(asset, fileName);
                    }
                    
                    EditorUtility.SetDirty(asset);
                    AssetDatabase.SaveAssetIfDirty(asset);
                }
            }
        }

        private static bool IsDefaultAsset(EnumCreator.EnumDefinition asset)
        {
            // Check if this asset has the default values (indicating it's newly created)
            var namespaceField = typeof(EnumCreator.EnumDefinition).GetField("@namespace", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var enumNameField = typeof(EnumCreator.EnumDefinition).GetField("enumName", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var valuesField = typeof(EnumCreator.EnumDefinition).GetField("values", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            string currentNamespace = (string)namespaceField?.GetValue(asset) ?? "";
            string currentEnumName = (string)enumNameField?.GetValue(asset) ?? "";
            var currentValues = valuesField?.GetValue(asset) as System.Collections.Generic.List<string>;

            return currentNamespace == "Game.Enums" && 
                   currentEnumName == "MyEnum" && 
                   currentValues != null && 
                   currentValues.Count == 0;
        }
    }
}
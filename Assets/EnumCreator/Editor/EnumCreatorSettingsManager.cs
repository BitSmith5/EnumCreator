using UnityEditor;
using UnityEngine;

namespace EnumCreator.Editor
{
    /// <summary>
    /// Manages access to EnumCreatorSettings throughout the editor
    /// </summary>
    public static class EnumCreatorSettingsManager
    {
        private static EnumCreatorSettings _cachedSettings;
        private static bool _settingsLoaded = false;

        /// <summary>
        /// Gets the current settings asset, loading it if necessary
        /// </summary>
        public static EnumCreatorSettings GetSettings()
        {
            if (!_settingsLoaded)
            {
                LoadSettings();
            }
            
            return _cachedSettings;
        }

        /// <summary>
        /// Forces a reload of the settings from disk
        /// </summary>
        public static void ReloadSettings()
        {
            _settingsLoaded = false;
            _cachedSettings = null;
            LoadSettings();
        }

        /// <summary>
        /// Creates default settings if none exist
        /// </summary>
        public static EnumCreatorSettings GetOrCreateSettings()
        {
            var settings = GetSettings();
            
            if (settings == null)
            {
                try
                {
                    settings = CreateDefaultSettings();
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"EnumCreator: Failed to create default settings: {ex.Message}");
                    return null;
                }
            }
            
            return settings;
        }

        private static void LoadSettings()
        {
            // Try to find existing settings asset
            string[] guids = AssetDatabase.FindAssets("t:EnumCreatorSettings");
            
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _cachedSettings = AssetDatabase.LoadAssetAtPath<EnumCreatorSettings>(path);
            }
            else
            {
                _cachedSettings = null;
            }
            
            _settingsLoaded = true;
        }

        private static EnumCreatorSettings CreateDefaultSettings()
        {
            // Create Settings folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder("Assets/EnumCreator/Settings"))
            {
                AssetDatabase.CreateFolder("Assets/EnumCreator", "Settings");
            }

                // Create the settings asset with default values
                var settings = ScriptableObject.CreateInstance<EnumCreatorSettings>();
            string assetPath = "Assets/EnumCreator/Settings/EnumCreatorSettings.asset";
            
            AssetDatabase.CreateAsset(settings, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            
            _cachedSettings = settings;
            return settings;
        }

        /// <summary>
        /// Applies settings to a new EnumDefinition
        /// </summary>
        public static void ApplyDefaultSettings(EnumCreator.EnumDefinition enumDef)
        {
            var settings = GetOrCreateSettings();
            
            if (settings != null)
            {
                // Use reflection to set private fields
                var namespaceField = typeof(EnumCreator.EnumDefinition).GetField("@namespace", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                namespaceField?.SetValue(enumDef, settings.DefaultNamespace);

                var useFlagsField = typeof(EnumCreator.EnumDefinition).GetField("useFlags", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                useFlagsField?.SetValue(enumDef, settings.DefaultUseFlags);
            }
        }

        /// <summary>
        /// Gets the enum name from filename (always uses filename)
        /// </summary>
        public static string GetEnumNameFromFilename(string fileName)
        {
            return fileName;
        }
    }
}

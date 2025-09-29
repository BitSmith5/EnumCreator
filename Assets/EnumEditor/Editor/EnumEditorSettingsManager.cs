using System.IO;
using UnityEditor;
using UnityEngine;

namespace EnumEditor.Editor
{
    public static class EnumEditorSettingsManager
    {
        private const string SETTINGS_PATH = "Assets/EnumEditor/Settings/EnumEditorSettings.asset";
        private static EnumEditorSettings _settings;
        
        public static EnumEditorSettings GetOrCreateSettings()
        {
            if (_settings == null)
            {
                _settings = AssetDatabase.LoadAssetAtPath<EnumEditorSettings>(SETTINGS_PATH);
                
                if (_settings == null)
                {
                    _settings = CreateSettings();
                }
            }
            
            return _settings;
        }
        
        private static EnumEditorSettings CreateSettings()
        {
            // Ensure the Settings directory exists
            string settingsDir = Path.GetDirectoryName(SETTINGS_PATH);
            if (!Directory.Exists(settingsDir))
            {
                Directory.CreateDirectory(settingsDir);
            }
            
            // Create the settings asset
            var settings = ScriptableObject.CreateInstance<EnumEditorSettings>();
            AssetDatabase.CreateAsset(settings, SETTINGS_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"EnumEditor: Created settings at {SETTINGS_PATH}");
            return settings;
        }
        
        public static void SaveSettings()
        {
            if (_settings != null)
            {
                EditorUtility.SetDirty(_settings);
                AssetDatabase.SaveAssets();
            }
        }
        
        public static void ResetSettings()
        {
            if (_settings != null)
            {
                // Reset to default values
                _settings.enableEnumEditor = true;
                _settings.showConfirmationDialog = true;
                _settings.addButtonText = "+";
                _settings.newValueFieldLabel = "New Value:";
                _settings.preventDuplicates = true;
                
                SaveSettings();
            }
        }
    }
}

using UnityEditor;
using UnityEngine;

namespace EnumEditor.Editor
{
    public static class EnumEditorMenuItems
    {
        
        [MenuItem("Tools/EnumEditor/Find All Enums", false, 1)]
        public static void FindAllEnums()
        {
            var enums = EnumDetector.FindEnumsInProject();
            
            if (enums.Count == 0)
            {
                EditorUtility.DisplayDialog("EnumEditor", "No enums found in the project.", "OK");
                return;
            }
            
            string message = $"Found {enums.Count} enum(s) in the project:\n\n";
            foreach (var enumInfo in enums)
            {
                message += $"• {enumInfo.enumName} ({enumInfo.namespaceName})\n";
                message += $"  File: {enumInfo.filePath}\n";
                message += $"  Values: {enumInfo.values.Count}\n";
                message += $"  Flags: {(enumInfo.isFlags ? "Yes" : "No")}\n\n";
            }
            
            EditorUtility.DisplayDialog("EnumEditor - Found Enums", message, "OK");
        }
        
        [MenuItem("Tools/EnumEditor/Refresh Enum Cache", false, 2)]
        public static void RefreshEnumCache()
        {
            // Force refresh of enum detection
            var enums = EnumDetector.FindEnumsInProject();
            Debug.Log($"EnumEditor: Refreshed enum cache. Found {enums.Count} enums.");
            
            EditorUtility.DisplayDialog("EnumEditor", 
                $"Enum cache refreshed. Found {enums.Count} enums in the project.", "OK");
        }
        
        
        [MenuItem("Tools/EnumEditor/About", false, 100)]
        public static void ShowAbout()
        {
            string message = "EnumEditor v1.0.0\n\n" +
                           "A Unity Editor tool for adding new values to existing enums.\n\n" +
                           "Features:\n" +
                           "• Add new enum values directly from the Inspector\n" +
                           "• Automatic enum detection and modification\n" +
                           "• Support for both regular and flags enums\n" +
                           "• Validation and confirmation dialogs\n" +
                           "• Configurable settings\n\n" +
                           "Created by Nicholas R. Gallo\n" +
                           "Based on EnumCreator Pro";
            
            EditorUtility.DisplayDialog("EnumEditor - About", message, "OK");
        }
        
        [MenuItem("Tools/EnumEditor/Enable EnumEditor", false, 10)]
        public static void EnableEnumEditor()
        {
            var settings = EnumEditorSettingsManager.GetOrCreateSettings();
            settings.enableEnumEditor = true;
            EnumEditorSettingsManager.SaveSettings();
            
            Debug.Log("EnumEditor: Enabled");
        }
        
        [MenuItem("Tools/EnumEditor/Disable EnumEditor", false, 11)]
        public static void DisableEnumEditor()
        {
            var settings = EnumEditorSettingsManager.GetOrCreateSettings();
            settings.enableEnumEditor = false;
            EnumEditorSettingsManager.SaveSettings();
            
            Debug.Log("EnumEditor: Disabled");
        }
        
        [MenuItem("Tools/EnumEditor/Enable EnumEditor", true)]
        public static bool EnableEnumEditorValidate()
        {
            var settings = EnumEditorSettingsManager.GetOrCreateSettings();
            return !settings.enableEnumEditor;
        }
        
        [MenuItem("Tools/EnumEditor/Disable EnumEditor", true)]
        public static bool DisableEnumEditorValidate()
        {
            var settings = EnumEditorSettingsManager.GetOrCreateSettings();
            return settings.enableEnumEditor;
        }
    }
}

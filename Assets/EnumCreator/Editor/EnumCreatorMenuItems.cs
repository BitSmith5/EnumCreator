using UnityEditor;
using UnityEngine;

namespace EnumCreator.Editor
{
    /// <summary>
    /// Additional menu items for the Enum Creator tool
    /// </summary>
    public static class EnumCreatorMenuItems
    {
        [MenuItem("Tools/Enum Creator/Create Settings Asset")]
        public static void CreateSettingsAsset()
        {
            // Create Settings folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder("Assets/EnumCreator/Settings"))
            {
                AssetDatabase.CreateFolder("Assets/EnumCreator", "Settings");
            }

            // Check if settings already exist
            string[] guids = AssetDatabase.FindAssets("t:EnumCreatorSettings");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var existingSettings = AssetDatabase.LoadAssetAtPath<EnumCreatorSettings>(path);
                
                if (existingSettings != null)
                {
                    EditorUtility.DisplayDialog("Settings Already Exist", 
                        $"Settings asset already exists at: {path}\n\nWould you like to open it?", 
                        "Open Settings Window");
                    EnumCreatorSettingsWindow.ShowWindow();
                    return;
                }
            }

            // Create the settings asset
            var settings = ScriptableObject.CreateInstance<EnumCreatorSettings>();
            string assetPath = "Assets/EnumCreator/Settings/EnumCreatorSettings.asset";
            
            AssetDatabase.CreateAsset(settings, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Select and ping the created asset
            Selection.activeObject = settings;
            EditorGUIUtility.PingObject(settings);
            
            Debug.Log($"Created Enum Creator Settings asset at: {assetPath}");
            
            // Show the settings window
            EnumCreatorSettingsWindow.ShowWindow();
        }


        [MenuItem("Tools/Enum Creator/About")]
        public static void ShowAbout()
        {
            EditorUtility.DisplayDialog("Enum Creator", 
                "Enum Creator v1.0\n\nA powerful Unity editor tool for creating and managing enums with automatic code generation.\n\nFeatures:\n• Visual enum editor\n• Automatic code generation\n• Settings management\n• File watching and sync\n• Tooltips support\n• Flags enum support\n\nCreated by: Nicholas R. Gallo", 
                "OK");
        }
    }
}

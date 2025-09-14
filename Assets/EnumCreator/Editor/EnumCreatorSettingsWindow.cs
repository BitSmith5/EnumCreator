using UnityEditor;
using UnityEngine;

namespace EnumCreator.Editor
{
    /// <summary>
    /// Settings window for the Enum Creator tool
    /// </summary>
    public class EnumCreatorSettingsWindow : EditorWindow
    {
        private EnumCreatorSettings settings;
        private Vector2 scrollPosition;
        private bool hasUnsavedChanges_local = false;

        private const string SETTINGS_ASSET_PATH = "Assets/EnumCreator/Settings/EnumCreatorSettings.asset";
        private const string SETTINGS_FOLDER_PATH = "Assets/EnumCreator/Settings";

        [MenuItem("Tools/Enum Creator/Settings")]
        public static void ShowWindow()
        {
            var window = GetWindow<EnumCreatorSettingsWindow>("Enum Creator Settings");
            window.minSize = new Vector2(400, 600);
            window.maxSize = new Vector2(600, 800);
            window.Show();
        }

        private void OnEnable()
        {
            LoadSettings();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            
            // Header
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Enum Creator Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            // Settings asset info
            DrawSettingsAssetInfo();
            EditorGUILayout.Space(10);
            
            if (settings == null)
            {
                EditorGUILayout.HelpBox("No settings asset found. Click 'Create Settings Asset' to create one.", MessageType.Warning);
                if (GUILayout.Button("Create Settings Asset"))
                {
                    CreateSettingsAsset();
                }
                EditorGUILayout.EndVertical();
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            EditorGUI.BeginChangeCheck();
            
            // Default Values Section
            DrawSectionHeader("Default Values");
            DrawDefaultValuesSection();
            
            EditorGUILayout.Space(10);
            
            // Generation Options Section
            DrawSectionHeader("Generation Options");
            DrawGenerationOptionsSection();
            
            // Check for changes
            if (EditorGUI.EndChangeCheck())
            {
                hasUnsavedChanges_local = true;
                EditorUtility.SetDirty(settings);
                
            }
            
            EditorGUILayout.EndScrollView();
            
            // Action buttons
            EditorGUILayout.Space(10);
            DrawActionButtons();
            
            EditorGUILayout.EndVertical();
        }

        private void DrawSettingsAssetInfo()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Settings Asset:", GUILayout.Width(100));
            
            if (settings != null)
            {
                EditorGUILayout.ObjectField(settings, typeof(EnumCreatorSettings), false);
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeObject = settings;
                    EditorGUIUtility.PingObject(settings);
                }
            }
            else
            {
                EditorGUILayout.LabelField("None", EditorStyles.miniLabel);
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSectionHeader(string title)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.Space(2);
        }

        private void DrawDefaultValuesSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Default Namespace
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Default Namespace:", "The default namespace that will be used for all newly created enum definitions. This helps maintain consistent code organization across your project."), GUILayout.Width(150));
            settings.MutableDefaultNamespace = EditorGUILayout.TextField(settings.MutableDefaultNamespace);
            EditorGUILayout.EndHorizontal();
            
            
            // Prevent Value Name Changes
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Prevent Value Name Changes:", "When enabled, enum value names that already exist in generated enum files become read-only, preventing accidental changes. New values can still be added freely. This protects existing values while allowing new ones."), GUILayout.Width(180));
            settings.MutablePreventValueNameChanges = EditorGUILayout.Toggle(settings.MutablePreventValueNameChanges);
            EditorGUILayout.EndHorizontal();
            
            // Generated Enums Path
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Generated Enums Path:", "The folder path where generated enum .cs files will be saved. Use forward slashes and ensure the path exists or can be created."), GUILayout.Width(150));
            settings.MutableGeneratedEnumsPath = EditorGUILayout.TextField(settings.MutableGeneratedEnumsPath);
            EditorGUILayout.EndHorizontal();
            
            // Default Use Flags
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Default Use Flags:", "Whether newly created enums should use the [System.Flags] attribute by default. Flags enums allow multiple values to be combined using bitwise operations."), GUILayout.Width(150));
            settings.MutableDefaultUseFlags = EditorGUILayout.Toggle(settings.MutableDefaultUseFlags);
            EditorGUILayout.EndHorizontal();
            
            // Use Powers of 2 for Unflagged
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Use Powers of 2 for Unflagged:", "Whether unflagged enums should use powers of 2 numbering (1, 2, 4, 8...) or sequential numbering (0, 1, 2, 3...). Powers of 2 ensures backward compatibility when toggling flags."), GUILayout.Width(200));
            settings.MutableUsePowersOfTwoForUnflagged = EditorGUILayout.Toggle(settings.MutableUsePowersOfTwoForUnflagged);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }

        private void DrawGenerationOptionsSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            
            // Include Tooltips
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Include Tooltips:", "When enabled, [UnityEngine.Tooltip] attributes will be included in the generated enum files for values that have tooltips defined. This provides IntelliSense documentation in your IDE."), GUILayout.Width(150));
            settings.MutableIncludeTooltips = EditorGUILayout.Toggle(settings.MutableIncludeTooltips);
            EditorGUILayout.EndHorizontal();
            
            // Include Auto Generated Header
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Include Auto Generated Header:", "When enabled, auto-generated header comments will be added to the top of generated enum files. These headers identify the file as auto-generated."), GUILayout.Width(200));
            settings.MutableIncludeAutoGeneratedHeader = EditorGUILayout.Toggle(settings.MutableIncludeAutoGeneratedHeader);
            EditorGUILayout.EndHorizontal();
            
            
            
            EditorGUILayout.EndVertical();
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal();
            
            // Save button
            GUI.enabled = hasUnsavedChanges_local;
            if (GUILayout.Button("Save Settings"))
            {
                SaveSettings();
            }
            GUI.enabled = true;
            
            // Reset to defaults button
            if (GUILayout.Button("Reset to Defaults"))
            {
                if (EditorUtility.DisplayDialog("Reset Settings", 
                    "Are you sure you want to reset all settings to their default values?", 
                    "Reset", "Cancel"))
                {
                    ResetToDefaults();
                }
            }
            
            // Reload settings button
            if (GUILayout.Button("Reload Settings"))
            {
                LoadSettings();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Status indicator
            if (hasUnsavedChanges_local)
            {
                EditorGUILayout.HelpBox("You have unsaved changes. Click 'Save Settings' to apply them.", MessageType.Info);
            }
        }

        private void LoadSettings()
        {
            // Try to find existing settings asset
            string[] guids = AssetDatabase.FindAssets("t:EnumCreatorSettings");
            
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                settings = AssetDatabase.LoadAssetAtPath<EnumCreatorSettings>(path);
            }
            else
            {
                settings = null;
            }
            
            hasUnsavedChanges_local = false;
        }

        private void CreateSettingsAsset()
        {
            // Create Settings folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder("Assets/EnumCreator/Settings"))
            {
                AssetDatabase.CreateFolder("Assets/EnumCreator", "Settings");
            }
            
            // Create the settings asset
            settings = CreateInstance<EnumCreatorSettings>();
            AssetDatabase.CreateAsset(settings, SETTINGS_ASSET_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Created Enum Creator Settings asset at: {SETTINGS_ASSET_PATH}");
            
            hasUnsavedChanges_local = false;
        }

        private void SaveSettings()
        {
            if (settings != null)
            {
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
                hasUnsavedChanges_local = false;
                Debug.Log("Enum Creator Settings saved successfully.");
            }
        }

        private void ResetToDefaults()
        {
            if (settings != null)
            {
                // Reset all values to defaults
                settings.MutableDefaultNamespace = "Game.Enums";
                settings.MutablePreventValueNameChanges = false;
                settings.MutableGeneratedEnumsPath = "Assets/GeneratedEnums";
                settings.MutableDefaultUseFlags = false;
                settings.MutableUsePowersOfTwoForUnflagged = true;
                settings.MutableIncludeTooltips = true;
                settings.MutableIncludeAutoGeneratedHeader = true;
                
                EditorUtility.SetDirty(settings);
                hasUnsavedChanges_local = true;
            }
        }

        private void OnDestroy()
        {
            // Auto-save on window close if there are unsaved changes
            if (hasUnsavedChanges_local && settings != null)
            {
                if (EditorUtility.DisplayDialog("Unsaved Changes", 
                    "You have unsaved changes. Would you like to save them before closing?", 
                    "Save", "Don't Save"))
                {
                    SaveSettings();
                }
            }
        }


    }
}

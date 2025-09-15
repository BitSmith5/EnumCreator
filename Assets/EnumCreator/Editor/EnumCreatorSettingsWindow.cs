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
        private Texture2D logoTexture;
        private GUIStyle headerStyle;
        private GUIStyle sectionStyle;
        private GUIStyle versionStyle;

        private const string SETTINGS_ASSET_PATH = "Assets/EnumCreator/Settings/EnumCreatorSettings.asset";
        private const string SETTINGS_FOLDER_PATH = "Assets/EnumCreator/Settings";
        private const string VERSION = "1.0.0";
        private const string COMPANY_NAME = "EnumCreator Pro";

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
            InitializeStyles();
            LoadLogo();
        }

        private void InitializeStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 18,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = EditorGUIUtility.isProSkin ? 
                        new Color(0.6f, 0.8f, 1f) : new Color(0.2f, 0.4f, 0.8f) }
                };
            }

            if (sectionStyle == null)
            {
                sectionStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 12,
                    normal = { textColor = EditorGUIUtility.isProSkin ? 
                        new Color(0.7f, 0.7f, 0.7f) : new Color(0.3f, 0.3f, 0.3f) }
                };
            }

            if (versionStyle == null)
            {
                versionStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleRight,
                    normal = { textColor = EditorGUIUtility.isProSkin ? 
                        new Color(0.6f, 0.6f, 0.6f) : new Color(0.5f, 0.5f, 0.5f) }
                };
            }
        }

        private void LoadLogo()
        {
            // Use the professional logo from EnumCreatorLogo class
            logoTexture = EnumCreatorLogo.LogoTexture;
        }

        private void OnGUI()
        {
            // Ensure styles are initialized
            if (versionStyle == null)
            {
                InitializeStyles();
            }
            
            EditorGUILayout.BeginVertical();
            
            // Professional Header with Logo
            DrawProfessionalHeader();
            
            EditorGUILayout.Space(10);
            
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

        private void DrawProfessionalHeader()
        {
            // Header background
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Center content vertically within the rectangle
            EditorGUILayout.BeginVertical();
            
            // Logo and title section
            EditorGUILayout.BeginHorizontal();
            
            // Small left margin for logo
            EditorGUILayout.Space(5);
            
            // Small top margin to center logo vertically
            EditorGUILayout.BeginVertical();
            
            // Logo - pixel perfect scaling
            if (logoTexture != null)
            {
                // Calculate optimal size based on texture dimensions
                float aspectRatio = (float)logoTexture.width / logoTexture.height;
                int maxSize = 120; // Much larger - twice as tall and proportionally wider
                
                int logoWidth, logoHeight;
                if (aspectRatio > 1) // Wider than tall
                {
                    logoWidth = maxSize;
                    logoHeight = Mathf.RoundToInt(maxSize / aspectRatio);
                }
                else // Taller than wide or square
                {
                    logoHeight = maxSize;
                    logoWidth = Mathf.RoundToInt(maxSize * aspectRatio);
                }
                
                // Create a GUIStyle with pixel perfect settings
                GUIStyle logoStyle = new GUIStyle();
                logoStyle.imagePosition = ImagePosition.ImageOnly;
                
                GUILayout.Label(logoTexture, logoStyle, GUILayout.Width(logoWidth), GUILayout.Height(logoHeight));
            }
            
            EditorGUILayout.EndVertical();
            
            // Spacer to push version to the right
            GUILayout.FlexibleSpace();
            
            // Version info (right aligned)
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(10);
            if (versionStyle != null)
            {
                EditorGUILayout.LabelField($"v{VERSION}", versionStyle, GUILayout.Width(50));
            }
            else
            {
                EditorGUILayout.LabelField($"v{VERSION}", GUILayout.Width(50));
            }
            EditorGUILayout.Space(5); // Small right margin
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        private void DrawSectionHeader(string title)
        {
            EditorGUILayout.Space(8);
            
            // Section header with icon
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("⚙", GUILayout.Width(20));
            EditorGUILayout.LabelField(title, sectionStyle);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(3);
        }

        private void DrawDefaultValuesSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Default Namespace
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Default Namespace:", "The default namespace that will be used for all newly created enum definitions. This helps maintain consistent code organization across your project."), GUILayout.Width(200));
            settings.MutableDefaultNamespace = EditorGUILayout.TextField(settings.MutableDefaultNamespace);
            EditorGUILayout.EndHorizontal();
            
            
            // Prevent Value Name Changes
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Prevent Value Name Changes:", "When enabled, enum value names that already exist in generated enum files become read-only, preventing accidental changes. New values can still be added freely. This protects existing values while allowing new ones."), GUILayout.Width(200));
            settings.MutablePreventValueNameChanges = EditorGUILayout.Toggle(settings.MutablePreventValueNameChanges);
            EditorGUILayout.EndHorizontal();
            
            // Generated Enums Path
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Generated Enums Path:", "The folder path where generated enum .cs files will be saved. Use forward slashes and ensure the path exists or can be created."), GUILayout.Width(200));
            settings.MutableGeneratedEnumsPath = EditorGUILayout.TextField(settings.MutableGeneratedEnumsPath);
            EditorGUILayout.EndHorizontal();
            
            // Default Multi-Select Enum
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Multi-Select Enum (use flags):", "Whether newly created enums should allow multiple selection by default. This enables selecting multiple enum values at once in Unity's inspector."), GUILayout.Width(200));
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
            EditorGUILayout.LabelField(new GUIContent("Include Tooltips:", "When enabled, [UnityEngine.Tooltip] attributes will be included in the generated enum files for values that have tooltips defined. This provides IntelliSense documentation in your IDE."), GUILayout.Width(200));
            settings.MutableIncludeTooltips = EditorGUILayout.Toggle(settings.MutableIncludeTooltips);
            EditorGUILayout.EndHorizontal();
            
            // Include Auto Generated Header
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Include Auto Generated Header:", "When enabled, auto-generated header comments will be added to the top of generated enum files. These headers identify the file as auto-generated."), GUILayout.Width(200));
            settings.MutableIncludeAutoGeneratedHeader = EditorGUILayout.Toggle(settings.MutableIncludeAutoGeneratedHeader);
            EditorGUILayout.EndHorizontal();
            
            // Show Enable/Disable Controls
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Show Enable/Disable Controls:", "When enabled, enable/disable checkboxes and labels will be shown for each enum value in the inspector. This allows you to temporarily disable enum values without removing them."), GUILayout.Width(200));
            settings.MutableShowEnableDisableControls = EditorGUILayout.Toggle(settings.MutableShowEnableDisableControls);
            EditorGUILayout.EndHorizontal();
            
            
            
            EditorGUILayout.EndVertical();
        }

        private void DrawActionButtons()
        {
            // Action buttons section header
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Actions", sectionStyle);
            EditorGUILayout.Space(3);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            
            // Save button with icon
            GUI.enabled = hasUnsavedChanges_local;
            if (GUILayout.Button("💾 Save Settings", GUILayout.Height(25)))
            {
                SaveSettings();
            }
            GUI.enabled = true;
            
            // Reset to defaults button with icon
            if (GUILayout.Button("🔄 Reset to Defaults", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("Reset Settings", 
                    "Are you sure you want to reset all settings to their default values?", 
                    "Reset", "Cancel"))
                {
                    ResetToDefaults();
                }
            }
            
            // Reload settings button with icon
            if (GUILayout.Button("🔄 Reload Settings", GUILayout.Height(25)))
            {
                LoadSettings();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Help and documentation buttons - temporarily removed until URLs are available
            // EditorGUILayout.Space(5);
            // EditorGUILayout.BeginHorizontal();
            // 
            // if (GUILayout.Button("📖 Documentation", GUILayout.Height(22)))
            // {
            //     Application.OpenURL("https://github.com/yourusername/enumcreator"); // Replace with your actual URL
            // }
            // 
            // if (GUILayout.Button("🐛 Report Bug", GUILayout.Height(22)))
            // {
            //     Application.OpenURL("https://github.com/yourusername/enumcreator/issues"); // Replace with your actual URL
            // }
            // 
            // EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            // Status indicator
            if (hasUnsavedChanges_local)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("⚠️ You have unsaved changes. Click 'Save Settings' to apply them.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("✅ All settings are saved.", MessageType.Info);
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
            
            // Don't clean up logo texture here - it's shared across instances
            // The logo will be cleaned up automatically when Unity unloads the domain
        }


    }
}

using UnityEditor;
using UnityEngine;

namespace EnumEditor.Editor
{
    public class EnumEditorSettingsWindow : EditorWindow
    {
        private EnumEditorSettings settings;
        private Vector2 scrollPosition;
        
        [MenuItem("Tools/EnumEditor/Settings Window", false, 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<EnumEditorSettingsWindow>("EnumEditor Settings");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }
        
        private void OnEnable()
        {
            settings = EnumEditorSettingsManager.GetOrCreateSettings();
        }
        
        private void OnGUI()
        {
            if (settings == null)
            {
                EditorGUILayout.HelpBox("Settings not found. Creating new settings...", MessageType.Warning);
                settings = EnumEditorSettingsManager.GetOrCreateSettings();
                return;
            }
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            EditorGUILayout.Space(10);
            
            // Header
            EditorGUILayout.LabelField("EnumEditor Settings", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("EnumEditor allows you to add new values to existing enums directly from the Inspector. Configure the tool's behavior here.", MessageType.Info);
            EditorGUILayout.Space(5);
            
            // General Settings
            DrawSectionHeader("General Settings");
            
            var enableToggle = new GUIContent("Enable EnumEditor", "Master switch to enable/disable the EnumEditor functionality globally");
            settings.enableEnumEditor = EditorGUILayout.Toggle(enableToggle, settings.enableEnumEditor);
            
            var confirmToggle = new GUIContent("Show Confirmation Dialog", "Show a confirmation popup before adding new enum values");
            settings.showConfirmationDialog = EditorGUILayout.Toggle(confirmToggle, settings.showConfirmationDialog);
            
            EditorGUILayout.Space(10);
            
            // Search Behavior (automatic)
            DrawSectionHeader("Search Behavior");
            EditorGUILayout.HelpBox("Search is automatically optimized: EnumEditor first searches the current script, then expands to all folders if not found.", MessageType.Info);
            
            EditorGUILayout.Space(10);
            
            // UI Settings
            DrawSectionHeader("UI Settings");
            
            var buttonTextContent = new GUIContent("Add Button Text", "Text displayed on the button that adds new enum values");
            settings.addButtonText = EditorGUILayout.TextField(buttonTextContent, settings.addButtonText);
            
            var fieldLabelContent = new GUIContent("New Value Field Label", "Label displayed next to the input field where users type new enum values");
            settings.newValueFieldLabel = EditorGUILayout.TextField(fieldLabelContent, settings.newValueFieldLabel);
            
            EditorGUILayout.Space(10);
            
            // Validation Settings
            DrawSectionHeader("Validation Settings");
            
            var preventDuplicatesContent = new GUIContent("Prevent Duplicates", "Check if the new enum value already exists before adding it");
            settings.preventDuplicates = EditorGUILayout.Toggle(preventDuplicatesContent, settings.preventDuplicates);
            
            EditorGUILayout.HelpBox("All enum values are automatically validated against C# identifier rules. Invalid names will show error messages.", MessageType.Info);
            
            EditorGUILayout.Space(20);
            
            // Buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button(new GUIContent("Reset to Defaults", "Reset all EnumEditor settings back to their original default values")))
            {
                if (EditorUtility.DisplayDialog("Reset Settings", 
                    "Are you sure you want to reset all settings to their default values?", 
                    "Yes", "Cancel"))
                {
                    EnumEditorSettingsManager.ResetSettings();
                    settings = EnumEditorSettingsManager.GetOrCreateSettings();
                }
            }
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button(new GUIContent("Save Settings", "Manually save the current EnumEditor settings (auto-saves on change)")))
            {
                EnumEditorSettingsManager.SaveSettings();
                EditorUtility.DisplayDialog("Settings Saved", "EnumEditor settings have been saved.", "OK");
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            // Status
            DrawSectionHeader("Status");
            EditorGUILayout.LabelField("EnumEditor Status:", settings.enableEnumEditor ? "Enabled" : "Disabled");
            
            var enums = EnumDetector.FindEnumsInProject();
            EditorGUILayout.LabelField("Enums Found:", enums.Count.ToString());
            
            EditorGUILayout.EndScrollView();
            
            // Auto-save on change
            if (GUI.changed)
            {
                EditorUtility.SetDirty(settings);
            }
        }
        
        private void DrawSectionHeader(string title)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.Space(2);
        }
    }
}

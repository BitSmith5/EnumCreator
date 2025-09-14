using UnityEditor;
using UnityEngine;

namespace EnumCreator.Editor
{
    /// <summary>
    /// Simple input dialog for getting enum names from the user
    /// </summary>
    public class EnumNameInputDialog : EditorWindow
    {
        private string inputText = "MyEnum";
        private System.Action<string> onComplete;
        private bool focused = false;

        public static void ShowDialog(System.Action<string> onComplete)
        {
            var dialog = CreateInstance<EnumNameInputDialog>();
            dialog.onComplete = onComplete;
            
            // Center the dialog on the Unity screen
            var mainWindow = EditorGUIUtility.GetMainWindowPosition();
            var dialogSize = new Vector2(400, 200);
            var centerX = mainWindow.x + (mainWindow.width - dialogSize.x) * 0.5f;
            var centerY = mainWindow.y + (mainWindow.height - dialogSize.y) * 0.5f;
            
            dialog.position = new Rect(centerX, centerY, dialogSize.x, dialogSize.y);
            dialog.minSize = dialogSize;
            dialog.maxSize = dialogSize;
            
            dialog.ShowModalUtility();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            
            // Title
            GUILayout.Label("Create New Enum File", EditorStyles.boldLabel);
            GUILayout.Space(5);
            
            // Message
            GUILayout.Label("Enter the name for your enum:", EditorStyles.wordWrappedLabel);
            GUILayout.Space(10);
            
            // Input field
            GUI.SetNextControlName("InputField");
            inputText = EditorGUILayout.TextField("Enum Name:", inputText);
            
            // Focus the input field on first draw
            if (!focused)
            {
                EditorGUI.FocusTextInControl("InputField");
                focused = true;
            }
            
            GUILayout.Space(20);
            
            // Buttons
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Create") || (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return))
            {
                if (!string.IsNullOrEmpty(inputText.Trim()))
                {
                    onComplete?.Invoke(inputText.Trim());
                    Close();
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Input", "Please enter a valid enum name.", "OK");
                }
            }
            
            if (GUILayout.Button("Cancel") || (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape))
            {
                onComplete?.Invoke(null);
                Close();
            }
            
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
        }
    }
}

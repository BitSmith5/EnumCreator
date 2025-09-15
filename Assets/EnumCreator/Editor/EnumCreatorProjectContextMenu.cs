using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EnumCreator.Editor
{
    /// <summary>
    /// Context menu items for creating enum files in the Project window
    /// </summary>
    public class EnumCreatorProjectContextMenu
    {
        [MenuItem("Assets/Create/Enum Creator/Enum File", false, 100)]
        public static void CreateEnumFileFromProject()
        {
            // Get the selected folder path
            string targetPath = GetSelectedPathOrFallback();
            
            // Check if we're in the generated enums folder or a subfolder
            var settings = EnumCreatorSettingsManager.GetOrCreateSettings();
            string generatedPath = settings?.GeneratedEnumsPath ?? "Assets/GeneratedEnums";
            
            if (!targetPath.StartsWith(generatedPath))
            {
                // If not in generated folder, ask if user wants to create in generated folder
                bool useGeneratedFolder = EditorUtility.DisplayDialog("Create Enum File", 
                    $"The selected folder is not in the Generated Enums folder.\n\n" +
                    $"Generated Enums folder: {generatedPath}\n" +
                    $"Selected folder: {targetPath}\n\n" +
                    $"Would you like to create the enum file in the Generated Enums folder instead?",
                    "Use Generated Folder", "Cancel");
                
                if (useGeneratedFolder)
                {
                    targetPath = generatedPath;
                }
                else
                {
                    return;
                }
            }
            
            // Show input dialog to get enum name
            EnumNameInputDialog.ShowDialog((enumName) =>
            {
                if (!string.IsNullOrEmpty(enumName))
                {
                    CreateEnumFileInPath(enumName, targetPath);
                }
            });
        }
        
        [MenuItem("Assets/Create/Enum Creator/Enum File", true)]
        public static bool CreateEnumFileFromProjectValidate()
        {
            // Only show the menu item when right-clicking on folders or the project root
            string selectedPath = GetSelectedPathOrFallback();
            return !string.IsNullOrEmpty(selectedPath);
        }
        
        private static string GetSelectedPathOrFallback()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            
            if (string.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!AssetDatabase.IsValidFolder(path))
            {
                // If selected object is not a folder, get its parent folder
                path = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            }
            
            return path;
        }
        
        private static void CreateEnumFileInPath(string enumName, string targetPath)
        {
            // Validate enum name
            if (!IsValidEnumName(enumName))
            {
                EditorUtility.DisplayDialog("Invalid Enum Name", 
                    "Enum names must start with a letter and contain only letters, digits, and underscores.", "OK");
                return;
            }
            
            // Ensure the target directory exists
            if (!System.IO.Directory.Exists(targetPath))
            {
                System.IO.Directory.CreateDirectory(targetPath);
            }
            
            // Check if file already exists
            string filePath = System.IO.Path.Combine(targetPath, $"{enumName}.cs");
            if (System.IO.File.Exists(filePath))
            {
                bool overwrite = EditorUtility.DisplayDialog("File Exists", 
                    $"A file named '{enumName}.cs' already exists. Do you want to overwrite it?", 
                    "Overwrite", "Cancel");
                
                if (!overwrite)
                {
                    return;
                }
            }
            
            // Get settings for default values
            var settings = EnumCreatorSettingsManager.GetOrCreateSettings();
            
            // Create a basic enum file template
            using (var writer = new System.IO.StreamWriter(filePath))
            {
                writer.WriteLine($"namespace {settings?.DefaultNamespace ?? "Game.Enums"}");
                writer.WriteLine("{");
                
                // Add flags attribute if default setting is enabled
                if (settings?.DefaultUseFlags == true)
                {
                    writer.WriteLine("    [System.Flags]");
                }
                
                writer.WriteLine($"    public enum {enumName}");
                writer.WriteLine("    {");
                writer.WriteLine("    }");
                writer.WriteLine("}");
            }
            
            AssetDatabase.Refresh();
            
            // Select the newly created file in the Project window
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filePath);
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
        
        private static bool IsValidEnumName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
                
            // Check if name starts with a letter or underscore
            if (!char.IsLetter(name[0]) && name[0] != '_')
                return false;
                
            // Check if all characters are valid (letters, digits, underscores)
            for (int i = 1; i < name.Length; i++)
            {
                if (!char.IsLetterOrDigit(name[i]) && name[i] != '_')
                    return false;
            }
            
            // Check if it's not a C# keyword
            string[] keywords = { "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override", "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while" };
            
            return !keywords.Contains(name.ToLower());
        }
    }
}

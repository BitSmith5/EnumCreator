using UnityEngine;

namespace EnumEditor
{
    [CreateAssetMenu(menuName = "Enum Editor/Settings", fileName = "EnumEditorSettings")]
    public class EnumEditorSettings : ScriptableObject
    {
        [Header("General Settings")]
        [Tooltip("Enable or disable the EnumEditor functionality")]
        public bool enableEnumEditor = true;
        
        [Tooltip("Show confirmation dialog when adding new enum values")]
        public bool showConfirmationDialog = true;
        
        
        [Header("UI Settings")]
        
        [Tooltip("Button text for adding new enum values")]
        public string addButtonText = "+";
        
        [Tooltip("Field label for new enum value input")]
        public string newValueFieldLabel = "New Value:";
        
        [Header("Validation")]
        
        [Tooltip("Prevent duplicate enum values")]
        public bool preventDuplicates = true;
        
        private static EnumEditorSettings _instance;
        
        public static EnumEditorSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<EnumEditorSettings>("EnumEditorSettings");
                    if (_instance == null)
                    {
                        _instance = CreateInstance<EnumEditorSettings>();
                    }
                }
                return _instance;
            }
        }
        
        private void OnEnable()
        {
            _instance = this;
        }
    }
}

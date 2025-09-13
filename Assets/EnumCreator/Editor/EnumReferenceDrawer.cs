using UnityEditor;
using UnityEngine;

namespace EnumCreator.Editor
{
    [CustomPropertyDrawer(typeof(EnumReference<>), true)]
    public class EnumReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (Application.isPlaying)
            {
                EditorGUI.LabelField(position, label.text, "Cannot edit at runtime");
                return;
            }

            EditorGUI.PropertyField(position, property.FindPropertyRelative("value"), label, true);
        }
    }
}

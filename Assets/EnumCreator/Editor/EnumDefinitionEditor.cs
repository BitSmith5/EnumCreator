using UnityEditor;
using UnityEngine;

namespace EnumCreator.Editor
{
    [CustomEditor(typeof(EnumCreator.EnumDefinition))]
    public class EnumDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var def = (EnumCreator.EnumDefinition)target;
            var enumNameProp = serializedObject.FindProperty("enumName");
            var nsProp = serializedObject.FindProperty("namespace");
            var valuesProp = serializedObject.FindProperty("values");
            var removedValuesProp = serializedObject.FindProperty("removedValues");
            var useFlagsProp = serializedObject.FindProperty("useFlags");

            EditorGUILayout.PropertyField(enumNameProp);
            EditorGUILayout.PropertyField(nsProp);
            EditorGUILayout.PropertyField(useFlagsProp, new GUIContent("Use as Flags"));

            EditorGUILayout.Space();

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Cannot edit enum values during Play Mode.", MessageType.Warning);
                GUI.enabled = false;
            }

            EditorGUILayout.LabelField("Values", EditorStyles.boldLabel);
            for (int i = 0; i < valuesProp.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(valuesProp.GetArrayElementAtIndex(i), GUIContent.none);
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    var removedName = valuesProp.GetArrayElementAtIndex(i).stringValue;
                    removedValuesProp.arraySize++;
                    removedValuesProp.GetArrayElementAtIndex(removedValuesProp.arraySize - 1).stringValue = removedName;
                    valuesProp.DeleteArrayElementAtIndex(i);
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Add Value"))
                valuesProp.InsertArrayElementAtIndex(valuesProp.arraySize);

            GUI.enabled = true;

            EditorGUILayout.Space();
            if (GUILayout.Button("Apply Changes"))
            {
                serializedObject.ApplyModifiedProperties();
                EnumGenerator.Generate(def);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

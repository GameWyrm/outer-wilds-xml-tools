using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(XMLEditorSettings))]
public class XMLEditorSettingsEditor : Editor
{

    private static XMLEditorSettings instance;

    private static bool showConditions;
    private static bool showPersistentConditions;
    //private static bool 

    private void OnEnable()
    {
        instance = serializedObject.targetObject as XMLEditorSettings;
    }

    public override void OnInspectorGUI()
    {
        List<string> loopConditions = instance.GetConditionList(false);
        List<string> persistentConditions = instance.GetConditionList(true);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("modPrefix"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(instance.conditionCase)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(instance.shipLogCase)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(instance.translationCase)));

        loopConditions = GUIBuilder.CreateArray(ref showConditions, loopConditions, "Loop Conditions", "Condition", instance.conditionCase);
        persistentConditions = GUIBuilder.CreateArray(ref showPersistentConditions, persistentConditions, "Persistent Conditions", "Condition", instance.conditionCase);

        instance.SetConditionList(loopConditions, false);
        instance.SetConditionList(persistentConditions, true);

        serializedObject.ApplyModifiedProperties();
    }

}

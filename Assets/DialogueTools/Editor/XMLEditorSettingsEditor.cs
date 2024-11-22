using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(XMLEditorSettings))]
public class XMLEditorSettingsEditor : Editor
{

    private static XMLEditorSettings instance;
    private static LanguageType selectedLanguage;
    private static bool showLanguages;
    private static bool showConditions;
    private static bool showPersistentConditions;
    //private static bool 
    private static string customLanguageName = "new_custom_lang";

    private void OnEnable()
    {
        instance = serializedObject.targetObject as XMLEditorSettings;
    }

    public override void OnInspectorGUI()
    {

        List<string> loopConditions = instance.GetConditionList(false);
        List<string> persistentConditions = instance.GetConditionList(true);

        showLanguages = EditorGUILayout.BeginFoldoutHeaderGroup(showLanguages, "Supported Languages");
        if (showLanguages)
        {
            List<string> languageNames = instance.supportedLanguages.Select(x => x.languageID).ToList();
            foreach (string languageName in languageNames)
            {
                EditorGUILayout.LabelField(languageName);
            }
            selectedLanguage = (LanguageType)EditorGUILayout.EnumPopup("Create new language", selectedLanguage);
            string newLanguageName = Language.GetLanguageFileName[selectedLanguage];
            if (selectedLanguage == LanguageType.Custom)
            {
                customLanguageName = EditorGUILayout.DelayedTextField("Custom Language File Name", customLanguageName);
            }
            bool createNewLanguage = GUILayout.Button("Create New Language Asset");
            string lang = selectedLanguage == LanguageType.Custom ? customLanguageName : newLanguageName;
            if (createNewLanguage) CreateNewLanguage(lang);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

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

    private void CreateNewLanguage(string languageName)
    { 
    
    }
}

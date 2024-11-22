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
            instance.supportedLanguages.RemoveAll(x => x == null);
            
            if (instance.supportedLanguages == null) instance.supportedLanguages = new List<Language>();
            List<string> languageNames = instance.supportedLanguages.Select(x => x.name).ToList();
            foreach (Language supportedLanguage in instance.supportedLanguages)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(supportedLanguage.name);
                if (GUILayout.Button("Sellect Asset"))
                {
                    Selection.activeObject = supportedLanguage;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (languageNames.Count > 0)
            {
                int index = 0;
                if (languageNames.Contains(instance.defaultLanguage)) index = languageNames.IndexOf(instance.defaultLanguage);
                else Debug.LogWarning($"Could not find language {instance.defaultLanguage} in the supported languages list! Defaulting to {languageNames[0]}, but this may lead to unexpected behavior.");
                index = EditorGUILayout.Popup("Default Language: ", index, languageNames.ToArray());
                instance.defaultLanguage = languageNames[index];
            }
            selectedLanguage = (LanguageType)EditorGUILayout.EnumPopup("Create new language", selectedLanguage);
            string newLanguageName = Language.GetLanguageFileName[selectedLanguage];
            if (selectedLanguage == LanguageType.Custom)
            {
                customLanguageName = EditorGUILayout.DelayedTextField("Custom Language File Name", customLanguageName);
            }
            string lang = selectedLanguage == LanguageType.Custom ? customLanguageName : newLanguageName;
            if (languageNames.Contains(lang))
            {
                EditorGUILayout.HelpBox($"Language \"{lang}\" already exists. You cannot add another language of this type.", MessageType.Error);
            }
            else
            {
                bool createNewLanguage = GUILayout.Button($"Create New Language Asset \"{lang}\"");
                if (createNewLanguage) CreateNewLanguage(lang, selectedLanguage);
            }
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

    private void CreateNewLanguage(string languageName, LanguageType type)
    {
        string[] pathPieces = AssetDatabase.GetAssetPath(instance).Split('/');
        string sourceName = pathPieces[pathPieces.Length - 1];
        string path = AssetDatabase.GetAssetPath(instance).Replace(sourceName, $"{languageName}.asset");

        Language lang = ScriptableObject.CreateInstance<Language>();
        lang.name = languageName;
        lang.type = type;

        if (instance.supportedLanguages != null && instance.supportedLanguages.Count > 1 && !string.IsNullOrEmpty(instance.defaultLanguage))
        {
            Language defaultLanguage = instance.supportedLanguages.Find(x => x.name == instance.defaultLanguage);
            if (defaultLanguage != null) Language.SyncTranslations(lang, defaultLanguage);
        }

        AssetDatabase.CreateAsset(lang, path);
        AssetDatabase.SaveAssets();

        if (instance.supportedLanguages == null) instance.supportedLanguages = new List<Language>();
        instance.supportedLanguages.Add(lang);
        if (string.IsNullOrEmpty(instance.defaultLanguage)) instance.defaultLanguage = languageName;

        Debug.Log("Created new language asset at path " + path);
    }
}

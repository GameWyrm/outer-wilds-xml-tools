using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;

[CustomEditor(typeof(Language))]
public class LanguageEditor : Editor
{
    private Language selectedData;
    private bool showDialogueTranslations;
    private bool showShipLogTranslations;

    private void OnEnable()
    {
        selectedData = serializedObject.targetObject as Language;
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("These values are primarily for viewing only. Be careful when editing them, and be sure to click the Parse Data button again to refresh.", MessageType.Info);
        showDialogueTranslations = EditorGUILayout.BeginFoldoutHeaderGroup(showDialogueTranslations, "Dialogue Translations");
        if (showDialogueTranslations && selectedData.dialogueKeys != null)
        {
            for (int i = 0; i < selectedData.dialogueKeys.Count; i++)
            {
                EditorGUILayout.LabelField(selectedData.dialogueKeys[i]);
                selectedData.dialogueValues[i] = EditorGUILayout.DelayedTextField(selectedData.dialogueValues[i]);
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        showShipLogTranslations = EditorGUILayout.BeginFoldoutHeaderGroup(showShipLogTranslations, "Ship Log Translations");
        if (showShipLogTranslations && selectedData.shipLogKeys != null)
        {
            for (int i = 0; i < selectedData.shipLogKeys.Count; i++)
            {
                EditorGUILayout.LabelField(selectedData.shipLogKeys[i]);
                selectedData.shipLogValues[i] = EditorGUILayout.DelayedTextField(selectedData.shipLogValues[i]);
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space(20);
        if (GUILayout.Button("Parse Data")) ParseData();

        if (selectedData.hasParsedData)
        {
            int index = EditorGUILayout.Popup("Copy to Clipboard", 0, dataSet);
            switch (index)
            {
                case 1:
                    EditorGUIUtility.systemCopyBuffer = selectedData.parsedDialogue;
                    break;
                case 2:
                    EditorGUIUtility.systemCopyBuffer = selectedData.parsedShipLogs;
                    break;
                case 3:
                    EditorGUIUtility.systemCopyBuffer = selectedData.parsedData;
                    break;
            }
            EditorGUILayout.TextArea(selectedData.parsedData, GUILayout.ExpandHeight(true));
        }
    }

    private void ParseData()
    {
        selectedData.parsedDialogue = JsonConvert.SerializeObject(selectedData.GetTranslation(true, false), Formatting.Indented);
        selectedData.parsedShipLogs = JsonConvert.SerializeObject(selectedData.GetTranslation(false, true), Formatting.Indented);
        selectedData.parsedData = JsonConvert.SerializeObject(selectedData.GetTranslation(), Formatting.Indented);
        selectedData.hasParsedData = true;
    }

    private static string[] dataSet = new string[]
    {
        "",
        "Dialogue Dictionary",
        "Ship Log Dictionary",
        "Entire File"
    };
}
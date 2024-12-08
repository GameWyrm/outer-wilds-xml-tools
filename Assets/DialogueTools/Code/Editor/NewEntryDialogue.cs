using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using XmlTools;

public class NewEntryDialogue : EditorWindow
{
    public static NewEntryDialogue Instance;

    private string entryID;
    private string entryName;
    private string curiosity;

    public static void ShowWindow(string defaultID, string defaultName, bool isChild)
    {
        Vector2 size = new Vector2(200, 100);

        Instance = GetWindow<NewEntryDialogue>();
        Instance.minSize = size;
        Instance.maxSize = size;
        Instance.entryID = defaultID;
        Instance.entryName = defaultName;
        Instance.titleContent = new GUIContent($"Create New{(isChild ? " Child " : " ")}Entry");
    }

    private void OnGUI()
    {
        entryID = EditorGUILayout.DelayedTextField("ID:", entryID);
        entryName = EditorGUILayout.DelayedTextField("Name:", entryName);

        List<string> curiositiesList = new List<string>();
        curiositiesList.Add("(None)");
        curiositiesList.AddRange(ShipLogManager.Instance.curiosities);
        curiosity = GUIBuilder.CreateDropdown("Curiosity", curiosity, curiositiesList.ToArray());

        if (GUILayout.Button("Create Entry")) CreateEntry();
    }

    private void CreateEntry()
    {

    }

    private void OnLostFocus()
    {
        Instance = null;
        Close();
    }
}
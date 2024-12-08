using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace XmlTools
{
    public class NewEntryDialogue : EditorWindow
    {
        public static NewEntryDialogue Instance;

        private string entryID;
        private string entryName;
        private string curiosity;
        private ShipLogEntry.Entry parent;
        private EntryData activeData;

        public static void ShowWindow(string defaultID, string defaultName, ShipLogEntry.Entry parent = null, EntryData data = null)
        {
            Vector2 size = new Vector2(400, 150);

            Instance = GetWindow<NewEntryDialogue>();
            Instance.minSize = size;
            Instance.maxSize = size;
            Instance.entryID = defaultID;
            Instance.entryName = defaultName;
            Instance.activeData = data;
            Instance.titleContent = new GUIContent($"Create New{(parent != null ? " Child " : " ")}Entry");
        }

        private void OnGUI()
        {
            // TODO add selection for EntryData
            EditorGUILayout.Space(20);
            entryID = EditorGUILayout.DelayedTextField("ID:", entryID);
            EditorGUILayout.Space();
            entryName = EditorGUILayout.DelayedTextField("Name:", entryName);
            EditorGUILayout.Space();

            List<string> curiositiesList = new List<string>();
            curiositiesList.Add("(None)");
            curiositiesList.AddRange(ShipLogManager.Instance.curiosities);
            curiosity = GUIBuilder.CreateDropdown("Curiosity", curiosity, curiositiesList.ToArray());
            EditorGUILayout.Space(20);

            if (GUILayout.Button("Create Entry")) CreateEntry();
        }

        private void CreateEntry()
        {
            activeData.AddEntry(entryID, entryName, parent);

            if (ShipLogEditor.Instance != null)
            {
                ShipLogEditor.Instance.BuildNodeTree();
            }
        }

        private void OnLostFocus()
        {
            Instance = null;
            Close();
        }
    }
}
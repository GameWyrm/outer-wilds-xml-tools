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
        private List<string> dataNames;
        private int selectedDataIndex;

        public static void ShowWindow(string defaultID, string defaultName, string curiosity, ShipLogEntry.Entry parent = null, EntryData data = null)
        {
            Vector2 size = new Vector2(400, 200);

            Instance = GetWindow<NewEntryDialogue>();
            Instance.minSize = size;
            Instance.maxSize = size;
            Instance.entryID = defaultID;
            Instance.entryName = defaultName;
            Instance.curiosity = curiosity;
            Instance.parent = parent;
            Instance.activeData = data;
            if (parent == null)
            {
                if (ShipLogManager.Instance.datas != null && ShipLogManager.Instance.datas.Count > 0)
                {
                    Instance.activeData = ShipLogManager.Instance.datas[0];
                    Instance.dataNames = new List<string>();
                    foreach (var entryData in ShipLogManager.Instance.datas)
                    {
                        Instance.dataNames.Add(entryData.name);
                    }
                }
            }
            Instance.titleContent = new GUIContent($"Create New{(parent != null ? " Child " : " ")}Entry");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(20);
            if (parent == null)
            {
                selectedDataIndex = EditorGUILayout.Popup(selectedDataIndex, dataNames.ToArray());
                activeData = ShipLogManager.Instance.datas[selectedDataIndex];
                EditorGUILayout.Space();
            }

            entryID = EditorGUILayout.TextField("ID:", entryID);
            EditorGUILayout.Space();
            entryName = EditorGUILayout.TextField("Name:", entryName);
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
            activeData.AddEntry(entryID, entryName, curiosity, parent);

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
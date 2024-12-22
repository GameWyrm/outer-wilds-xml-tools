using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XmlTools
{
    public class NewPlanetDialogue : EditorWindow
    {
        public static NewPlanetDialogue Instance;

        private string planetName;
        private string defaultEntryID;
        private string defaultEntryName;

        public static void ShowWindow()
        {
            Vector2 size = new Vector2(400, 200);

            Instance = GetWindow<NewPlanetDialogue>();
            Instance.minSize = size;
            Instance.maxSize = size;
            Instance.defaultEntryID = "DEFAULT_ID";
            Instance.defaultEntryName = "Default Name";
            Instance.titleContent = new GUIContent("New Shiplog File");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(20);
            planetName = EditorGUILayout.TextField("Planet name: ", planetName);
            EditorGUILayout.Space();
            defaultEntryID = EditorGUILayout.TextField("Default Entry ID: ", defaultEntryID);
            EditorGUILayout.Space();
            defaultEntryName = EditorGUILayout.TextField("Default Entry Name: ", defaultEntryName);
            EditorGUILayout.Space();
            if (GUILayout.Button("Create New Shiplog File"))
            {
                if (string.IsNullOrEmpty(planetName))
                {
                    EditorUtility.DisplayDialog("You need to specify a planet name.", "You must specify a planet name. It should match the name field at the top of your planet JSON file.", "OK");
                }
                else
                {
                    CreateNewFile();
                }
            }
        }

        private void CreateNewFile()
        {
            EntryData data = ScriptableObject.CreateInstance<EntryData>();
            data.name = planetName;
            data.entry = new ShipLogEntry();
            data.entry.planetID = planetName;
            data.entryPaths = new List<string>();
            data.entryIDs = new List<string>();
            data.entriesWhoAreChildren = new List<string>();
            data.rootEntries = new List<string>();
            data.factIDs = new List<string>();
            data.factPaths = new List<string>();
            data.nodes = new List<NodeData>();
            data.AddEntry(defaultEntryID, defaultEntryName, "");

            string savePath = EditorUtility.SaveFilePanelInProject("Save Entry Data as...", data.entry.planetID, "asset", "Select a location to save your Entry Data to.");
            if (string.IsNullOrEmpty(savePath))
            {
                Debug.LogError("Save path is invalid!");
                return;
            }

            AssetDatabase.CreateAsset(data, savePath);
            AssetDatabase.SaveAssets();

            ShipLogManager.Instance.datas.Add(data);

            if (ShipLogEditor.Instance != null)
            {
                ShipLogEditor.Instance.BuildNodeTree();
            }

            Close();
        }
    }
}
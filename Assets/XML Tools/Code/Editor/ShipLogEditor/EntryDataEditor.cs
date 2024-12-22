using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Xml.Serialization;

namespace XmlTools
{
    [CustomEditor(typeof(EntryData))]
    public class EntryDataEditor : Editor
    {
        private EntryData selectedData;
        private string parsedData;

        private void OnEnable()
        {
            selectedData = serializedObject.targetObject as EntryData;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Export XML")) Export();
            EditorGUILayout.Space(40);
            if (string.IsNullOrEmpty(parsedData))
            {
                if (GUILayout.Button("Parse Position JSON"))
                {
                    List<StarSystem.EntryPositionInfo> positions = new List<StarSystem.EntryPositionInfo>();
                    for (int i = 0; i < selectedData.nodes.Count; i++)
                    {
                        var node = selectedData.nodes[i];

                        StarSystem.EntryPositionInfo info = new StarSystem.EntryPositionInfo();

                        info.id = node.name;
                        info.position = node.position;

                        positions.Add(info);
                    }

                    StarSystem system = new StarSystem();
                    system.entryPositions = positions.ToArray();

                    parsedData = "\"entryPositions\": " + JsonConvert.SerializeObject(system.entryPositions, Formatting.Indented);
                }
            }
            else
            {
                if (GUILayout.Button("Copy To Clipboard"))
                {
                    EditorGUIUtility.systemCopyBuffer = parsedData;
                }

                EditorGUILayout.TextArea(parsedData, GUILayout.ExpandHeight(true));
            }
        }

        private void Export()
        {
            string savePath = EditorUtility.SaveFilePanelInProject("Save Ship Log Data as XML...", selectedData.name, "xml", "Select a location to export your Ship Log XML to.");

            if (!string.IsNullOrEmpty(savePath))
            {
                foreach (var entry in selectedData.entry.entries)
                {
                    FixTags(entry);
                }

                XmlSerializer serializer = new XmlSerializer(typeof(ShipLogEntry));

                using (StreamWriter writer = new StreamWriter(savePath))
                {
                    serializer.Serialize(writer, selectedData.entry);
                }
                AssetDatabase.Refresh();
                Debug.Log($"Exported dialogue xml to {savePath}");
            }
        }

        /// <summary>
        /// Fix self-closing tags and other data
        /// </summary>
        /// <param name="entry"></param>
        private void FixTags(ShipLogEntry.Entry entry)
        {
            if (entry.curiosity == "") entry.curiosity = null;
            entry.m_isCuriosity = entry.isCuriosity ? "" : null;
            entry.m_ignoreMoreToExplore = entry.ignoreMoreToExplore ? "" : null;
            entry.m_parentIgnoreNotRevealed = entry.parentIgnoreNotRevealed ? "" : null;
            if (entry.ignoreMoreToExploreCondition == "") entry.ignoreMoreToExploreCondition = null;
            if (entry.altPhotoCondition == "") entry.altPhotoCondition = null;

            foreach (var fact in entry.exploreFacts)
            {
                fact.m_ignoreMoreToExplore = fact.ignoreMoreToExplore ? "" : null;
            }

            foreach (var fact in entry.rumorFacts)
            {
                fact.m_ignoreMoreToExplore = fact.ignoreMoreToExplore ? "" : null;
                if (fact.rumorName == "")
                {
                    fact.rumorName = null;
                    // Can't figure out how to make the priority not show up though
                }
            }

            if (entry.childEntries != null && entry.childEntries.Length > 0)
            {
                foreach (var childEntry in entry.childEntries)
                {
                    FixTags(childEntry);
                }
            }
        }
    }
}

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace XmlTools
{
    //[CreateAssetMenu(fileName = "Ship Log Manager", menuName = "Tools/Ship Log Manager")]
    public class ShipLogManager : ScriptableObject
    {
        [SerializeField]
        public List<EntryData> datas;

        public static ShipLogManager Instance
        {
            get
            {
                if (instance == null)
                {
                    if (AssetDatabase.FindAssets("t:XMLTools.ShipLogManager").Length == 0) return null;
                    instance = AssetDatabase.LoadAssetAtPath<ShipLogManager>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:XmlTools.ShipLogManager")[0]));
                }
                return instance;
            }
        }

        [SerializeField]
        public List<string> allEntryPaths;
        [SerializeField]
        public List<string> allParentEntriesPaths;
        [SerializeField]
        public List<string> allEntries;

        [SerializeField]
        public List<string> allExploreFactsList;
        [SerializeField]
        public List<string> allRumorFactsList;

        [SerializeField]
        public List<string> curiosities;

        [SerializeField]
        private List<Color> curiosityColors;
        [SerializeField]
        private List<Color> curiosityHighlightColors;

        private static ShipLogManager instance;

        private void GenerateFactsLists()
        {
            allExploreFactsList = new List<string>();
            allRumorFactsList = new List<string>();
            ValidateData();
            if (datas.Count == 0) return;

            foreach (var entryFile in datas)
            {
                foreach (var entry in entryFile.entry.entries)
                {
                    if ((entry.exploreFacts == null || entry.exploreFacts.Length == 0) && (entry.rumorFacts == null || entry.rumorFacts.Length == 0)) continue;
                    string entryPath = "";
                    if (string.IsNullOrEmpty(entry.curiosity))
                    {
                        entryPath = "No Curiosity/";
                    }
                    else
                    {
                        entryPath = entry.curiosity + "/";
                    }
                    entryPath += entry.entryID + "/";
                    if (entry.exploreFacts != null)
                    {
                        foreach (var exploreFact in entry.exploreFacts)
                        {
                            string factPath = entryPath;
                            factPath += exploreFact.exploreID;
                            allExploreFactsList.Add(factPath);
                        }
                    }
                    if (entry.rumorFacts != null)
                    {
                        foreach (var rumorFact in entry.rumorFacts)
                        {
                            string factPath = entryPath;
                            factPath += rumorFact.rumorID;
                            allRumorFactsList.Add(factPath);
                        }
                    }
                }
            }
        }

        private void GeneratePaths()
        {
            allEntryPaths = new List<string>();
            allParentEntriesPaths = new List<string>();
            allEntries = new List<string>();

            foreach (var data in datas)
            {
                foreach (var entry in data.entryPaths)
                {
                    allEntryPaths.Add(entry);
                    if (!entry.Contains("/")) allParentEntriesPaths.Add(entry);
                }
                foreach (var entry in data.entryIDs)
                {
                    allEntries.Add(entry);
                }
            }
        }

        /// <summary>
        /// Maintenance step, call this whenever editing the amount of entries or facts
        /// </summary>
        public void BuildInfo()
        {
            if (datas == null) datas = new List<EntryData>();
            foreach (var item in datas)
            {
                item.BuildEntryDataPaths(false);
            }
            GenerateFactsLists();
            GeneratePaths();
        }

        public ShipLogEntry.Entry GetEntry(string entryName)
        {
            return GetEntry(entryName, out _, out _);
        }

        public ShipLogEntry.Entry GetEntry(string entryName, out EntryData data)
        {
            return GetEntry(entryName, out data, out _);
        }

        public ShipLogEntry.Entry GetEntry(string entryName, out ShipLogEntry.Entry parent)
        {
            return GetEntry(entryName, out _, out parent);
        }

        public ShipLogEntry.Entry GetEntry(string entryName, out EntryData data, out ShipLogEntry.Entry parent)
        {
            ValidateData();
            parent = null;
            foreach (var file in datas)
            {
                if (file.entryPaths == null || file.entryPaths.Count == 0) file.BuildEntryDataPaths(true);

                ShipLogEntry.Entry entry = file.GetEntry(entryName, out parent);
                if (entry != null)
                {
                    data = file;
                    return entry;
                }
            }
            data = null;
            return null;
        }

        public EntryData CreateEntryData(ShipLogEntry newEntryFile, StarSystem systemData)
        {
            ValidateData();
            if (newEntryFile.entries == null || newEntryFile.entries.Length <= 0) return null;
            EntryData data = ScriptableObject.CreateInstance<EntryData>();
            foreach (var entry in newEntryFile.entries)
            {
                FixEntryData(entry);
            }
            data.entry = newEntryFile;
            data.BuildEntryDataPaths(true);
            data.nodes = new List<NodeData>();
            if (systemData.entryPositions != null)
            {
                foreach (var node in systemData.entryPositions)
                {
                    if (!string.IsNullOrEmpty(node.id) && node.position != null && data.entryIDs.Contains(node.id))
                    {
                        NodeData newNode = new NodeData(node.id, new Vector2(node.position.x, node.position.y));
                        data.nodes.Add(newNode);
                    }
                }
            }
            if (systemData.curiosities != null)
            {
                foreach (var curiosity in systemData.curiosities)
                {
                    SetCuriosityColor(curiosity.id, curiosity.color);
                    SetCuriosityHighlightColor(curiosity.id, curiosity.highlightColor);
                }
            }

            return data;
        }

        public void CreateCuriosity(string id)
        {
            curiosities.Add(id);
            Color newColor = Random.ColorHSV(0, 1, 0.5f, 1, 0.8f, 1, 1, 1);
            SetCuriosityColor(id, newColor);
            SetCuriosityHighlightColor(id, Color.Lerp(newColor, Color.white, 0.5f));
        }

        public void DeleteCuriosity(string id)
        {
            if (curiosities.Contains(id))
            {
                int index = curiosities.IndexOf(id);
                curiosities.RemoveAt(index);
                curiosityColors.RemoveAt(index);
                curiosityHighlightColors.RemoveAt(index);
            }
        }

        public Color GetCuriosityColor(string curiosity)
        {
            if (curiosities == null || curiosityColors == null) return Color.grey;
            if (string.IsNullOrEmpty(curiosity)) return Color.grey;
            if (curiosities.Contains(curiosity))
            {
                int index = curiosities.IndexOf(curiosity);
                return curiosityColors[index];
            }
            else return Color.grey;
        }

        public void SetCuriosityColor(string curiosity, Color color)
        {
            if (curiosities == null) curiosities = new List<string>();
            if (curiosityColors == null) curiosityColors = new List<Color>();
            if (curiosityHighlightColors == null) curiosityHighlightColors = new List<Color>();

            if (string.IsNullOrEmpty(curiosity) || color == null) return;
            if (curiosities.Contains(curiosity))
            {
                int index = curiosities.IndexOf(curiosity);
                curiosityColors[index] = color;
            }
            else
            {
                curiosities.Add(curiosity);
                curiosityColors.Add(color);
                curiosityHighlightColors.Add(Color.Lerp(color, Color.white, 0.5f));
            }
        }

        public Color GetCuriosityHighlightColor(string curiosity)
        {
            if (curiosities == null || curiosityHighlightColors == null) return new Color(0.75f, 0.75f, 0.75f, 1);
            if (string.IsNullOrEmpty(curiosity)) return new Color(0.75f, 0.75f, 0.75f, 1);
            if (curiosities.Contains(curiosity))
            {
                int index = curiosities.IndexOf(curiosity);
                return curiosityHighlightColors[index];
            }
            else return new Color(0.75f, 0.75f, 0.75f, 1);
        }

        public void SetCuriosityHighlightColor(string curiosity, Color color)
        {
            if (curiosities == null) curiosities = new List<string>();
            if (curiosityColors == null) curiosityColors = new List<Color>();
            if (curiosityHighlightColors == null) curiosityHighlightColors = new List<Color>();

            if (string.IsNullOrEmpty(curiosity) || color == null) return;
            if (curiosities.Contains(curiosity))
            {
                int index = curiosities.IndexOf(curiosity);
                curiosityHighlightColors[index] = color;
            }
            else
            {
                curiosities.Add(curiosity);
                curiosityColors.Add(Color.Lerp(color, Color.black, 0.5f));
                curiosityHighlightColors.Add(color);
            }
        }

        private void FixEntryData(ShipLogEntry.Entry entry)
        {
            entry.isCuriosity = entry.m_isCuriosity != null;
            entry.ignoreMoreToExplore = entry.m_ignoreMoreToExplore != null;
            entry.parentIgnoreNotRevealed = entry.m_parentIgnoreNotRevealed != null;

            if (entry.exploreFacts != null)
            {
                foreach (var fact in entry.exploreFacts)
                {
                    fact.ignoreMoreToExplore = fact.m_ignoreMoreToExplore != null;
                }
            }
            if (entry.rumorFacts != null)
            {
                foreach (var fact in entry.rumorFacts)
                {
                    fact.ignoreMoreToExplore = fact.m_ignoreMoreToExplore != null;
                }
            }
            if (entry.childEntries != null)
            {
                foreach (var childEntry in entry.childEntries)
                {
                    FixEntryData(childEntry);
                }
            }
        }

        /// <summary>
        /// Cleanup data in case of deletions
        /// </summary>
        public void ValidateData()
        {
            if (datas == null)
            {
                datas = new List<EntryData>();
                EditorUtility.SetDirty(this);
                return;
            }

            int dataLength = datas.Count;
            datas.RemoveAll(x => x == null);
            if (dataLength < datas.Count) EditorUtility.SetDirty(this);
        }
    }
}

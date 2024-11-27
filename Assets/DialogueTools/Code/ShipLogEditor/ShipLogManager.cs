using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ship Log Manager", menuName = "Tools/Ship Log Manager")]
public class ShipLogManager : ScriptableObject
{
    [SerializeField]
    public List<EntryData> datas = new List<EntryData>();

    public static ShipLogManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = AssetDatabase.LoadAssetAtPath<ShipLogManager>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:ShipLogManager")[0]));
            }
            return instance;
        }
    }

    public List<string> allExploreFactsList;
    public List<string> allRumorFactsList;


    private static ShipLogManager instance;

    private void GenerateFactsLists()
    {
        allExploreFactsList = new List<string>();
        allRumorFactsList = new List<string>();

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

    public ShipLogEntry.Entry GetEntry(string entryName)
    {
        return GetEntry(entryName, out _);
    }

    public ShipLogEntry.Entry GetEntry(string entryName, out EntryData data)
    {
        foreach (var file in datas)
        {
            if (file.entryPaths == null || file.entryPaths.Count == 0) file.BuildEntryDataPaths();

            ShipLogEntry.Entry entry = file.GetEntry(entryName);
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
        if (newEntryFile.entries == null || newEntryFile.entries.Length <= 0) return null;
        EntryData data = ScriptableObject.CreateInstance<EntryData>();
        foreach (var entry in newEntryFile.entries)
        {
            FixEntryData(entry);
        }
        data.entry = newEntryFile;
        data.BuildEntryDataPaths();
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

        return data;
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
}
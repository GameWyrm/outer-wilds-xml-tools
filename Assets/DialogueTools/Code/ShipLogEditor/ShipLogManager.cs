using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ship Log Data")]
public class ShipLogManager : ScriptableObject
{
    [SerializeField]
    public List<ShipLogEntry> entryFiles = new List<ShipLogEntry>();

    public List<string> allExploreFactsList;
    public List<string> allRumorFactsList;

    private void GenerateFactsLists()
    {
        allExploreFactsList = new List<string>();
        allRumorFactsList = new List<string>();
        foreach (var entryFile in entryFiles)
        {
            foreach (var entry in entryFile.entries)
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
}
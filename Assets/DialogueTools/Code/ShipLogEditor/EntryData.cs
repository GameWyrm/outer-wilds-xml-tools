using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace XmlTools
{
    public class EntryData : ScriptableObject
    {
        [SerializeField]
        public ShipLogEntry entry;

        [SerializeField, HideInInspector]
        public List<string> entryPaths;
        [SerializeField, HideInInspector]
        public List<string> entryIDs;
        [SerializeField, HideInInspector]
        public List<string> entriesWhoAreChildren;
        [SerializeField, HideInInspector]
        public List<string> rootEntries;

        // These should always stay in sync. They are effectively a Dictionary<string, string>.
        [SerializeField, HideInInspector]
        public List<string> factIDs;
        [SerializeField, HideInInspector]
        public List<string> factPaths;

        [SerializeField, HideInInspector]
        public List<NodeData> nodes;

        public void BuildInfo() => ShipLogManager.Instance.BuildInfo();

        public void AddFact(string factPath, bool asRumor, bool logWarnings = true)
        {
            if (!factPaths.Contains(factPath))
            {
                if (logWarnings) Debug.LogError($"Fact at path {factPath} already exists. \nFact paths must be unique. Check that a {(asRumor ? "ExploreFact" : "RumorFact")} doesn't already exist with the same path.");
                return;
            }
            string entryName = GetEntryName(factPath);
            string factName = GetFactIDName(factPath);

            if (entry.entries == null) entry.entries = new ShipLogEntry.Entry[0];

            List<ShipLogEntry.Entry> newEntries = new List<ShipLogEntry.Entry>(entry.entries);

            ShipLogEntry.Entry newEntry = GetEntry(entryName);
            if (newEntry == null)
            {
                if (logWarnings) Debug.LogWarning($"You cannot add {factPath} as a new fact since entry {entryName} does not exist!");
            }
            else
            {
                if (asRumor)
                {
                    if (newEntry.exploreFacts == null) newEntry.exploreFacts = new ShipLogEntry.ExploreFact[0];
                    List<ShipLogEntry.ExploreFact> exploreFacts = new List<ShipLogEntry.ExploreFact>(newEntry.exploreFacts);
                    ShipLogEntry.ExploreFact fact = new ShipLogEntry.ExploreFact();
                    fact.exploreID = factName;
                    fact.ignoreMoreToExplore = false;
                    exploreFacts.Add(fact);
                    newEntry.exploreFacts = exploreFacts.ToArray();
                }
                else
                {
                    if (newEntry.rumorFacts == null) newEntry.rumorFacts = new ShipLogEntry.RumorFact[0];
                    List<ShipLogEntry.RumorFact> rumorFacts = new List<ShipLogEntry.RumorFact>(newEntry.rumorFacts);
                    ShipLogEntry.RumorFact fact = new ShipLogEntry.RumorFact();
                    fact.rumorID = factName;
                    fact.ignoreMoreToExplore = false;
                    rumorFacts.Add(fact);
                    newEntry.rumorFacts = rumorFacts.ToArray();
                }
                factIDs.Add(factName);
                factPaths.Add(factPath);
                BuildInfo();
            }
        }

        public void RemoveFact(string factPath)
        {
            if (string.IsNullOrEmpty(factPath) || !factPaths.Contains(factPath))
            {
                Debug.LogError($"Unable to remove fact {factPath} as it does not exist.");
                return;
            }

            string entryName = GetEntryName(factPath);
            string factID = GetFactIDName(factPath);

            ShipLogEntry.Entry entryToEdit = GetEntry(entryName);
            if (entryToEdit == null)
            {
                Debug.LogError($"Entry {entryName} does not exist, cannot remove fact {factID}.");
                return;
            }

            if (entryToEdit.exploreFacts != null)
            {
                try
                {
                    ShipLogEntry.ExploreFact factToRemove = entryToEdit.exploreFacts.First(x => x.exploreID == factID);
                    List<ShipLogEntry.ExploreFact> facts = new List<ShipLogEntry.ExploreFact>(entryToEdit.exploreFacts);
                    facts.Remove(factToRemove);
                    entryToEdit.exploreFacts = facts.ToArray();
                }
                catch (System.Exception)
                {
                    // There's no matching entry, moving on
                }
            }
            if (entryToEdit.rumorFacts != null)
            {
                try
                {
                    ShipLogEntry.RumorFact factToRemove = entryToEdit.rumorFacts.First(x => x.rumorID == factID);
                    List<ShipLogEntry.RumorFact> facts = new List<ShipLogEntry.RumorFact>(entryToEdit.rumorFacts);
                    facts.Remove(factToRemove);
                    entryToEdit.rumorFacts = facts.ToArray();
                }
                catch (System.Exception)
                {
                    // There's no matching entry, moving on
                }
            }

            int index = factPaths.IndexOf(factPath);
            factPaths.RemoveAt(index);
            factIDs.RemoveAt(index);
            BuildInfo();
        }

        public void RenameFact(string factPath, string newName)
        {
            if (string.IsNullOrEmpty(factPath) || string.IsNullOrEmpty(newName) || !factPaths.Contains(factPath)) return;

            ShipLogEntry.ExploreFact exploreFact = GetExploreFact(factPath);
            ShipLogEntry.RumorFact rumorFact = GetRumorFact(factPath);
            if (exploreFact != null)
            {
                exploreFact.exploreID = newName;
            }
            else
            {
                rumorFact.rumorID = newName;
            }
            int index = factPaths.IndexOf(factPath);
            string newPath = factPath.Remove(factPath.LastIndexOf('/') + 1, GetFactIDName(factPath).Length);
            newPath += newName;
            factPaths[index] = newPath;
            factIDs[index] = newPath;
            BuildInfo();
        }

        public ShipLogEntry.ExploreFact GetExploreFact(string factPath)
        {
            string factID = GetFactIDName(factPath);
            string[] pathElements = factPath.Split('/');
            string entryPath = "";
            for (int i = 1; i < pathElements.Length - 1; i++)
            {
                entryPath += pathElements[i] + '/';
            }
            entryPath = entryPath.TrimEnd('/');

            ShipLogEntry.Entry factEntry = GetEntry(entryPath);

            if (factEntry.exploreFacts == null) return null;

            ShipLogEntry.ExploreFact returnedFact = null;
            try
            {
                returnedFact = factEntry.exploreFacts.First(x => x.exploreID == factID);
            }
            catch (System.Exception)
            {
                return null;
            }
            return returnedFact;
        }

        public ShipLogEntry.RumorFact GetRumorFact(string factPath)
        {
            string factID = GetFactIDName(factPath);
            string[] pathElements = factPath.Split('/');
            string entryPath = "";
            for (int i = 1; i < pathElements.Length - 1; i++)
            {
                entryPath += pathElements[i] + '/';
            }
            entryPath = entryPath.TrimEnd('/');

            ShipLogEntry.Entry factEntry = GetEntry(entryPath);

            if (factEntry.rumorFacts == null) return null;

            ShipLogEntry.RumorFact returnedFact = null;
            try
            {
                returnedFact = factEntry.rumorFacts.First(x => x.rumorID == factID);
            }
            catch (System.Exception)
            {
                return null;
            }
            return returnedFact;
        }

        public void AddEntry(string entryID, string entryName, bool logWarnings = true)
        {
            if (!string.IsNullOrEmpty(entryID)) return;

            if (entry.entries == null) entry.entries = new ShipLogEntry.Entry[0];

            if (GetEntry(entryID) != null)
            {
                if (logWarnings) Debug.LogError($"Entry with name {entryID} already exists, aborting.");
                return;
            }
            else
            {
                ShipLogEntry.Entry newEntry = new ShipLogEntry.Entry();
                newEntry.entryID = entryID;
                newEntry.name = entryName;
                newEntry.exploreFacts = new ShipLogEntry.ExploreFact[0];
                newEntry.rumorFacts = new ShipLogEntry.RumorFact[0];
                newEntry.childEntries = new ShipLogEntry.Entry[0];

                List<ShipLogEntry.Entry> oldEntries = new List<ShipLogEntry.Entry>(entry.entries);
                oldEntries.Add(newEntry);
                entry.entries = oldEntries.ToArray();
                BuildInfo();
            }
        }

        /// <summary>
        /// Moves the selected entry to the provided parent. If no parent is provided, makes it a root entry instead.
        /// </summary>
        /// <param name="targetEntry"></param>
        /// <param name="newParent"></param>
        public void MoveEntry(ShipLogEntry.Entry targetEntry, ShipLogEntry.Entry newParent = null, ShipLogEntry.Entry oldParent = null)
        {
            List<ShipLogEntry.Entry> oldParentChildren = new List<ShipLogEntry.Entry>();
          
            if (!entryIDs.Contains(targetEntry.entryID))
            {
                Debug.LogError($"Looks like {targetEntry.entryID} is not a member of {name}.");
            }
            if (newParent != null)
            {
                if (!entryIDs.Contains(newParent.entryID))
                {
                    Debug.LogError($"Looks like {newParent.entryID} is not a member of {name}.");
                }
                List<ShipLogEntry.Entry> oldChildren = new List<ShipLogEntry.Entry>(newParent.childEntries);
                oldChildren.Add(targetEntry);
                newParent.childEntries = oldChildren.ToArray();

                if (oldParent == null)
                {
                    oldParentChildren.AddRange(entry.entries);
                    oldParentChildren.Remove(targetEntry);
                    entry.entries = oldParentChildren.ToArray();
                }
                else
                {
                    oldParentChildren.AddRange(oldParent.childEntries);
                    oldParentChildren.Remove(targetEntry);
                    oldParent.childEntries = oldChildren.ToArray();
                }
                
            }
            else
            {
                List<ShipLogEntry.Entry> oldChildren = new List<ShipLogEntry.Entry>(entry.entries);
                oldChildren.Add(targetEntry);
                entry.entries = oldChildren.ToArray();

                oldParentChildren.AddRange(oldParent.childEntries);
                oldParentChildren.Remove(targetEntry);
                oldParent.childEntries = oldChildren.ToArray();
            }

            BuildEntryDataPaths(false);
            BuildInfo();
        }

        public void RemoveEntry(string entryID)
        {
            if (!string.IsNullOrEmpty(entryID)) return;
            string[] pathElements = entryID.Split('/');

            ShipLogEntry.Entry entryToRemove = GetEntry(entryID);
            ShipLogEntry.Entry parentEntry = null;

            if (pathElements.Length > 1)
            {
                string parentPath = "";
                for (int i = 0; i < pathElements.Length - 1; i++)
                {
                    parentPath += pathElements[i] + '/';
                }
                parentPath = parentPath.TrimEnd('/');
                parentEntry = GetEntry(parentPath);
            }

            List<ShipLogEntry.Entry> parentEntries;
            if (parentEntry == null)
            {
                parentEntries = new List<ShipLogEntry.Entry>(entry.entries);
            }
            else
            {
                parentEntries = new List<ShipLogEntry.Entry>(parentEntry.childEntries);
            }


            if (entryToRemove == null)
            {
                Debug.LogError($"Entry {entryID} does not exist.");
                return;
            }

            string rootPath = $"{entryToRemove.curiosity}/{entryToRemove.entryID}/";

            if (entryToRemove.exploreFacts != null)
            {
                foreach (var fact in entryToRemove.exploreFacts)
                {
                    RemoveFact(rootPath + fact.exploreID);
                }
            }
            if (entryToRemove.rumorFacts != null)
            {
                foreach (var fact in entryToRemove.rumorFacts)
                {
                    RemoveFact(rootPath + fact.rumorID);
                }
            }

            if (entryToRemove.childEntries != null && entryToRemove.childEntries.Length > 0)
            {
                foreach (var childEntry in entryToRemove.childEntries)
                {
                    RemoveEntry(entryID + '/' + childEntry.entryID);
                }
            }

            parentEntries.Remove(entryToRemove);

            if (parentEntry == null)
            {
                entry.entries = parentEntries.ToArray();
            }
            else
            {
                parentEntry.childEntries = parentEntries.ToArray();
            }
            BuildInfo();
        }

        /// <summary>
        /// Renames an entry and changes all paths appropriately
        /// </summary>
        /// <param name="entryPath">The entry's name. If the entry is a child entry, should include the parent entries.</param>
        /// <param name="newName"></param>
        public void RenameEntry(string entryPath, string newName)
        {
            if (string.IsNullOrEmpty(entryPath) || string.IsNullOrEmpty(newName)) return;

            ShipLogEntry.Entry editingEntry = GetEntry(entryPath);
            if (editingEntry == null) return;

            editingEntry.entryID = newName;
            string[] pathElements = entryPath.Split('/');
            string newNamePath = "";
            if (entryPath.Contains('/'))
            {
                newNamePath = entryPath.Remove(entryPath.LastIndexOf('/') + 1, pathElements[pathElements.Length - 1].Length);
            }
            else
            {
                newNamePath = newName;
            }

            for (int i = 0; i < factPaths.Count; i++)
            {
                if (factPaths[i].Contains(entryPath))
                {
                    factPaths[i].Replace(entryPath, newNamePath);
                }
            }
            BuildInfo();
        }

        public ShipLogEntry.Entry GetEntry(string entryPath, ShipLogEntry.Entry parentEntry = null)
        {
            return GetEntry(entryPath, out _, parentEntry);
        }


        public ShipLogEntry.Entry GetEntry(string entryID, out ShipLogEntry.Entry parent, ShipLogEntry.Entry parentEntry = null)
        {
            parent = null;
            if (string.IsNullOrEmpty(entryID)) return null;
            if (parentEntry == null && !entryPaths.Contains(entryID))
            {
                if (entryIDs.Contains(entryID))
                {
                    int index = entryIDs.IndexOf(entryID);
                    entryID = entryPaths[index];
                }
                else
                {
                    return null;
                }
            }
            if (entry == null || entry.entries == null) return null;
            ShipLogEntry.Entry[] entries;
            if (parentEntry == null)
            {
                entries = entry.entries;
            }
            else
            {
                if (parentEntry.childEntries == null) return null;

                entries = parentEntry.childEntries;
            }

            string[] pathElements = entryID.Split('/');

            ShipLogEntry.Entry rootEntry;
            ShipLogEntry.Entry returnedEntry = null;
            try
            {
                rootEntry = entries.First(x => x.entryID == pathElements[0]);
            }
            catch (System.Exception)
            {
                return null;
            }

            if (pathElements.Length == 1)
            {
                parent = parentEntry;
                return rootEntry;
            }
            else
            {
                string newPath = "";
                for (int i = 1; i < pathElements.Length; i++)
                {
                    newPath += pathElements[i] + '/';
                }
                newPath = newPath.TrimEnd('/');

                returnedEntry = GetEntry(newPath, out parent, rootEntry);
            }
            return returnedEntry;
        }



        public void BuildEntryDataPaths(bool refreshInfo)
        {
            entryPaths = new List<string>();
            entryIDs = new List<string>();
            entriesWhoAreChildren = new List<string>();
            rootEntries = new List<string>();
            factPaths = new List<string>();
            factIDs = new List<string>();
            foreach (var entry in entry.entries)
            {
                rootEntries.Add(entry.entryID);
                AddToEntryList(entry, "");
            }
            if (refreshInfo) BuildInfo();
            EditorUtility.SetDirty(this);
        }

        private void AddToEntryList(ShipLogEntry.Entry entry, string pathSoFar)
        {
            pathSoFar += entry.entryID;
            entryPaths.Add(pathSoFar);
            entryIDs.Add(entry.entryID);

            if (entry.exploreFacts != null)
            {
                foreach (var fact in entry.exploreFacts)
                {
                    factPaths.Add(pathSoFar + fact.exploreID);
                    factIDs.Add(fact.exploreID);
                }
            }
            if (entry.rumorFacts != null)
            {
                foreach (var fact in entry.rumorFacts)
                {
                    factPaths.Add(pathSoFar + fact.rumorID);
                    factIDs.Add(fact.rumorID);
                }
            }

            if (entry.childEntries != null && entry.childEntries.Count() > 0)
            {
                foreach (var childEntry in entry.childEntries)
                {
                    entriesWhoAreChildren.Add(childEntry.entryID);
                    AddToEntryList(childEntry, pathSoFar + "/");
                }
            }
        }

        public static string GetCuriosityName(string factPath)
        {
            return factPath.Split('/')[0];
        }

        public static string GetEntryName(string factPath)
        {
            string[] factElements = factPath.Split('/');
            // we can ignore the first and last element in paths
            string joinedPath = "";
            for (int i = 1; i < factElements.Length - 1; i++)
            {
                joinedPath += factElements[i] + '/';
            }
            joinedPath = joinedPath.TrimEnd('/');

            return joinedPath;
        }

        public static string GetFactIDName(string factPath)
        {
            string[] factElements = factPath.Split('/');
            return factPath.Split('/')[factElements.Length - 1];
        }
    }
}

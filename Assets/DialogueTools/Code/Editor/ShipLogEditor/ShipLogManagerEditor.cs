using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace XmlTools
{
    [CustomEditor(typeof(ShipLogManager))]
    public class ShipLogManagerEditor : Editor
    {
        public static ShipLogManager instance;
        public static ShipLogEntry.Entry selectedEntry;
        public static ShipLogEntry.Entry parentEntry;
        public static EntryData selectedData;

        private static bool showCuriosities;
        private static bool showExploreFacts;
        private static bool showRumorFacts;
        private static bool showChildEntries;

        private void OnEnable()
        {
            instance = serializedObject.targetObject as ShipLogManager;
        }

        public override void OnInspectorGUI()
        {
            bool setDirty = false;
            bool setRedraw = false;
            bool updateInfo = false;
            showCuriosities = EditorGUILayout.BeginFoldoutHeaderGroup(showCuriosities, "Curiosities");
            if (showCuriosities)
            {
                if (instance.curiosities == null) instance.curiosities = new List<string>();
                if (instance.curiosities.Count > 0)
                {
                    for (int i = 0; i < instance.curiosities.Count; i++)
                    {
                        string curiosity = instance.curiosities[i];
                        Color oldColor = instance.GetCuriosityColor(curiosity);
                        Color oldHighlightColor = instance.GetCuriosityHighlightColor(curiosity);
                        GUIBuilder.CreateColorSetter(curiosity, oldColor, oldHighlightColor, out Color newCuriosityColor, out Color newHighlightColor);
                        if (oldColor != newCuriosityColor)
                        {
                            instance.SetCuriosityColor(curiosity, newCuriosityColor);
                            setRedraw = true;
                        }
                        if (oldHighlightColor != newHighlightColor)
                        {
                            instance.SetCuriosityHighlightColor(curiosity, newHighlightColor);
                            setRedraw = true;
                        }
                    }
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (ShipLogEditor.Instance != null && selectedEntry != null)
            {
                EditorGUILayout.Space(20);

                // Parent display
                if (parentEntry != null)
                {
                    GUIBuilder.CreateEntryButton("Parent entry: " + parentEntry.entryID, parentEntry.entryID);
                }
                else
                {
                    EditorGUILayout.LabelField("No parent found for this node.");
                }

                EditorGUILayout.Space(20);

                // Transform
                NodeData node = selectedData.GetNode(selectedEntry.entryID);
                Vector2 newPosition = EditorGUILayout.Vector2Field("Position", node.position);
                if (newPosition != node.position)
                {
                    node.position = newPosition;
                    setRedraw = true;
                }
                if (parentEntry != null)
                {
                    NodeData parentNode = selectedData.GetNode(parentEntry.entryID);
                    Vector2 localPosition = node.position - parentNode.position;
                    Vector2 newLocalPosition = EditorGUILayout.Vector2Field("Local Position", localPosition);
                    if (newLocalPosition != localPosition)
                    {
                        node.position = parentNode.position + newLocalPosition;
                        setRedraw = true;
                    }
                }

                EditorGUILayout.Space(20);

                // Language
                XMLEditorSettings settings = XMLEditorSettings.Instance;
                Language language = settings.GetSelectedLanguage(); 
                settings.selectedLanguage = EditorGUILayout.Popup("Language: ", settings.selectedLanguage, settings.supportedLanguages.Select(x => x.name).ToArray());
                EditorGUILayout.Space();

                EditorGUILayout.LabelField(selectedEntry.entryID, EditorStyles.boldLabel);

                // Name
                string newName = GUIBuilder.CreateTranslatedArrayItem("Entry Name", selectedEntry.name, language, false, false, out _);
                if (newName != selectedEntry.name)
                {
                    selectedEntry.name = newName;
                    setRedraw = true;
                }

                // Curiosity
                string newCuriosity = GUIBuilder.CreateDropdown("Curiosity", selectedEntry.curiosity, instance.curiosities.ToArray());
                if (newCuriosity != selectedEntry.curiosity)
                {
                    selectedEntry.curiosity = newCuriosity;
                    setDirty = true;
                }

                // Is Curiosity
                bool newIsCuriosity = EditorGUILayout.ToggleLeft("Is Curiosity", selectedEntry.isCuriosity);
                if (newIsCuriosity != selectedEntry.isCuriosity)
                {
                    selectedEntry.isCuriosity = newIsCuriosity;
                    if (newIsCuriosity)
                    {
                        instance.CreateCuriosity(selectedEntry.entryID);
                        selectedEntry.curiosity = selectedEntry.entryID;
                    }
                    else
                    {
                        instance.DeleteCuriosity(selectedEntry.entryID);
                    }
                    setDirty = true;
                }

                // Ignore More To Explore
                bool entryIgnoreMoreToExplore = EditorGUILayout.ToggleLeft("Ignore More To Explore", selectedEntry.ignoreMoreToExplore);
                if (entryIgnoreMoreToExplore)
                {
                    selectedEntry.ignoreMoreToExplore = entryIgnoreMoreToExplore;
                    setDirty = true;
                }

                // Entry Ignore Parent
                bool entryIgnoreParent = EditorGUILayout.ToggleLeft("Parent Ignore Not Revealed", selectedEntry.parentIgnoreNotRevealed);
                if (entryIgnoreParent)
                {
                    selectedEntry.parentIgnoreNotRevealed = entryIgnoreParent;
                    setDirty = true;
                }

                // Ignore More To Explore Condition
                List<string> allConditions = new List<string>();
                allConditions.Add("(None)");
                allConditions.Add("");
                allConditions.AddRange(settings.GetConditionList(false));
                allConditions.Add("");
                allConditions.AddRange(settings.GetConditionList(true));

                string ignoreMoreToExploreCondition = GUIBuilder.CreateDropdown("Ignore More To Explore Condition", selectedEntry.ignoreMoreToExploreCondition, allConditions.ToArray());
                if (ignoreMoreToExploreCondition != selectedEntry.ignoreMoreToExploreCondition)
                {
                    if (ignoreMoreToExploreCondition == "(None)") ignoreMoreToExploreCondition = "";
                    selectedEntry.ignoreMoreToExploreCondition = ignoreMoreToExploreCondition;
                    setDirty = true;
                }

                // Alt Photo Condition
                string altPhotoCondition = GUIBuilder.CreateDropdown("Alternate Photo Condition", selectedEntry.altPhotoCondition, allConditions.ToArray());
                if (altPhotoCondition != selectedEntry.altPhotoCondition)
                {
                    if (altPhotoCondition == "(None)") altPhotoCondition = "";
                    selectedEntry.altPhotoCondition = altPhotoCondition;
                    setDirty = true;
                }

                // Rumors
                showRumorFacts = EditorGUILayout.BeginFoldoutHeaderGroup(showRumorFacts, "Rumor Facts");
                if (showRumorFacts)
                {
                    List<string> sources = new List<string>();
                    sources.Add("(None)");
                    foreach (string entry in instance.allEntries)
                    {
                        sources.Add(entry);
                    }

                    int clearIndex = -1;
                    for (int i = 0; i < selectedEntry.rumorFacts.Length; i++)
                    {
                        var fact = selectedEntry.rumorFacts[i];
                        if (GUIBuilder.CreateRumorFactItem(fact, sources, out bool requireRedraw, out bool shouldClear))
                        {
                            if (shouldClear)
                            {
                                clearIndex = i;
                            }
                            setDirty = true;
                        }
                        if (requireRedraw) setRedraw = true;
                        EditorGUILayout.Space(20);
                    }

                    if (clearIndex != -1)
                    {
                        List<ShipLogEntry.RumorFact> factList = new List<ShipLogEntry.RumorFact>(selectedEntry.rumorFacts);
                        factList.RemoveAt(clearIndex);
                        selectedEntry.rumorFacts = factList.ToArray();
                        updateInfo = true;
                    }

                    EditorGUILayout.HelpBox("IDs cannot be edited after creation.", MessageType.Warning);
                    string newFactName = EditorGUILayout.DelayedTextField("Create new fact with custom ID:", "");
                    bool autoFact = GUILayout.Button("Create new fact with automatic ID");
                    if (newFactName != "" || autoFact)
                    {
                        if (newFactName == "") newFactName = $"{selectedEntry.entryID}_{selectedEntry.rumorFacts.Length}";
                        newFactName = newFactName.Replace(' ', '_');
                        string newText = newFactName;
                        int attempts = 0;
                        while (language.dialogueKeys.Contains(newText))
                        {
                            newText += "_1";
                            if (attempts > 1000)
                            {
                                Debug.LogError("Are you insane or trying to break this?");
                                break;
                            }
                        }

                        ShipLogEntry.RumorFact newFact = new ShipLogEntry.RumorFact(newFactName, newText);
                        List<ShipLogEntry.RumorFact> facts = new List<ShipLogEntry.RumorFact>(selectedEntry.rumorFacts);
                        facts.Add(newFact);
                        selectedEntry.rumorFacts = facts.ToArray();

                        updateInfo = true;
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

                // Explore Facts
                showExploreFacts = EditorGUILayout.BeginFoldoutHeaderGroup(showExploreFacts, "Explore Facts");
                if (showExploreFacts)
                {
                    int clearIndex = -1;
                    for (int i = 0; i < selectedEntry.exploreFacts.Length; i++)
                    {
                        var fact = selectedEntry.exploreFacts[i];

                        if (GUIBuilder.CreateExploreFactItem(fact, out bool shouldClear)) setDirty = true;
                        if (shouldClear)
                        {
                            clearIndex = i;
                        }
                        EditorGUILayout.Space(20);
                    }

                    if (clearIndex != -1)
                    {
                        List<ShipLogEntry.ExploreFact> factList = new List<ShipLogEntry.ExploreFact>(selectedEntry.exploreFacts);
                        factList.RemoveAt(clearIndex);
                        selectedEntry.exploreFacts = factList.ToArray();
                        updateInfo = true;
                    }

                    EditorGUILayout.HelpBox("IDs cannot be edited after creation.", MessageType.Warning);
                    string newFactName = EditorGUILayout.DelayedTextField("Create new fact with custom ID:", "");
                    bool autoFact = GUILayout.Button("Create new fact with automatic ID");
                    if (newFactName != "" || autoFact)
                    {
                        if (newFactName == "") newFactName = $"{selectedEntry.entryID}_{selectedEntry.exploreFacts.Length}";
                        newFactName = newFactName.Replace(' ', '_');
                        string newText = newFactName;
                        int attempts = 0;
                        while (language.dialogueKeys.Contains(newText))
                        {
                            newText += "_1";
                            if (attempts > 1000)
                            {
                                Debug.LogError("Are you insane or trying to break this?");
                                break;
                            }
                        }

                        ShipLogEntry.ExploreFact newFact = new ShipLogEntry.ExploreFact(newFactName, newText);
                        List<ShipLogEntry.ExploreFact> facts = new List<ShipLogEntry.ExploreFact>(selectedEntry.exploreFacts);
                        facts.Add(newFact);
                        selectedEntry.exploreFacts = facts.ToArray();
                        
                        updateInfo = true;
                    }

                }
                EditorGUILayout.EndFoldoutHeaderGroup();

                // Children
                showChildEntries = EditorGUILayout.BeginFoldoutHeaderGroup(showChildEntries, "Child Entries");
                if (showChildEntries)
                {
                    if (selectedEntry.childEntries != null && selectedEntry.childEntries.Length > 0)
                    {
                        foreach (var child in selectedEntry.childEntries)
                        {
                            if (GUIBuilder.CreateEntryButton(child.entryID, child.entryID, true))
                            {
                                selectedData.MoveEntry(child, null, selectedEntry);
                                setRedraw = true;
                            }
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("No children for this entry.");
                    }

                    if (parentEntry == null)
                    {
                        List<string> eligibleEntries = new List<string>();
                        eligibleEntries.Add("(None)");
                        foreach (var entryName in selectedData.rootEntries)
                        {
                            var entry = selectedData.GetEntry(entryName);
                            if (entry != null && entry.entryID != selectedEntry.entryID && (entry.childEntries == null || entry.childEntries.Length == 0))
                            {
                                eligibleEntries.Add(entryName);
                            }
                        }
                        string newChild = GUIBuilder.CreateDropdown("Add Child Entry", "", eligibleEntries.ToArray());
                        if (newChild != "" && newChild != "(None)")
                        {
                            if (EditorUtility.DisplayDialog("Confirm", $"Are you sure you want to make {newChild} a child of {selectedEntry.entryID}?", "Yes", "No"))
                            {
                                var newChildEntry = selectedData.GetEntry(newChild);
                                selectedData.MoveEntry(newChildEntry, selectedEntry, parentEntry);
                                updateInfo = true;
                                setRedraw = true;
                            }
                        }

                        if (GUILayout.Button("Create New Entry"))
                        {
                            string defaultID = $"{selectedEntry.entryID}_Child";
                            string defaultName = $"Child of {selectedEntry.name}";

                            NewEntryDialogue.ShowWindow(defaultID, defaultName, selectedEntry, selectedData);
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Child entries should not be added to entries that are already children.", MessageType.Warning);
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            if (updateInfo)
            {
                setDirty = true;
                ShipLogManager.Instance.BuildInfo();
            }
            if (setRedraw)
            {
                EditorUtility.SetDirty(instance);
                if (ShipLogEditor.Instance != null) ShipLogEditor.Instance.BuildNodeTree();
            }
            else if (setDirty)
            {
                EditorUtility.SetDirty(instance);
            }
        }
    }
}
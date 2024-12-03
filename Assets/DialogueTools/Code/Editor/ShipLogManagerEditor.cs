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

            if (selectedEntry != null)
            {
                EditorGUILayout.Space(20);

                if (parentEntry != null)
                {
                    GUIBuilder.CreateEntryButton("Parent entry: " + parentEntry.entryID, parentEntry.entryID);
                }
                else
                {
                    EditorGUILayout.LabelField("No parent found for this node.");
                }

                EditorGUILayout.Space(20);

                XMLEditorSettings settings = XMLEditorSettings.Instance;
                Language language = settings.GetSelectedLanguage(); 
                settings.selectedLanguage = EditorGUILayout.Popup("Language: ", settings.selectedLanguage, settings.supportedLanguages.Select(x => x.name).ToArray());
                EditorGUILayout.Space();

                EditorGUILayout.LabelField(selectedEntry.entryID, EditorStyles.boldLabel);

                string newName = GUIBuilder.CreateTranslatedArrayItem("Entry Name", selectedEntry.name, language, false, false, out _);
                if (newName != selectedEntry.name)
                {
                    selectedEntry.name = newName;
                    setRedraw = true;
                }

                string newCuriosity = GUIBuilder.CreateDropdown("Curiosity", selectedEntry.curiosity, instance.curiosities.ToArray());
                if (newCuriosity != selectedEntry.curiosity)
                {
                    selectedEntry.curiosity = newCuriosity;
                    setDirty = true;
                }

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

                bool entryIgnoreMoreToExplore = EditorGUILayout.ToggleLeft("Ignore More To Explore", selectedEntry.ignoreMoreToExplore);
                if (entryIgnoreMoreToExplore)
                {
                    selectedEntry.ignoreMoreToExplore = entryIgnoreMoreToExplore;
                    setDirty = true;
                }

                bool entryIgnoreParent = EditorGUILayout.ToggleLeft("Parent Ignore Not Revealed", selectedEntry.parentIgnoreNotRevealed);
                if (entryIgnoreParent)
                {
                    selectedEntry.parentIgnoreNotRevealed = entryIgnoreParent;
                    setDirty = true;
                }

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

                string altPhotoCondition = GUIBuilder.CreateDropdown("Alternate Photo Condition", selectedEntry.altPhotoCondition, allConditions.ToArray());
                if (altPhotoCondition != selectedEntry.altPhotoCondition)
                {
                    if (altPhotoCondition == "(None)") altPhotoCondition = "";
                    selectedEntry.altPhotoCondition = altPhotoCondition;
                    setDirty = true;
                }

                showRumorFacts = EditorGUILayout.BeginFoldoutHeaderGroup(showRumorFacts, "Rumor Facts");
                if (showRumorFacts)
                {
                    // TODO display rumors
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

                showExploreFacts = EditorGUILayout.BeginFoldoutHeaderGroup(showExploreFacts, "Explore Facts");
                if (showExploreFacts)
                {
                    // TODO display facts
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

                showChildEntries = EditorGUILayout.BeginFoldoutHeaderGroup(showChildEntries, "Child Entries");
                if (showChildEntries)
                {
                    if (selectedEntry.childEntries != null && selectedEntry.childEntries.Length > 0)
                    {
                        foreach (var child in selectedEntry.childEntries)
                        {
                            if (GUIBuilder.CreateEntryButton(child.entryID, child.entryID, true))
                            {
                                selectedData.MoveEntry(child);
                                setRedraw = true;
                            }
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("No children for this entry.");
                    }

                    List<string> eligibleEntries = new List<string>();
                    eligibleEntries.Add("(None)");
                    eligibleEntries.AddRange(selectedData.rootEntries);
                    string newChild = GUIBuilder.CreateDropdown("Add Child Entry", "", eligibleEntries.ToArray());
                    if (newChild != "" && newChild != "(None)")
                    {
                        if (EditorUtility.DisplayDialog("Confirm", $"Are you sure you want to make {newChild} a child of {selectedEntry.entryID}?", "Yes", "No"))
                        {
                            var newChildEntry = selectedData.GetEntry(newChild);
                            selectedData.MoveEntry(newChildEntry, selectedEntry);
                            setRedraw = true;
                        }
                    }

                }
                EditorGUILayout.EndFoldoutHeaderGroup();
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
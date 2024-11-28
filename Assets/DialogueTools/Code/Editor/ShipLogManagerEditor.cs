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
                    if (selectedEntry.childEntries != null)
                    {
                        foreach (var child in selectedEntry.childEntries)
                        {
                            EditorGUILayout.LabelField(child.entryID);
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
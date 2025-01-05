using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System;

namespace XmlTools
{
    public class GUIBuilder
    {

        #region nodeTree
        /// <summary>
        /// Creates a Dialogue Node visual element
        /// </summary>
        /// <param name="nodeName">The text shown on the node</param>
        /// <param name="nodeManipulators">The list of manipulators that nodes will be added to</param>
        /// <param name="window">The window this belongs to</param>
        /// <returns>The created node</returns>
        public static VisualElement CreateDialogueNode(string nodeName, Dictionary<string, NodeManipulator> nodeManipulators, NodeWindow window)
        {
            var settings = EditorReferences.Instance;
            VisualElement nodeParent = new VisualElement();
            nodeParent.name = nodeName;
            VisualElement newNode = new VisualElement();
            newNode.styleSheets.Add(settings.DialogueStyle);
            newNode.name = nodeName + " Node";
            newNode.EnableInClassList("node_bg", true);
            Label label = new Label(nodeName);
            label.styleSheets.Add(settings.DialogueStyle);
            label.name = "node_label";
            label.EnableInClassList("node_label", true);
            newNode.Add(label);
            nodeParent.Add(newNode);
            nodeParent.transform.position = Vector3.zero;
            nodeParent.transform.rotation = Quaternion.identity;
            nodeParent.transform.scale = Vector3.one;
            newNode.transform.position = new Vector3(-100, -25);

            var nodeManipulator = new NodeManipulator(nodeParent);
            nodeManipulator.RegisterCallbacksOnTarget();
            nodeManipulator.window = window;
            nodeManipulator.arrows = new List<ArrowManipulator>();
            nodeManipulators.Add(nodeName, nodeManipulator);

            return nodeParent;
        }

        /// <summary>
        /// Creates a Ship Log Node Visual element, with an image and title
        /// </summary>
        /// <param name="nodeName">The internal name of the node</param>
        /// <param name="labelText">The text shown on the node</param>
        /// <param name="nodeManipulators">The list of node manipulators this will be added to</param>
        /// <param name="window">The window this node will be added to</param>
        /// <param name="entryType">Whether this node is a Curiosity, normal entry, or child entry</param>
        /// <returns>The created node</returns>
        public static VisualElement CreateShipLogNode(string nodeName, string labelText, Dictionary<string, NodeManipulator> nodeManipulators, NodeWindow window, EntryType entryType)
        {
            var settings = EditorReferences.Instance;
            VisualElement nodeParent = new VisualElement();
            nodeParent.name = nodeName;
            VisualElement bg = new VisualElement();
            bg.styleSheets.Add(settings.ShipLogStyle);
            bg.name = "bg";
            bg.EnableInClassList("node", true);
            Label label = new Label(labelText);
            label.styleSheets.Add(settings.ShipLogStyle);
            label.name = "label";
            label.EnableInClassList("node_label", true);
            bg.Add(label);
            label.transform.position = new Vector2(1, 1);
            Image image = new Image();
            image.styleSheets.Add(settings.ShipLogStyle);
            image.name = "icon";
            image.EnableInClassList("node_image", true);
            image.image = settings.NoPhotoTexture;
            bg.Add(image);
            image.transform.position = new Vector2(1, 43f);
            nodeParent.Add(bg);
            nodeParent.transform.position = Vector3.zero;
            nodeParent.transform.rotation = Quaternion.identity;
            nodeParent.transform.scale = Vector3.one;
            bg.transform.position = new Vector3(-55, -76);
            if (entryType == EntryType.Curiosity)
            {
                nodeParent.transform.scale = Vector3.one * 2;
            }
            else if (entryType == EntryType.Child)
            {
                nodeParent.transform.scale = Vector3.one * 0.6f;
            }

            var manipulator = new NodeManipulator(nodeParent);
            manipulator.RegisterCallbacksOnTarget();
            manipulator.window = window;
            manipulator.arrows = new List<ArrowManipulator>();
            nodeManipulators.Add(nodeName, manipulator);

            return nodeParent;
        }

        /// <summary>
        /// Creates an arrow that connects two nodes
        /// </summary>
        /// <param name="source">Origin node</param>
        /// <param name="target">Ending node</param>
        /// <param name="arrowManipulator">The Arrow Manipulator this creates</param>
        /// <returns>The created arrow</returns>
        public static VisualElement CreateArrow(VisualElement source, VisualElement target, out ArrowManipulator arrowManipulator)
        {
            VisualElement offsetElement = new VisualElement();
            offsetElement.name = "Arrow Container";
            offsetElement.transform.position = Vector3.zero;
            offsetElement.transform.rotation = Quaternion.identity;
            offsetElement.transform.scale = Vector3.one;
            var settings = EditorReferences.Instance;
            offsetElement.styleSheets.Add(settings.DialogueStyle);
            offsetElement.EnableInClassList("arrow_container", true);
            VisualElement newLineContainer = new VisualElement();
            newLineContainer.name = "Line Container";
            Image newLine = new Image();
            newLine.image = settings.LineTexture;
            newLine.styleSheets.Add(settings.DialogueStyle);
            newLine.EnableInClassList("line", true);
            newLineContainer.Add(newLine);
            newLine.transform.position = new Vector2(-16, -0.5f);
            offsetElement.Add(newLineContainer);
            newLineContainer.transform.position = Vector2.zero;
            Image newArrow = new Image();
            newArrow.image = settings.ArrowTexture;
            newArrow.styleSheets.Add(settings.DialogueStyle);
            newArrow.EnableInClassList("arrow", true);
            offsetElement.Add(newArrow);
            newArrow.transform.position = new Vector2(-16, -32);

            arrowManipulator = new ArrowManipulator(source, target, newArrow, newLineContainer);

            arrowManipulator.OrientArrow();

            return offsetElement;
        }
        #endregion

        #region DialogueEditorElements
        /// <summary>
        /// Creates an array of conditions
        /// </summary>
        /// <param name="isShowing">Whether it's visible</param>
        /// <param name="data">The data that you are mutating</param>
        /// <param name="label">The title of the array</param>
        /// <param name="itemLabel">The text shown before each item</param>
        /// <param name="casing"></param>
        /// <returns></returns>
        public static List<string> CreateArray(ref bool isShowing, List<string> data, string label, string itemLabel, Casing casing)
        {
            int clearValue = -1;
            if (data == null) data = new List<string>();
            isShowing = EditorGUILayout.BeginFoldoutHeaderGroup(isShowing, label);
            if (isShowing)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    data[i] = CreateArrayItem($"{itemLabel} {i}", data[i], out bool shouldClear);
                    if (shouldClear) clearValue = i;
                }
                if (clearValue >= 0)
                {
                    data.RemoveAt(clearValue);
                }
                EditorGUILayout.BeginHorizontal();
                string newCondition = EditorGUILayout.DelayedTextField("Create new condition", string.Empty);
                if (!string.IsNullOrEmpty(newCondition))
                {
                    string prefix = XMLEditorSettings.Instance.modPrefix;
                    if (newCondition.StartsWith(prefix) && !string.IsNullOrEmpty(prefix)) newCondition = newCondition.Remove(0, prefix.Length);
                    newCondition = ConvertToCase(newCondition, casing);
                    if ((casing == Casing.snake_case || casing == Casing.SCREAMING_SNAKE_CASE) && !newCondition.StartsWith("_")) prefix += "_";
                    newCondition = prefix + newCondition;

                    data.Add(newCondition);
                }
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.Space(20);

            return data;
        }

        /// <summary>
        /// Creates an array of dropdowns
        /// </summary>
        /// <param name="isShowing">Whether it's visible</param>
        /// <param name="title">The text shown at the top</param>
        /// <param name="itemsLabel">The text shown next ot each item</param>
        /// <param name="selectedItems">The items that are currently active</param>
        /// <param name="items">The possible items that will appear in each dropdown</param>
        /// <returns></returns>
        public static string[] CreateDropdownArray(ref bool isShowing, string title, string itemsLabel, string[] selectedItems, string[] items)
        {
            int clearValue = -1;
            List<string> data = new List<string>(selectedItems);
            isShowing = EditorGUILayout.BeginFoldoutHeaderGroup(isShowing, title);
            if (isShowing)
            {
                List<string> itemList = new List<string>();
                itemList.Add("(None)");
                itemList.AddRange(items);
                for (int i = 0; i < data.Count; i++)
                {
                    int index = -1;
                    if (itemList.Contains(data[i])) index = itemList.IndexOf(data[i]);
                    else
                    {
                        itemList.Add(data[i]);
                        index = itemList.Count - 1;
                    }

                    string newItem = CreateDropdownArrayItem($"{itemsLabel} {i}", itemList, index, out bool shouldClear);
                    if (shouldClear) clearValue = i;
                    if (newItem == "(None)") newItem = "";

                    data[i] = newItem;
                    if (clearValue >= 0) data.RemoveAt(clearValue);
                }
                bool addNew = GUILayout.Button("+ Add New");
                if (addNew)
                {
                    data.Add("");
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.Space(20);

            return data.ToArray();
        }

        /// <summary>
        /// Creates an array of translated texts
        /// </summary>
        /// <param name="isShowing">Whether the array is showing</param>
        /// <param name="title">Text shown at the top of the array</param>
        /// <param name="itemsLabel">Text shown next to each of the items</param>
        /// <param name="language">The language that's currently active</param>
        /// <param name="dialogueKeys">List of keys</param>
        /// <param name="dialogueName">Name of the entry</param>
        /// <param name="nodeName">The name of the node this entry belongs to</param>
        /// <param name="dialogue">Whether this should be dialogue, or ship log text</param>
        public static string[] CreateTranslatedArray(ref bool isShowing, string title, string itemsLabel, Language language, string[] dialogueKeys, string dialogueName, string nodeName, bool dialogue, out bool setDirty)
        {
            return CreateTranslatedArray(ref isShowing, title, itemsLabel, language, dialogueKeys, dialogueName, nodeName, dialogue, out setDirty, false, out _);
        }

        /// <summary>
        /// Creates an array of translated texts
        /// </summary>
        /// <param name="isShowing">Whether the array is showing</param>
        /// <param name="title">Text shown at the top of the array</param>
        /// <param name="itemsLabel">Text shown next to each of the items</param>
        /// <param name="language">The language that's currently active</param>
        /// <param name="dialogueKeys">List of keys</param>
        /// <param name="dialogueName">Name of the entry</param>
        /// <param name="nodeName">The name of the node this entry belongs to</param>
        /// <param name="dialogue">Whether this should be dialogue, or ship log text</param>
        /// <param name="clearable">Whether this is part of a list and thus should have a clear button.</param>
        /// <param name="shouldClear">Whether the clear button was pressed.</param>
        public static string[] CreateTranslatedArray(ref bool isShowing, string title, string itemsLabel, Language language, string[] dialogueKeys, string dialogueName, string nodeName, bool dialogue, out bool setDirty, bool clearable, out bool cleared)
        {
            int clearValue = -1;
            cleared = false;
            setDirty = false;
            List<string> data = new List<string>(dialogueKeys);
            if (clearable) EditorGUILayout.BeginHorizontal();
            isShowing = EditorGUILayout.BeginFoldoutHeaderGroup(isShowing, title);
            if (clearable)
            {
                if (GUILayout.Button("X", GUILayout.Width(20))) cleared = true;
                EditorGUILayout.EndHorizontal();
            }
            if (isShowing)
            {
                for (int i = 0; i < dialogueKeys.Length; i++)
                {
                    CreateTranslatedArrayItem($"{itemsLabel} {i + 1}", dialogueKeys[i], language, true, dialogue, out bool shouldClear, out bool shouldSetDirty);
                    if (shouldSetDirty) setDirty = true;
                    if (shouldClear) clearValue = i;
                }
                if (clearValue >= 0)
                {
                    data.RemoveAt(clearValue);
                    setDirty = true;
                }

                EditorGUILayout.LabelField($"------------------ ADD NEW {itemsLabel.ToUpper()} ------------------");
                string prefix = XMLEditorSettings.Instance.modPrefix.ToUpper();
                if (GUILayout.Button($"New {itemsLabel} (Auto)"))
                {
                    string newName = "";
                    if (!string.IsNullOrEmpty(prefix)) newName = prefix + "_";
                    newName += $"{dialogueName.ToUpper()}_{nodeName.ToUpper()}_{data.Count}";
                    data.Add(newName);
                    setDirty = true;
                }
                string newCustomTranslation = EditorGUILayout.DelayedTextField($"New {itemsLabel}", "");
                if (!string.IsNullOrEmpty(newCustomTranslation))
                {
                    if (!newCustomTranslation.ToUpper().StartsWith(prefix)) newCustomTranslation = prefix + "_" + newCustomTranslation;
                    data.Add(newCustomTranslation);
                    setDirty = true;
                }
                if (language.tieredDialogueKeys != null)
                {
                    int reuseIndex = EditorGUILayout.Popup($"Reuse {itemsLabel}", 0, language.tieredDialogueKeys.ToArray());
                    string reusedTranslation = language.dialogueKeys[reuseIndex];
                    if (reuseIndex > 0)
                    {
                        data.Add(reusedTranslation);
                        setDirty = true;
                    }
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.Space(20);

            return data.ToArray();
        }

        /// <summary>
        /// Creates a dropdown of string items
        /// </summary>
        /// <param name="label">Text shown next to the dropdown</param>
        /// <param name="selectedItem">The string shown in the dropdown</param>
        /// <param name="items">The options within the dropdown</param>
        /// <returns></returns>
        public static string CreateDropdown(string label, string selectedItem, string[] items)
        {
            int index = -1;
            List<string> itemList = new List<string>();
            itemList.Add("(None)");
            itemList.AddRange(items);
            if (itemList.Contains(selectedItem)) index = itemList.IndexOf(selectedItem);
            {
                itemList.Add(selectedItem);
                index = itemList.Count - 1;
            }

            selectedItem = CreateDropdownItem(label, itemList, index);

            if (selectedItem == "(None)") selectedItem = "";

            return selectedItem;
        }

        /// <summary>
        /// Creates a translated textbox
        /// </summary>
        /// <param name="label">The text shown next to the textbox</param>
        /// <param name="key">The language key of the text</param>
        /// <param name="lang">The currently active language</param>
        /// <param name="clearable">Whether this item should have an X to clear it</param>
        /// <param name="dialogue">Whether this should be a dialogue or ship log translation</param>
        /// <param name="shouldClear">Whether the clear button was pressed this frame</param>
        /// <returns></returns>
        public static string CreateTranslatedArrayItem(string label, string key, Language lang, bool clearable, bool dialogue, out bool shouldClear, out bool setDirty)
        {
            shouldClear = false;
            setDirty = false;
            EditorGUILayout.BeginHorizontal();
            string newKey = EditorGUILayout.DelayedTextField(label, key);
            if (clearable)
            {
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    shouldClear = true;
                    setDirty = true;
                }
            }
            else shouldClear = false;
            EditorGUILayout.EndHorizontal();
            if (newKey != key)
            {
                bool success;
                if (dialogue) success = lang.TryRenameDialogueKey(key, newKey);
                else success = lang.TryRenameShipLogKey(key, newKey);
                if (!success)
                {
                    Debug.LogError($"Dialogue dictionary for {lang.name} already includes key {newKey}. Aborting key change.");
                    newKey = key;
                }
                else
                {
                    foreach (var language in XMLEditorSettings.Instance.supportedLanguages)
                    {
                        if (language == lang) continue;
                        if (dialogue) language.TryRenameDialogueKey(key, newKey);
                        else language.TryRenameShipLogKey(key, newKey);
                    }
                    Debug.Log($"Renamed key {key} to {newKey}.");
                    setDirty = true;
                }
            }
            if (dialogue) lang.SetDialogueValue(newKey, EditorGUILayout.DelayedTextField(lang.GetDialogueValue(newKey), GUILayout.ExpandHeight(true)));
            else lang.SetShipLogValue(newKey, EditorGUILayout.DelayedTextField(lang.GetShipLogValue(newKey), GUILayout.ExpandHeight(true)));
            return newKey;
        }

        /// <summary>
        /// Creates a text field with a label and can be cleared
        /// </summary>
        /// <param name="itemLabel">The text shown next to the textbox</param>
        /// <param name="data">The text in the textbox</param>
        /// <param name="shouldClear">Whether the clear button was pressed this frame</param>
        /// <returns></returns>
        private static string CreateArrayItem(string itemLabel, string data, out bool shouldClear)
        {
            shouldClear = false;
            EditorGUILayout.BeginHorizontal();
            data = EditorGUILayout.TextField(itemLabel, data);
            bool clear = GUILayout.Button("X", GUILayout.Width(20));
            shouldClear = clear;
            EditorGUILayout.EndHorizontal();

            return data;
        }

        /// <summary>
        /// Creates a clearable dropdown item
        /// </summary>
        /// <param name="label">The text shown next to the dropdown</param>
        /// <param name="items">The possible items within the dropdown</param>
        /// <param name="shownItemIndex">The currently selected index</param>
        /// <param name="shouldClear">Whether the clear button was pressed this frame</param>
        /// <returns></returns>
        private static string CreateDropdownArrayItem(string label, List<string> items, int shownItemIndex, out bool shouldClear)
        {
            shouldClear = false;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label);
            shownItemIndex = EditorGUILayout.Popup(shownItemIndex, items.ToArray());

            bool clear = GUILayout.Button("X", GUILayout.Width(20));
            if (clear)
            {
                shouldClear = true;
            }
            EditorGUILayout.EndHorizontal();
            return items[shownItemIndex];
        }

        /// <summary>
        /// Creates a dropdown
        /// </summary>
        /// <param name="label">The text next to the dropdown</param>
        /// <param name="items">The items within the dropdown</param>
        /// <param name="shownItemIndex">The index of the currently selected item</param>
        /// <returns></returns>
        private static string CreateDropdownItem(string label, List<string> items, int shownItemIndex)
        {
            EditorGUILayout.BeginHorizontal();
            if (!string.IsNullOrEmpty(label)) EditorGUILayout.LabelField(label);
            shownItemIndex = EditorGUILayout.Popup(shownItemIndex, items.ToArray());
            EditorGUILayout.EndHorizontal();
            return items[shownItemIndex];
        }

        /// <summary>
        /// Creates a textbox that lets you select a path on your PC
        /// </summary>
        /// <param name="label">The label for the textbox</param>
        /// <param name="path">The path within the textbox</param>
        /// <param name="setDirty">Whether this has been edited</param>
        /// <param name="startingPath">Default path</param>
        /// <returns></returns>
        public static string CreatePathSetter(string label, string path, out bool setDirty, string startingPath = "")
        {
            setDirty = false;
            EditorGUILayout.LabelField(label);
            EditorGUILayout.BeginHorizontal();
            string newPath = EditorGUILayout.DelayedTextField(path);
            if (Directory.Exists(newPath))
            {
                path = newPath;
                setDirty = true;
            }
            if (GUILayout.Button(EditorReferences.Instance.BrowseTexture, GUILayout.Width(20)))
            {
                newPath = EditorUtility.OpenFolderPanel("Select new path...", startingPath, "");
                if (!string.IsNullOrEmpty(newPath))
                {
                    path = newPath;
                    setDirty = true;
                }
            }
            EditorGUILayout.EndHorizontal();
            return path;
        }

        /// <summary>
        /// Creates a dropdown of Ship Logs
        /// </summary>
        /// <param name="label">The label shown in front of the dropdown</param>
        /// <param name="existingFact">The fact that's already selected</param>
        /// <param name="clearable">Whether this should be clearable</param>
        /// <param name="setDirty">Whether this has been edited this frame</param>
        /// <param name="shouldClear">Whether the clear button has been pressed this frame</param>
        /// <returns></returns>
        public static string CreateLogSelector(string label, string existingFact, bool clearable, out bool setDirty, out bool shouldClear)
        {
            setDirty = false;
            shouldClear = false;
            List<string> allFacts = new List<string>();
            allFacts.Add("(None)");
            allFacts.Add("");
            allFacts.AddRange(ShipLogManager.Instance.allRumorFactsList);
            allFacts.Add("");
            allFacts.AddRange(ShipLogManager.Instance.allExploreFactsList);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label);
            if (clearable)
            {
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    shouldClear = true;
                }
                EditorGUILayout.EndHorizontal();
            }
            string result = CreateDropdown("", existingFact, allFacts.ToArray());
            if (result == null) result = "";
            if (!clearable) EditorGUILayout.EndHorizontal();
            string[] elements = result.Split('/');
            result = elements[elements.Length - 1];

            if (result != existingFact) setDirty = true;
            return result;
        }

        /// <summary>
        /// Creates an array of Log Selectors
        /// </summary>
        /// <param name="label">The text shown before each dropdown (plus an increment)</param>
        /// <param name="existingFacts">The facts that are currently selected</param>
        /// <param name="isShowing">Whether this array is currently being shown</param>
        /// <param name="setDirty">Whether this array was edited this frame</param>
        /// <returns></returns>
        public static string[] CreateLogSelectorArray(string label, string[] existingFacts, ref bool isShowing, out bool setDirty)
        {
            if (existingFacts == null) existingFacts = new string[0];
            List<string> logs = new List<string>(existingFacts);
            setDirty = false;
            isShowing = EditorGUILayout.BeginFoldoutHeaderGroup(isShowing, label + "s");
            if (isShowing)
            {
                int clearIndex = -1;
                for (int i = 0; i < logs.Count; i++)
                {
                    string newLog = CreateLogSelector($"{label} {i}", logs[i], true, out bool shouldSetDirty, out bool shouldClear);
                    if (shouldSetDirty)
                    {
                        logs[i] = newLog;
                        setDirty = true;
                    }
                    if (shouldClear)
                    {
                        clearIndex = i;
                    }
                }

                if (clearIndex > -1)
                {
                    logs.RemoveAt(clearIndex);
                }

                string addedLog = CreateLogSelector($"Add New {label}", "", false, out bool newLogCreated, out _);
                if (newLogCreated)
                {
                    logs.Add(addedLog);
                    setDirty = true;
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            return logs.ToArray();
        }

        #endregion

        #region ShipLogEditorElements
        /// <summary>
        /// Creates a color selector for curiosities
        /// </summary>
        /// <param name="label">The text shown before the selector</param>
        /// <param name="curiosityColor">The existing curiosity color</param>
        /// <param name="highlightColor">The existing highlighted color</param>
        /// <param name="newCuriosityColor">The curiosity color this is being set to</param>
        /// <param name="newHighlightColor">The highlighted color this is being set to</param>
        public static void CreateColorSetter(string label, Color curiosityColor, Color highlightColor, out Color newCuriosityColor, out Color newHighlightColor)
        {
            EditorGUILayout.LabelField(label);
            newCuriosityColor = CreateColorItem("Normal Color", curiosityColor);
            newHighlightColor = CreateColorItem("Highlight Color", highlightColor);
            EditorGUILayout.Space();
        }

        public static bool CreateEntryButton(string label, string targetEntry, bool haveRemoveButton = false)
        {
            bool shouldRemove = false;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label);
            if (GUILayout.Button("Select Entry"))
            {
                ShipLogEditor.Instance.SelectNodeByName(targetEntry);
            }
            if (haveRemoveButton)
            {
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    shouldRemove = true;
                }
            }
            EditorGUILayout.EndHorizontal();
            return shouldRemove;
        }

        /// <summary>
        /// Creates a Rumor Fact entry item. Returns true if the fact was edited this OnInspectorGui step
        /// </summary>
        /// <param name="inputFact">The Rumor Fact being edited</param>
        /// <param name="possibleSources">List of facts that can be sources for this fact</param>
        /// <param name="requireRedraw">If this was edited this frame, so a redraw of the node window is required</param>
        /// <param name="shouldClear">Whether the clear button was pressed this frame</param>
        /// <returns></returns>
        public static bool CreateRumorFactItem(ShipLogEntry.RumorFact inputFact, List<string> possibleSources, out bool requireRedraw, out bool shouldClear)
        {
            shouldClear = false;
            bool dirty = false;
            requireRedraw = false;
            Language selectedLanguage = XMLEditorSettings.Instance.GetSelectedLanguage();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(inputFact.rumorID, EditorStyles.boldLabel);
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                if (EditorUtility.DisplayDialog("Delete Fact?", $"Are you sure you want to permanently delete fact \"{inputFact.rumorID}\"? You cannot undo this action.", "Yes", "No"))
                {
                    shouldClear = true;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            if (!possibleSources.Contains("(None)")) possibleSources.Insert(0, "(None)");
            string newSource = CreateDropdown("Source ID", inputFact.sourceID, possibleSources.ToArray());
            if (newSource != inputFact.sourceID)
            {
                requireRedraw = true;
                inputFact.sourceID = newSource;
            }
            string newName = CreateTranslatedArrayItem("Rumor Name", inputFact.rumorName, selectedLanguage, false, false, out _, out bool shouldSetNewNameDirty);
            if (shouldSetNewNameDirty)
            {
                dirty = true;
                inputFact.rumorName = newName;
            }
            int newPriority = EditorGUILayout.DelayedIntField("Rumor Name Priority", inputFact.rumorNamePriority);
            if (newPriority != inputFact.rumorNamePriority)
            {
                dirty = true;
                inputFact.rumorNamePriority = newPriority;
            }
            bool newIgnoreMoreToExplore = EditorGUILayout.ToggleLeft("Ignore More To Explore", inputFact.ignoreMoreToExplore);
            if (newIgnoreMoreToExplore != inputFact.ignoreMoreToExplore)
            {
                dirty = true;
                inputFact.ignoreMoreToExplore = newIgnoreMoreToExplore;
            }
            string newText = CreateTranslatedArrayItem("Text", inputFact.text, selectedLanguage, false, false, out _, out bool shouldSetNewTextDirty);
            if (shouldSetNewTextDirty)
            {
                dirty = true;
                inputFact.text = newText;
            }

            return requireRedraw || dirty;
        }

        /// <summary>
        /// Creates an Explore Fact entry item. Returns true if the fact was edited this OnInspectorGui step.
        /// </summary>
        /// <param name="inputFact">The explore fact being edited</param>
        /// <param name="shouldClear">Whether the clear button was pressed this frame</param>
        /// <returns></returns>
        public static bool CreateExploreFactItem(ShipLogEntry.ExploreFact inputFact, out bool shouldClear)
        {
            shouldClear = false;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(inputFact.exploreID, EditorStyles.boldLabel);
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                if (EditorUtility.DisplayDialog("Delete Fact?", $"Are you sure you want to permanently delete fact \"{inputFact.exploreID}\"? You cannot undo this action.", "Yes", "No"))
                {
                    shouldClear = true;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            bool newIgnoreMoreToExplore = EditorGUILayout.ToggleLeft("Ignore More To Explore", inputFact.ignoreMoreToExplore);
            string newText = CreateTranslatedArrayItem("Text", inputFact.text, XMLEditorSettings.Instance.GetSelectedLanguage(), false, false, out _, out _);
            bool dirty = newIgnoreMoreToExplore != inputFact.ignoreMoreToExplore || newText != inputFact.text;
            inputFact.ignoreMoreToExplore = newIgnoreMoreToExplore;
            inputFact.text = newText;

            return dirty;
        }

        /// <summary>
        /// Creates a color field
        /// </summary>
        /// <param name="label">Label of the color field</param>
        /// <param name="color">The existing color</param>
        /// <returns></returns>
        private static Color CreateColorItem(string label, Color color)
        {
            color = EditorGUILayout.ColorField(label, color);
            return color;
        }
        #endregion

        #region NomaiTextEditorElements
        /// <summary>
        /// Creates an array of conditions for Nomai text
        /// </summary>
        /// <param name="conditions">The conditions that already exist</param>
        /// <param name="asset">The Nomai Text Asset you're editing</param>
        /// <param name="setDirty">Whether anything here was edited this frame</param>
        /// <returns></returns>
        public static NomaiText.ShipLogCondition[] CreateNomaiConditionsArray(NomaiText.ShipLogCondition[] conditions, NomaiTextAsset asset, out bool setDirty)
        {
            setDirty = false;

            if (conditions == null) conditions = new NomaiText.ShipLogCondition[0];
            int clearIndex = -1;
            for (int i = 0; i < conditions.Length; i++)
            {
                var condition = conditions[i];
                condition = CreateNomaiConditionItem($"Log Set {i}", condition, asset, out bool shouldClear, out bool shouldSetDirty);
                if (shouldClear) clearIndex = i;
                if (shouldSetDirty) setDirty = true;
            }

            List<NomaiText.ShipLogCondition> newConditions = new List<NomaiText.ShipLogCondition>(conditions);
            if (clearIndex > -1)
            {
                newConditions.RemoveAt(clearIndex);
                setDirty = true;
            }

            if (GUILayout.Button("Add new condition set"))
            {
                NomaiText.ShipLogCondition newCondition = new NomaiText.ShipLogCondition();
                newCondition.revealFacts = new NomaiText.RevealFact[0];
                newConditions.Add(newCondition);
                setDirty = true;
            }
            conditions = newConditions.ToArray();

            return conditions;
        }

        /// <summary>
        /// Creates an item for the Nomai Condition array
        /// </summary>
        /// <param name="label">The text shown next to this item</param>
        /// <param name="condition">The condition being edited</param>
        /// <param name="asset">The NomaiTextAsset being edited</param>
        /// <param name="shouldClear">Whether the clear button was pressed this frame</param>
        /// <param name="setDirty">Whether this was edited this frame</param>
        /// <returns></returns>
        private static NomaiText.ShipLogCondition CreateNomaiConditionItem(string label, NomaiText.ShipLogCondition condition, NomaiTextAsset asset, out bool shouldClear, out bool setDirty)
        {
            shouldClear = false;
            setDirty = false;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label);
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                if (EditorUtility.DisplayDialog("Remove Condition Set?", "Are you sure you want to remove this condition set? This action cannot be undone.", "Yes", "No"))
                {
                    shouldClear = true;
                }
            }
            EditorGUILayout.EndHorizontal();

            bool newLocA = EditorGUILayout.ToggleLeft("Reveal for A source", condition.isLocationA);
            if (newLocA != condition.isLocationA)
            {
                condition.isLocationA = newLocA;
                setDirty = true;
            }

            bool newLocB = EditorGUILayout.ToggleLeft("Reveal for B source", condition.isLocationB);
            if (newLocB != condition.isLocationB)
            {
                condition.isLocationB = newLocB;
                setDirty = true;
            }

            if (condition.revealFacts == null) condition.revealFacts = new NomaiText.RevealFact[0];
            List<NomaiText.RevealFact> workingFacts = new List<NomaiText.RevealFact>(condition.revealFacts);
            int clearIndex = -1;
            for (int i = 0; i < workingFacts.Count; i++)
            {
                var fact = workingFacts[i];
                string newLog = CreateLogSelector("Reveal Fact", fact.factID, true, out bool shouldSetDirty, out bool shouldClearFact);
                if (shouldSetDirty)
                {
                    setDirty = true;
                    fact.factID = newLog;
                }
                if (shouldClearFact) clearIndex = i;
                if (fact.condition == null) fact.condition = "";
                fact.condition = fact.condition.Replace(" ", "");
                string[] selectedIDs = fact.condition.Split(',');
                List<string> everyID = asset.GetTextIDs();
                bool[] bools = new bool[everyID.Count];
                int bitmask = CreateBitmask(selectedIDs, everyID);

                int newFlags = EditorGUILayout.MaskField("Arcs", bitmask, everyID.ToArray());

                if (newFlags != bitmask)
                {
                    fact.condition = "";
                    bool[] bits = GetDataFromBitmask(newFlags, everyID.Count);
                    for (int j = 0; j < bits.Length; j++)
                    {
                        if (bits[j] == true)
                        {
                            fact.condition += everyID[j] + ",";
                        }
                    }
                    fact.condition = fact.condition.TrimEnd(',');
                }
            }
            if (GUILayout.Button("Add New Reveal Fact"))
            {
                workingFacts.Add(new NomaiText.RevealFact());
                setDirty = true;
            }

            condition.revealFacts = workingFacts.ToArray();

            return condition;
        }
        #endregion

        #region utilities
        /// <summary>
        /// Converts the case of a string
        /// </summary>
        /// <param name="text">The text to convert</param>
        /// <param name="casing"></param>
        /// <returns></returns>
        private static string ConvertToCase(string text, Casing casing)
        {
            if (string.IsNullOrEmpty(text)) return "";
            if (casing == Casing.DoNotChange) return text;

            // Support PascalCase and camelCase string
            text = Regex.Replace(text, "(?<!^)([A-Z])", " $1");
            // Support snake_case strings
            text = text.Replace("_", " ");
            string[] elements = text.Split(' ');

            switch (casing)
            {
                case Casing.PascalCase:
                    text = "";
                    for (int i = 0; i < elements.Length; i++)
                    {
                        if (elements[i].Length > 0)
                        {
                            string startCharacter = elements[i].Substring(0, 1).ToUpper();
                            text += startCharacter + elements[i].Remove(0, 1);
                        }
                    }
                    break;
                case Casing.snake_case:
                    text = "";
                    foreach (string element in elements)
                    {
                        text += element + "_";
                    }
                    text = text.TrimEnd('_').ToLower();
                    break;
                case Casing.SCREAMING_SNAKE_CASE:
                    text = "";
                    foreach (string element in elements)
                    {
                        text += element + "_";
                    }
                    text = text.TrimEnd('_').ToUpper();
                    break;
            }

            return text;
        }

        /// <summary>
        /// Creates a bitmask for a list of strings for Nomai text
        /// </summary>
        /// <param name="selectedIDs">The currently selected IDs in the dropdown</param>
        /// <param name="allIDs">List of all IDs in the dropdown</param>
        /// <returns></returns>
        private static int CreateBitmask(string[] selectedIDs, List<string> allIDs)
        {
            int bitmask = 0;

            for (int i = 0; i < selectedIDs.Length; i++)
            {
                if (allIDs.Contains(selectedIDs[i]))
                {
                    bitmask |= (1 << allIDs.IndexOf(selectedIDs[i]));
                }
            }
            return bitmask;
        }

        /// <summary>
        /// Retrieves a bitmask as an array of bools
        /// </summary>
        /// <param name="bitmask">Input bitmask</param>
        /// <param name="length">The size of the list</param>
        /// <returns></returns>
        private static bool[] GetDataFromBitmask(int bitmask, int length)
        {
            bool[] boolArray = new bool[length];
            for (int i = 0; i < length; i++)
            {
                boolArray[i] = (bitmask & (1 << i)) != 0;
            }
            return boolArray;
        }

        #endregion
    }
}

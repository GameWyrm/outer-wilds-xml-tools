using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine.UIElements.Experimental;

public class GUIBuilder
{

    #region nodeTree
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

    public static VisualElement CreateShipLogNode(string nodeName, string labelText, Dictionary<string, NodeManipulator> nodeManipulators, NodeWindow window, EntryType entryType)
    {
        var settings = EditorReferences.Instance;
        VisualElement nodeParent = new VisualElement();
        nodeParent.name = nodeName;
        VisualElement bg = new VisualElement();
        bg.styleSheets.Add(settings.ShipLogStyle);
        bg.name = nodeName + " Node";
        bg.EnableInClassList("node", true);
        Label label = new Label(labelText);
        label.styleSheets.Add(settings.ShipLogStyle);
        label.name = nodeName + " Label";
        label.EnableInClassList("node_label", true);
        bg.Add(label);
        label.transform.position = new Vector2(1, 1);
        Image image = new Image();
        image.styleSheets.Add(settings.ShipLogStyle);
        image.name = nodeName + " Icon";
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
            bg.transform.scale = Vector3.one * 2;
        }
        else if (entryType == EntryType.Child)
        {
            bg.transform.scale = Vector3.one * 0.6f;
        }

        var manipulator = new NodeManipulator(nodeParent);
        manipulator.RegisterCallbacksOnTarget();
        manipulator.window = window;
        manipulator.arrows = new List<ArrowManipulator>();
        nodeManipulators.Add(nodeName, manipulator);

        return nodeParent;
    }

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

        // TODO Fix
        arrowManipulator.OrientArrow();

        return offsetElement;
    }
    #endregion

    #region editorElements
    public static List<string> CreateArray(ref bool isShowing, List<string> data, string label, string itemLabel, Casing casing, bool useTranslation = false)
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

        return data;
    }

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

        return data.ToArray();
    }

    public static string[] CreateTranslatedArray(ref bool isShowing, string title, string itemsLabel, Language language, string[] dialogueKeys, string dialogueName, string nodeName)
    {
        int clearValue = -1;
        List<string> data = new List<string>(dialogueKeys);
        isShowing = EditorGUILayout.BeginFoldoutHeaderGroup(isShowing, title);
        if (isShowing)
        {
            for (int i = 0; i < dialogueKeys.Length; i++)
            {
                CreateTranslatedArrayItem($"{itemsLabel} {i + 1}", dialogueKeys[i], language, true, out bool shouldClear);
                if (shouldClear) clearValue = i;
            }
            if (clearValue >= 0) data.RemoveAt(clearValue);

            EditorGUILayout.LabelField($"------------------ ADD NEW {itemsLabel.ToUpper()} ------------------");
            string prefix = XMLEditorSettings.Instance.modPrefix.ToUpper();
            if (GUILayout.Button("New Entry (Auto)"))
            {
                string newName = "";
                if (!string.IsNullOrEmpty(prefix)) newName = prefix + "_";
                newName += $"{dialogueName.ToUpper()}_{nodeName.ToUpper()}_{data.Count}";
                data.Add(newName);
            }
            string newCustomTranslation = EditorGUILayout.DelayedTextField("New Entry", "");
            if (!string.IsNullOrEmpty(newCustomTranslation))
            {
                if (!newCustomTranslation.ToUpper().StartsWith(prefix)) newCustomTranslation = prefix + "_" + newCustomTranslation;
                data.Add(newCustomTranslation);
            }
            if (language.tieredDialogueKeys != null)
            {
                int reuseIndex = EditorGUILayout.Popup("Reuse entry", 0, language.tieredDialogueKeys.ToArray());
                string reusedTranslation = language.tieredDialogueKeys[reuseIndex];
                if (reuseIndex > 0) data.Add(reusedTranslation);
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        return data.ToArray();
    }

    public static string CreateDropdown(string label, string selectedCondition, string[] items)
    {
        int index = -1;
        List<string> conditionList = new List<string>();
        conditionList.Add("(None)");
        conditionList.AddRange(items);
        if (conditionList.Contains(selectedCondition)) index = conditionList.IndexOf(selectedCondition);
        {
            conditionList.Add(selectedCondition);
            index = conditionList.Count - 1;
        }

        selectedCondition = CreateDropdownItem(label, conditionList, index);

        if (selectedCondition == "(None)") selectedCondition = "";

        return selectedCondition;
    }

    public static string CreateTranslatedArrayItem(string label, string key, Language lang, bool clearable, out bool shouldClear)
    {
        EditorGUILayout.BeginHorizontal();
        string newKey = EditorGUILayout.DelayedTextField(label, key);
        if (clearable)
        {
            bool clear = GUILayout.Button("X", GUILayout.Width(20));
            shouldClear = clear;
        }
        else shouldClear = false;
        EditorGUILayout.EndHorizontal();
        if (newKey != key)
        {
            bool success = lang.TryRenameDialogueKey(key, newKey);
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
                    language.TryRenameDialogueKey(key, newKey);
                }
                Debug.Log($"Renamed key {key} to {newKey}.");
            }
        }
        lang.SetDialogueValue(newKey, EditorGUILayout.TextArea(lang.GetDialogueValue(newKey)));
        return newKey;
    }

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

    private static string CreateDropdownItem(string label, List<string> items, int shownItemIndex)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label);
        shownItemIndex = EditorGUILayout.Popup(shownItemIndex, items.ToArray());
        EditorGUILayout.EndHorizontal();
        return items[shownItemIndex];
    }


    #endregion

    #region utilities
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

    #endregion
}

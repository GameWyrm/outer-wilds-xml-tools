using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GUIBuilder
{

    #region nodeTree
    public static VisualElement CreateDialogueNode(string nodeName, Dictionary<string, NodeManipulator> nodeManipulators)
    {
        var settings = DialogueEditorSettings.Instance;
        VisualElement newNode = new VisualElement();
        newNode.styleSheets.Add(settings.Style);
        newNode.name = nodeName;
        newNode.EnableInClassList("node_bg", true);
        Label label = new Label(nodeName);
        label.styleSheets.Add(settings.Style);
        label.name = "node_label";
        label.EnableInClassList("node_label", true);
        newNode.Add(label);

        var nodeManipulator = new NodeManipulator(newNode);
        nodeManipulator.RegisterCallbacksOnTarget();
        nodeManipulator.arrows = new List<ArrowManipulator>();
        nodeManipulators.Add(nodeName, nodeManipulator);

        return newNode;
    }

    public static VisualElement CreateArrow(VisualElement source, VisualElement target, string nodeName, string targetName, Dictionary<string, NodeManipulator> nodeManipulators, int arrowState)
    {
        VisualElement element = new VisualElement();
        var settings = DialogueEditorSettings.Instance;
        element.styleSheets.Add(settings.Style);
        element.EnableInClassList("arrow_container", true);
        Image newLine = new Image();
        newLine.image = settings.LineTexture;
        newLine.styleSheets.Add(settings.Style);
        newLine.EnableInClassList("line", true);
        element.Add(newLine);
        Image newArrow = new Image();
        newArrow.image = settings.ArrowTexture;
        newArrow.styleSheets.Add(settings.Style);
        newArrow.EnableInClassList("arrow", true);
        element.Add(newArrow);

        var arrowManipulator = new ArrowManipulator();
        arrowManipulator.sourceNode = source;
        arrowManipulator.targetNode = target;
        arrowManipulator.line = newLine;
        arrowManipulator.arrow = newArrow;

        nodeManipulators[nodeName].arrows.Add(arrowManipulator);
        nodeManipulators[targetName].arrows.Add(arrowManipulator);
        // TODO Fix
        arrowManipulator.OrientArrow(arrowState);

        return element;
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

    private static void CreateTranslatedArrayItem()
    {

    }

    private static string CreateArrayItem(string itemLabel, string data, out bool shouldClear)
    {
        shouldClear = false;
        EditorGUILayout.BeginHorizontal();
        data = EditorGUILayout.TextField(itemLabel, data);
        bool clear = GUILayout.Button("X", GUILayout.Width(20));
        if (clear)
        {
            shouldClear = true;
        }
        EditorGUILayout.EndHorizontal();

        return data;
    }

    public static void CreateDropdown(string label, List<string> items, string shownItem)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label);



        if (EditorGUILayout.DropdownButton(new GUIContent(shownItem), FocusType.Passive))
        {
            GenericMenu menu = new GenericMenu();
            foreach (string item in items)
            {
                menu.AddItem(new GUIContent(item), false, EditMenuSelection);
            }
            menu.ShowAsContext();
        }
        EditorGUILayout.EndHorizontal();
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

    private static void EditMenuSelection()
    {
        // TODO add code
    }
    #endregion
}

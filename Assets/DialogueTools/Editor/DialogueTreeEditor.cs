using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DialogueTreeAsset))]
public class DialogueTreeEditor : Editor
{
    public static DialogueTreeAsset selectedAsset;
    public static DialogueNode activeNode;

    private static bool showEntryConditions;
    private static bool showDialogues;
    private static bool showRevealFacts;
    private static bool showSetConditions;
    private static bool showDialogueTargetConditions;
    private static List<bool> showRequiredConditions;
    private static List<bool> showRequiredPersistentConditions;
    private static List<bool> showCancelledPersistentConditions;
    private static List<string> nodeNames;

    private void OnEnable()
    {
        selectedAsset = serializedObject.targetObject as DialogueTreeAsset;
    }

    public static void SelectionUpdate(DialogueNode node)
    {
        activeNode = node;
        showEntryConditions = true;
        showDialogues = true;
        showRevealFacts = true;
        showSetConditions = true;
        showDialogueTargetConditions = true;
        if (activeNode.dialogueOptionsList != null && activeNode.dialogueOptionsList.dialogueOptions != null)
        {
            showRequiredConditions = new List<bool>(new bool[activeNode.dialogueOptionsList.dialogueOptions.Length]);
            showRequiredPersistentConditions = new List<bool>(new bool[activeNode.dialogueOptionsList.dialogueOptions.Length]);
            showCancelledPersistentConditions = new List<bool>(new bool[activeNode.dialogueOptionsList.dialogueOptions.Length]);
        }
        else
        {
            showRequiredConditions = null;
            showRequiredPersistentConditions = null;
            showCancelledPersistentConditions = null;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        nodeNames = new List<string>();
        nodeNames.Add(string.Empty);
        bool defaultError = true;
        bool duplicateNodeError = false;
        List<string> offendingNodes = new List<string>();
        foreach (var node in selectedAsset.tree.dialogueNodes)
        {
            if (node.entryConditions.Contains("DEFAULT")) defaultError = false;
            if (nodeNames.Contains(node.nodeName))
            {
                duplicateNodeError = true;
                if (!offendingNodes.Contains(node.nodeName)) offendingNodes.Add(node.nodeName);
            }
            else
            {
                nodeNames.Add(node.nodeName);
            }
        }
        if (defaultError || duplicateNodeError)
        {
            string errorText = "ERRORS: ";
            if (defaultError) errorText += "\nThere is no \"DEFAULT\" node set. Dialogue cannot be started.";
            if (duplicateNodeError) errorText += $"\nYou have two nodes with the same name. {offendingNodes.ToString()}";
            EditorGUILayout.HelpBox(errorText, MessageType.Error);
        }

        if (activeNode == null)
        {
            // When no node is selected
            if (GUILayout.Button("Open in Editor", GUILayout.ExpandWidth(true))) OpenEditor();
        }
        else
        {
            DrawNodeData();
        }
    }

    private void DrawNodeData()
    {
        // Name
        EditorGUILayout.DelayedTextField("Name", activeNode.nodeName);

        // Entry Conditions
        showEntryConditions = EditorGUILayout.BeginFoldoutHeaderGroup(showEntryConditions, "Entry Conditions");
        if (activeNode.entryConditions != null)
        {
            if (showEntryConditions)
            {
                for (int i = 0; i < activeNode.entryConditions.Length; i++)
                {
                    var condition = activeNode.entryConditions[i];
                    EditorGUILayout.DelayedTextField($"Condition {i}", condition);
                }
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space();

        // Randomize
        EditorGUILayout.Toggle("Randomize", activeNode.randomize != null);

        // Dialogues
        showDialogues = EditorGUILayout.BeginFoldoutHeaderGroup(showDialogues, "Dialogues");
        if (activeNode.dialogues != null && showDialogues)
        {
            foreach (var dialogue in activeNode.dialogues)
            {
                if (dialogue.pages != null &&  dialogue.pages.Length > 0)
                {
                    for (int i = 0; i < dialogue.pages.Length; i++)
                    {
                        EditorGUILayout.DelayedTextField($"Page {i + 1}", dialogue.pages[i]);
                    }
                }
                EditorGUILayout.Space();
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space();

        // Reveal Facts
        showRevealFacts = EditorGUILayout.BeginFoldoutHeaderGroup(showRevealFacts, "Reveal Facts");
        if (activeNode.revealFacts != null && activeNode.revealFacts.factIDs != null && showRevealFacts)
        {
            var facts = activeNode.revealFacts.factIDs;
            for (int i = 0; i < facts.Length; i++)
            {
                EditorGUILayout.DelayedTextField($"FactID {i}", facts[i]);
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space();

        // Set Persistent Condition
        EditorGUILayout.DelayedTextField("Set Persistent Condition", activeNode.setPersistentCondition);

        // Set Conditions
        showSetConditions = EditorGUILayout.BeginFoldoutHeaderGroup(showSetConditions, "Set Loop Conditions");
        if (activeNode.setConditions != null && showSetConditions)
        {
            for (int i = 0; i < activeNode.setConditions.Length; i++)
            {
                EditorGUILayout.DelayedTextField($"Set Condition {i}", activeNode.setConditions[i]);
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Space();

        // Disable Persistent Condition
        EditorGUILayout.DelayedTextField("Disable Persistent Condition", activeNode.disablePersistentCondition);

        // Dialogue Target Shiplog Conditions
        showDialogueTargetConditions = EditorGUILayout.BeginFoldoutHeaderGroup(showDialogueTargetConditions, "Dialogue Target Shiplog Conditions");
        if (activeNode.dialogueTargetShipLogConditions != null && showDialogueTargetConditions)
        {
            for (int i = 0; i < activeNode.dialogueTargetShipLogConditions.Length; i++)
            {
                EditorGUILayout.DelayedTextField($"Shiplog Fact {i}", activeNode.dialogueTargetShipLogConditions[i]);
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        // Dialogue Target
        CreateDropdown("Dialogue Target", nodeNames, activeNode.dialogueTarget);
        //EditorGUILayout.DelayedTextField("Dialogue Target", activeNode.dialogueTarget);
        EditorGUILayout.Space();

        // Dialogue Options List
        EditorGUILayout.LabelField("Dialogue Options");
        if (activeNode.dialogueOptionsList != null && activeNode.dialogueOptionsList.dialogueOptions != null)
        {
            for (int i = 0; i < activeNode.dialogueOptionsList.dialogueOptions.Length; i++)
            {
                var option = activeNode.dialogueOptionsList.dialogueOptions[i];
                string optionName = option.dialogueTarget;
                if (string.IsNullOrEmpty(optionName)) optionName = "EXIT";
                EditorGUILayout.LabelField($"- Option {optionName}");

                showRequiredConditions[i] = EditorGUILayout.BeginFoldoutHeaderGroup(showRequiredConditions[i], "Required Ship Log Conditions");
                if (showRequiredConditions[i] && option.requiredLogConditions != null)
                {
                    for (int j = 0; j < option.requiredLogConditions.Length; j++)
                    {
                        EditorGUILayout.DelayedTextField($"Required Condition {j}", option.requiredLogConditions[j]);
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

                showRequiredPersistentConditions[i] = EditorGUILayout.BeginFoldoutHeaderGroup(showRequiredPersistentConditions[i], "Required Persistent Conditions");
                if (showRequiredPersistentConditions[i] && option.requiredPersistentConditions != null)
                {
                    for (int j = 0; j < option.requiredLogConditions.Length; j++)
                    {
                        EditorGUILayout.DelayedTextField($"Required Condition {j}", option.requiredPersistentConditions[j]);
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

                showCancelledPersistentConditions[i] = EditorGUILayout.BeginFoldoutHeaderGroup(showCancelledPersistentConditions[i], "Cancelled Persistent Conditions");
                if (showCancelledPersistentConditions[i] && option.cancelledPersistentConditions != null)
                {
                    for (int j = 0; j < option.cancelledPersistentConditions.Length; j++)
                    {
                        EditorGUILayout.DelayedTextField($"Cancelled Condition {j}", option.requiredPersistentConditions[j]);
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

                EditorGUILayout.DelayedTextField("Required Loop Condition", option.requiredCondition);
                EditorGUILayout.DelayedTextField("Cancelled Loop Condition", option.cancelledCondition);
                EditorGUILayout.DelayedTextField("Text", option.text);
                CreateDropdown("Dialogue Target", nodeNames, option.dialogueTarget);
                //EditorGUILayout.DelayedTextField("Dialogue Target", option.dialogueTarget);
                EditorGUILayout.DelayedTextField("Loop Condition To Set", option.conditionToSet);
                EditorGUILayout.DelayedTextField("Loop Condition To Cancel", option.conditionToCancel);

                EditorGUILayout.Space();
            }
        }
    }

    private void OpenEditor()
    {
        selectedAsset = serializedObject.targetObject as DialogueTreeAsset;
        if (selectedAsset != null)
        {
            DialogueEditor.OpenWindowWithSelection(selectedAsset);
        }
        else
        {
            Debug.LogError($"{serializedObject.targetObject.name} is still null for some reason.");
        }
    }

    private void CreateDropdown(string label, List<string> items, string shownItem)
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

    private void EditMenuSelection()
    {
        // TODO add code
    }
}

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
    private static bool[] showDialogues;
    private static bool showRevealFacts;
    private static bool showSetConditions;
    private static bool showDialogueTargetConditions;
    private static bool[] showRequiredLogConditions;
    private static bool[] showRequiredPersistentConditions;
    private static bool[] showCancelledPersistentConditions;
    private static List<string> nodeNames;

    private void OnEnable()
    {
        selectedAsset = serializedObject.targetObject as DialogueTreeAsset;
    }

    public static void SelectionUpdate(DialogueNode node)
    {
        activeNode = node;
        showEntryConditions = true;
        if (activeNode.dialogues != null)
        {
            showDialogues = new bool[activeNode.dialogues.Length];
        }
        else
        {
            showDialogues = null;
        }
        showRevealFacts = true;
        showSetConditions = true;
        showDialogueTargetConditions = true;
        if (activeNode.dialogueOptionsList != null && activeNode.dialogueOptionsList.dialogueOptions != null)
        {
            showRequiredLogConditions = new bool[activeNode.dialogueOptionsList.dialogueOptions.Length];
            showRequiredPersistentConditions = new bool[activeNode.dialogueOptionsList.dialogueOptions.Length];
            showCancelledPersistentConditions = new bool[activeNode.dialogueOptionsList.dialogueOptions.Length];
        }
        else
        {
            showRequiredLogConditions = null;
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
            if (node.entryConditions != null)
            {
                if (node.entryConditions.Contains("DEFAULT")) defaultError = false;
            }
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
        if (GUILayout.Button("Open Settings")) OpenSettings();
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
        XMLEditorSettings settings = XMLEditorSettings.Instance;
        string[] loopConditions = settings.GetConditionList(false).ToArray();
        string[] persistentConditions = settings.GetConditionList(true).ToArray();
        bool rebuildNodeTree = false;

        // Language Setting
        settings.selectedLanguage = EditorGUILayout.Popup("Language: ", settings.selectedLanguage, settings.supportedLanguages.Select(x => x.name).ToArray());
        EditorGUILayout.Space();

        // Name
        EditorGUILayout.DelayedTextField("Name", activeNode.nodeName);

        // Entry Conditions
        List<string> entryList = new List<string>();
        entryList.Add("DEFAULT");
        entryList.Add("");
        entryList.AddRange(loopConditions);
        entryList.Add("");
        entryList.AddRange(persistentConditions);

        bool hasADefaultNode = false;
        foreach (var entry in activeNode.entryConditions)
        {
            if (entry == "DEFAULT")
            {
                hasADefaultNode = true;
                break;
            }
        }
        bool didHaveADefaultNode = hasADefaultNode;
        activeNode.entryConditions = GUIBuilder.CreateDropdownArray(ref showEntryConditions, "Entry Conditions", "Condition", activeNode.entryConditions, entryList.ToArray());
        hasADefaultNode = false;
        foreach (var entry in activeNode.entryConditions)
        {
            if (entry == "DEFAULT")
            {
                hasADefaultNode = true;
                break;
            }
        }
        if (hasADefaultNode != didHaveADefaultNode)
        {
            string debug = "Rebuilding";
            Debug.Log(debug);
            rebuildNodeTree = true;
        }
        EditorGUILayout.Space();

        // Randomize
        EditorGUILayout.Toggle("Randomize", activeNode.randomize);

        // Dialogues
        for (int i = 0; i < activeNode.dialogues.Length; i++)
        {
            activeNode.dialogues[i].pages = GUIBuilder.CreateTranslatedArray(ref showDialogues[i], $"Entries {i}", "Page", settings.GetSelectedLanguage(), activeNode.dialogues[i].pages, selectedAsset.tree.nameField, activeNode.nodeName);
        }
        //showDialogues = EditorGUILayout.BeginFoldoutHeaderGroup(showDialogues, "Dialogues");
        //if (activeNode.dialogues != null && showDialogues)
        //{
        //    foreach (var dialogue in activeNode.dialogues)
        //    {
        //        if (dialogue.pages != null &&  dialogue.pages.Length > 0)
        //        {
        //            for (int i = 0; i < dialogue.pages.Length; i++)
        //            {
        //                EditorGUILayout.DelayedTextField($"Page {i + 1}", dialogue.pages[i]);
        //            }
        //        }
        //        EditorGUILayout.Space();
        //    }
        //}
        //EditorGUILayout.EndFoldoutHeaderGroup();
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
        activeNode.setPersistentCondition = GUIBuilder.CreateDropdown("Set Persistent Condition", activeNode.setPersistentCondition, persistentConditions);
        EditorGUILayout.Space();

        // Set Conditions
        activeNode.setConditions = GUIBuilder.CreateDropdownArray(ref showSetConditions, "Set Loop Conditions", "Condition", activeNode.setConditions, loopConditions);
        EditorGUILayout.Space();

        // Disable Persistent Condition
        activeNode.disablePersistentCondition = GUIBuilder.CreateDropdown("Disable Persistent Condition", activeNode.disablePersistentCondition, persistentConditions);
        EditorGUILayout.Space();

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
        EditorGUILayout.Space();

        // Dialogue Target
        string oldTarget = activeNode.dialogueTarget;
        activeNode.dialogueTarget = GUIBuilder.CreateDropdown("Dialogue Target", activeNode.dialogueTarget, nodeNames.ToArray());
        if (oldTarget != activeNode.dialogueTarget)
        {
            rebuildNodeTree = true;
        }
        EditorGUILayout.Space();

        // Dialogue Options List
        EditorGUILayout.LabelField("Dialogue Options");

        bool addNew = GUILayout.Button("+ Add New Option");
        if (addNew)
        {
            if (activeNode.dialogueOptionsList == null) activeNode.dialogueOptionsList = new DialogueNode.DialogueOptionsList();
            if (activeNode.dialogueOptionsList.dialogueOptions == null) activeNode.dialogueOptionsList.dialogueOptions = new DialogueNode.DialogueOption[0];

            List<DialogueNode.DialogueOption> newOptions = new List<DialogueNode.DialogueOption>(activeNode.dialogueOptionsList.dialogueOptions);
            DialogueNode.DialogueOption option = new DialogueNode.DialogueOption();
            option.requiredLogConditions = new string[0];
            option.requiredPersistentConditions = new string[0];
            option.cancelledPersistentConditions = new string[0];

            List<bool> newLogBools = new List<bool>(showRequiredLogConditions);
            newLogBools.Add(false);
            showRequiredLogConditions = newLogBools.ToArray();
            List<bool> newReqBools = new List<bool>(showRequiredPersistentConditions);
            newReqBools.Add(false);
            showRequiredPersistentConditions = newReqBools.ToArray();
            List<bool> newCancelledBools = new List<bool>(showCancelledPersistentConditions);
            newCancelledBools.Add(false);
            showCancelledPersistentConditions = newCancelledBools.ToArray();

            newOptions.Add(option);
            activeNode.dialogueOptionsList.dialogueOptions = newOptions.ToArray();
            EditorUtility.SetDirty(selectedAsset);
        }

        if (activeNode.dialogueOptionsList != null && activeNode.dialogueOptionsList.dialogueOptions != null)
        {
            int clearIndex = -1;
            for (int i = 0; i < activeNode.dialogueOptionsList.dialogueOptions.Length; i++)
            {
                var option = activeNode.dialogueOptionsList.dialogueOptions[i];
                string optionName = option.dialogueTarget;
                if (string.IsNullOrEmpty(optionName)) optionName = "EXIT";
                EditorGUILayout.LabelField($"- Option {optionName}");

                // TODO replace with log selector
                //showRequiredLogConditions[i] = EditorGUILayout.BeginFoldoutHeaderGroup(showRequiredLogConditions[i], "Required Ship Log Conditions");
                //if (showRequiredLogConditions[i] && option.requiredLogConditions != null)
                //{
                //    for (int j = 0; j < option.requiredLogConditions.Length; j++)
                //    {
                //        EditorGUILayout.DelayedTextField($"Required Condition {j}", option.requiredLogConditions[j]);
                //    }
                //}
                //EditorGUILayout.EndFoldoutHeaderGroup();

                option.requiredPersistentConditions = GUIBuilder.CreateDropdownArray(ref showRequiredPersistentConditions[i], "Required Persistent Conditions", "Condition", option.requiredPersistentConditions, persistentConditions);
                //showRequiredPersistentConditions[i] = EditorGUILayout.BeginFoldoutHeaderGroup(showRequiredPersistentConditions[i], "Required Persistent Conditions");
                //if (showRequiredPersistentConditions[i] && option.requiredPersistentConditions != null)
                //{
                //    for (int j = 0; j < option.requiredLogConditions.Length; j++)
                //    {
                //        EditorGUILayout.DelayedTextField($"Required Condition {j}", option.requiredPersistentConditions[j]);
                //    }
                //}
                //EditorGUILayout.EndFoldoutHeaderGroup();

                option.cancelledPersistentConditions = GUIBuilder.CreateDropdownArray(ref showCancelledPersistentConditions[i], "Cancelled Persistent Conditions", "Condition", option.cancelledPersistentConditions, persistentConditions);
                //showCancelledPersistentConditions[i] = EditorGUILayout.BeginFoldoutHeaderGroup(showCancelledPersistentConditions[i], "Cancelled Persistent Conditions");
                //if (showCancelledPersistentConditions[i] && option.cancelledPersistentConditions != null)
                //{
                //    for (int j = 0; j < option.cancelledPersistentConditions.Length; j++)
                //    {
                //        EditorGUILayout.DelayedTextField($"Cancelled Condition {j}", option.requiredPersistentConditions[j]);
                //    }
                //}
                //EditorGUILayout.EndFoldoutHeaderGroup();

                option.requiredCondition = GUIBuilder.CreateDropdown("Required Loop Condition", option.requiredCondition, loopConditions);
                //EditorGUILayout.DelayedTextField("Required Loop Condition", option.requiredCondition);
                option.cancelledCondition = GUIBuilder.CreateDropdown("Cancelled Loop Condition", option.cancelledCondition, loopConditions);
                //EditorGUILayout.DelayedTextField("Cancelled Loop Condition", option.cancelledCondition);
                // TODO translated text field
                EditorGUILayout.DelayedTextField("Text", option.text);
                GUIBuilder.CreateDropdown("Dialogue Target", option.dialogueTarget, nodeNames.ToArray());
                //EditorGUILayout.DelayedTextField("Dialogue Target", option.dialogueTarget);
                option.conditionToSet = GUIBuilder.CreateDropdown("Loop Condition To Set", option.conditionToSet, loopConditions);
                option.conditionToCancel = GUIBuilder.CreateDropdown("Loop Condition To Cancel", option.conditionToCancel, loopConditions);
                //EditorGUILayout.DelayedTextField("Loop Condition To Set", option.conditionToSet);
                //EditorGUILayout.DelayedTextField("Loop Condition To Cancel", option.conditionToCancel);
                EditorGUILayout.Space();
                bool deleteOption = GUILayout.Button("Delete this option (No Undo)");
                if (deleteOption) clearIndex = i;
                EditorGUILayout.Space();
            }
            if (clearIndex != -1)
            {
                List<DialogueNode.DialogueOption> workingOptions = new List<DialogueNode.DialogueOption>(activeNode.dialogueOptionsList.dialogueOptions);
                workingOptions.RemoveAt(clearIndex);
                activeNode.dialogueOptionsList.dialogueOptions = workingOptions.ToArray();
                EditorUtility.SetDirty(selectedAsset);
            }
            
        }

        if (rebuildNodeTree)
        {
            if (DialogueEditor.instance != null)
            {
                DialogueEditor.instance.BuildNodeTree();
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

    private void OpenSettings()
    {
        Selection.activeObject = XMLEditorSettings.Instance;
    }
}

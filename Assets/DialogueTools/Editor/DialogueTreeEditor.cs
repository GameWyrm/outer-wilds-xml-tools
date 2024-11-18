using System.Collections;
using System.Collections.Generic;
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
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
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
        if (activeNode.entryConditions != null)
        {
            showEntryConditions = EditorGUILayout.BeginFoldoutHeaderGroup(showEntryConditions, "Entry Conditions");
            if (showEntryConditions)
            {
                for (int i = 0; i < activeNode.entryConditions.Length; i++)
                {
                    var condition = activeNode.entryConditions[i];
                    EditorGUILayout.DelayedTextField($"Condition {i}", condition);
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

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

        // Dialogue Target
        EditorGUILayout.DelayedTextField("Dialogue Target", activeNode.dialogueTarget);

        // Dialogue Options List
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
}

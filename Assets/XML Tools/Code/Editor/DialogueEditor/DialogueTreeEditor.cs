using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace XmlTools
{
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

        private static List<string> characters = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

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
            selectedAsset.tree.nameField = GUIBuilder.CreateTranslatedArrayItem("Name of NPC (set to \"SIGN\" for \"Read\" prompt)", selectedAsset.tree.nameField, XMLEditorSettings.Instance.GetSelectedLanguage(), false, true, out _, out bool setDirty);
            if (setDirty) EditorUtility.SetDirty(selectedAsset);
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
            Language language = settings.GetSelectedLanguage();
            string[] loopConditions = settings.GetConditionList(false).ToArray();
            string[] persistentConditions = settings.GetConditionList(true).ToArray();

            bool setDirty = false;
            bool rebuildNodeTree = false;

            // Language Setting
            settings.selectedLanguage = EditorGUILayout.Popup("Language: ", settings.selectedLanguage, settings.supportedLanguages.Select(x => x.name).ToArray());
            EditorGUILayout.Space();

            // Name
            string newName = EditorGUILayout.DelayedTextField("Name", activeNode.nodeName);
            if (newName != activeNode.nodeName)
            {
                if (selectedAsset.NodeDatas.Find(x => x.name == newName) != null)
                {
                    Debug.LogError($"Node with name {newName} already exist in this Dialogue Tree. Cannot rename.");
                }
                else
                {
                    var oldNode = selectedAsset.NodeDatas.Find(x => x.name == activeNode.nodeName);
                    oldNode.name = newName;

                    activeNode.nodeName = newName;

                    rebuildNodeTree = true;
                }
            }

            // Entry Conditions
            List<string> entryList = new List<string>();
            entryList.Add("DEFAULT");
            entryList.Add("");
            entryList.AddRange(loopConditions);
            entryList.Add("");
            entryList.AddRange(persistentConditions);

            bool hasADefaultNode = false;
            if (activeNode.entryConditions == null) activeNode.entryConditions = new string[0];
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
            if (activeNode.dialogues == null) activeNode.dialogues = new DialogueNode.Dialogue[0];
            int dialoguesClearIndex = -1;
            for (int i = 0; i < activeNode.dialogues.Length; i++)
            {
                activeNode.dialogues[i].pages = GUIBuilder.CreateTranslatedArray(ref showDialogues[i], $"Entries {i}", "Page", language, activeNode.dialogues[i].pages, selectedAsset.tree.nameField, activeNode.nodeName, true, out bool dialogueDirty, true, out bool dialogueCleared);
                if (dialogueDirty) setDirty = true;
                if (dialogueCleared)
                {
                    dialoguesClearIndex = i;
                    setDirty = true;
                }
            }
            if (dialoguesClearIndex >= 0)
            {
                List<DialogueNode.Dialogue> newLogs = new List<DialogueNode.Dialogue>(activeNode.dialogues);
                newLogs.RemoveAt(dialoguesClearIndex);
                activeNode.dialogues = newLogs.ToArray();
                showDialogues = new bool[activeNode.dialogues.Length];
            }
            if (GUILayout.Button("Add New Entry Set"))
            {
                List<DialogueNode.Dialogue> newLogs = new List<DialogueNode.Dialogue>(activeNode.dialogues);
                DialogueNode.Dialogue newlog = new DialogueNode.Dialogue();
                newlog.pages = new string[0];
                newLogs.Add(newlog);
                activeNode.dialogues = newLogs.ToArray();
                showDialogues = new bool[activeNode.dialogues.Length];
                setDirty = true;
            }
            EditorGUILayout.Space();

            // Reveal Facts
            if (activeNode.revealFacts == null) activeNode.revealFacts = new DialogueNode.RevealFacts();
            if (activeNode.revealFacts.factIDs == null) activeNode.revealFacts.factIDs = new string[0];
            activeNode.revealFacts.factIDs = GUIBuilder.CreateLogSelectorArray("Reveal Fact", activeNode.revealFacts.factIDs, ref showRevealFacts, out bool shouldSetDirty);
            if (shouldSetDirty) setDirty = true;

            EditorGUILayout.Space();

            // Set Persistent Condition
            activeNode.setPersistentCondition = GUIBuilder.CreateDropdown("Set Persistent Condition", activeNode.setPersistentCondition, persistentConditions);
            EditorGUILayout.Space();

            // Set Conditions
            if (activeNode.setConditions == null) activeNode.setConditions = new string[0];
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
            EditorGUILayout.LabelField("================== Dialogue Options ==================");

            EditorGUILayout.LabelField("================== CREATE NEW OPTION ==================");
            string prefix = settings.modPrefix.ToUpper();
            string newKey = "";
            bool addNew = false;
            if (GUILayout.Button("New Entry (Auto)"))
            {
                if (!string.IsNullOrEmpty(prefix)) newKey = prefix + "_";
                string suffix = "";
                if (activeNode.dialogueOptionsList == null || activeNode.dialogueOptionsList.dialogueOptions == null) suffix = characters[0];
                else
                {
                    int i = activeNode.dialogueOptionsList.dialogueOptions.Length;
                    if (i > characters.Count)
                    {
                        suffix = characters[Mathf.FloorToInt(i / characters.Count)];
                        i = i % characters.Count;
                    }
                    suffix += characters[i];
                }
                newKey += $"{selectedAsset.tree.nameField.ToUpper()}_{activeNode.nodeName.ToUpper()}_{suffix}";
                if (!string.IsNullOrEmpty(language.GetDialogueValue(newKey))) newKey += "_1";
                addNew = true;
            }
            string newCustomTranslation = EditorGUILayout.DelayedTextField("New Entry", "");
            if (!string.IsNullOrEmpty(newCustomTranslation))
            {
                if (!newCustomTranslation.ToUpper().StartsWith(prefix)) newCustomTranslation = prefix + "_" + newCustomTranslation;
                newKey = newCustomTranslation;
                addNew = true;
            }
            if (language.tieredDialogueKeys != null)
            {
                if (language.tieredDialogueKeys.Count == 0) language.BuildTieredDialogueKeys();
                if (language.tieredDialogueKeys.Count == 0)
                {
                    Debug.LogWarning($"Language {language.name} could not tier its dialogue keys, unable to retrieve translation keys.");
                }
                else
                {
                    int reuseIndex = EditorGUILayout.Popup("Reuse entry", 0, language.tieredDialogueKeys.ToArray());
                    string reusedTranslation = language.tieredDialogueKeys[reuseIndex];
                    if (reuseIndex > 0)
                    {
                        newKey = reusedTranslation;
                        addNew = true;
                    }
                }
            }

            if (addNew)
            {
                if (activeNode.dialogueOptionsList == null) activeNode.dialogueOptionsList = new DialogueNode.DialogueOptionsList();
                if (activeNode.dialogueOptionsList.dialogueOptions == null) activeNode.dialogueOptionsList.dialogueOptions = new DialogueNode.DialogueOption[0];

                List<DialogueNode.DialogueOption> newOptions = new List<DialogueNode.DialogueOption>(activeNode.dialogueOptionsList.dialogueOptions);
                DialogueNode.DialogueOption option = new DialogueNode.DialogueOption();
                option.requiredLogConditions = new string[0];
                option.requiredPersistentConditions = new string[0];
                option.cancelledPersistentConditions = new string[0];

                option.text = newKey;

                if (showRequiredLogConditions == null) showRequiredLogConditions = new bool[0];
                List<bool> newLogBools = new List<bool>(showRequiredLogConditions);
                newLogBools.Add(false);
                showRequiredLogConditions = newLogBools.ToArray();
                if (showRequiredPersistentConditions == null) showRequiredPersistentConditions = new bool[0];
                List<bool> newReqBools = new List<bool>(showRequiredPersistentConditions);
                newReqBools.Add(false);
                showRequiredPersistentConditions = newReqBools.ToArray();
                if (showCancelledPersistentConditions == null) showCancelledPersistentConditions = new bool[0];
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
                    EditorGUILayout.LabelField($"------------------ Option {optionName} ------------------");

                    option.requiredLogConditions = GUIBuilder.CreateLogSelectorArray("Required Ship Log", option.requiredLogConditions, ref showRequiredLogConditions[i], out bool requiredLogSetDirty);

                    if (requiredLogSetDirty) setDirty = true;

                    if (option.requiredPersistentConditions == null) option.requiredPersistentConditions = new string[0];
                    option.requiredPersistentConditions = GUIBuilder.CreateDropdownArray(ref showRequiredPersistentConditions[i], "Required Persistent Conditions", "Condition", option.requiredPersistentConditions, persistentConditions);
                    if (option.cancelledPersistentConditions == null) option.cancelledPersistentConditions = new string[0];
                    option.cancelledPersistentConditions = GUIBuilder.CreateDropdownArray(ref showCancelledPersistentConditions[i], "Cancelled Persistent Conditions", "Condition", option.cancelledPersistentConditions, persistentConditions);

                    option.requiredCondition = GUIBuilder.CreateDropdown("Required Loop Condition", option.requiredCondition, loopConditions);
                    option.cancelledCondition = GUIBuilder.CreateDropdown("Cancelled Loop Condition", option.cancelledCondition, loopConditions);
                    option.text = GUIBuilder.CreateTranslatedArrayItem("Text", option.text, settings.GetSelectedLanguage(), false, true, out _, out bool shouldSetOptionDirty);
                    if (shouldSetOptionDirty) setDirty = true;
                    string oldOptionTarget = option.dialogueTarget;
                    option.dialogueTarget = GUIBuilder.CreateDropdown("Dialogue Target", option.dialogueTarget, nodeNames.ToArray());
                    if (oldOptionTarget != option.dialogueTarget)
                    {
                        rebuildNodeTree = true;
                    }
                    option.conditionToSet = GUIBuilder.CreateDropdown("Loop Condition To Set", option.conditionToSet, loopConditions);
                    option.conditionToCancel = GUIBuilder.CreateDropdown("Loop Condition To Cancel", option.conditionToCancel, loopConditions);
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
                    rebuildNodeTree = true;
                }

            }

            if (GUILayout.Button("DELETE NODE (NO UNDO)"))
            {
                List<DialogueNode> nodes = new List<DialogueNode>(selectedAsset.tree.dialogueNodes);
                nodes.RemoveAll(x => x.nodeName == activeNode.nodeName);
                selectedAsset.tree.dialogueNodes = nodes.ToArray();
                selectedAsset.NodeDatas.RemoveAll(x => x.name == activeNode.nodeName);
                rebuildNodeTree = true;
                activeNode = null;
            }

            if (rebuildNodeTree)
            {
                if (DialogueEditor.instance != null)
                {
                    DialogueEditor.instance.BuildNodeTree();
                }
            }
            else if (setDirty)
            {
                EditorUtility.SetDirty(selectedAsset);
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
}

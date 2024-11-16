using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class ParseAndDisplayXML : MonoBehaviour
{
    public string filePath;
    public Text text;

    private XmlSerializer serializer;
    private DialogueTree dialogue;

    // Start is called before the first frame update
    void Start()
    {
        serializer = new XmlSerializer(typeof(DialogueTree));

        using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
        {
            dialogue = (DialogueTree)serializer.Deserialize(fileStream);
        }

        if (dialogue != null)
        {
            text.text = ParseFile();
        }
        else
        {
            text.text = "<color=red><b>SADGE</b></color>";
        }
    }

    private string ParseFile()
    {
        string newText = $"NAME FIELD: {dialogue.nameField}\n";

        foreach (var node in dialogue.dialogueNode)
        {
            newText += "\n";
            newText += $"NAME: {node.name}\n";
            if (node.entryCondition != null) foreach (var entryCondition in node.entryCondition) newText += $"ENTRY CONDITION: {entryCondition}\n";
            if (node.randomize != null) newText += "RANDOMIZED!\n";
            if (node.dialogue != null)
            {
                foreach (var dialogue in node.dialogue)
                {
                    newText += "DIALOGUE PAGE:\n";
                    if (dialogue.page != null) foreach (var page in dialogue.page) newText += $"- {page}\n";
                }
            }
            if (node.revealFacts != null)
            {
                newText += "REVEAL FACTS:\n";
                if (node.revealFacts.factID != null) foreach (var fact in node.revealFacts.factID) newText += $"- {fact}\n";
            }
            if (!string.IsNullOrEmpty(node.setPersistentCondition)) newText += $"SET PERSISTENT CONDITION: {node.setPersistentCondition}\n";
            if (node.setCondition != null) foreach (var condition in node.setCondition) newText += $"SET TEMP CONDITION: {condition}\n";
            if (!string.IsNullOrEmpty(node.disablePersistentCondition)) newText += $"DISABLE PERSISTENT CONDITION: {node.disablePersistentCondition}\n";
            if (node.dialogueTargetShipLogCondition != null) foreach (var dialogueTargetCondition in node.dialogueTargetShipLogCondition) newText += $"TARGET CONDITION: {dialogueTargetCondition}\n";
            if (!string.IsNullOrEmpty(node.dialogueTarget)) newText += $"TARGET: {node.dialogueTarget}\nDIALOGUE OPTIONS:\n";
            if (node.dialogueOptionsList != null)
            {
                if (node.dialogueOptionsList.dialogueOption != null)
                {
                    foreach (var option in node.dialogueOptionsList.dialogueOption)
                    {
                        newText += "{\n";
                        if (option.requiredLogCondition != null) foreach (var requiredLogCondition in option.requiredLogCondition) newText += $"REQUIRED LOG CONDITION: {requiredLogCondition}\n";
                        if (option.requiredPersistentCondition != null) foreach (var requiredPersistentCondition in option.requiredPersistentCondition) newText += $"REQUIRED PERSISTENT CONDITION: {requiredPersistentCondition}\n";
                        if (option.cancelledPersistentCondition != null) foreach (var cancelledPersistentCondition in option.cancelledPersistentCondition) newText += $"CANCELLED PERSISTENT CONDITION: {cancelledPersistentCondition}\n";
                        if (!string.IsNullOrEmpty(option.requiredCondition)) newText += $"REQUIRED CONDITION: {option.requiredCondition}\n";
                        if (!string.IsNullOrEmpty(option.cancelledCondition)) newText += $"CANCELLED CONDITION: {option.cancelledCondition}\n";
                        if (!string.IsNullOrEmpty(option.text)) newText += $"TEXT: {option.text}\n";
                        if (!string.IsNullOrEmpty(option.dialogueTarget)) newText += $"DIALOGUE TARGET: {option.dialogueTarget}\n";
                        if (!string.IsNullOrEmpty(option.conditionToSet)) newText += $"CONDITION TO SET: {option.conditionToSet}\n";
                        if (!string.IsNullOrEmpty(option.conditionToCancel)) newText += $"CONDITION TO CANCEL: {option.conditionToCancel}\n";
                        newText += "}\n";
                    }
                }
                if (!string.IsNullOrEmpty(node.dialogueOptionsList.ReuseDialogueOptionsListFrom)) newText += $"COPIED FROM {node.dialogueOptionsList.ReuseDialogueOptionsListFrom}\n\n";
            }
        }

        return newText;
    }
}

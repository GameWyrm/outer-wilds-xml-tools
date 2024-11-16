using System;
using System.Xml.Serialization;

/// <summary>
/// Dummy script to parse Dialogue XML files to
/// </summary>
[XmlRoot("DialogueTree"), Serializable]
public class DialogueTree
{
    [XmlElement("DialogueNode")]
    public DialogueNode[] dialogueNode;
    [XmlElement("NameField")]
    public string nameField;

    public class DialogueNode
    {
        [XmlElement("Name")]
        public string name;
        [XmlElement("EntryCondition")]
        public string[] entryCondition;
        [XmlElement("Randomize")]
        public string randomize;
        [XmlElement("Dialogue")]
        public Dialogue[] dialogue;
        [XmlElement("RevealFacts")]
        public RevealFacts revealFacts;
        [XmlElement("SetPersistentCondition")]
        public string setPersistentCondition;
        [XmlElement("SetCondition")]
        public string[] setCondition;
        [XmlElement("DisablePersistentCondition")]
        public string disablePersistentCondition;
        [XmlElement("DialogueTargetShipLogCondition")]
        public string[] dialogueTargetShipLogCondition;
        [XmlElement("DialogueTarget")]
        public string dialogueTarget;
        [XmlElement("DialogueOptionsList")]
        public DialogueOptionsList dialogueOptionsList;
    }

    [Serializable]
    public class Dialogue
    {
        [XmlElement("Page")]
        public string[] page;
    }

    [Serializable]
    public class RevealFacts
    {
        [XmlElement("FactID")]
        public string[] factID;
    }

    [Serializable]
    public class DialogueOptionsList
    {
        [XmlElement("DialogueOption")]
        public DialogueOption[] dialogueOption;
        [XmlElement("ReuseDialogueOptionsListFrom")]
        public string ReuseDialogueOptionsListFrom;
    }

    [Serializable]
    public class DialogueOption
    {
        [XmlElement("RequiredLogCondition")]
        public string[] requiredLogCondition;
        [XmlElement("RequiredPersistentCondition")]
        public string[] requiredPersistentCondition;
        [XmlElement("CancelledPersistentCondition")]
        public string[] cancelledPersistentCondition;
        [XmlElement("RequiredCondition")]
        public string requiredCondition;
        [XmlElement("CancelledCondition")]
        public string cancelledCondition;
        [XmlElement("Text")]
        public string text;
        [XmlElement("DialogueTarget")]
        public string dialogueTarget;
        [XmlElement("ConditionToSet")]
        public string conditionToSet;
        [XmlElement("ConditionToCancel")]
        public string conditionToCancel;
    }
}

using System.Xml.Serialization;
using System;

[Serializable]
public class DialogueNode
{
    [XmlElement("Name")]
    public string nodeName;
    [XmlElement("EntryCondition")]
    public string[] entryConditions;
    /// <summary>Do no use this unless serializing. Use randomize instead.</summary>
    [XmlElement("Randomize")]
    public string m_randomize;
    public bool randomize;
    [XmlElement("Dialogue")]
    public Dialogue[] dialogues;
    [XmlElement("RevealFacts")]
    public RevealFacts revealFacts;
    [XmlElement("SetPersistentCondition")]
    public string setPersistentCondition;
    [XmlElement("SetCondition")]
    public string[] setConditions;
    [XmlElement("DisablePersistentCondition")]
    public string disablePersistentCondition;
    [XmlElement("DialogueTargetShipLogCondition")]
    public string[] dialogueTargetShipLogConditions;
    [XmlElement("DialogueTarget")]
    public string dialogueTarget;
    [XmlElement("DialogueOptionsList")]
    public DialogueOptionsList dialogueOptionsList;

    [Serializable]
    public class Dialogue
    {
        [XmlElement("Page")]
        public string[] pages;
    }

    [Serializable]
    public class RevealFacts
    {
        [XmlElement("FactID")]
        public string[] factIDs;
    }

    [Serializable]
    public class DialogueOptionsList
    {
        [XmlElement("DialogueOption")]
        public DialogueOption[] dialogueOptions;
        [XmlElement("ReuseDialogueOptionsListFrom")]
        public string ReuseDialogueOptionsListFrom;
    }

    [Serializable]
    public class DialogueOption
    {
        [XmlElement("RequiredLogCondition")]
        public string[] requiredLogConditions;
        [XmlElement("RequiredPersistentCondition")]
        public string[] requiredPersistentConditions;
        [XmlElement("CancelledPersistentCondition")]
        public string[] cancelledPersistentConditions;
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


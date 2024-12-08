using System;
using System.Xml.Serialization;

namespace XmlTools
{
    /// <summary>
    /// Dummy script to parse Dialogue XML files to
    /// </summary>
    [XmlRoot("DialogueTree"), Serializable]
    public class DialogueTree
    {
        [XmlElement("DialogueNode")]
        public DialogueNode[] dialogueNodes;
        [XmlElement("NameField")]
        public string nameField;
    }
}

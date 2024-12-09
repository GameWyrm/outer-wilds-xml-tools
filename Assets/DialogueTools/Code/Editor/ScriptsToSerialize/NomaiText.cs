using System;
using System.Xml.Serialization;

namespace XmlTools
{
    /// <summary>
    /// Dummy script to parse Nomai Text XMLs to
    /// </summary>
    [XmlRoot("NomaiObject"), Serializable]
    public class NomaiText
    {
        [XmlElement("TextBlock")]
        public TextBlock[] textBlocks;

        [XmlElement("ShipLogConditions")]
        public ShipLogCondition[] shipLogConditions;

        [Serializable]
        public class TextBlock
        {
            [XmlElement("ID")]
            public int textID;

            [XmlElement("ParentID")]
            public string parentID;
            [XmlIgnore]
            public bool parentIDSpecified { get { return parentID != ""; } }

            /// <summary> Do not use unless serializing, use isLocationA instead</summary>
            [XmlElement("LocationA")]
            public string m_isLocationA;
            [XmlIgnore]
            public bool isLocationA;

            /// <summary> Do not use unless serializing, use isLocationB instead</summary>
            [XmlElement("LocationB")]
            public string m_isLocationB;
            [XmlIgnore]
            public bool isLocationB;

            [XmlElement("Text")]
            public string text;
        }

        [Serializable]
        public class ShipLogCondition
        {
            /// <summary> Do not use unless serializing, use isLocationA instead</summary>
            [XmlElement("LocationA")]
            public string m_isLocationA;
            [XmlIgnore]
            public bool isLocationA;

            /// <summary> Do not use unless serializing, use isLocationB instead</summary>
            [XmlElement("LocationB")]
            public string m_isLocationB;
            [XmlIgnore]
            public bool isLocationB;

            [XmlElement("RevealFact")]
            public RevealFact[] revealFacts;
        }

        [Serializable]
        public class RevealFact
        {
            [XmlElement("FactID")]
            public string factID;

            [XmlElement("Condition")]
            public string condition;
        }
    }
}

using System;
using System.Xml.Serialization;
using UnityEngine;

namespace XmlTools
{
    /// <summary>
    /// Dummy script to parse Ship Log XMLs to
    /// </summary>
    [XmlRoot("AstroObjectEntry"), Serializable]
    public class ShipLogEntry
    {
        [XmlElement("ID")]
        public string planetID;

        [XmlElement("Entry")]
        public Entry[] entries;

        [Serializable]
        public class Entry
        {
            [XmlElement("ID")]
            public string entryID;

            [XmlElement("Name")]
            public string name;

            [XmlElement("Curiosity")]
            public string curiosity;

            /// <summary> Do not use unless serializing, use isCuriosity instead</summary>
            [XmlElement("IsCuriosity"), HideInInspector]
            public string m_isCuriosity;

            [XmlIgnore]
            public bool isCuriosity;

            /// <summary> Do not use unless serializing, use ignoreMoreToExplore instead</summary>
            [XmlElement("IgnoreMoreToExplore"), HideInInspector]
            public string m_ignoreMoreToExplore;

            [XmlIgnore]
            public bool ignoreMoreToExplore;

            /// <summary> Do not use unless serializing, use parentIgnoreNotRevealed instead</summary>
            [XmlElement("ParentIgnoreNotRevealed"), HideInInspector]
            public string m_parentIgnoreNotRevealed;

            [XmlIgnore]
            public bool parentIgnoreNotRevealed;

            [XmlElement("IgnoreMoreToExploreCondition")]
            public string ignoreMoreToExploreCondition;

            [XmlElement("AltPhotoCondition")]
            public string altPhotoCondition;

            [XmlElement("RumorFact")]
            public RumorFact[] rumorFacts;

            [XmlElement("ExploreFact")]
            public ExploreFact[] exploreFacts;

            [XmlElement("Entry")]
            public Entry[] childEntries;
        }

        [Serializable]
        public class RumorFact
        {
            public RumorFact()
            {
                this.rumorID = "";
                this.sourceID = "";
                this.rumorName = "";
                this.rumorNamePriority = 0;
                this.ignoreMoreToExplore = false;
                this.text = "";
            }

            public RumorFact(string ID, string text)
            {
                this.rumorID = ID;
                this.sourceID = "";
                this.rumorName = "";
                this.rumorNamePriority = 0;
                this.ignoreMoreToExplore = false;
                this.text = text;
            }

            [XmlElement("ID")]
            public string rumorID;

            [XmlElement("SourceID")]
            public string sourceID;

            [XmlElement("RumorName")]
            public string rumorName;

            [XmlElement("RumorNamePriority")]
            public int rumorNamePriority;

            /// <summary> Do not use unless serializing, use ignoreMoreToExplore instead</summary>
            [XmlElement("IgnoreMoreToExplore"), HideInInspector]
            public string m_ignoreMoreToExplore;

            [XmlIgnore]
            public bool ignoreMoreToExplore;

            [XmlElement("Text")]
            public string text;
        }

        [Serializable]
        public class ExploreFact
        {
            public ExploreFact()
            {
                this.exploreID = "";
                this.ignoreMoreToExplore = false;
                this.text = "";
            }

            public ExploreFact(string ID, string text)
            {
                this.exploreID = ID;
                this.ignoreMoreToExplore = false;
                this.text = text;
            }

            [XmlElement("ID")]
            public string exploreID;

            /// <summary> Do not use unless serializing, use ignoreMoreToExplore instead</summary>
            [XmlElement("IgnoreMoreToExplore"), HideInInspector]
            public string m_ignoreMoreToExplore;

            [XmlIgnore]
            public bool ignoreMoreToExplore;

            [XmlElement("Text")]
            public string text;
        }
    }
}

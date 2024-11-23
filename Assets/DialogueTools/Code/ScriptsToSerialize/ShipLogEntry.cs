using System;
using System.Xml.Serialization;

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
        [XmlElement("IsCuriosity")]
        public string m_isCuriosity;
        public bool isCuriosity;

        /// <summary> Do not use unless serializing, use ignoreMoreToExplore instead</summary>
        [XmlElement("IgnoreMoreToExplore")]
        public string m_ignoreMoreToExplore;
        public bool ignoreMoreToExplore;

        /// <summary> Do not use unless serializing, use parentIgnoreNotRevealed instead</summary>
        [XmlElement("ParentIgnoreNotRevealed")]
        public string m_parentIgnoreNotRevealed;
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
        [XmlElement("ID")]
        public string rumorID;

        [XmlElement("SourceID")]
        public string sourceID;

        [XmlElement("RumorName")]
        public string rumorName;

        [XmlElement("RumorNamePriority")]
        public string rumorNamePriority;

        /// <summary> Do not use unless serializing, use ignoreMoreToExplore instead</summary>
        [XmlElement("IgnoreMoreToExplore")]
        public string m_ignoreMoreToExplore;
        public bool ignoreMoreToExplore;
    }

    [Serializable]
    public class ExploreFact
    {
        [XmlElement("ID")]
        public string exploreID;

        /// <summary> Do not use unless serializing, use ignoreMoreToExplore instead</summary>
        [XmlElement("IgnoreMoreToExplore")]
        public string m_ignoreMoreToExplore;
        public bool ignoreMoreToExplore;
    }
}
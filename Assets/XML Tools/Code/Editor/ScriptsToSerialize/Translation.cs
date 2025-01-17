using System;
using System.Collections.Generic;

namespace XmlTools
{
    public class Translation
    {
        public Dictionary<string, string> DialogueDictionary;
        public Dictionary<string, string> ShipLogDictionary;
        public Dictionary<string, string> UIDictionary;
        public Dictionary<string, string> OtherDictionary;
        public Dictionary<string, AchievementTranslation> AchievementTranslations;
    }

    public class AchievementTranslation
    {
        public string Name;
        public string Description;
    }
}

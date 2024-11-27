using UnityEditor;
using UnityEngine;

namespace XmlTools
{
    [CreateAssetMenu(fileName = "XML User Settings", menuName = "Tools/XML User Settings")]
    public class XMLUserSettings : ScriptableObject
    {
        [SerializeField]
        public string modPath;
        [SerializeField]
        public string modIconsPath;
        [SerializeField]
        public string vanillaIconsPath;

        public static XMLUserSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = AssetDatabase.LoadAssetAtPath<XMLUserSettings>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:XMLUserSettings")[0]));
                }
                return instance;
            }
        }

        private static XMLUserSettings instance;
    }
}
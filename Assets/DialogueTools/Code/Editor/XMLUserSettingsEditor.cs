using UnityEngine;
using UnityEditor;
using System.IO;

namespace XmlTools
{
    [CustomEditor(typeof(XMLUserSettings))]
    public class XMLUserSettingsEditor : Editor
    {
        private static XMLUserSettings instance;

        private void OnEnable()
        {
            instance = serializedObject.targetObject as XMLUserSettings;
        }

        public override void OnInspectorGUI()
        {
            bool setDirty = false;
            EditorGUILayout.HelpBox("This contains non-essential paths for convenience and enabling certain editor features.\n" +
                "If collaborating, this should be added to your gitignore.", MessageType.None);
            instance.modPath = GUIBuilder.CreatePathSetter("Mod Path", instance.modPath, out setDirty);
            instance.modIconsPath = GUIBuilder.CreatePathSetter("Icons Path", instance.modIconsPath, out setDirty, instance.modPath);
            instance.vanillaIconsPath = GUIBuilder.CreatePathSetter("Vanilla Icons Path", instance.vanillaIconsPath, out setDirty, Application.dataPath);
            if (setDirty) EditorUtility.SetDirty(instance);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("shipLogFont"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
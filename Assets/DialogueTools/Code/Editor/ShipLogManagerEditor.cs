using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace XmlTools
{
    [CustomEditor(typeof(ShipLogManager))]
    public class ShipLogManagerEditor : Editor
    {
        public static ShipLogManager instance;
        public static ShipLogEntry.Entry selectedEntry;

        private static bool showCuriosities;

        private void OnEnable()
        {
            instance = serializedObject.targetObject as ShipLogManager;
        }

        public override void OnInspectorGUI()
        {
            bool setDirty = false;
            showCuriosities = EditorGUILayout.BeginFoldoutHeaderGroup(showCuriosities, "Curiosities");
            if (showCuriosities)
            {
                if (instance.curiosities == null) instance.curiosities = new List<string>();
                if (instance.curiosities.Count > 0)
                {
                    for (int i = 0; i < instance.curiosities.Count; i++)
                    {
                        string curiosity = instance.curiosities[i];
                        Color oldColor = instance.GetCuriosityColor(curiosity);
                        Color oldHighlightColor = instance.GetCuriosityHighlightColor(curiosity);
                        GUIBuilder.CreateColorSetter(curiosity, oldColor, oldHighlightColor, out Color newCuriosityColor, out Color newHighlightColor);
                        if (oldColor != newCuriosityColor)
                        {
                            instance.SetCuriosityColor(curiosity, newCuriosityColor);
                            setDirty = true;
                        }
                        if (oldHighlightColor != newHighlightColor)
                        {
                            instance.SetCuriosityHighlightColor(curiosity, newHighlightColor);
                            setDirty = true;
                        }
                    }
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (selectedEntry == null)
            {

            }

            if (setDirty)
            {
                EditorUtility.SetDirty(instance);
                if (ShipLogEditor.Instance != null) ShipLogEditor.Instance.BuildNodeTree();
            }
        }
    }
}
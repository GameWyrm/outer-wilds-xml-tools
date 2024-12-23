using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace XmlTools
{
    [CustomEditor(typeof(NomaiTextAsset))]
    public class NomaiTextAssetEditor : Editor
    {
        public static NomaiTextAsset selectedAsset;
        public static NomaiText.TextBlock activeText;

        

        private void OnEnable()
        {
            selectedAsset = serializedObject.targetObject as NomaiTextAsset;
        }

        public static void SelectionUpdate(NomaiText.TextBlock node)
        {
            activeText = node;
        }

        public override void OnInspectorGUI()
        {
            if (activeText == null)
            {
                if (GUILayout.Button("Open in Editor"))
                {
                    selectedAsset = serializedObject.targetObject as NomaiTextAsset;
                    if (selectedAsset != null)
                    {
                        NomaiTextEditor.OpenWindowWithSelection(selectedAsset);
                    }
                    else
                    {
                        Debug.LogError($"{serializedObject.targetObject.name} is still null for some reason.");
                    }
                }
            }
            else
            {
                DrawNodeData();
            }
            DrawConditionData();
        }

        private void DrawNodeData()
        {
            var settings = XMLEditorSettings.Instance;
            Language language = settings.GetSelectedLanguage();

            bool setDirty = false;
            bool setRedraw = false;

            settings.selectedLanguage = EditorGUILayout.Popup("Language: ", settings.selectedLanguage, settings.supportedLanguages.Select(x => x.name).ToArray());
            EditorGUILayout.Space();

            // TextID
            EditorGUILayout.LabelField("Block " + activeText.textID.ToString(), EditorStyles.boldLabel);

            // ParentID
            List<string> nodes = new List<string>(selectedAsset.GetTextIDs());
            if (nodes.Contains(activeText.textID.ToString())) nodes.Remove(activeText.textID.ToString());
            string newParentID = GUIBuilder.CreateDropdown("Parent ID", activeText.parentID, nodes.ToArray());
            if (newParentID != activeText.parentID)
            {
                activeText.parentID = newParentID;
                setRedraw = true;
            }

            // Location
            NomaiLocation loc;
            if (activeText.isLocationB) loc = NomaiLocation.LocationB;
            else if (activeText.isLocationA) loc = NomaiLocation.LocationA;
            else loc = NomaiLocation.None;

            NomaiLocation newLoc = (NomaiLocation)EditorGUILayout.EnumPopup("Location", loc);
            if (newLoc != loc)
            {
                setRedraw = true;
                switch (newLoc)
                {
                    case NomaiLocation.None:
                        activeText.isLocationA = false;
                        activeText.isLocationB = false;
                        break;
                    case NomaiLocation.LocationA:
                        activeText.isLocationA = true;
                        activeText.isLocationB = false;
                        break;
                    case NomaiLocation.LocationB:
                        activeText.isLocationA = false;
                        activeText.isLocationB = true;
                        break;
                }
            }

            // Text
            string newText = GUIBuilder.CreateTranslatedArrayItem("Text", activeText.text, language, false, true, out _, out bool shouldSetTextDirty);
            if (shouldSetTextDirty)
            {
                setDirty = true;
                activeText.text = newText;
            }

            if (setRedraw)
            {
                if (NomaiTextEditor.Instance != null)
                {
                    NomaiTextEditor.Instance.BuildNodeTree();
                }
                EditorUtility.SetDirty(selectedAsset);
            }
            else if (setDirty)
            {
                EditorUtility.SetDirty(selectedAsset);
            }
        }

        private void DrawConditionData()
        {
            EditorGUILayout.LabelField("Ship Log", EditorStyles.boldLabel);

            if (selectedAsset.text.shipLogConditions == null) selectedAsset.text.shipLogConditions = new NomaiText.ShipLogCondition[0];
            NomaiText.ShipLogCondition[] newConditions = GUIBuilder.CreateNomaiConditionsArray(selectedAsset.text.shipLogConditions, selectedAsset, out bool setDirty);

            if (setDirty)
            {
                selectedAsset.text.shipLogConditions = newConditions;

                EditorUtility.SetDirty(selectedAsset);
            }
        }
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace XmlTools
{

    public class NomaiTextEditor : NodeWindow
    {
        public static NomaiTextEditor Instance;
        public static NomaiTextAsset selection;

        private NomaiText selectedText;

        private static Color defaultColor => new Color(0.32f, 0.22f, 0.6f);
        private static Color defaultHighlight => new Color(0.45f, 0.34f, 0.77f);

        private static Color altColor => new Color(0.95f, 0.62f, 0f);
        private static Color altHighlightColor => new Color(1f, 0.77f, 0.35f);


        [MenuItem("Tools/XML Editors/Nomai Text Editor")]
        public static void ShowWindow()
        {
            Instance = GetWindow<NomaiTextEditor>();
            Instance.titleContent = new GUIContent("NomaiTextEditor");
        }

        public static void OpenWindowWithSelection(NomaiTextAsset newSelection)
        {
            AssetDatabase.SaveAssets();
            selection = newSelection;
            ShowWindow();
        }

        protected override void ConstructGUI()
        {
            visualTree = EditorReferences.Instance.NomaiTextVisualTree;
            base.ConstructGUI();
        }

        public override void BuildNodeTree()
        {
            if (selection == null) return;
            nodes = selection.NodeDatas;
            selectedText = selection.text;
            base.BuildNodeTree();
        }

        protected override void ConstructGUILate()
        {
            var exportButton = root.Q<Button>("export");
            exportButton.clicked += OnClickExport;

            var newTreeButton = root.Q<Button>("newTree");
            newTreeButton.clicked += OnClickNewTree;

            var newNodeButton = root.Q<Button>("newNode");
            newNodeButton.clicked += OnClickNewNode;

            var resetButton = root.Q<Button>("resetNodes");
            resetButton.clicked += OnClickResetNodes;
            base.ConstructGUILate();
        }

        protected override void OnClickImport()
        {
            string xmlFilePath = EditorUtility.OpenFilePanel("Select Nomai Text XML File", "", "xml");

            if (!string.IsNullOrEmpty(xmlFilePath))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(NomaiText));
                NomaiText nomaiText;

                using (FileStream fileStream = new FileStream(xmlFilePath, FileMode.Open))
                {
                    nomaiText = (NomaiText)serializer.Deserialize(fileStream);
                }

                if (nomaiText == null)
                {
                    Debug.LogError("Unable to parse selected file. Are you sure it is Nomai text xml file?");
                    return;
                }

                string savePath = EditorUtility.SaveFilePanelInProject("Save Nomai Text as...", "New Nomai Text", "asset", "Select a location to save your Nomai Text to.");

                // fix self-closing tags
                if (nomaiText.textBlocks == null) nomaiText.textBlocks = new NomaiText.TextBlock[0];
                foreach (var node in nomaiText.textBlocks)
                {
                    node.isLocationA = node.m_isLocationA != null;
                    node.isLocationB = node.m_isLocationB != null;
                }
                if (nomaiText.shipLogConditions == null) nomaiText.shipLogConditions = new NomaiText.ShipLogCondition[0];
                foreach (var node in nomaiText.shipLogConditions)
                {
                    node.isLocationA = node.m_isLocationA != null;
                    node.isLocationB = node.m_isLocationB != null;
                }

                if (!string.IsNullOrEmpty(savePath))
                {

                    NomaiTextAsset nomaiTextAsset = ScriptableObject.CreateInstance<NomaiTextAsset>();
                    nomaiTextAsset.text = nomaiText;

                    // populate with default node data
                    List<NodeData> data = new List<NodeData>();
                    for (int i = 0; i < nomaiTextAsset.text.textBlocks.Length; i++)
                    {
                        var node = nomaiTextAsset.text.textBlocks[i];
                        NodeData newNode = new NodeData(node.textID.ToString(), GetResetPosition(i));
                        data.Add(newNode);
                    }
                    nomaiTextAsset.NodeDatas = data;

                    AssetDatabase.CreateAsset(nomaiTextAsset, savePath);
                    AssetDatabase.SaveAssets();
                    Selection.activeObject = nomaiTextAsset;
                    selection = nomaiTextAsset;
                    BuildNodeTree();

                    Debug.Log($"Created new Nomai Text Tree at {savePath}");
                }
            }
        }

        private void OnClickExport()
        {
            if (selection == null)
            {
                Debug.LogWarning("No object to export selected.");
                return;
            }

            string savePath = EditorUtility.SaveFilePanelInProject("Save Nomai text as XML...", "New Nomai Text", "xml", "Select a location to export your Nomai Text XML to.");

            if (!string.IsNullOrEmpty(savePath))
            {
                // fix self-closing tags
                foreach (var node in selection.text.textBlocks)
                {
                    node.m_isLocationA = node.isLocationA ? "" : null;
                    node.m_isLocationB = node.isLocationB ? "" : null;
                }
                foreach (var node in selection.text.shipLogConditions)
                {
                    node.m_isLocationA = node.isLocationA ? "" : null;
                    node.m_isLocationB = node.isLocationB ? "" : null;
                }

                XmlSerializer serializer = new XmlSerializer(typeof(NomaiText));

                using (StreamWriter writer = new StreamWriter(savePath))
                {
                    serializer.Serialize(writer, selection.text);
                }
                AssetDatabase.Refresh();
                Debug.Log($"Exported Nomai text xml to {savePath}");
            }
        }

        private void OnClickNewTree()
        {
            string savePath = EditorUtility.SaveFilePanelInProject("Save New Nomai Text Tree as...", "New Nomai Text", "asset", "Select a location to save your Nomai text to.");

            if (!string.IsNullOrEmpty(savePath))
            {
                NomaiTextAsset nomaiTextAsset = ScriptableObject.CreateInstance<NomaiTextAsset>();
                nomaiTextAsset.text = new NomaiText();
                AssetDatabase.CreateAsset(nomaiTextAsset, savePath);
                AssetDatabase.SaveAssets();
                Selection.activeObject = nomaiTextAsset;
                selection = nomaiTextAsset;
                nodesRoot.Clear();

                Debug.Log($"Created new Nomai Text at {savePath}");
            }
        }

        private void OnClickNewNode()
        {
            List<NomaiText.TextBlock> nodes = new List<NomaiText.TextBlock>(selection.text.textBlocks);
            NomaiText.TextBlock newNode = new NomaiText.TextBlock();
            List<string> nodeNames = new List<string>(selection.text.textBlocks.Select(x => x.textID.ToString()));
            newNode.textID = nodes.Count;
            // prevent duplicate names
            while (nodeNames.Contains(newNode.textID.ToString())) newNode.textID += 1;
            nodes.Add(newNode);
            selection.text.textBlocks = nodes.ToArray();
            selection.NodeDatas.Add(new NodeData(newNode.textID.ToString(), new Vector2(100, 100)));
            BuildNodeTree();

            Debug.Log("New Node Created");
        }

        private void OnClickResetNodes()
        {
            panRoot.transform.position = Vector3.zero;
            int i = 0;
            if (nodes == null) return;
            foreach (var node in nodeElements.Keys)
            {
                selection.SetNodePosition(node, GetResetPosition(i));
                i++;
            }

            BuildNodeTree();
        }

        private Vector2 GetResetPosition(int index)
        {
            float x = 150;
            float y = 50;
            if (index != 0)
            {
                x += 350 * (index % 4);
                y += 200 * Mathf.Floor(index / 4);
            }
            return new Vector2(x, y);
        }

        private void OnDestroy()
        {
            NomaiTextAssetEditor.activeText = null;
            isFocused = false;
            AssetDatabase.SaveAssets();
        }

        private void OnSelectionChange()
        {
            NomaiTextAssetEditor.activeText = null;
        }

        private void OnFocus()
        {
            Instance = this;
            GainFocus();
            Selection.activeObject = selection;
        }

        private void OnLostFocus()
        {
            isFocused = false;
            AssetDatabase.SaveAssets();
        }

        // The Inspector doesn't automatically update to the new selection when you click into the editor.
        // I don't know how to fix this, so for now I require you to click into the window first.
        private async void GainFocus()
        {
            await Task.Delay(100);
            isFocused = true;
        }

        private NomaiText.TextBlock GetTextBlock(int id)
        {
            NomaiText.TextBlock node = selectedText.textBlocks.First(x => x.textID == id);
            if (node == null) Debug.LogError("Obtained node is null!");
            return node;
        }

        protected override void OnCreateNode(VisualElement createdNode)
        {
            VisualElement child = createdNode.ElementAt(0);
            NodeData data = selection.NodeDatas.Find(x => x.name == createdNode.name);

            createdNode.transform.position = panRoot.LocalToWorld(data.position);

            NomaiText.TextBlock node = selection.text.textBlocks.First(x => x.textID.ToString() == createdNode.name);

            if (node.isLocationB)
            {
                child.style.backgroundColor = altColor;
            }
            else
            {
                child.style.backgroundColor = defaultColor;
            }
        }

        protected override List<VisualElement> GetTargetNodes(string nodeName, out bool flip)
        {
            flip = true;

            List<VisualElement> elements = new List<VisualElement>();
            var node = selection.text.textBlocks.First(x => x.textID.ToString() == nodeName);

            if (node.parentID != null)
            {
                elements.Add(nodeElements[node.parentID.ToString()]);
            }

            return elements;
        }

        public override void SelectNode(VisualElement newSelection)
        {
            if (selectedNode != null)
            {
                if (int.TryParse(selectedNode.name, out int parentID))
                {
                    var oldText = GetTextBlock(parentID);

                    selectedNode.ElementAt(0).style.backgroundColor = oldText.isLocationB ? altColor : defaultColor;
                }
                else
                {
                    selectedNode.ElementAt(0).style.backgroundColor = new Color(0.24f, 0.24f, 0.24f);
                }
            }

            VisualElement child = newSelection.ElementAt(0);
            if (int.TryParse(newSelection.name, out int selectionID))
            {
                var newText = GetTextBlock(selectionID);

                NomaiTextAssetEditor.SelectionUpdate(newText);
                Selection.activeObject = selection;
                EditorUtility.SetDirty(selection);

                child.style.backgroundColor = newText.isLocationB ? altHighlightColor : defaultHighlight;
            }
            else
            {
                child.style.backgroundColor = new Color(0.05f, 0.43f, 0.68f);
            }

            selectedNode = newSelection;
        }

        public override void MoveNode(VisualElement node, Vector2 newPosition)
        {
            selection.SetNodePosition(node.name, panRoot.WorldToLocal(newPosition));
            EditorUtility.SetDirty(selection);
        }
    }
}

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public class DialogueEditor : NodeWindow
{
    public static DialogueEditor instance;
    public static DialogueTreeAsset selection;

    private DialogueTree dialogueTree;
    private VisualElement defaultNode;
    private List<VisualElement> exitNodes;

    [MenuItem("Tools/XML Editors/Dialogue Editor")]
    public static void ShowWindow()
    {
        instance = GetWindow<DialogueEditor>();
        instance.titleContent = new GUIContent("Dialogue Editor");
    }

    public static void OpenWindowWithSelection(DialogueTreeAsset newSelection)
    {
        AssetDatabase.SaveAssets();
        selection = newSelection;
        ShowWindow();
    }

    protected override void ConstructGUI()
    {
        visualTree = EditorReferences.Instance.DialogueVisualTree;
        base.ConstructGUI();
    }

    public override void BuildNodeTree()
    {
        if (selection == null) return;
        nodes = selection.NodeDatas;
        dialogueTree = selection.tree;
        exitNodes = new List<VisualElement>();
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
        string xmlFilePath = EditorUtility.OpenFilePanel("Select Dialogue XML File", "", "xml");

        if (!string.IsNullOrEmpty(xmlFilePath))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(DialogueTree));
            DialogueTree dialogueTree;

            using (FileStream fileStream = new FileStream(xmlFilePath, FileMode.Open))
            {
                dialogueTree = (DialogueTree)serializer.Deserialize(fileStream);
            }

            if (dialogueTree == null)
            {
                Debug.LogError("Unable to parse selected file. Are you sure it is dialogue xml file?");
                return;
            }

            string savePath = EditorUtility.SaveFilePanelInProject("Save Dialogue Tree as...", "New Dialogue Tree", "asset", "Select a location to save your Dialogue Tree to.");

            // fix self-closing tags
            foreach (var node in dialogueTree.dialogueNodes)
            {
                node.randomize = node.m_randomize != null;
            }

            if (!string.IsNullOrEmpty(savePath))
            {
                
                DialogueTreeAsset dialogueTreeAsset = ScriptableObject.CreateInstance<DialogueTreeAsset>();
                dialogueTreeAsset.tree = dialogueTree;

                // populate with default node data
                List<NodeData> data = new List<NodeData>();
                for(int i = 0; i < dialogueTreeAsset.tree.dialogueNodes.Length; i++)
                {
                    var node = dialogueTreeAsset.tree.dialogueNodes[i];
                    NodeData newNode = new NodeData(node.nodeName, GetResetPosition(i));
                    data.Add(newNode);
                }
                dialogueTreeAsset.NodeDatas = data;

                AssetDatabase.CreateAsset(dialogueTreeAsset, savePath);
                AssetDatabase.SaveAssets();
                Selection.activeObject = dialogueTreeAsset;
                selection = dialogueTreeAsset;
                BuildNodeTree();

                // build conditions
                XMLEditorSettings global = XMLEditorSettings.Instance;
                foreach (var node in dialogueTree.dialogueNodes)
                {
                    if (node.entryConditions != null)
                    {
                        foreach (var condition in node.entryConditions)
                        {
                            // TODO double-check these are persistent
                            global.RegisterCondition(condition, true);
                        }
                    }
                    global.RegisterCondition(node.setPersistentCondition, true);
                    if (node.setConditions != null)
                    {
                        foreach (var condition in node.setConditions)
                        {
                            global.RegisterCondition(condition, false);
                        }
                    }
                    global.RegisterCondition(node.disablePersistentCondition, true);
                    if (node.dialogueOptionsList != null && node.dialogueOptionsList.dialogueOptions != null)
                    {
                        foreach (var option in node.dialogueOptionsList.dialogueOptions)
                        {
                            if (option.requiredPersistentConditions != null)
                            {
                                foreach (var condition in option.requiredPersistentConditions)
                                {
                                    global.RegisterCondition(condition, true);
                                }
                            }
                            if (option.cancelledPersistentConditions != null)
                            {
                                foreach (var condition in option.cancelledPersistentConditions)
                                {
                                    global.RegisterCondition(condition, true);
                                }
                            }
                            global.RegisterCondition(option.requiredCondition, false);
                            global.RegisterCondition(option.cancelledCondition, false);
                            global.RegisterCondition(option.conditionToSet, false);
                            global.RegisterCondition(option.conditionToCancel, false);
                        }
                    }
                }

                Debug.Log($"Created new Dialogue Tree at {savePath}");
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

        string savePath = EditorUtility.SaveFilePanelInProject("Save Dialogue Tree as XML...", selection.tree.nameField.Replace(" ", "_"), "xml", "Select a location to export your Dialogue Tree XML to.");

        if (!string.IsNullOrEmpty(savePath))
        {
            // fix self-closing tags
            foreach (var node in selection.tree.dialogueNodes)
            {
                if (node.randomize)
                {
                    node.m_randomize = "";
                }
                else
                {
                    node.m_randomize = null;
                }
            }

            XmlSerializer serializer = new XmlSerializer(typeof(DialogueTree));

            using (StreamWriter writer = new StreamWriter(savePath))
            {
                serializer.Serialize(writer, selection.tree);
            }
            AssetDatabase.Refresh();
            Debug.Log($"Exported dialogue xml to {savePath}");
        }

    }

    private void OnClickNewTree()
    {
        string savePath = EditorUtility.SaveFilePanelInProject("Save New Dialogue Tree as...", "New Dialogue Tree", "asset", "Select a location to save your Dialogue Tree to.");

        if (!string.IsNullOrEmpty(savePath))
        {
            XMLEditorSettings.Instance.RegisterCondition("DEFAULT", true);
            DialogueTreeAsset dialogueTreeAsset = ScriptableObject.CreateInstance<DialogueTreeAsset>();
            dialogueTreeAsset.tree = new DialogueTree();
            AssetDatabase.CreateAsset(dialogueTreeAsset, savePath);
            AssetDatabase.SaveAssets();
            Selection.activeObject = dialogueTreeAsset;
            selection = dialogueTreeAsset;
            nodesRoot.Clear();

            Debug.Log($"Created new Dialogue Tree at {savePath}");
        }
    }

    private void OnClickNewNode()
    {
        List<DialogueNode> nodes = new List<DialogueNode>(selection.tree.dialogueNodes);
        DialogueNode newNode = new DialogueNode();
        List<string> nodeNames = new List<string>(selection.tree.dialogueNodes.Select(x => x.nodeName));
        newNode.nodeName = "New_Dialogue_Node";
        // prevent duplicate names
        while (nodeNames.Contains(newNode.nodeName)) newNode.nodeName += "_1";
        nodes.Add(newNode);
        selection.tree.dialogueNodes = nodes.ToArray();
        selection.NodeDatas.Add(new NodeData(newNode.nodeName, new Vector2(100, 100)));
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
        DialogueTreeEditor.activeNode = null;
        isFocused = false;
        AssetDatabase.SaveAssets();
    }

    private void OnSelectionChange()
    {
        DialogueTreeEditor.activeNode = null;
    }

    private void OnFocus()
    {
        instance = this;
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

    private DialogueNode GetNode(string name)
    {
        DialogueNode node = dialogueTree.dialogueNodes.First(x => x.nodeName == name);
        if (node == null) Debug.LogError("Obtained node is null!");
        return node;
    }
    protected override void OnCreateNode(VisualElement createdNode)
    {
        VisualElement child = createdNode.ElementAt(0);
        NodeData data = selection.NodeDatas.Find(x => x.name == createdNode.name);

        createdNode.transform.position = panRoot.LocalToWorld(data.position);

        DialogueNode node = selection.tree.dialogueNodes.First(x => x.nodeName == createdNode.name);

        if (node.entryConditions != null && node.entryConditions.Contains("DEFAULT"))
        {
            defaultNode = createdNode;
            child.EnableInClassList("node_bg", false);
            child.EnableInClassList("node_default", true);
        }
        else
        {
            bool hasExit = false;
            if (string.IsNullOrEmpty(node.dialogueTarget))
            {
                if (node.dialogueOptionsList == null || node.dialogueOptionsList.dialogueOptions == null || node.dialogueOptionsList.dialogueOptions.Length == 0)
                {
                    hasExit = true;
                }
                else
                {
                    foreach (var option in node.dialogueOptionsList.dialogueOptions)
                    {
                        if (string.IsNullOrEmpty(option.dialogueTarget)) hasExit = true;
                    }
                }
            }
            if (hasExit)
            {
                exitNodes.Add(createdNode);
                child.EnableInClassList("node_bg", false);
                child.EnableInClassList("node_exit", true);
            }
        }
    }

    protected override List<VisualElement> GetTargetNodes(string nodeName)
    {
        List<VisualElement> elements = new List<VisualElement>();

        var node = selection.tree.dialogueNodes.First(x => x.nodeName == nodeName);

        if (!string.IsNullOrEmpty(node.dialogueTarget))
        {
            elements.Add(nodeElements[node.dialogueTarget]);
        }
        if (node.dialogueOptionsList != null && node.dialogueOptionsList.dialogueOptions != null)
        {
            foreach (var option in node.dialogueOptionsList.dialogueOptions)
            {
                if (!string.IsNullOrEmpty(option.dialogueTarget))
                {
                    elements.Add(nodeElements[option.dialogueTarget]);
                }
            }
        }
        return elements;
    }

    public override void SelectNode(VisualElement newSelection)
    {
        if (selectedNode != null)
        {
            VisualElement oldChild = selectedNode.ElementAt(0);
            oldChild.EnableInClassList("node_selected", false);
            if (selectedNode == defaultNode)
            {
                oldChild.EnableInClassList("node_default", true);
            }
            else if (exitNodes.Contains(selectedNode))
            {
                oldChild.EnableInClassList("node_exit", true);
            }
            else oldChild.EnableInClassList("node_bg", true);
        }

        VisualElement newChild = newSelection.ElementAt(0);
        newChild.EnableInClassList("node_bg", false);
        newChild.EnableInClassList("node_default", false);
        newChild.EnableInClassList("node_exit", false);
        newChild.EnableInClassList("node_selected", true);
        selectedNode = newSelection;

        DialogueTreeEditor.SelectionUpdate(GetNode(selectedNode.name));
        Selection.activeObject = selection;
        EditorUtility.SetDirty(selection);
    }

    public override void MoveNode(VisualElement node, Vector2 newPosition)
    {
        selection.SetNodePosition(node.name, panRoot.WorldToLocal(newPosition));
        EditorUtility.SetDirty(selection);
    }
}
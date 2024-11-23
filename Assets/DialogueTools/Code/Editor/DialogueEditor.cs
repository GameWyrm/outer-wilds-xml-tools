using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;


public class DialogueEditor : EditorWindow
{
    public static DialogueEditor instance;
    public static DialogueTreeAsset selection;
    public static bool isFocused;

    private Dictionary<string, VisualElement> nodes;
    private DialogueTree dialogueTree;
    private VisualElement panRoot;
    private VisualElement arrowsRoot;
    private VisualElement nodesRoot;
    private Dictionary<string, NodeManipulator> nodeManipulators;
    private VisualElement selectedNode;
    private VisualElement defaultNode;
    private List<VisualElement> exitNodes;
    private VisualElement background;
    private PannerManipulator panner;
    // should be 1 for creating the GUI, 2 for resetting the nodes
    private int initializingState;

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

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = DialogueEditorSettings.Instance.VisualTree;
        VisualElement labelFromUXML = visualTree.CloneTree();
        root.Add(labelFromUXML);

        panRoot = new VisualElement();
        panRoot.name = "Pan Root";
        arrowsRoot = new VisualElement();
        arrowsRoot.name = "Arrows Root";
        nodesRoot = new VisualElement();
        nodesRoot.name = "Node Root";
        panRoot.Add(arrowsRoot);
        panRoot.Add(nodesRoot);

        initializingState = 1;
        if (selection != null) BuildNodeTree();
        initializingState = 2;

        var importButton = root.Q<Button>("import");
        importButton.clicked += OnClickImport;

        var exportButton = root.Q<Button>("export");
        exportButton.clicked += OnClickExport;

        var newTreeButton = root.Q<Button>("newTree");
        newTreeButton.clicked += OnClickNewTree;

        var newNodeButton = root.Q<Button>("newNode");
        newNodeButton.clicked += OnClickNewNode;

        var resetButton = root.Q<Button>("resetNodes");
        resetButton.clicked += OnClickResetNodes;

        var toolbar = root.Q<Toolbar>("toolbar");
        toolbar.parent.Add(panRoot);

        background = root.Q<Box>("bg");
        if (panner != null) panner.UnregisterCallbacks();
        panner = new PannerManipulator();
        panner.background = background;
        panner.panRoot = panRoot;
        panner.RegisterCallbacks();
        
        toolbar.BringToFront();

        isFocused = true;
    }

    public void BuildNodeTree()
    {
        Vector2 oldPanRootPosition = panRoot.transform.position;
        nodesRoot.Clear();
        arrowsRoot.Clear();
        defaultNode = null;
        exitNodes = new List<VisualElement>();
        nodeManipulators = new Dictionary<string, NodeManipulator>();
        nodes = new Dictionary<string, VisualElement>();
        panRoot.transform.position = Vector2.zero;
        if (selection.tree == null || selection.tree.dialogueNodes == null) return;
        dialogueTree = selection.tree;
        for (int i = 0; i < selection.tree.dialogueNodes.Length; i++)
        {
            var node = selection.tree.dialogueNodes[i];
            var nodeInfo = selection.nodes[i];
            VisualElement newNode = GUIBuilder.CreateDialogueNode(node.nodeName, nodeManipulators);
            newNode.transform.position = panRoot.LocalToWorld(nodeInfo.position);

            if (node.entryConditions != null && node.entryConditions.Contains("DEFAULT"))
            {
                defaultNode = newNode;
                newNode.EnableInClassList("node_bg", false);
                newNode.EnableInClassList("node_default", true);
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
                    exitNodes.Add(newNode);
                    newNode.EnableInClassList("node_bg", false);
                    newNode.EnableInClassList("node_exit", true);
                }
            }
            nodesRoot.Add(newNode);
            nodes.Add(node.nodeName, newNode);
        }
        for (int i = 0; i < selection.tree.dialogueNodes.Length; i++)
        {
            var node = selection.tree.dialogueNodes[i];

            VisualElement sourceNode = nodes[node.nodeName];
            VisualElement targetNode;

            if (!string.IsNullOrEmpty(node.dialogueTarget))
            {
                targetNode = nodes[node.dialogueTarget];
                arrowsRoot.Add(GUIBuilder.CreateArrow(sourceNode, targetNode, node.nodeName, node.dialogueTarget, nodeManipulators, initializingState));
            }

            if (node.dialogueOptionsList != null && node.dialogueOptionsList.dialogueOptions != null)
            {
                foreach (var option in node.dialogueOptionsList.dialogueOptions)
                {
                    if (!string.IsNullOrEmpty(option.dialogueTarget))
                    {
                        targetNode = nodes[option.dialogueTarget];
                        
                        arrowsRoot.Add(GUIBuilder.CreateArrow(sourceNode, targetNode, node.nodeName, option.dialogueTarget, nodeManipulators, initializingState));
                    }
                }
            }
        }
        panRoot.transform.position = oldPanRootPosition;
    }

    private void OnClickImport()
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
                dialogueTreeAsset.nodes = new List<DialogueTreeAsset.DialogueNodeInfo>();
                for(int i = 0; i < dialogueTreeAsset.tree.dialogueNodes.Length; i++)
                {
                    var node = dialogueTreeAsset.tree.dialogueNodes[i];
                    DialogueTreeAsset.DialogueNodeInfo newNode = new DialogueTreeAsset.DialogueNodeInfo(node.nodeName, GetResetPosition(i));
                    dialogueTreeAsset.nodes.Add(newNode);
                }

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
        selection.nodes.Add(new DialogueTreeAsset.DialogueNodeInfo(newNode.nodeName, new Vector2(100, 100)));
        BuildNodeTree();

        Debug.Log("New Node Created");
    }

    private void OnClickResetNodes()
    {
        panRoot.transform.position = Vector3.zero;
        int i = 0;
        if (nodes == null) return;
        foreach (var node in nodes.Keys)
        {
            selection.SetNodePosition(node, GetResetPosition(i));
            i++;
        }

        BuildNodeTree();
    }

    private Vector2 GetResetPosition(int index)
    {
        float x = 50;
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

    public void SelectNode(VisualElement newSelection)
    {
        if (selectedNode != null)
        {
            selectedNode.EnableInClassList("node_selected", false);
            if (selectedNode == defaultNode)
            {
                selectedNode.EnableInClassList("node_default", true);
            }
            else if (exitNodes.Contains(selectedNode))
            {
                selectedNode.EnableInClassList("node_exit", true);
            }
            else selectedNode.EnableInClassList("node_bg", true);
        }


        newSelection.EnableInClassList("node_bg", false);
        newSelection.EnableInClassList("node_default", false);
        newSelection.EnableInClassList("node_exit", false);
        newSelection.EnableInClassList("node_selected", true);
        selectedNode = newSelection;

        DialogueTreeEditor.SelectionUpdate(GetNode(selectedNode.name));
        Selection.activeObject = selection;
        EditorUtility.SetDirty(selection);
    }

    public void MoveNode(VisualElement node, Vector2 newPosition)
    {
        selection.SetNodePosition(node.name, panRoot.WorldToLocal(newPosition));
        EditorUtility.SetDirty(selection);
    }
}
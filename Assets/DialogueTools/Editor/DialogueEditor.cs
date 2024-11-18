using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public class DialogueEditor : EditorWindow
{
    public static DialogueEditor instance;
    public static DialogueTreeAsset selection;
    public static bool isFocused;

    private Dictionary<string, VisualElement> nodes;
    private DialogueTree dialogueTree;
    private VisualElement arrowsRoot;
    private VisualElement nodesRoot;
    private Dictionary<string, NodeManipulator> nodeManipulators;
    private VisualElement selectedNode;


    [MenuItem("Tools/XML Editors/Dialogue Editor")]
    public static void ShowExample()
    {
        instance = GetWindow<DialogueEditor>();
        instance.titleContent = new GUIContent("Dialogue Editor");
    }

    public static void OpenWindowWithSelection(DialogueTreeAsset newSelection)
    {
        selection = newSelection;
        ShowExample();
    }

    public void CreateGUI()
    {

        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/DialogueTools/Editor/DialogueEditor.uxml");
        VisualElement labelFromUXML = visualTree.CloneTree();
        root.Add(labelFromUXML);

        arrowsRoot = new VisualElement();
        arrowsRoot.name = "Arrows Root";
        nodesRoot = new VisualElement();
        nodesRoot.name = "Node Root";
        root.Add(arrowsRoot);
        root.Add(nodesRoot);

        if (selection != null) CreateNodes();

        var importButton = root.Q<Button>("import");
        importButton.clicked += OnClickImport;

        var exportButton = root.Q<Button>("export");
        exportButton.clicked += OnClickExport;

        var newTreeButton = root.Q<Button>("newTree");
        newTreeButton.clicked += OnClickNewTree;

        var newNodeButton = root.Q<Button>("newNode");
        newNodeButton.clicked += OnClickNewNode;

        isFocused = true;
    }

    private void CreateNodes()
    {
        nodesRoot.Clear();
        nodeManipulators = new Dictionary<string, NodeManipulator>();
        nodes = new Dictionary<string, VisualElement>();
        if (selection.tree == null || selection.tree.dialogueNodes == null) return;
        dialogueTree = selection.tree;
        for (int i = 0; i < selection.tree.dialogueNodes.Length; i++)
        {
            var node = selection.tree.dialogueNodes[i];
            var nodeInfo = selection.nodes[i];
            VisualElement newNode = CreateNewNode(node.nodeName, nodeInfo.labelColor);
            newNode.transform.position = nodeInfo.position;
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
                arrowsRoot.Add(CreateArrow(sourceNode, targetNode, node.nodeName, node.dialogueTarget));
            }

            if (node.dialogueOptionsList != null && node.dialogueOptionsList.dialogueOptions != null)
            {
                foreach (var option in node.dialogueOptionsList.dialogueOptions)
                {
                    if (!string.IsNullOrEmpty(option.dialogueTarget))
                    {
                        targetNode = nodes[option.dialogueTarget];
                        arrowsRoot.Add(CreateArrow(sourceNode, targetNode, node.nodeName, option.dialogueTarget));
                    }
                }
            }
        }
    }

    private VisualElement CreateArrow(VisualElement source, VisualElement target, string nodeName, string targetName)
    {
        VisualElement element = new VisualElement();
        var settings = DialogueEditorSettings.Instance;
        element.styleSheets.Add(settings.Style);
        element.EnableInClassList("arrow_container", true);
        Image newLine = new Image();
        newLine.image = settings.LineTexture;
        newLine.styleSheets.Add(settings.Style);
        newLine.EnableInClassList("line", true);
        element.Add(newLine);
        Image newArrow = new Image();
        newArrow.image = settings.ArrowTexture;
        newArrow.styleSheets.Add(settings.Style);
        newArrow.EnableInClassList("arrow", true);
        element.Add(newArrow);

        ArrowManipulator arrowManipulator = new ArrowManipulator();
        arrowManipulator.sourceNode = source;
        arrowManipulator.targetNode = target;
        arrowManipulator.line = newLine;
        arrowManipulator.arrow = newArrow;

        nodeManipulators[nodeName].arrows.Add(arrowManipulator);
        nodeManipulators[targetName].arrows.Add(arrowManipulator);

        arrowManipulator.OrientArrow();

        return element;
    }

    private VisualElement CreateNewNode(string nodeName, DialogueTreeAsset.LabelColor styleID)
    {
        var settings = DialogueEditorSettings.Instance;
        VisualElement newNode = new VisualElement();
        newNode.styleSheets.Add(settings.Style);
        newNode.name = nodeName;
        newNode.EnableInClassList("node_bg", true);
        Label label = new Label(nodeName);
        label.styleSheets.Add(settings.Style);
        label.name = "node_label";
        string labelColor = "label_" + styleID.ToString().ToLower();
        label.EnableInClassList(labelColor, true);
        newNode.Add(label);

        var nodeManipulator = new NodeManipulator(newNode);
        nodeManipulator.RegisterCallbacksOnTarget();
        nodeManipulator.arrows = new List<ArrowManipulator>();
        nodeManipulators.Add(nodeName, nodeManipulator);

        return newNode;
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

            if (!string.IsNullOrEmpty(savePath))
            {

                DialogueTreeAsset dialogueTreeAsset = ScriptableObject.CreateInstance<DialogueTreeAsset>();
                dialogueTreeAsset.tree = dialogueTree;

                // populate with default node data
                dialogueTreeAsset.nodes = new List<DialogueTreeAsset.DialogueNodeInfo>();
                int counter = 1;
                foreach (var node in dialogueTreeAsset.tree.dialogueNodes)
                {
                    DialogueTreeAsset.DialogueNodeInfo newNode = new DialogueTreeAsset.DialogueNodeInfo();
                    newNode.node = node;
                    newNode.position = new Vector2(counter * 20, counter * 20);
                    newNode.label = node.nodeName;
                    newNode.labelColor = (DialogueTreeAsset.LabelColor)Random.Range((int)0, (int)6);
                    dialogueTreeAsset.nodes.Add(newNode);
                    counter++;
                }

                AssetDatabase.CreateAsset(dialogueTreeAsset, savePath);
                AssetDatabase.SaveAssets();
                Selection.activeObject = dialogueTreeAsset;
                selection = dialogueTreeAsset;
                CreateNodes();

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
        Debug.Log("New Node button pressed!");
    }

    private void OnDestroy()
    {
        DialogueTreeEditor.activeNode = null;
        isFocused = false;
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
            selectedNode.EnableInClassList("node_bg", true);
            selectedNode.EnableInClassList("node_selected", false);
        }

        newSelection.EnableInClassList("node_bg", false);
        newSelection.EnableInClassList("node_selected", true);
        selectedNode = newSelection;

        DialogueTreeEditor.SelectionUpdate(GetNode(selectedNode.name));
        Selection.activeObject = selection;
        EditorUtility.SetDirty(selection);
    }
}
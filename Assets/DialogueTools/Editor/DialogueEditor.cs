using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using static DialogueTree;
using System.IO;
using System.Xml.Serialization;
using System.Xml;


public class DialogueEditor : EditorWindow
{
    public static DialogueEditor instance;
    public static DialogueTreeAsset selection;

    private NodeManipulator manipulator;

    [MenuItem("Tools/DialogueEditor")]
    public static void ShowExample()
    {
        instance = GetWindow<DialogueEditor>();
        instance.titleContent = new GUIContent("DialogueEditor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/DialogueTools/Editor/DialogueEditor.uxml");
        VisualElement labelFromUXML = visualTree.CloneTree();
        root.Add(labelFromUXML);

        // A stylesheet can be added to a VisualElement.
        // The style will be applied to the VisualElement and all of its children.
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/DialogueTools/Editor/DialogueEditor.uss");

        var importButton = root.Q<Button>("import");
        importButton.clicked += OnClickImport;

        var exportButton = root.Q<Button>("export");
        exportButton.clicked += OnClickExport;

        var newTreeButton = root.Q<Button>("newTree");
        newTreeButton.clicked += OnClickNewTree;

        var newNodeButton = root.Q<Button>("newNode");
        newNodeButton.clicked += OnClickNewNode;

        if (manipulator != null) manipulator.UnregisterCallbacksOnTarget();
        manipulator = new NodeManipulator(rootVisualElement.Q<VisualElement>("object"));
        manipulator.RegisterCallbacksOnTarget();
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
                AssetDatabase.CreateAsset(dialogueTreeAsset, savePath);
                AssetDatabase.SaveAssets();
                Selection.activeObject = dialogueTreeAsset;
                selection = dialogueTreeAsset;

                Debug.Log($"Created new Dialogue Tree at {savePath}");
            }
        }
    }

    private void OnClickExport()
    {
        if (selection == null)
        {
            Debug.LogWarning("No object to export selected.");
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

            Debug.Log($"Created new Dialogue Tree at {savePath}");
        }
    }

    private void OnClickNewNode()
    {
        Debug.Log("New Node button pressed!");
    }

    private void OnDestroy()
    {
        manipulator.UnregisterCallbacksOnTarget();
        manipulator = null;
    }
}
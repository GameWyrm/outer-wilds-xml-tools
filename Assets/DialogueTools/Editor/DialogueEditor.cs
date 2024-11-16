using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class DialogueEditor : EditorWindow
{
    public static DialogueEditor instance;

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
        Debug.Log("Import button pressed!");
    }

    private void OnClickExport()
    {
        Debug.Log("Export button pressed!");
    }

    private void OnClickNewTree()
    {
        Debug.Log("New Tree button pressed!");
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
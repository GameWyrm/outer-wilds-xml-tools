using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class NodeWindow : EditorWindow
{
    public static bool isFocused;

    protected Dictionary<string, VisualElement> nodeElements;
    protected List<NodeData> nodes = new List<NodeData>();
    protected VisualTreeAsset visualTree;
    protected VisualElement root;
    protected VisualElement scaleRoot;
    protected VisualElement panRoot;
    protected VisualElement arrowsRoot;
    protected VisualElement nodesRoot;
    protected Dictionary<string, NodeManipulator> nodeManipulators;
    protected VisualElement selectedNode;
    protected VisualElement background;
    protected PannerManipulator panner;
    protected int initializingState;

    private void CreateGUI()
    {
        ConstructGUI();
        initializingState = 1;
        BuildNodeTree();
        initializingState = 2;
        ConstructGUILate();
    }

    protected virtual void ConstructGUI()
    {
        root = rootVisualElement;

        VisualElement UXMLdata = visualTree.CloneTree();
        root.Add(UXMLdata);

        scaleRoot = new VisualElement();
        scaleRoot.name = "Scale Root";
        panRoot = new VisualElement();
        panRoot.name = "Pan Root";
        arrowsRoot = new VisualElement();
        arrowsRoot.name = "Arrows Root";
        nodesRoot = new VisualElement();
        nodesRoot.name = "Node Root";
        scaleRoot.Add(panRoot);
        panRoot.Add(arrowsRoot);
        panRoot.Add(nodesRoot);
    }

    protected virtual void ConstructGUILate()
    {
        var importButton = root.Q<Button>("import");
        importButton.clicked += OnClickImport;
        
        var toolbar = root.Q<Toolbar>("toolbar");
        toolbar.parent.Add(scaleRoot);

        background = root.Q<Box>("bg");
        if (panner != null) panner.UnregisterCallbacks();
        panner = new PannerManipulator();
        panner.background = background;
        panner.panRoot = panRoot;
        panner.RegisterCallbacks();

        toolbar.BringToFront();

        isFocused = true;
    }

    public virtual void BuildNodeTree()
    {
        float oldScale = scaleRoot.transform.scale.x;
        Vector2 oldPosition = panRoot.transform.position;

        nodeManipulators = new Dictionary<string, NodeManipulator>();
        nodeElements = new Dictionary<string, VisualElement>();
        panRoot.transform.position = Vector2.zero;

        for (int i = 0; i < nodes.Count; i++)
        {
            VisualElement newNode = GUIBuilder.CreateDialogueNode(nodes[i].name, nodeManipulators, this);
            newNode.transform.position = panRoot.LocalToWorld(nodes[i].position);
            OnCreateNode(newNode);

            nodesRoot.Add(newNode);
            nodeElements.Add(nodes[i].name, newNode);

            
        }
        // We need to wait for all the nodes to be created before we can start making arrows
        for (int i = 0; i < nodes.Count; i++)
        {
            VisualElement sourceNode = nodeElements[nodes[i].name];
            List<VisualElement> targetNodes = GetTargetNodes(nodes[i].name);

            foreach (var targetNode in targetNodes)
            {
                arrowsRoot.Add(GUIBuilder.CreateArrow(sourceNode, targetNode, initializingState, out ArrowManipulator manipulator));
                nodeManipulators[sourceNode.name].arrows.Add(manipulator);
                nodeManipulators[targetNode.name].arrows.Add(manipulator);
            }
        }
        scaleRoot.transform.scale = Vector3.one * oldScale;
        panRoot.transform.position = oldPosition;
    }

    protected abstract void OnClickImport();

    /// <summary>
    /// Edit nodes as they are being created
    /// </summary>
    /// <param name="createdNode"></param>
    protected abstract void OnCreateNode(VisualElement createdNode);

    protected abstract List<VisualElement> GetTargetNodes(string nodeName);

    public abstract void SelectNode(VisualElement newSelection);

    public abstract void MoveNode(VisualElement node, Vector2 newPosition);
}
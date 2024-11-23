using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

public abstract class NodeWindow : EditorWindow
{
    public static bool isFocused;

    private Dictionary<string, VisualElement> nodes;
    private VisualElement scaleRoot;
    private VisualElement panRoot;
    private VisualElement arrowsRoot;
    private VisualElement nodesRoot;
    private Dictionary<string, NodeManipulator> nodeManipulators;
    private VisualElement selectedNode;
    private VisualElement background;
    private PannerManipulator panner;

    public abstract void BuildNodeTree();
}
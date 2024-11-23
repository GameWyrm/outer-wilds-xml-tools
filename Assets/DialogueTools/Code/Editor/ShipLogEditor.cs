using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class ShipLogEditor : EditorWindow
{
    public static ShipLogEditor Instance;

    private VisualElement scaleRoot;
    private VisualElement panRoot;
    private VisualElement arrowsRoot;
    private VisualElement nodesRoot;
    private VisualElement background;

    [MenuItem("Tools/XML Editors/Ship Log Editor")]
    public static void ShowWindow()
    {
        Instance = GetWindow<ShipLogEditor>();
        Instance.titleContent = new GUIContent("ShipLogEditor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = EditorReferences.Instance.ShipLogVisualTree;
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
        panRoot.Add(arrowsRoot);
        panRoot.Add(nodesRoot);
        scaleRoot.Add(panRoot);

        var importButton = root.Q<Button>("import");
        importButton.clicked += OnClickImport;

        var centerCameraButton = root.Q<Button>("centerCamera");
        centerCameraButton.clicked += OnClickCenterCamera;

        var zoomInButton = root.Q<Button>("addZoom");
        zoomInButton.clicked += OnClickZoomIn;

        var zoomOutButton = root.Q<Button>("subtractZoom");
        zoomOutButton.clicked += OnClickZoomOut;

        var toolbar = root.Q<Toolbar>("toolbar");
        toolbar.parent.Add(scaleRoot);

        background = root.Q<Box>("bg");
    }

    public void BuildNodeTree()
    {
        float oldScale = scaleRoot.transform.scale.x;
        Vector2 oldPosition = panRoot.transform.position;

        scaleRoot.transform.scale = Vector3.one;
        panRoot.transform.position = Vector3.zero;


    }

    private void OnClickImport()
    {
        Debug.Log("Import Button Clicked");
    }

    private void OnClickCenterCamera()
    {
        Debug.Log("Center Camera Button Clicked");

    }

    private void OnClickZoomIn()
    {
        Debug.Log("Zoom In Button Clicked");

    }

    private void OnClickZoomOut()
    {
        Debug.Log("Zoom Out Button Clicked");

    }
}
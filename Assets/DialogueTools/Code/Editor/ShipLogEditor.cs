using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using System.IO;


public class ShipLogEditor : NodeWindow
{
    public static ShipLogEditor Instance;

    [MenuItem("Tools/XML Editors/Ship Log Editor")]
    public static void ShowWindow()
    {
        Instance = GetWindow<ShipLogEditor>();
        Instance.titleContent = new GUIContent("ShipLogEditor");
        //Instance.BuildNodeTree();
    }

    protected override void ConstructGUI()
    {
        visualTree = EditorReferences.Instance.ShipLogVisualTree;
        base.ConstructGUI();
    }

    public override void BuildNodeTree()
    {
        float oldScale = scaleRoot.transform.scale.x;
        Vector2 oldPosition = panRoot.transform.position;

        nodeManipulators = new Dictionary<string, NodeManipulator>();
        nodeElements = new Dictionary<string, VisualElement>();
        scaleRoot.transform.scale = Vector3.one;
        panRoot.transform.position = Vector2.zero;

        arrowsRoot.Clear();
        nodesRoot.Clear();
        ShipLogManager manager = ShipLogManager.Instance;
        if (manager == null) return;
        if (manager.datas == null || manager.datas.Count == 0) return;
        nodes = new List<NodeData>();
        foreach (var data in manager.datas)
        {
            nodes.AddRange(data.nodes);
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            Debug.Log("Node name:" + nodes[i].name);
            ShipLogEntry.Entry entry = manager.GetEntry(nodes[i].name, out EntryData data);


            EntryType entryType = EntryType.Normal;
            if (entry.isCuriosity) entryType = EntryType.Curiosity;
            else if (data.entriesWhoAreChildren.Contains(entry.entryID)) entryType = EntryType.Child;

            VisualElement newNode;
            newNode = GUIBuilder.CreateShipLogNode(entry.entryID, entry.name, nodeManipulators, this, entryType);
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
                arrowsRoot.Add(GUIBuilder.CreateArrow(targetNode, sourceNode, out ArrowManipulator manipulator));
                nodeManipulators[sourceNode.name].arrows.Add(manipulator);
                nodeManipulators[targetNode.name].arrows.Add(manipulator);
            }
        }
        scaleRoot.transform.scale = Vector3.one * oldScale;
        panRoot.transform.position = oldPosition;
    }

    protected override void ConstructGUILate()
    {
        var centerCameraButton = root.Q<Button>("centerCamera");
        centerCameraButton.clicked += OnClickCenterCamera;

        var zoomInButton = root.Q<Button>("addZoom");
        zoomInButton.clicked += OnClickZoomIn;

        var zoomOutButton = root.Q<Button>("subtractZoom");
        zoomOutButton.clicked += OnClickZoomOut;

        base.ConstructGUILate();
    }

    protected override void OnCreateNode(VisualElement createdNode)
    {
        //Debug.Log($"Creating node {createdNode.name}");
    }

    protected override void OnClickImport()
    {
        string xmlPath = EditorUtility.OpenFilePanel("Select Planet Ship Log XML File", "", "xml");

        if (string.IsNullOrEmpty(xmlPath)) return;

        XmlSerializer serializer = new XmlSerializer(typeof(ShipLogEntry));
        ShipLogEntry entry;

        using (FileStream fileStream = new FileStream(xmlPath, FileMode.Open))
        {
            entry = (ShipLogEntry)serializer.Deserialize(fileStream);
        }

        if (entry == null)
        {
            Debug.LogError("Unable to parse selected file. Are you sure it is ship log xml file?");
            return;
        }

        string jsonPath = EditorUtility.OpenFilePanel("Select Star System JSON file", "", "json");
        if (string.IsNullOrEmpty(jsonPath)) return;
        string jsonFile = File.ReadAllText(jsonPath);
        StarSystem systemInfo = JsonConvert.DeserializeObject<StarSystem>(jsonFile);
        if (systemInfo == null) return;

        EntryData data = ShipLogManager.Instance.CreateEntryData(entry, systemInfo);

        string savePath = EditorUtility.SaveFilePanelInProject("Save Entry Data as...", data.entry.planetID, "asset", "Select a location to save your Entry Data to.");
        if (string.IsNullOrEmpty(savePath))
        {
            Debug.LogError("Save path is invalid!");
            return;
        }

        AssetDatabase.CreateAsset(data, savePath);
        AssetDatabase.SaveAssets();

        ShipLogManager.Instance.datas.Add(data);

        BuildNodeTree();

        Debug.Log($"Created new planet data at {savePath}.");
    }

    private void OnClickCenterCamera()
    {
        BuildNodeTree();
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

    protected override List<VisualElement> GetTargetNodes(string nodeName)
    {
        ShipLogEntry.Entry entry = ShipLogManager.Instance.GetEntry(nodeName);
        if (entry == null) return null;
        List<VisualElement> targetNodes = new List<VisualElement>();

        if (entry.rumorFacts == null) return targetNodes;

        foreach (var rumor in entry.rumorFacts)
        {
            if (nodeElements.ContainsKey(rumor.sourceID))
            {
                targetNodes.Add(nodeElements[rumor.sourceID]);
            }
        }
        return targetNodes;
    }

    public override void SelectNode(VisualElement newSelection)
    {
        Debug.LogWarning("Can't select nodes yet");
    }

    public override void MoveNode(VisualElement node, Vector2 newPosition)
    {
        Debug.LogWarning("Can't move nodes yet");
    }
}
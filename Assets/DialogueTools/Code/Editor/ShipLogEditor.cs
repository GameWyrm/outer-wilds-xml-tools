using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using System.IO;

namespace XmlTools
{
    public class ShipLogEditor : NodeWindow
    {
        public static ShipLogEditor Instance;

        private Label zoomText;

        [MenuItem("Tools/XML Editors/Ship Log Editor")]
        public static void ShowWindow()
        {
            Instance = GetWindow<ShipLogEditor>();
            Instance.titleContent = new GUIContent("ShipLogEditor");
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
            manager.ValidateData();
            if (manager == null) return;
            if (manager.datas == null || manager.datas.Count == 0) return;
            nodes = new List<NodeData>();
            foreach (var data in manager.datas)
            {
                nodes.AddRange(data.nodes);
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                ShipLogEntry.Entry entry = manager.GetEntry(nodes[i].name, out EntryData data);

                EntryType entryType = EntryType.Normal;
                if (entry.isCuriosity) entryType = EntryType.Curiosity;
                else if (data.entriesWhoAreChildren.Contains(entry.entryID)) entryType = EntryType.Child;

                VisualElement newNode;
                newNode = GUIBuilder.CreateShipLogNode(entry.entryID, entry.name, nodeManipulators, this, entryType);
                newNode.transform.position = panRoot.LocalToWorld(new Vector2(nodes[i].position.x, nodes[i].position.y * -1));
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

            zoomText = root.Q<Label>("zoomText");

            base.ConstructGUILate();
        }

        private void OnGUI()
        {
            Event currentEvent = Event.current;

            if (currentEvent.type == EventType.ScrollWheel)
            {
                if (currentEvent.delta.y < 0) OnClickZoomIn();
                else if (currentEvent.delta.y > 0) OnClickZoomOut();
            }
        }

        protected override void OnCreateNode(VisualElement createdNode)
        {
            string id = createdNode.name;
            ShipLogManager manager = ShipLogManager.Instance;
            ShipLogEntry.Entry entry = manager.GetEntry(id);
            if (entry == null) return;
            VisualElement bg = createdNode.Q<VisualElement>("bg");
            bg.style.backgroundColor = manager.GetCuriosityColor(entry.curiosity);

            Language lang = XMLEditorSettings.Instance.GetSelectedLanguage();

            Label label = bg.Q<Label>("label");
            string logName = lang.GetShipLogValue(entry.name);
            if (!string.IsNullOrEmpty(logName)) label.text = logName;

            // TODO set image
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
            zoom += 0.25f;
            if (zoom > 2f) zoom = 2f;
            UpdateZoom();
        }

        private void OnClickZoomOut()
        {
            zoom -= 0.25f;
            if (zoom < 0.25f) zoom = 0.25f;
            UpdateZoom();
        }

        private void UpdateZoom()
        {
            scaleRoot.transform.scale = Vector3.one * zoom;
            zoomText.text = Mathf.RoundToInt(100 * zoom) + "%";
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
}
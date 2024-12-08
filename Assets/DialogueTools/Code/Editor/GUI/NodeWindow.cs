using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace XmlTools
{
    public abstract class NodeWindow : EditorWindow
    {
        public bool isFocused;
        public float zoom;

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
        protected bool init;

        private void CreateGUI()
        {
            init = true;
            zoom = 1;
            ConstructGUI();
            BuildNodeTree();
            ConstructGUILate();
            init = false;
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

        public virtual void BuildNodeTree()
        {
            float oldScale = scaleRoot.transform.scale.x;
            Vector2 oldPosition = panRoot.transform.position;

            nodeManipulators = new Dictionary<string, NodeManipulator>();
            nodeElements = new Dictionary<string, VisualElement>();
            scaleRoot.transform.scale = Vector3.one;
            panRoot.transform.position = Vector2.zero;

            arrowsRoot.Clear();
            nodesRoot.Clear();

            for (int i = 0; i < nodes.Count; i++)
            {
                VisualElement newNode;
                newNode = GUIBuilder.CreateDialogueNode(nodes[i].name, nodeManipulators, this);
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
                    arrowsRoot.Add(GUIBuilder.CreateArrow(sourceNode, targetNode, out ArrowManipulator manipulator));
                    nodeManipulators[sourceNode.name].arrows.Add(manipulator);
                    nodeManipulators[targetNode.name].arrows.Add(manipulator);
                }
            }
            scaleRoot.transform.scale = Vector3.one * oldScale;
            panRoot.transform.position = oldPosition;
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
            panner.window = this;
            panner.RegisterCallbacks();

            toolbar.BringToFront();

            isFocused = true;
        }

        public virtual void OnPan(Vector2 newPosition)
        {
            // optional
        }

        /// <summary>
        /// Runs when you click the "Import" button on the toolbar
        /// </summary>
        protected abstract void OnClickImport();

        /// <summary>
        /// Edit nodes as they are being created
        /// </summary>
        /// <param name="createdNode"></param>
        protected abstract void OnCreateNode(VisualElement createdNode);

        /// <summary>
        /// Returns the nodes that should be the target of connected arrows
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        protected abstract List<VisualElement> GetTargetNodes(string nodeName);

        /// <summary>
        /// Runs when a node is selected, and determines how it should act when selected
        /// </summary>
        /// <param name="newSelection"></param>
        public abstract void SelectNode(VisualElement newSelection);

        /// <summary>
        /// How a node should act when moved
        /// </summary>
        /// <param name="node"></param>
        /// <param name="newPosition"></param>
        public abstract void MoveNode(VisualElement node, Vector2 newPosition);

        public NodeData GetNode(string nodeName)
        {
            return nodes.Find(x => x.name == nodeName);
        }
    }
}

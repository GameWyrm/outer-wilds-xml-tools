using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Tree", menuName = "Tools/Dialogue Tree", order = 0)]
public class DialogueTreeAsset : ScriptableObject
{
    [HideInInspector]
    public DialogueTree tree;
    //[HideInInspector]
    //public DialogueNodeInfo selectedNode;
    //[HideInInspector]
    //public List<DialogueNodeInfo> nodes;

    [HideInInspector]
    public List<NodeData> NodeDatas
    {
        get
        {
            if (tree == null || tree.dialogueNodes == null) return null;
            if (nodeDatas == null)
            {
                nodeDatas = new List<NodeData>();
                for (int i = 0; i < tree.dialogueNodes.Length; i++)
                {
                    var node = tree.dialogueNodes[i];

                    Vector2 newPos = new Vector2(0, 0);
                    if (i != 0)
                    {
                        newPos.x += 350 * (i % 4);
                        newPos.y += 200 * Mathf.Floor(i / 4);
                    }

                    NodeData data = new NodeData(node.nodeName, newPos);
                    nodeDatas.Add(data);
                }
            }
            return nodeDatas;
        }
        set
        {
            nodeDatas = value;
        }
    }

    [SerializeField]
    private List<NodeData> nodeDatas;

    [Serializable]
    public class DialogueNodeInfo
    {
        public DialogueNodeInfo(string nodeName, Vector2 position)
        {
            this.nodeName = nodeName;
            this.position = position;
            this.label = nodeName;
        }

        public string nodeName;
        public Vector2 position;
        public string label;
    }



    public void SetNodePosition(string nodeName, Vector2 position)
    {
        var node = NodeDatas.First(x => x.name == nodeName);
        node.position = position;
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Tree", menuName = "Tools/Dialogue Tree", order = 0)]
public class DialogueTreeAsset : ScriptableObject
{
    [HideInInspector]
    public DialogueTree tree;
    [HideInInspector]
    public DialogueNodeInfo selectedNode;
    [HideInInspector]
    public List<DialogueNodeInfo> nodes;

    [Serializable]
    public class DialogueNodeInfo
    {
        public string nodeName;
        public Vector2 position;
        public string label;
    }

    public void SetNodePosition(string nodeName, Vector2 position)
    {
        var node = nodes.First(x => x.nodeName == nodeName);
        node.position = position;
    }
}


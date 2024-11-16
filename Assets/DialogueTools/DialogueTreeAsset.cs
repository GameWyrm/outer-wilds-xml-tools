using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Tree", menuName = "Tools/Dialogue Tree", order = 0)]
public class DialogueTreeAsset : ScriptableObject
{
    [HideInInspector]
    public DialogueTree tree;
    [HideInInspector]
    public DialogueNode selectedNode;
    [HideInInspector]
    public List<DialogueNode> nodes;

    public class DialogueNode
    {
        public DialogueTree.DialogueNode node;
        public Dictionary<string, Color> targetColors;
        public Vector2 position;
        public string label;
    }
}

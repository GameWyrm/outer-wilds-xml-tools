using System;
using System.Collections;
using System.Collections.Generic;
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
        public DialogueNode node;
        public LabelColor labelColor;
        public Vector2 position;
        public string label;
    }
    public enum LabelColor
    {
        Pink = 0,
        Orange = 1,
        Yellow = 2,
        Green = 3,
        Blue = 4,
        Magenta = 5
    }
}


using System;
using UnityEngine;

namespace XmlTools
{
    [Serializable]
    public class NodeData
    {
        public NodeData(string name, Vector2 position)
        {
            this.name = name;
            this.position = position;
        }

        public string name;
        public Vector2 position;
    }
}

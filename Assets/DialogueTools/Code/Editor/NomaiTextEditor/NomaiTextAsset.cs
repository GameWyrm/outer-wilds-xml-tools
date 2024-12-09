using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace XmlTools
{
    public class NomaiTextAsset : ScriptableObject
    {
        [SerializeField, HideInInspector]
        public NomaiText text;

        [HideInInspector]
        public List<NodeData> NodeDatas
        {
            get
            {
                if (text == null || text.textBlocks == null) return null;
                if (nodeDatas == null)
                {
                    nodeDatas = new List<NodeData>();
                    for (int i = 0; i < text.textBlocks.Length; i++)
                    {
                        var node = text.textBlocks[i];

                        Vector2 newPos = new Vector2(0, 0);
                        if (i != 0)
                        {
                            newPos.x += 350 * (i % 4);
                            newPos.y += 200 * Mathf.Floor(i / 4);
                        }

                        NodeData data = new NodeData(node.textID.ToString(), newPos);
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



        public void SetNodePosition(string nodeName, Vector2 position)
        {
            var node = NodeDatas.First(x => x.name == nodeName);
            node.position = position;
        }
    }
}
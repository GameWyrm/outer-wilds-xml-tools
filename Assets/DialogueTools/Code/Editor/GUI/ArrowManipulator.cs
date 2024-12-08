using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace XmlTools
{
    public class ArrowManipulator
    {
        public VisualElement sourceNode;
        public VisualElement targetNode;
        public Image arrow;
        public VisualElement line;

        public ArrowManipulator(VisualElement sourceNode, VisualElement targetNode, Image arrow, VisualElement line)
        {
            this.sourceNode = sourceNode;
            this.targetNode = targetNode;
            this.arrow = arrow;
            this.line = line;
        }

        /// <summary>
        /// Makes arrows point towards their targets from their sources
        /// </summary>
        /// <param name="state"></param>
        public void OrientArrow()
        {
            VisualElement panRoot = sourceNode.parent;
            VisualElement container = arrow.parent;
            Vector2 sourcePosition = sourceNode.transform.position - panRoot.transform.position;
            Vector2 targetPosition = targetNode.transform.position - panRoot.transform.position;
            // set scale
            float lineLength = Vector2.Distance(sourcePosition, targetPosition);
            line.transform.scale = new Vector3(1, lineLength, 1);

            // set angle
            float angle = Vector2.SignedAngle(Vector2.up, targetPosition - sourcePosition);

            container.transform.rotation = Quaternion.Euler(0, 0, angle + 180);

            // set position
            Vector2 sourceCenter = new Vector2(sourcePosition.x, sourcePosition.y);
            Vector2 targetCenter = new Vector2(targetPosition.x, targetPosition.y);
            Vector2 centerPoint = Vector2.Lerp(sourceCenter, targetCenter, 0.5f);

            container.transform.position = centerPoint;
        }
    }
}

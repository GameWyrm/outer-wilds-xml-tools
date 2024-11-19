using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class ArrowManipulator
{
    public VisualElement sourceNode;
    public VisualElement targetNode;
    public Image arrow;
    public Image line;

    /// <summary>
    /// 0 - normal
    /// 1 - opening the GUI
    /// 2 - resetting the nodes
    /// </summary>
    /// <param name="state"></param>
    public void OrientArrow(int state)
    {
        VisualElement panRoot = sourceNode.parent;
        Vector2 sourcePosition = panRoot.LocalToWorld(sourceNode.transform.position);
        Vector2 targetPosition = panRoot.LocalToWorld(targetNode.transform.position);
        // set scale
        float lineLength = Vector2.Distance(sourcePosition, targetPosition);

        line.transform.scale = new Vector3(1, lineLength, 1);

        // set angle
        float angle = Vector2.SignedAngle(Vector2.up, targetPosition - sourcePosition);
                
        line.transform.rotation = Quaternion.Euler(0, 0, angle);
        arrow.transform.rotation = Quaternion.Euler(0, 0, angle + 180);

        // set position
        int offset = 0;
        switch (state)
        {
            case 0:
                offset = 25;
                break;
            case 1:
                offset = 25;
                break;
            case 2:
                offset = 0;
                break;
        }

        Vector2 sourceCenter = new Vector2(sourcePosition.x + 100, sourcePosition.y + offset);
        Vector2 targetCenter = new Vector2(targetPosition.x + 100, targetPosition.y + offset);
        Vector2 centerPoint = Vector2.Lerp(sourceCenter, targetCenter, 0.5f);

        Vector2 arrowOffset = arrow.LocalToWorld(new Vector2(16, 32)) - (Vector2)arrow.transform.position;
        Vector2 lineOffset = line.LocalToWorld(new Vector2(16, 0)) - (Vector2)line.transform.position;

        arrow.transform.position = centerPoint - arrowOffset;
        line.transform.position = sourceCenter - lineOffset;
    }
}
using UnityEngine;
using UnityEngine.UIElements;

public class ArrowManipulator
{
    public VisualElement sourceNode;
    public VisualElement targetNode;
    public Image arrow;
    public Image line;

    public void OrientArrow()
    {
        Vector2 sourceCenter = new Vector2(sourceNode.transform.position.x + 100, sourceNode.transform.position.y + 80);
        Vector2 targetCenter = new Vector2(targetNode.transform.position.x + 100, targetNode.transform.position.y + 80);

        Vector2 centerPoint = Vector2.Lerp(sourceCenter, targetCenter, 0.5f);
        float angle = Vector2.SignedAngle(Vector2.up, targetNode.transform.position - sourceNode.transform.position);
        float lineLength = Vector2.Distance(sourceNode.transform.position, targetNode.transform.position);

        Vector2 arrowOffset = arrow.LocalToWorld(new Vector2(16, 32)) - (Vector2)arrow.transform.position;
        Vector2 lineOffset = line.LocalToWorld(new Vector2(16, 0)) - (Vector2)line.transform.position;

        arrow.transform.position = centerPoint - arrowOffset;
        line.transform.position = sourceCenter - lineOffset;
        line.transform.rotation = Quaternion.Euler(0, 0, angle);
        arrow.transform.rotation = Quaternion.Euler(0, 0, angle + 180);
        line.transform.scale = new Vector3(1, lineLength, 1);
    }
}
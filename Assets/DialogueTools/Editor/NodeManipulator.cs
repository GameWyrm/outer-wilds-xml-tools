using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeManipulator
{
    public NodeManipulator(VisualElement target)
    {
        this.target = target;
        root = target.parent;
    }

    public List<ArrowManipulator> arrows;

    private VisualElement target;
    private VisualElement root;
    private Vector2 targetStartPosition;
    private Vector3 pointerStartPosition;
    private bool enabled;

    // I don't have the luxery of inheriting from PointerManipulator so I have to call these manually.
    public void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(OnPointerDown);
        target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        target.RegisterCallback<PointerUpEvent>(OnPointerUp);
        target.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
    }

    public void UnregisterCallbacksOnTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
        target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        target.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
    }

    private void OnPointerDown(PointerDownEvent e)
    {
        if (!DialogueEditor.isFocused) return;
        targetStartPosition = target.transform.position;
        pointerStartPosition = e.position;
        target.CapturePointer(e.pointerId);
        DialogueEditor.instance.SelectNode(target);
        enabled = true;
    }

    private void OnPointerMove(PointerMoveEvent e)
    {
        if (enabled && target.HasPointerCapture(e.pointerId))
        {
            Vector3 pointerDelta = e.position - pointerStartPosition;

            target.transform.position = new Vector2(
                Mathf.Clamp(targetStartPosition.x + pointerDelta.x, 0, target.panel.visualTree.worldBound.width),
                Mathf.Clamp(targetStartPosition.y + pointerDelta.y, 0, target.panel.visualTree.worldBound.height));

            foreach (var arrow in arrows)
            {
                arrow.OrientArrow();
            }
        }
    }

    private void OnPointerUp(PointerUpEvent e)
    {
        if (enabled && target.HasPointerCapture(e.pointerId))
        {
            target.ReleasePointer(e.pointerId);
        }
    }

    private void OnPointerCaptureOut(PointerCaptureOutEvent e)
    {
        if (enabled)
        {
            enabled = false;
        }
    }
}

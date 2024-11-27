using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace XmlTools
{
    public class NodeManipulator
    {
        public NodeManipulator(VisualElement target)
        {
            this.target = target;
            root = target.parent;
        }

        public List<ArrowManipulator> arrows;
        public NodeWindow window;

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
            if (!window.isFocused) return;
            targetStartPosition = target.transform.position;
            pointerStartPosition = e.position;
            target.CapturePointer(e.pointerId);
            window.SelectNode(target);
            enabled = true;
        }

        private void OnPointerMove(PointerMoveEvent e)
        {
            if (enabled && target.HasPointerCapture(e.pointerId))
            {
                Vector3 pointerDelta = (e.position - pointerStartPosition) / window.zoom;

                target.transform.position = new Vector2(targetStartPosition.x + pointerDelta.x, targetStartPosition.y + pointerDelta.y);

                foreach (var arrow in arrows)
                {
                    arrow.OrientArrow();
                }

                window.MoveNode(target, target.transform.position);
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
}

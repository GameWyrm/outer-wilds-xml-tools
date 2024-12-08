using UnityEngine;
using UnityEngine.UIElements;

namespace XmlTools
{
    public class PannerManipulator
    {
        public VisualElement background;
        public VisualElement panRoot;
        public NodeWindow window;

        private bool enabled;
        private Vector2 panRootStartPosition;
        private Vector3 pointerStartPosition;

        // I don't have the luxery of inheriting from PointerManipulator so I have to call these manually.
        public void RegisterCallbacks()
        {
            background.RegisterCallback<PointerDownEvent>(OnPointerDown);
            background.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            background.RegisterCallback<PointerUpEvent>(OnPointerUp);
            background.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }

        public void UnregisterCallbacks()
        {
            background.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            background.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            background.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            background.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }

        private void OnPointerDown(PointerDownEvent e)
        {
            if (!window.isFocused) return;
            panRootStartPosition = panRoot.transform.position;
            pointerStartPosition = e.position;
            background.CapturePointer(e.pointerId);
            enabled = true;
            window.OnPan(panRoot.transform.position);
        }

        private void OnPointerMove(PointerMoveEvent e)
        {
            if (enabled && background.HasPointerCapture(e.pointerId))
            {
                Vector3 pointerDelta = e.position - pointerStartPosition;

                panRoot.transform.position = panRootStartPosition + ((Vector2)pointerDelta / window.zoom);
                window.OnPan(panRoot.transform.position);
            }
        }

        private void OnPointerUp(PointerUpEvent e)
        {
            if (enabled && background.HasPointerCapture(e.pointerId))
            {
                background.ReleasePointer(e.pointerId);
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

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

public class DialogueEditor : EditorWindow
{
    public static DialogueEditor Instance
    {
        get 
        {
            if (instance == null)
            {
                instance = (DialogueEditor)GetWindow(typeof(DialogueEditor));
            }
            return instance;
        }
    }

    private static DialogueEditor instance;

    private DialogueEditorSettings settings;
    private float nodePosX = 0;
    private float nodePosY = 0;

    [MenuItem("Tools/Dialogue Editor")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(DialogueEditor));
    }

    private void Awake()
    {
        settings = DialogueEditorSettings.instance;
    }


    private void OnGUI()
    {
        GUILayout.BeginHorizontal(GUILayout.Height(30), GUILayout.Width(500));
        if (GUILayout.Button("Import...")) OnClickImport();
        if (GUILayout.Button("Export...")) OnClickExport();
        if (GUILayout.Button("New Dialogue Tree...")) OnClickNewTree();
        if (GUILayout.Button("New Dialogue Node")) OnClickNewNode();
        if (GUILayout.Button("Reset node"))
        {
            nodePosX = 0;
            nodePosY = 0;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginArea(new Rect(0, 0, 1000, 1000));
        Rect nodeRect = new Rect(20, 20, 150, 100);
        nodeRect = GUI.Window(0, nodeRect, DrawNode, "Sample Node");
        GUILayout.EndArea();

        //Rect nodeRect = new Rect(nodePosX, nodePosY, 150, 100);
        //Vector2 centerOffset = nodeRect.center - new Vector2(nodeRect.x, nodeRect.y);
        //if (GUI.RepeatButton(nodeRect, "This is a node mockup."))
        //{
        //    DragNode(centerOffset);
        //    thisFrame = true;
        //}
        //else thisFrame = false;
    }

    private void DrawNode(int windowID)
    {
        GUI.Box(new Rect(0, 0, 150, 100), settings.UnselectedTexture);
        GUI.DragWindow(new Rect(0, 0, 10000, 1000));
    }

    private static void OnClickImport()
    {
        Debug.Log("Import button pressed.");
    }

    private static void OnClickExport()
    {
        Debug.Log("Export button pressed.");
    }

    private static void OnClickNewTree()
    {
        Debug.Log("New Tree button pressed.");
    }

    private static void OnClickNewNode()
    {
        Debug.Log("New Node button pressed.");
    }

    private void DragNode(Vector2 center)
    {
        nodePosX = Event.current.mousePosition.x - center.x;
        nodePosY = Event.current.mousePosition.y - center.y;
    }
}

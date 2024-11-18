using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DialogueTreeAsset))]
public class DialogueTreeEditor : Editor
{
    public static DialogueTreeAsset selectedAsset;

    public static DialogueNode activeNode;

    private static SerializedObject m_node;

    private SerializedProperty m_name;
    private SerializedProperty m_entryConditions;
    private SerializedProperty m_randomize;

    private void OnEnable()
    {
        selectedAsset = serializedObject.targetObject as DialogueTreeAsset;
    }

    public static void SelectionUpdate(DialogueNode node)
    {
        activeNode = node;
        //m_node = new SerializedObject(activeNode);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        if (activeNode == null)
        {
            // When no node is selected
            if (GUILayout.Button("Open in Editor", GUILayout.ExpandWidth(true))) OpenEditor();
        }
        else
        {
            //GetNodeData();
            //DrawNodeData();
        }
    }

    private void GetNodeData()
    {
        m_name = GetProperty("nodeName");
        m_entryConditions = GetProperty("entryConditions");
        m_randomize = GetProperty("randomize");
    }

    private SerializedProperty GetProperty(string name)
    {
        return m_node.FindProperty(name);
    }

    private void DrawNodeData()
    {
        EditorGUILayout.PropertyField(m_name, new GUIContent("Name"));
        EditorGUILayout.PropertyField(m_entryConditions);
        EditorGUILayout.PropertyField(m_randomize);
    }

    private void OpenEditor()
    {
        selectedAsset = serializedObject.targetObject as DialogueTreeAsset;
        if (selectedAsset != null)
        {
            DialogueEditor.OpenWindowWithSelection(selectedAsset);
        }
        else
        {
            Debug.LogError($"{serializedObject.targetObject.name} is still null for some reason.");
        }
    }
}

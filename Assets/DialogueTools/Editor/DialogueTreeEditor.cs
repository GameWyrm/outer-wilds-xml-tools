using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DialogueTreeAsset))]
public class DialogueTreeEditor : Editor
{
    private DialogueTreeAsset selectedAsset;

    private void OnEnable()
    {
        selectedAsset = serializedObject.targetObject as DialogueTreeAsset;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        if (GUILayout.Button("Open in Editor", GUILayout.ExpandWidth(true))) OpenEditor();
    }

    private void OpenEditor()
    {
        DialogueEditor.ShowExample();
    }
}

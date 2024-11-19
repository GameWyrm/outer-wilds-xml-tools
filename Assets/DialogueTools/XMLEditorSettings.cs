using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "XML Editor Settings", menuName = "Tools/XML Editor Settings")]
public class XMLEditorSettings : ScriptableObject
{
    public static XMLEditorSettings Instance
    {
        get
        {
            if (instance == null)
            {
                instance = AssetDatabase.LoadAssetAtPath<XMLEditorSettings>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:XMLEditorSettings")[0]));
            }
            return instance;
        }
    }

    private static XMLEditorSettings instance;

    private List<string> loopConditions;
    private List<string> persistentConditions;


    // First string is the condition name, second string is the user name, int is number of times it is used
    private Dictionary<string, Dictionary<string, int>> loopConditionUsers;
    private Dictionary<string, Dictionary<string, int>> persistentConditionUsers;

    public void RegisterCondition(string conditionName, bool isPersistent)
    {
        if (loopConditions == null) loopConditions = new List<string>();
        if (persistentConditions == null) persistentConditions = new List<string>();

        if (isPersistent)
        {
            if (!persistentConditions.Contains(conditionName))
            {
                persistentConditions.Add(conditionName);
            }
        }
        else
        {
            if (!loopConditions.Contains(conditionName))
            {
                loopConditions.Add(conditionName);
            }
        }
    }

    public void RemoveCondition(string conditionName, bool isPersistent)
    {
        if (loopConditions == null) loopConditions = new List<string>();
        if (persistentConditions == null) persistentConditions = new List<string>();

        if (isPersistent)
        {
            if (persistentConditions.Contains(conditionName))
            {
                persistentConditions.Remove(conditionName);
                persistentConditionUsers?.Remove(conditionName);
            }
        }
        else
        {
            if (loopConditions.Contains(conditionName))
            {
                loopConditions.Remove(conditionName);
                loopConditionUsers?.Remove(conditionName);
            }
        }
    }

    public List<string> GetConditionList(bool isPersistent)
    {
        if (loopConditions == null || persistentConditions == null) return null;

        List<string> list;
        if (isPersistent)
        {
            list = new List<string>(persistentConditions);
        }
        else
        {
            list = new List<string>(loopConditions);
        }
        return list;
    }

}
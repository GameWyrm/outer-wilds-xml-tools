using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

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

    public string defaultLanguage;
    public List<Language> supportedLanguages;
    public int selectedLanguage = 0;

    [Tooltip("The prefix that will be automatically added to all your conditions and ship log entries.")]
    public string modPrefix = "";
    [Tooltip("Casing to use for new conditions")]
    public Casing conditionCase = Casing.SCREAMING_SNAKE_CASE;
    [Tooltip("Casing to use for new ship log entries")]
    public Casing shipLogCase = Casing.SCREAMING_SNAKE_CASE;
    [Tooltip("Casing to use for new translation keys")]
    public Casing translationCase = Casing.PascalCase;

    private static XMLEditorSettings instance;

    [SerializeField]
    private List<string> loopConditions = new List<string>();
    [SerializeField]
    private List<string> persistentConditions = new List<string>();

    // First string is the condition name, second string is the user name, int is number of times it is used
    private Dictionary<string, Dictionary<string, int>> loopConditionUsers;
    private Dictionary<string, Dictionary<string, int>> persistentConditionUsers;


    private void Awake()
    {
        loopConditions = new List<string>();
        persistentConditions = new List<string>();
    }

    public Language GetSelectedLanguage()
    {
        return supportedLanguages[selectedLanguage];
    }

    public void RegisterCondition(string conditionName, bool isPersistent)
    {
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

    /// <summary>
    /// Overwrite an entire conditions list. You probably want to use RegisterCondition or RemoveCondition instead.
    /// </summary>
    /// <param name="conditions"></param>
    /// <param name="isPersistent"></param>
    public void SetConditionList(List<string> conditions, bool isPersistent)
    {
        if (isPersistent)
        {
            persistentConditions = conditions;
        }
        else
        {
            loopConditions = conditions;
        }
    }
}
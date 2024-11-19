using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// Do not touch this file unless you know what you are doing
//[CreateAssetMenu(fileName = "Dialogue Editor Settings", menuName = "Tools/Dialogue Editor Settings", order = 99)]
public class DialogueEditorSettings : ScriptableObject
{
    public static DialogueEditorSettings Instance
    {
        get
        {
            if (instance == null)
            {
                instance = AssetDatabase.LoadAssetAtPath<DialogueEditorSettings>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:DialogueEditorSettings")[0]));
            }
            return instance;
        }
    }

    private static DialogueEditorSettings instance;

    public Texture UnselectedTexture;
    public Texture SelectedTexture;
    public Texture LineTexture;
    public Texture ArrowTexture;
    public StyleSheet Style;
    public VisualTreeAsset VisualTree;
}
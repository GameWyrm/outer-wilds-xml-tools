using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// Do not touch this file unless you know what you are doing
[CreateAssetMenu(fileName = "Editor References", menuName = "Tools/Editor References", order = 99)]
public class EditorReferences : ScriptableObject
{
    public static EditorReferences Instance
    {
        get
        {
            if (instance == null)
            {
                instance = AssetDatabase.LoadAssetAtPath<EditorReferences>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:EditorReferences")[0]));
            }
            return instance;
        }
    }

    private static EditorReferences instance;

    public Texture UnselectedTexture;
    public Texture SelectedTexture;
    public Texture LineTexture;
    public Texture ArrowTexture;
    public Texture NoPhotoTexture;
    public StyleSheet DialogueStyle;
    public StyleSheet ShipLogStyle;
    public VisualTreeAsset DialogueVisualTree;
    public VisualTreeAsset ShipLogVisualTree;
}
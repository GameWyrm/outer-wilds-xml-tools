using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue Editor Settings", menuName = "Tools/Dialogue Editor Settings", order = 99)]
public class DialogueEditorSettings : ScriptableObject
{
    public static DialogueEditorSettings instance;

    public Texture UnselectedTexture;
    public Texture SelectedTexture;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Dialogue Editor Settings should be unique! Delete extra instances of this object.");
        }
    }
}
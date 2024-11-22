using UnityEngine;
using UnityEditor;

public class TextDisplay : EditorWindow
{
    public static void ShowWindow(string title = "JSON Display")
    {
        var window = new TextDisplay();
        window.titleContent = new GUIContent(title);
        window.Show();
    }

    // TODO get JSON strings from language files
}

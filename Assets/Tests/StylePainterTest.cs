#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

static class StylePainterTest
{
    [MenuItem("Tests/Toggle Experimental StylePainter _F3")]
    static void ToggleStylePainter()
    {
        var state = SessionState.GetBool("Experimental StylePainter", false);
        SessionState.SetBool("Experimental StylePainter", !state);
        SceneView.lastActiveSceneView.ShowNotification(new GUIContent($"{(state ? "StylePainter" : "_OptimizedStylePainter")}"));
    }
}
#endif
#if IGNORE_ACCESS_CHECKS // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
#if UNITY_EDITOR
using System;
using AV.Bridge._EditorWindows;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditorInternal;
using Object = UnityEngine.Object;

namespace AV.Bridge
{
    public static class _EditorUI
    {
        public static readonly string ProjectPath;
        public static readonly string ProjectAppDataPath;
        public static readonly string UserAppDataFolder = InternalEditorUtility.userAppDataFolder;
        
        static _EditorUI()
        {
            ProjectPath = Application.dataPath;
            ProjectPath = ProjectPath.Remove(ProjectPath.Length - 6, 6);
            ProjectAppDataPath = Application.persistentDataPath;
        }
        
        public static void GetCommonDarkStyleSheet() => UIElementsEditorUtility.GetCommonDarkStyleSheet();
        public static void GetCommonLightStyleSheet() => UIElementsEditorUtility.GetCommonLightStyleSheet();
        public static void AddDefaultStyleSheets(VisualElement ve) => UIElementsEditorUtility.AddDefaultEditorStyleSheets(ve);
        
        
        public static void MarkHotRegion(Rect rect) => MarkHotRegion(GUIView.current, rect);
        public static void MarkHotRegion(Object guiView, Rect rect)
        {
            GUIClip.UnclipToWindow_Rect_Injected(ref rect, out var hotRect);
            ((GUIView)guiView).MarkHotRegion_Injected(ref hotRect);
        }
    }
}
#endif
#endif // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
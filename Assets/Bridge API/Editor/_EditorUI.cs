#if IGNORE_ACCESS_CHECKS // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
#if UNITY_EDITOR
using System;
using AV.Bridge._EditorWindows;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Object = UnityEngine.Object;

namespace AV.Bridge
{
    public class _EditorUI
    {
        public static void GetCommonDarkStyleSheet() => UIElementsEditorUtility.GetCommonDarkStyleSheet();
        public static void GetCommonLightStyleSheet() => UIElementsEditorUtility.GetCommonLightStyleSheet();
        public static void AddDefaultStyleSheets(VisualElement ve) => UIElementsEditorUtility.AddDefaultEditorStyleSheets(ve);
        
        
        public static void MarkHotRegion(Rect rect)
        {
            new _GUIView(GUIView.current).MarkHotRegion(rect);
        }
    }
}
#endif
#endif // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
#if IGNORE_ACCESS_CHECKS // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.

using AV.Bridge._UIElements;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
using AV.Bridge._EditorWindows;
using AV.Bridge._UnityEditor;
#endif

namespace AV.Bridge
{
    public enum _ActionInject { Before, Override, After } // used for delegate subscriptions
    
    public static partial class _BridgeAPI_Extensions
    {
        public static _VisualElement _(this VisualElement x) => new _VisualElement(x);
        
        #if UNITY_EDITOR
        public static _Editor _(this Editor x) => new _Editor(x);
        public static _EditorWindow _(this EditorWindow x) => new _EditorWindow(x);
        public static _GenericMenu _(this GenericMenu x) => new _GenericMenu(x);
        
        
        [InitializeOnLoadMethod] 
        static void OnLoad()
        {
            SetExperimentalStylePainter();
            EditorApplication.update += SetExperimentalStylePainter;
        }
        static void SetExperimentalStylePainter()
        {
            if (SessionState.GetBool("Experimental StylePainter", false))
                GUIStyle.onDraw = _OptimizedStylePainter.DrawStyle;
            else 
                GUIStyle.onDraw = UnityEditor.StyleSheets.StylePainter.DrawStyle;
        } 
        #endif
    }
}
#endif // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
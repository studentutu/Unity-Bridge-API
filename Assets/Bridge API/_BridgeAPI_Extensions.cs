#if IGNORE_ACCESS_CHECKS // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.

using AV.Bridge._UIElements;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
using AV.Bridge._EditorWindows;
#endif

namespace AV.Bridge
{
    public static class _BridgeAPI_Extensions
    {
        public static _VisualElement _(this VisualElement x) => new _VisualElement(x);
        
        #if UNITY_EDITOR
        public static _Editor _(this Editor x) => new _Editor(x);
        public static _EditorWindow _(this EditorWindow x) => new _EditorWindow(x);
        #endif
    }
}
#endif // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
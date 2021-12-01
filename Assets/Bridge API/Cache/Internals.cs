#if IGNORE_ACCESS_CHECKS // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
using System;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
#endif
using UnityEngine;
using UnityEngine.UIElements;

namespace AV.Bridge 
{
    public static class Internals
    {
        #if UNITY_EDITOR
        public static readonly EditorTypes editorTypes = new EditorTypes();
        
        static Type EditorType(string typeName) => Asm.Editor.GetType("UnityEditor." + typeName);
        #endif
        
        public static class Asm
        {
            public static readonly Assembly Engine = typeof(GameObject).Assembly;
            public static readonly Assembly UnityUI = typeof(UnityEngine.UI.Button).Assembly;
            public static readonly Assembly UIElementsEngine = typeof(VisualElement).Assembly;
            
            #if UNITY_EDITOR
            public static readonly Assembly Editor = typeof(Editor).Assembly;
            public static readonly Assembly UIElementsEditor = typeof(InspectorElement).Assembly;
            public static readonly Assembly ShaderGraphEditor = typeof(UnityEditor.ShaderGraph.Drawing.MaterialGraphEditWindow).Assembly;
            #endif
        }
        
        #if UNITY_EDITOR
        public class EditorTypes
        {
            internal EditorTypes() {}

            public readonly Type GUIView = typeof(GUIView);
            public readonly Type DockArea = typeof(DockArea);
            public readonly Type SplitView = typeof(SplitView);
            public readonly Type HostView = typeof(HostView);
            public readonly Type MaximizedHostView = typeof(MaximizedHostView);

            public readonly Type MainToolbar = EditorType("Toolbar") ?? EditorType("UnityMainToolbar");

            public readonly Type AppStatusBar = typeof(AppStatusBar);
            public readonly Type ProjectBrowser = typeof(ProjectBrowser);
            public readonly Type PropertyEditor = typeof(PropertyEditor);
            public readonly Type InspectorWindow = typeof(InspectorWindow);
            public readonly Type GameView = typeof(GameView);
            public readonly Type SceneView = typeof(SceneView);
            
            public readonly Type EditorElement = typeof(EditorElement);

            public readonly Type ShaderGraphWindow = typeof(UnityEditor.ShaderGraph.Drawing.MaterialGraphEditWindow);
            //public readonly Type ShaderGraphWindow = Type("Unity.ShaderGraph.Editor", "UnityEditor.ShaderGraph.Drawing.MaterialGraphEditWindow");
            
            public readonly Type SceneHierarchy = typeof(SceneHierarchy);
            public readonly Type SceneHierarchyWindow = typeof(SceneHierarchyWindow);
            public readonly Type TreeViewController = typeof(TreeViewController);
            public readonly Type TreeViewGUI = typeof(GameObjectTreeViewDataSource);
            public readonly Type TreeViewDataSource = typeof(GameObjectTreeViewDataSource);
            public readonly Type GameObjectTreeViewGUI = typeof(GameObjectTreeViewGUI);
            public readonly Type GameObjectTreeViewDataSource = typeof(GameObjectTreeViewDataSource);
        }
        #endif
    }
}

#endif // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
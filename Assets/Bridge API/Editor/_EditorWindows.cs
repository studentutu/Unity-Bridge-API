#if IGNORE_ACCESS_CHECKS // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AV.Bridge._EditorWindows
{
    public enum _ShowMode { NormalWindow, PopupMenu, Utility, NoShadow, MainWindow, AuxWindow, Tooltip, ModalUtility, }
    public enum _PopupLocation { Below, BelowAlignLeft = Below, Above, AboveAlignLeft = Above, Left, Right, Overlay, BelowAlignRight, AboveAlignRight }
    
    public struct _EditorElement
    {
        public object obj => x; EditorElement x;
        public _EditorElement(VisualElement editorElement) => this.x = (EditorElement)editorElement;
        
        public Editor editor => x.editor;
        public int editorIndex => x.m_EditorIndex;
        public bool wasVisible => x.m_WasVisible;
        public InspectorElement inspectorElement => x.m_InspectorElement;
    }
    
    public struct _EditorWindow
    {
        public EditorWindow obj => x; EditorWindow x;
        public _EditorWindow(EditorWindow window) => this.x = window;
        
        public _HostView hostView => new _HostView(x.m_Parent);
        public _GUIView guiView => new _GUIView(x.m_Parent);
        public _View view => new _View(x.m_Parent);
        public _DockArea dockArea => new _DockArea(x.m_Parent);
        public _SplitView splitView => new _SplitView((SplitView)x.m_Parent.parent);
        
        public bool docked => x.docked;

        public void Reflow() => x.m_Parent.Reflow();
        public void ReflowAll() => SplitView.RecalcMinMaxAndReflowAll(splitView.x);
        

        SplitView GetRootSplitView()
        {
            var root = splitView.x; while (root.parent is SplitView next) { root = next; } return root;
        }
        
        public void ShowAsDropDown(Rect buttonRect, Vector2 windowSize, _ShowMode mode, bool giveFocus, params _PopupLocation[] locationPriorityOrder)
        {
            var array = locationPriorityOrder.Select(x => (PopupLocation)x).ToArray();
            x.ShowAsDropDown(buttonRect, windowSize, array, (ShowMode)mode, giveFocus);
        }
        
        /*  // Trying to resize docked editor window..
            // One clumsy way i see is to drag each border of a window at the same time, but couldn't get this to work..
        void SplitZoom(float delta)
        {
            var mainRect = view.position;
            
            foreach (var v in GetRootSplitView().allChildren)
            {
                if (!(v is SplitView split)) 
                    continue;
                
                //split.SetupSplitter();
                var state = split.splitState;
            
                var children = split.children;
                if (children.Length == 1) 
                    continue;
                    
                var vertical = split.vertical;
                var first = children[0];
                var cursor = vertical ? first.position.y : first.position.x;
                    
                for (int i = 0; i < children.Length - 1; i++) 
                {
                    var splitRect = vertical ?
                        new Rect(first.position.x, cursor + state.realSizes[i] - state.splitSize / 2, first.position.width, state.splitSize) :
                        new Rect(cursor + state.realSizes[i] - state.splitSize / 2, first.position.y, state.splitSize, first.position.height);
    
                    cursor += state.realSizes[i];
                        
                    var isTop = splitRect.Contains(new Vector2(mainRect.center.x, mainRect.yMin));
                    var isLeft = splitRect.Contains(new Vector2(mainRect.xMin, mainRect.center.y));
                    var isRight = splitRect.Contains(new Vector2(mainRect.xMax, mainRect.center.y));
                    var isBottom = splitRect.Contains(new Vector2(mainRect.center.x, mainRect.yMax));
    
                    if (!isTop && !isLeft && !isRight && !isBottom)
                        continue;
    
                    Debug.Log($"{children[i]} t:{isTop} l:{isLeft} r:{isRight} b:{isBottom}");
                        
                    state.currentActiveSplitter = i;
                    state.DoSplitter(state.currentActiveSplitter, state.currentActiveSplitter + 1, delta);
                
                    split.SetupRectsFromSplitter();
                }
            }
        }*/
    }
    
    public struct _InspectorWindow
    {
        public object obj => x; PropertyEditor x;
        public _InspectorWindow(EditorWindow propertyEditor) => x = (PropertyEditor)propertyEditor;
        
        public InspectorMode inspectorMode { get => x.inspectorMode; set => x.inspectorMode = value; }
        public ActiveEditorTracker tracker => x.tracker;
    }
    
    public struct _HostView
    {
        public object obj => x; HostView x;
        public _HostView(object hostView) => x = (HostView)hostView;
        
        public EditorWindow actualView { get => x.actualView; set => x.actualView = value; }
        
        public static void OnActualViewChanged(Action<_HostView> action)
        {
            HostView.actualViewChanged += view => action.Invoke(new _HostView(view));
        }
        
        public Action m_Update { set => x.m_Update = WindowDelegate(value); }
        public Action m_OnGUI { set => x.m_OnGUI = WindowDelegate(value); }
        public Action m_OnFocus { set => x.m_OnFocus = WindowDelegate(value); }
        public Action m_OnLostFocus { set => x.m_OnLostFocus = WindowDelegate(value); }
        public Action m_OnProjectChange { set => x.m_OnProjectChange = WindowDelegate(value); }
        public Action m_OnSelectionChange { set => x.m_OnSelectionChange = WindowDelegate(value); }
        public Action m_OnDidOpenScene { set => x.m_OnDidOpenScene = WindowDelegate(value); }
        public Action m_OnInspectorUpdate { set => x.m_OnInspectorUpdate = WindowDelegate(value); }
        public Action m_OnHierarchyChange { set => x.m_OnHierarchyChange = WindowDelegate(value); }
        public Action m_OnBecameVisible { set => x.m_OnBecameVisible = WindowDelegate(value); }
        public Action m_OnBecameInvisible { set => x.m_OnBecameInvisible = WindowDelegate(value); }
        public Action m_ModifierKeysChanged { set => x.m_ModifierKeysChanged = WindowDelegate(value); }
        
        public float GetExtraButtonsWidth() => x.GetExtraButtonsWidth();
        
        
        static T ConvertDelegate<T>(Delegate src) where T : Delegate {
            return (T)Delegate.CreateDelegate(typeof(T), src.Target, src.Method);
        }
        static HostView.EditorWindowDelegate WindowDelegate(Action action) => ConvertDelegate<HostView.EditorWindowDelegate>(action);
    }
    
    public struct _GUIView
    {
        public object obj => x; GUIView x;
        public _GUIView(object guiView) => x = (GUIView)guiView;
        
        public VisualElement visualTree => x.windowBackend.visualTree as VisualElement;
        
        public void MarkHotRegion(Rect rect) => x.MarkHotRegion(GUIClip.UnclipToWindow(rect));
    }
    
    public struct _ContainerWindow
    {
        public object obj => x; ContainerWindow x;
        public _ContainerWindow(object view) => x = (ContainerWindow)view;
        
        public _View rootView { get => new _View(x.rootView); set => x.rootView = (View)value.obj; }
        public _SplitView rootSplitView => new _SplitView(x.rootSplitView);
    }
    
    public struct _DockArea
    {
        public object obj => x; DockArea x;
        public _DockArea(object dockArea) => x = (DockArea)dockArea;
    }
    
    public struct _SplitView
    {
        public object obj => x; SplitView x;
        public _SplitView(object view) => x = (SplitView)view;
    }
    
    public struct _View
    {
        public object obj => x; View x;
        public _View(object view) => x = (View)view;
        
        public Rect position { get => x.position; set => x.position = value; }
        public Vector2 size { get => x.position.size; set => x.position = new Rect(x.position.position, value); } 
        
        public Vector2 minSize { get => x.minSize; set => SetMinMaxSizes(value, maxSize); } 
        public Vector2 maxSize { get => x.minSize; set => SetMinMaxSizes(minSize, value); }
        
        public _ShowMode showMode { get => (_ShowMode)x.window.showMode; set => x.window.m_ShowMode = (int)value; }

        /// <summary> ContainerWindow </summary>
        public object window => x.window;
        
        public void SetMinMaxSizes(Vector2 min, Vector2 max) => x.SetMinMaxSizes(min, max);
        
        public _View parent => new _View(x.m_Parent);
        public _View Children(int idx) => new _View(x.children[idx]);
        public void Reflow() => x.Reflow();
    }
    
    public struct _Editor
    {
        public Editor obj => x; Editor x;
        public _Editor(Editor editor) => x = editor;
        
        public bool hideInspector { get => x.hideInspector; set => x.hideInspector = value; }
        
        public void CleanupPropertyEditor() => x.CleanupPropertyEditor();
    }
}

#endif
#endif // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
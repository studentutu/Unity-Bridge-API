#if IGNORE_ACCESS_CHECKS // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace AV.Bridge._EditorHierarchy
{
    public struct _GameObjectTreeViewItem
    {
        public TreeViewItem obj => x; GameObjectTreeViewItem x;
        public _GameObjectTreeViewItem(TreeViewItem item) => this.x = item as GameObjectTreeViewItem;
        
        public string displayName { get => x.displayName; set => x.displayName = value; }
        public bool isSceneHeader { get => x.isSceneHeader; set => x.isSceneHeader = value; }
        
        public ref Scene scene => ref x.m_UnityScene;
        public ref int colorCode => ref x.m_ColorCode;
        public ref bool lazyInitializationDone => ref x.m_LazyInitializationDone;
        public ref bool showPrefabModeButton => ref x.m_ShowPrefabModeButton;
        public ref Object objectPPTR => ref x.m_ObjectPPTR;
        public ref Texture2D overlayIcon => ref x.m_OverlayIcon;
        public ref Texture2D selectedIcon => ref x.m_SelectedIcon;
    }
    
    public struct _SceneHierarchy
    {
        public object obj => x; SceneHierarchy x;
        public _SceneHierarchy(object sceneHierarchy) => this.x = (SceneHierarchy)sceneHierarchy;
        
        public _TreeViewController treeView => new _TreeViewController(x.treeView);
    } 
    
    public struct _TreeViewController
    {
        public object obj => x; TreeViewController x;
        public _TreeViewController(object sceneHierarchy) => this.x = (TreeViewController)sceneHierarchy;
        
        public TreeViewState state => x.state;
        public _TreeViewGUI gui => new _TreeViewGUI(x.gui);
        public _TreeViewDataSource data => new _TreeViewDataSource(x.data);
        
        public Action<int[]> selectionChanged { get => x.selectionChangedCallback; set => x.selectionChangedCallback = value; }
        
        public TreeViewItem hoveredItem => x.hoveredItem;
        
        public void Frame(int id, bool frame = true, bool ping = false, bool animated = true) =>  x.Frame(id, true, false, true);
    }
    
    public struct _TreeViewGUI
    {
        public object obj => x; TreeViewGUI x;
        public _TreeViewGUI(object gui) => this.x = (TreeViewGUI)gui;
        
        public ref float lineHeight => ref x.m_LineHeight;
        public ref float indentWidth => ref x.k_IndentWidth;
        public ref float baseIndent => ref x.k_BaseIndent;
        
        public void OnRowGUI(Rect rowRect, TreeViewItem item, int row, bool selected, bool focused)
        {
            x.OnRowGUI(rowRect, item, row, selected, focused);
        }
    }
    
    public struct _TreeViewDataSource
    {
        public object obj => x; TreeViewDataSource x;
        public _TreeViewDataSource(object data) => this.x = (TreeViewDataSource)data;
    }
}
#endif
#endif // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
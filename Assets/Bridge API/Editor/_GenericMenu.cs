using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if IGNORE_ACCESS_CHECKS // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
namespace AV.Bridge._UnityEditor
{
    public struct _GenericMenu
    {
        public GenericMenu obj => x; GenericMenu x; 
        public _GenericMenu(GenericMenu menu) => this.x = menu;
        
        public struct _MenuItem
        {
            public object obj => x; GenericMenu.MenuItem x;
            public _MenuItem(object item) => this.x = item as GenericMenu.MenuItem;
            
            public GUIContent content { get => x.content; set => x.content = value; }
            public bool separator { get => x.separator; set => x.separator = value; }
            public bool on { get => x.on; set => x.on = value; }
            public GenericMenu.MenuFunction func { get => x.func; set => x.func = value; }
            public GenericMenu.MenuFunction2 func2  { get => x.func2; set => x.func2 = value; }
            public object userData { get => x.userData; set => x.userData = value; }
        }
        
        public IList list => x.m_MenuItems;
        
        public IEnumerable<_MenuItem> Items()
        {
            var list = x.m_MenuItems;
            for (int i = list.Count - 1; i >= 0; i--) 
                yield return new _MenuItem(list[i]);
        }

        public void RemoveItem(_MenuItem item) => list.Remove(item.obj);
        public void RemoveItem(int index) => list.RemoveAt(index);
    }
}
#endif // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
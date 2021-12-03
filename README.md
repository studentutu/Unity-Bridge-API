# Unity-Bridge-API
Provides direct access to ***private*** (!!) Unity C# APIs, compiled with [Open Sesame](https://github.com/mob-sakai/OpenSesameCompilerForUnity)

Forget about Reflection or Lambda.Compile :)

![image](https://user-images.githubusercontent.com/29812914/143566401-65120fa8-2aaf-4e13-9edc-e6ac257d0969.png)

## Optimizations
[_OptimizedStylePainter](Assets/Bridge%20API/Editor/_OptimizedStylePainter.cs) is ___x2.5~ times faster___ than StylePainter (which draws all GUIStyles), 
[use F3 to enable it](Assets/Tests/StylePainterTest.cs).


## How to Build

![image](https://user-images.githubusercontent.com/29812914/143575401-cde3047e-1f8b-4a61-ad4f-d4bdadac5072.png)

Published `.dll` then can be used as a managed plugin, without OpenSesame compiler.
### For usage in other projects / packages
Rename the `AV.BridgeAPI.asmdef` in order to avoid collisions!


## API Signature Design
``` csharp

// varName_ is a recommended signature when used as a local variable in user scripts
var inspector_ = new _InspectorWindow(inspector);
var tracker = inspector_.tracker;


namespace AV.Bridge
{
    // _() extension shorthand for public classes
    public static class _BridgeAPI_Extensions
    {
        // visualElement._().layout;
        public static _VisualElement _(this VisualElement x) => new _VisualElement(x);
    }
}

namespace AV.Bridge._UIElements
{
    // _ClassName, must be struct to avoid GC alloc
    public struct _VisualElement
    {
        public VisualElement obj => x; VisualElement x; // public obj reference for later usage
        public _VisualElement(VisualElement x) => this.x = x;
    
        public Shader standardShader { get => panel.standardShader; set => panel.standardShader = value; } 
        public Rect layout { get => x.layout; set => x.layout = value; }
        public Rect worldClip => x.worldClip;
    }
    public struct _GameObjectTreeViewItem
    {
        // obj should be of assignable public type (can be an interface), or just object
        public TreeViewItem obj => x; GameObjectTreeViewItem x;
        // if it needs to be nullable, use x as T instead of (T)x
        public _GameObjectTreeViewItem(TreeViewItem item) => this.x = item as GameObjectTreeViewItem;
        
        // use ref return, in cases where field can be used directly
        public ref Scene scene => ref x.m_UnityScene;
        public ref int colorCode => ref x.m_ColorCode;
        public ref Object objectPPTR => ref x.m_ObjectPPTR;
        public ref Texture2D overlayIcon => ref x.m_OverlayIcon;
    }
}

namespace AV.Bridge
{
    // Static API
    public static class _Physics
    {
        public static bool Raycast(PhysicsScene scene, Vector3 pos, Vector3 dir, out RaycastHit hit, float maxDistance, int layerMask)
        {
            hit = default; var ray = new Ray { m_Origin = pos, m_Direction = dir };
            return PhysicsScene.Internal_Raycast_Injected(ref scene, ref ray, maxDistance, ref hit, layerMask, QueryTriggerInteraction.UseGlobal);
        }
        
        // RaycastHit extension
        public static bool TryGetCollider(this RaycastHit hit, out Collider collider)
        {
            collider = null; if (hit.m_Collider != 0)
            {
                collider = (Collider)Object.FindObjectFromInstanceID(hit.m_Collider); return true;
            } return false;
        }
    }
}
```

## UI Elements
``` csharp
// RegisterCallback<> via interface
rootVisualElement.OnCallback<IPointerEvent>(evt => {
    if (evt is PointerMoveEvent moveEvt)
        Debug.Log(moveEvt);
});

// called when any event is triggered
// similar to ExecuteDefaultAction, but does not require inheriting from class
rootVisualElement.OnAnyEvent((EventBase evt) => Debug.Log($"any event: {evt}"));

// internal event that is called on BaseVisualElementPanel
// not an EventBase, not registered per-element!
rootVisualElement.OnHierarchyChanged((element, type) => Debug.Log($"{element} {type}"));

// returns all registered callbacks
foreach (var evt in rootVisualElement._().Callbacks())
{
    if (evt.IsTypeOf<IPanelChangedEvent>()) // _EventFunctor
        evt.Unregister();
}
```


## References
[mob-sakai/OpenSesameCompilerForUnity](https://github.com/mob-sakai/OpenSesameCompilerForUnity)

[No InternalsVisibleTo, no problem – bypassing C# visibility rules with Roslyn](https://www.strathweb.com/2018/10/no-internalvisibleto-no-problem-bypassing-c-visibility-rules-with-roslyn/)

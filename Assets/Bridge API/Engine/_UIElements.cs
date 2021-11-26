#if IGNORE_ACCESS_CHECKS // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using VE = UnityEngine.UIElements.VisualElement;

namespace AV.Bridge._UIElements
{
    public enum _CallbackPhase { TargetAndBubbleUp = 1, TrickleDownAndTarget = 2, }
    [Flags] public enum _PseudoStates { Active = 1, Hover = 2, Checked = 8, Disabled = 32, Focus = 64, Root = 128, }
    
    public struct _EventCallbackFunctor
    {
        public object obj => x; EventCallbackFunctorBase x;
        internal _EventCallbackFunctor(object x) { this.x = (EventCallbackFunctorBase) x; handler = null; }
        internal VisualElement handler;
    
        public _CallbackPhase phase { get => (_CallbackPhase)x.phase; set => x.phase = (CallbackPhase)value; }
        
        //static Dictionary<Type, (FieldInfo callback)> FunctorCache = new Dictionary<Type, (FieldInfo)>();
        
        public Type eventType => x.GetType().GetGenericArguments()[0];
        public bool IsTypeOf<T>() => typeof(T).IsAssignableFrom(eventType);
        
        public void Unregister()
        {
            new _VisualElement(handler).RemoveCallback(this);
        }
    }
    
    public struct _VisualElement
    {
        public VisualElement obj => x; VisualElement x;
        public _VisualElement(VisualElement x) => this.x = x;
        
        BaseVisualElementPanel panel => (BaseVisualElementPanel)x.panel;
    
        public _PseudoStates pseudoStates { get => (_PseudoStates)x.pseudoStates; set => x.pseudoStates = (PseudoStates)value; }
        
        public Shader standardShader { get => panel.standardShader; set => panel.standardShader = value; } 
        public Shader standardWorldSpaceShader { get => panel.standardWorldSpaceShader; set => panel.standardWorldSpaceShader = value; }
        
        public Rect layout { get => x.layout; set => x.layout = value; }
        public Rect worldClip => x.worldClip;
        public Rect boundingBox => x.boundingBox;
        public Rect worldBoundingBox => x.worldBoundingBox;
        
        public IEnumerable<_EventCallbackFunctor> Callbacks()
        {
            var reg = x.m_CallbackRegistry; 
            if (reg == null) yield break;
            var list = reg.GetCallbackListForWriting();
    
            for (int i = 0; i < list.Count; i++)
            {
                var functor = list[i];
                yield return new _EventCallbackFunctor(functor) { handler = x };
            }
        } 
        public void RemoveCallback(_EventCallbackFunctor functor)
        {
            var reg = x.m_CallbackRegistry; 
            if (reg == null) return;
            var list = reg.GetCallbackListForWriting(); 
            
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == functor.obj)
                    list.m_List.RemoveAt(i);
            }
        }
        
        public void RegisterCallback<T>(EventCallback<T> callback, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown)
        {
            var registry = x.m_CallbackRegistry ?? (x.m_CallbackRegistry = new EventCallbackRegistry());
            
            if (callback == null) throw new ArgumentException("callback parameter is null");
            
            var eventTypeId = EventFunctor<T>.TypeId;
            var phase = useTrickleDown == TrickleDown.TrickleDown ? CallbackPhase.TrickleDownAndTarget : CallbackPhase.TargetAndBubbleUp;
            var callbackListForReading = registry.GetCallbackListForReading();
            if (callbackListForReading != null && callbackListForReading.Contains(eventTypeId, callback, phase))
                return;
            registry.GetCallbackListForWriting().Add(new EventFunctor<T>(callback, phase));
        }
        
        static long LastEventTypeId;
        class EventFunctor<T> : EventCallbackFunctorBase
        {
            public static readonly long TypeId = ++LastEventTypeId;
            
            readonly EventCallback<T> m_Callback;
    
            public EventFunctor(EventCallback<T> callback, CallbackPhase phase) : base(phase) { this.m_Callback = callback; }
    
            public override void Invoke(EventBase evt)
            {
                if (!(evt is T tEvt))
                    return;
                if (!this.PhaseMatches(evt))
                    return;
                using (new EventDebuggerLogCall(this.m_Callback, evt))
                    this.m_Callback(tEvt);
            }
    
            public override bool IsEquivalentTo(long eventTypeId, Delegate callback, CallbackPhase phase)
            {
                return TypeId == eventTypeId && (Delegate)m_Callback == callback && this.phase == phase;
            }
        }
    }
    
    public static unsafe partial class _Engine
    {
        public enum TreeChangeType { Add, Remove, Move, }
        public delegate void TreeEvent(VE e, TreeChangeType changeType);
        
        static Dictionary<TreeEvent, HierarchyEvent> treeEvents = new Dictionary<TreeEvent, HierarchyEvent>();
        static HierarchyEvent GetHierarchyEvent(TreeEvent evt)
        {
            if (!treeEvents.TryGetValue(evt, out var value)) 
                treeEvents.Add(evt, value = (e, t) => evt.Invoke(e, (TreeChangeType)t));
            return value;
        }
        
        //public static void OnHierarchyChanged(this IPanel panel, TreeEvent treeEvt)
        //{
        //    (panel as BaseVisualElementPanel).hierarchyChanged += GetHierarchyEvent(treeEvt);
        //}
        
        public static void OnHierarchyChanged(this VE ve, TreeEvent treeEvt)
        {
            var panel = ve.panel as BaseVisualElementPanel;
    
            if (panel != null)
                panel.hierarchyChanged += GetHierarchyEvent(treeEvt);
            
            // During window.OnEnable, rootVisualElement.panel might be null, so we need to register during IPanelChangedEvent
            ve.OnCallback<IPanelChangedEvent>(evt =>
            {
                if (evt is AttachToPanelEvent attachEvt)
                {
                    panel = attachEvt.destinationPanel as BaseVisualElementPanel;
                    panel.hierarchyChanged -= GetHierarchyEvent(treeEvt);
                    panel.hierarchyChanged += GetHierarchyEvent(treeEvt);
                }
                if (evt is DetachFromPanelEvent)
                    panel.hierarchyChanged -= GetHierarchyEvent(treeEvt);
            });
        }
        
        
        public static void OnCallback<T>(this VE e, EventCallback<T> callback, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown)
        {
            e._().RegisterCallback(callback, useTrickleDown);
        }
        public static void OnAnyEvent(this VE e, EventCallback<EventBase> evt) => e.OnCallback(evt);
        public static void OnValueChange(this VE e, EventCallback<IChangeEvent> evt) => e.OnCallback(evt);
        public static void OnValueChange<T>(this VE e, EventCallback<ChangeEvent<T>> evt) => e.RegisterCallback(evt);
        public static void OnLayoutChange(this VE e, EventCallback<GeometryChangedEvent> evt) => e.RegisterCallback(evt);
        public static void OnPanelChange(this VE e, EventCallback<IPanelChangedEvent> evt) => e.OnCallback(evt);
        public static void OnPointerEvent(this VE e, EventCallback<IPointerEvent> evt) => e.OnCallback(evt);
        public static void OnMouseEvent(this VE e, EventCallback<IMouseEvent> evt) => e.OnCallback(evt);
        public static void OnFocusEvent(this VE e, EventCallback<IFocusEvent> evt) => e.OnCallback(evt);
        public static void OnDragEvent(this VE e, EventCallback<IDragAndDropEvent> evt) => e.OnCallback(evt);
        public static void OnCommandEvent(this VE e, EventCallback<ICommandEvent> evt) => e.OnCallback(evt);
        public static void OnKeyboardEvent(this VE e, EventCallback<IKeyboardEvent> evt) => e.OnCallback(evt);
    }
}
#endif // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
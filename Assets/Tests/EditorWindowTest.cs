#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using AV.Bridge;
using AV.Bridge._UIElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class EditorWindowTest : EditorWindow
{
    [MenuItem("Tests/Editor Window Test")]
    static void ShowWindow()
    {
        GetWindow<EditorWindowTest>();
    }

    private void OnGUI()
    {
        Debug.Log("original GUI");
        //this.Internal().DrawSplits();
    }

    private void CreateGUI()
    {
    }
    

    private void OnEnable()
    {
        
        
        rootVisualElement.OnHierarchyChanged((e, type) =>
        {
            Debug.Log($"{e} {type}");
        });
        
        var butt = new Button(() =>
        {
            rootVisualElement.Add(new Button());
        });
        
        rootVisualElement.Add(butt);
        
        var slider = new Slider();

        rootVisualElement.Add(slider);
        rootVisualElement.Add(new Toggle());
        
        rootVisualElement.RegisterCallback<WheelEvent>(evt => 
        {
            var delta = -evt.delta.y * 10;
        });
        rootVisualElement.OnAnyEvent(evt =>
        {
            //Debug.Log(evt);
        });
        rootVisualElement.OnValueChange(evt =>
        {
            //Debug.Log(evt);
        });
        rootVisualElement.OnLayoutChange(evt =>
        {
            //Debug.Log(evt);
        });
        rootVisualElement.OnFocusEvent(evt =>
        {
        });

        foreach (var evt in rootVisualElement._().Callbacks())
        {
            if (evt.IsTypeOf<IPanelChangedEvent>())
                evt.Unregister();
                //rootVisualElement.Fluent().RemoveCallback(evt);
        }
        foreach (var evt in rootVisualElement._().Callbacks())
        {
            Debug.Log(evt.eventType);
        }
    }
}
#endif
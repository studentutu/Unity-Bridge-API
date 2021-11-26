using System;
using System.Collections;
using System.Collections.Generic;
using AV.Bridge;
using UnityEngine;

public class PhysicsTest : MonoBehaviour
{
    public int raycastCount;
    Transform trs;

    private void OnEnable()
    {
        trs = transform;
    }

    void Update()
    {
        var physicsScene = Physics.defaultPhysicsScene;
        
        for (int i = 0; i < raycastCount; i++)
        {
            var localPos = trs.localPosition; 
            var localRot = trs.localRotation;
            var fwd = localRot * Vector3.forward;
            
            //Physics.Raycast(localPos, fwd, out var hit, 5, -1);
            _Physics.Raycast(physicsScene, localPos, fwd, out var hit, 5, -1);
            
            if (hit.TryGetCollider(out var collider))
            {
            }
        }
    }
}

#if IGNORE_ACCESS_CHECKS // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
using System.Runtime.CompilerServices;
using UnityEngine;

namespace AV.Bridge
{
    public static class _Physics
    {
        public static bool Raycast(PhysicsScene scene, Vector3 pos, Vector3 dir, out RaycastHit hit, float maxDistance, int layerMask)
        {
            hit = default;
            var ray = new Ray { m_Origin = pos, m_Direction = dir };
                
            return PhysicsScene.Internal_Raycast_Injected(ref scene, ref ray, maxDistance, ref hit, layerMask, QueryTriggerInteraction.UseGlobal);
        }
        
        public static bool TryGetCollider(this RaycastHit hit, out Collider collider)
        {
            collider = null;
            if (hit.m_Collider != 0)
            {
                collider = (Collider)Object.FindObjectFromInstanceID(hit.m_Collider);
                return true;
            }
            return false;
        }
    }
}

#endif // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
#if IGNORE_ACCESS_CHECKS // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace AV.Bridge
{
    public static class _Object
    {
        public static Object FindByID(int instanceID) => Object.FindObjectFromInstanceID(instanceID);
        public static bool IsPersistent(Object obj) => Object.IsPersistent(obj);
        
        public static bool IsAlive(Object obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            if (obj.m_CachedPtr == IntPtr.Zero) return false;
            return true;
        }
        
        /*
        public static bool Exist(this Object obj)
        {
            var isNull = ReferenceEquals(obj, null); //((object)a) == null;
            return !isNull && IsNativeObjectAlive(obj);
        }
        
        static bool IsNativeObjectAlive(Object obj)
        {
            if (obj.m_CachedPtr != IntPtr.Zero && obj.m_InstanceID != 0) return true;
            if (obj is MonoBehaviour || obj is ScriptableObject) return false;
            return  Object.DoesObjectWithInstanceIDExist(obj.m_InstanceID);
        }*/
    }
    
    public static unsafe partial class _Engine
    {
        public static string GUID(this Scene scene) => scene.guid;
    }
}
#endif // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
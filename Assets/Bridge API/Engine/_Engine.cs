#if IGNORE_ACCESS_CHECKS // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace AV.Bridge
{
    public static unsafe partial class _Engine
    {
        public static Object FindObjectByID(int instanceID) => Object.FindObjectFromInstanceID(instanceID);
        public static bool IsObjectPersistent(Object obj) => Object.IsPersistent(obj);
        
        public static string GUID(this Scene scene) 
        {
            return scene.guid;
        }
        
        
        
        /*
        public static bool Exist(this Object obj)
        {
            var isNull = ReferenceEquals(obj, null); //((object)a) == null;
            return !isNull && IsNativeObjectAlive(obj);
        }
        
        static bool IsNativeObjectAlive(Object obj)
        {
            if (obj.m_CachedPtr != IntPtr.Zero && obj.m_InstanceID != 0)
                return true;
            if (obj is MonoBehaviour || obj is ScriptableObject)
                return false;
            return  Object.DoesObjectWithInstanceIDExist(obj.m_InstanceID);
        }*/
    }
}
#endif // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
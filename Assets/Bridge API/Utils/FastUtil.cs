#if IGNORE_ACCESS_CHECKS // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
using UnityEngine;

namespace AV.Bridge
{
    public static class FastUtil
    {
        const float kEpsilon = 0.00001F;
        
        // Manually inlined in order to avoid implicit struct conversion cost
        public static bool ColorEquals(Color a, Color b)
        {
            var x = a.r - b.r; var y = a.g - b.g; var z = a.b - b.b; var w = a.a - b.a;
            var sqrMag = x * x + y * y + z * z + w * w;
            return sqrMag < kEpsilon * kEpsilon;
        }
        public static bool VectorEquals(Vector4 a, Vector4 b)
        {
            var x = a.x - b.x; var y = a.y - b.y; var z = a.z - b.z; var w = a.x - b.x;
            var sqrMag = x * x + y * y + z * z + w * w;
            return sqrMag < kEpsilon * kEpsilon;
        }
        public static Color ColorMultiply(Color a, Color b)
        {
            a.r *= b.r; a.g *= b.g; a.b *= b.b; a.a *= b.a; return a;
        }
        public static Color ColorMultiply(Color a, float b)
        {
            a.r *= b; a.g *= b; a.b *= b; a.a *= b; return a;
        }
    }
}

#endif // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
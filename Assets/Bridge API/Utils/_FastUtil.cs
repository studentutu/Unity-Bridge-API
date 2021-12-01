#if IGNORE_ACCESS_CHECKS // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
using System.Runtime.CompilerServices;
using UnityEngine;
using Impl = System.Runtime.CompilerServices.MethodImplAttribute;

namespace AV.Bridge
{
    public static class _FastUtil
    {
        const float kEpsilon = 0.00001F;
        const float kEpsilon2 = kEpsilon * kEpsilon;
        
        // Compiler is so stupid that it cannot inline those, but oh well.. Let it be just in case
        const MethodImplOptions Inline = MethodImplOptions.AggressiveInlining;
        
            
        // Avoids property calls
        // 500 calls - 0.07~ms
        [Impl(Inline)] public static bool RectOverlaps(in Rect a, in Rect b) => 
            b.m_XMin + b.m_Width  > a.m_XMin              && 
            b.m_XMin              < a.m_XMin + a.m_Width  && 
            b.m_YMin + b.m_Height > a.m_YMin              && 
            b.m_YMin              < a.m_YMin + a.m_Height; 
        
        /// x - t, y - r, z - b, w - l
        // 100 calls - 0.02ms
        [Impl(Inline)] public static void RectPadding(ref Rect r, float top, float right, float bottom, float left) 
        {
            r.m_Width = (r.m_XMin + r.m_Width) - (r.m_XMin += left); // xMin += left
            r.m_Height = (r.m_YMin + r.m_Height) - (r.m_YMin += top); // yMin += top
            r.m_Height = (r.m_YMin + r.m_Height) + (bottom - r.m_YMin); // yMax += bottom
            r.m_Width = (r.m_XMin + r.m_Width) + (right - r.m_XMin); // xMax += right
        }
        
        [Impl(Inline)] 
        public static bool RectContains(in Rect a, Vector2 point) => 
            point.x >= a.m_XMin              && 
            point.x <  a.m_XMin + a.m_Width  && 
            point.y >= a.m_YMin              && 
            point.y <  a.m_YMin + a.m_Height;

        
        // Avoids implicit operator struct conversion cost
        [Impl(Inline)] public static bool ColorEquals(ref Color a, ref Color b)
        {
            var x = a.r - b.r; var y = a.g - b.g; var z = a.b - b.b; var w = a.a - b.a;
            var mag = x * x + y * y + z * z + w * w;
            return mag < kEpsilon2;
        }
        [Impl(Inline)] public static bool VectorEquals(ref Vector4 a, ref Vector4 b)
        {
            var x = a.x - b.x; var y = a.y - b.y; var z = a.z - b.z; var w = a.w - b.w;
            var mag = x * x + y * y + z * z + w * w;
            return mag < kEpsilon2;
        }
        
        [Impl(Inline)] public static void ColorMultiply(ref Color a, in Color b) { a.r *= b.r; a.g *= b.g; a.b *= b.b; a.a *= b.a; }
        
        [Impl(Inline)] public static void ColorMultiply(ref Color c, float r, float g, float b, float a) { c.r *= r; c.g *= g; c.b *= b; c.a *= a; }
        
        [Impl(Inline)] public static void BorderColorMultiply(ref Color T, ref Color R, ref Color B, ref Color L, in Color c)
        {
            // Could those be... Multiplied in a batch, somehow?
            T.r *= c.r; T.g *= c.g; T.b *= c.b; T.a *= c.a;
            R.r *= c.r; R.g *= c.g; R.b *= c.b; R.a *= c.a;
            B.r *= c.r; B.g *= c.g; B.b *= c.b; B.a *= c.a;
            L.r *= c.r; L.g *= c.g; L.b *= c.b; L.a *= c.a;
        }
        
        [Impl(Inline)] public static void ColorMultiply(ref Color a, float b) { a.r *= b; a.g *= b; a.b *= b; a.a *= b; }
    }
}

#endif // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
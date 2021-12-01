#if IGNORE_ACCESS_CHECKS // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace AV.Bridge
{
    public static class _ImmediateStylePainter
    {
        public static void DrawTexture(Rect screenRect, Texture texture, Rect sourceRect, Color color, Vector4 borderWidths, Vector4 borderRadiuses, 
            bool usePremultiplyAlpha = true, int leftBorder = 0, int topBorder = 0, int rightBorder = 0, int bottomBorder = 0)
        {
            ImmediateStylePainter.DrawTexture(screenRect, texture, sourceRect, color, borderWidths, borderRadiuses, leftBorder, topBorder, rightBorder, bottomBorder, usePremultiplyAlpha);
        }
        public static void DrawRect(
            Rect screenRect,
            Color color,
            Vector4 borderWidths,
            Vector4 borderRadiuses)
        {
            ImmediateStylePainter.DrawRect(screenRect, color, borderWidths, borderRadiuses);
        }
        public static void DrawText(
            Rect screenRect,
            string text,
            Font font,
            int fontSize,
            Color fontColor,
            FontStyle fontStyle = FontStyle.Normal,
            TextAnchor anchor = TextAnchor.MiddleCenter,
            bool wordWrap = false,
            float wordWrapWidth = 0,
            bool richText = false,
            TextClipping textClipping = TextClipping.Overflow)
        {
            ImmediateStylePainter.DrawText(screenRect, text, font, fontSize, fontStyle, fontColor, anchor, wordWrap, wordWrapWidth, richText, textClipping);
        }
    }
    
    public static class _GUI
    {
        static readonly Material blendMat;
        static readonly Material roundedRectMat;
        static readonly Material roundedRectColorPerBorderMat;
        
        static _GUI()
        {
            blendMat = GUI.blendMaterial;
            roundedRectMat = GUI.roundedRectMaterial;
            roundedRectColorPerBorderMat = GUI.roundedRectWithColorPerBorderMaterial;
        }
        
        public static void OnProcessEvent(Func<int, IntPtr, bool> func, _ActionInject inject)
        {
            if (inject == _ActionInject.Before) { var evt = GUIUtility.processEvent; GUIUtility.processEvent = evt; GUIUtility.processEvent += evt; }
            if (inject == _ActionInject.Override) GUIUtility.processEvent = func;
            if (inject == _ActionInject.After) GUIUtility.processEvent += func;
        }

        public static Vector2 GUIToScreenPoint(Vector2 guiPoint) => GUIUtility.InternalWindowToScreenPoint(GUIClip.UnclipToWindow(guiPoint));
        public static Rect GUIToScreenPoint(Rect guiRect)
        {
            var screenPoint = GUIToScreenPoint(new Vector2(guiRect.x, guiRect.y));
            guiRect.x = screenPoint.x;
            guiRect.y = screenPoint.y; return guiRect;
        }
        public static Vector2 ScreenToGUIPoint(Vector2 screenPoint) => GUIClip.ClipToWindow(GUIUtility.InternalScreenToWindowPoint(screenPoint));
        public static Rect ScreenToGUIPoint(Rect screenRect)
        {
            var guiPoint = ScreenToGUIPoint(new Vector2(screenRect.x, screenRect.y));
            screenRect.x = guiPoint.x;
            screenRect.y = guiPoint.y; return screenRect;
        }
        
        // Avoids String.memcpy, String.memset
        static Internal_DrawTextureArguments args = new Internal_DrawTextureArguments { sourceRect = StretchToFillRect };
        static readonly Rect StretchToFillRect = new Rect { m_Width = 1f, m_Height = 1f };
        
        
        /// width - LTRB (left, top, right, bottom),
        /// corners - TRBL (top-left, top-right, bottom-right, bottom-left)
        
        public static void DrawTexture(in Rect rect, Texture image, in Color color, in Vector4 corners = default, bool smoothCorners = false
            /*, ScaleMode scaleMode = ScaleMode.StretchToFill*/)
        {
            args.screenRect = rect;
            args.sourceRect = StretchToFillRect;
            
            args.borderWidths = default; args.cornerRadiuses = corners; args.smoothCorners = smoothCorners;
            
            args.color = color; args.texture = image; args.mat = smoothCorners ? roundedRectMat : blendMat;
            Graphics.Internal_DrawTexture(ref args);
        }
        
        public static void DrawTexture(Rect rect, Texture image, Color color, Vector4 widths, Vector4 corners)
        {
            var mat = roundedRectMat;
            
            args.screenRect = rect;
            args.sourceRect = StretchToFillRect;
            
            args.borderWidths = widths; args.cornerRadiuses = corners; args.smoothCorners = true;
            
            args.color = color; args.mat = mat; args.texture = image;
            
            //GUI.CalculateScaledTextureRects(rect, ScaleMode.StretchToFill, 0, ref args.screenRect, ref args.sourceRect);
            Graphics.Internal_DrawTexture(ref args);
        }
        public static void DrawTexture(Rect rect, Texture image, Vector4 widths, Vector4 corners, in Color t, in Color r, in Color b, in Color l)
        {
            var mat = roundedRectColorPerBorderMat;
            
            args.screenRect = rect;
            args.sourceRect = StretchToFillRect;
            
            args.borderWidths = widths; args.cornerRadiuses = corners; args.smoothCorners = true;
            
            args.color = t; args.mat = mat; args.texture = image;
            
            args.topBorderColor = t; args.rightBorderColor = r; args.bottomBorderColor = b; args.leftBorderColor = l;
                
            //GUI.CalculateScaledTextureRects(rect, ScaleMode.StretchToFill, 0, ref args.screenRect, ref args.sourceRect);
            Graphics.Internal_DrawTexture(ref args);
        }
    }
}
#endif // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
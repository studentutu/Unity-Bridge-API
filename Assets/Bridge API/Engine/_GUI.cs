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
        static Material blendMat;
        static Material roundedRectMat;
        static Material roundedRectColorPerBorderMat;
        
        static _GUI()
        {
            blendMat = GUI.blendMaterial;
            roundedRectMat = GUI.roundedRectMaterial;
            roundedRectColorPerBorderMat = GUI.roundedRectWithColorPerBorderMaterial;
        }
        
        public static void DrawTexture(Rect rect, Texture image, Color color, Vector4 corners = default, bool smoothCorners = false
            /*, ScaleMode scaleMode = ScaleMode.StretchToFill*/)
        {
            var mat = corners == default ? blendMat : roundedRectMat;
            var args = new Internal_DrawTextureArguments
            {
                color = color,
                borderWidths = default,
                cornerRadiuses = corners,
                texture = image,
                smoothCorners = smoothCorners,
                mat = mat
            };
            //GUI.CalculateScaledTextureRects(rect, scaleMode, 0, ref args.screenRect, ref args.sourceRect);
            args.screenRect = rect;
            args.sourceRect = new Rect(0f, 0f, 1f, 1f);
            Graphics.Internal_DrawTexture(ref args);
        }
        
        
        public static void DrawBorder(Rect rect, Texture image, Color32 color, Vector4 corners, Vector4 widths, 
            Color32 left = default, Color32 top = default, Color32 right = default, Color32 bottom = default)
        {
            var perBorderColor = FastUtil.ColorEquals(left, top) || 
                                 FastUtil.ColorEquals(left, right) || 
                                 FastUtil.ColorEquals(left, bottom);
            var mat = perBorderColor ? roundedRectColorPerBorderMat : roundedRectMat;
            
            var args = new Internal_DrawTextureArguments
            {
                color = color,
                borderWidths = widths,
                cornerRadiuses = corners,
                texture = image,
                smoothCorners = true,
                mat = mat
            };
            if (perBorderColor)
            {
                args.leftBorderColor = FastUtil.ColorMultiply(left, color);
                args.topBorderColor = FastUtil.ColorMultiply(top, color);
                args.rightBorderColor = FastUtil.ColorMultiply(right, color);
                args.bottomBorderColor = FastUtil.ColorMultiply(bottom, color);
            }
            GUI.CalculateScaledTextureRects(rect, ScaleMode.StretchToFill, 0, ref args.screenRect, ref args.sourceRect);
            Graphics.Internal_DrawTexture(ref args);
        }
    }
}
#endif // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
#if IGNORE_ACCESS_CHECKS // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using AV.Bridge;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental;
using UnityEditor.StyleSheets;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using KW = UnityEditor.StyleSheets.StyleCatalogKeyword;
using SV = UnityEditor.StyleSheets.StyleValue;

namespace AV.Bridge._UnityEditor
{
    // EXPERIMENTAL GUIStyle Painter that tries to squeeze every bit of performance and provide some customization options
    // Overview:
    // - Tons of micro-optimizations
    // - Gradient code *temporarily* removed
    // - Manually inlined methods and property calls, uses privates directly
    // - Manually getting buffer value indices
    // - Fixed hash table for StyleState[]
    // - Cached static values
    static class _OptimizedStylePainter
    {
        private static readonly int k_EnableHovering = "-unity-enable-hovering".GetHashCode();

        internal static bool enableHovering;
        
        static readonly StyleCatalog catalog;
        static readonly StyleCatalog.StyleBuffers Buff;
        static readonly StyleBlock[] Blocks;
        
        static readonly string[] Strings;
        static readonly float[] Numbers;
        static readonly Color[] Colors;
        static readonly StyleRect[] Rects;
        static readonly StyleValueGroup[] Groups;
        static readonly StyleFunction[] Functions;
        static readonly FixedHashTable<StyleState[]> statesTable = new FixedHashTable<StyleState[]>(17);
        static readonly FixedHashTable<StyleCache> stylesCache = new FixedHashTable<StyleCache>(1000);
        
        struct StyleCache
        {
            public int fixedWidth;
            public int fixedHeight;
        }
        
        static readonly Texture2D whiteTexture;
        
        static bool guiEnabled;
        static GUIView guiView;
        static GUIView mouseOverView;
        static int hotControl;
        static float pixelsPerPoint;
        static EventType evtType;
        static Vector2 mousePos;
        static HighlightSearchMode highlightSearchMode;
        
        
        static _OptimizedStylePainter()
        {
            whiteTexture = EditorGUIUtility.whiteTexture;
            
            catalog = EditorResources.styleCatalog;
            
            Buff = catalog.buffers;
            Blocks = catalog.m_Blocks;
            Strings = Buff.strings; Numbers = Buff.numbers; Colors = Buff.colors; Rects = Buff.rects; Groups = Buff.groups; Functions = Buff.functions;

            _GUI.OnProcessEvent(ProcessEvent, _ActionInject.Before);
            ProcessEvent(0, default);
        }

        static bool ProcessEvent(int _, IntPtr __)
        {
            evtType = Event.s_Current.type;
            if (evtType != EventType.Repaint) 
                return false;
            
            mouseOverView = GUIView.mouseOverView;
            
            pixelsPerPoint = GUIUtility.pixelsPerPoint;
            highlightSearchMode = Highlighter.searchMode;
            return false;
        }
        
        
        static int FindStyleIndex(int key)
        {
            int low = 0; int high = Blocks.Length - 1; int middle = (high + 1) / 2;
            do 
            {
                int currentKey = Blocks[middle].name;
                if (key == currentKey)
                    return middle;

                if (key < currentKey)
                    high = middle - 1;
                else
                    low = middle + 1;
                middle = (low + high + 1) / 2;
            }
            while (low <= high);
            
            return -1;
        }
        
        internal static bool DrawStyle(GUIStyle gs, Rect position, GUIContent content, DrawStates states)
        {
            if (highlightSearchMode == HighlightSearchMode.Identifier || highlightSearchMode == HighlightSearchMode.Auto)
                Highlighter.HighlightIdentifier(position, content?.text);

            if (gs.blockId == -1) 
                return false;
            
            if (ReferenceEquals(gs, GUIStyle.s_None) ||
                !ReferenceEquals(gs.m_Normal?.background, null)) // for fields like ColorField
                return false; // non StyleCatalog, draw natively
            
            GUIClip.get_visibleRect_Injected(out var visibleRect);
            if (!_FastUtil.RectOverlaps(visibleRect, position)) 
                return true;

            if (gs.blockId == 0)
            {
                var name = gs.name;
                var blockName = GUIStyleExtensions.StyleNameToBlockName(name, false);
                gs.blockId = blockName.GetHashCode();
                //Debug.Log($"{name} {blockName}");
            }

            var block = FindBlock(gs.blockId, states);
            if (block.name == -1)
            {
                gs.blockId = -1;
                return false;
            }
            
            guiEnabled = GUI.enabled;
            guiView = GUIView.current;
            //mouseOverView = GUIView.mouseOverView;
            //ProcessEvent(0, default);
            Event.s_Current.get_mousePosition_Injected(out mousePos);
            DrawBlock(gs, in block, position, content, states);
            return true;
        }
        
        static readonly StyleValue[] k_NoValue = {};
        static readonly StyleState[] k_NoState = {};
        static readonly StyleBlock k_ElementNotFound = new StyleBlock(-1, k_NoState, k_NoValue, null);
        
        internal static StyleBlock FindBlock(int name, in DrawStates draw)
        {
            StyleState flags = 0;
            if (guiEnabled)
            {
                if (draw.hasKeyboardFocus && guiView.hasFocus) flags |= StyleState.focus;
                if (draw.isActive || GUI.HasMouseControl(draw.controlId)) flags |= StyleState.active;
                if (draw.isHover && GUIUtility.Internal_GetHotControl() == 0 && GUIView.mouseOverView == guiView) flags |= StyleState.hover;
            }
            else { flags |= StyleState.disabled; }

            if (draw.on) flags |= StyleState.@checked;

            var flagsInt = (int)flags;
            
            ref var bucket = ref statesTable.GetBucket(flagsInt);
            ref var states = ref bucket.Get(flagsInt);
            
            if (states == null)
                states = new[] { flags,
                                 flags & StyleState.disabled,
                                 flags & StyleState.active,
                                 flags & StyleState.@checked,
                                 flags & StyleState.hover,
                                 flags & StyleState.focus,
                                 StyleState.normal }.Distinct().Where(s => s != StyleState.none).ToArray();
            
            var location = FindStyleIndex(name);
            if (location == -1) return k_ElementNotFound;
            
            var block = Blocks[location];
            return new StyleBlock(block.name, states, block.values, block.catalog);
        }

        // https://thomaslevesque.com/2020/05/15/things-every-csharp-developer-should-know-1-hash-codes/
        public class FixedHashTable<V>
        {
            public struct Bucket
            {
                readonly int[] keys; readonly V[] values; int idx;
                
                public Bucket(int size) 
                {
                    keys = new int[size]; values = new V[size]; idx = 0;
                }
                
                public ref V Get(int key) 
                {
                    for (int i = 0; i < idx; i++)
                    {
                        if (keys[i] == key) 
                            return ref values[i];
                    }
                    keys[idx] = key;
                    return ref values[idx++];
                }
            }
            readonly int size;
            readonly Bucket[] buckets;
    
            public FixedHashTable(int size, int perKeySize = 10) 
            {
                this.size = size;
                buckets = new Bucket[size];
                
                for (int i = 0; i < size; i++)
                    buckets[i] = new Bucket(perKeySize);
            }
    
            public ref Bucket GetBucket(int key) => ref buckets[key % size];
        }
        
        #region Bitwise Fuckery (unused)
        // https://stackoverflow.com/questions/677204/counting-the-number-of-flags-set-on-an-enumeration
        static int FlagsCount(int v)
        {
            v -= (v >> 1) & 0x55555555;
            v = (v & 0x33333333) + ((v >> 2) & 0x33333333);
            return ((v + (v >> 4) & 0xF0F0F0F) * 0x1010101) >> 24;
        }
        // https://stackoverflow.com/questions/22478029/how-to-get-numeric-position-of-an-enum-in-its-definition-list
        // https://stackoverflow.com/questions/8970101/whats-the-quickest-way-to-compute-log2-of-an-integer-in-c
        static int BitLog2(int n)
        {
            int bits = 0;
            if (n > 0xffff) { bits = 0x10; n >>= 16; }
            if (n > 0xff) { bits |= 0x8; n >>= 8; }
            if (n > 0xf) { bits |= 0x4; n >>= 4;  }
            if (n > 0x3) { bits |= 0x2; n >>= 2;  }
            if (n > 0x1) { bits |= 0x1; }
            return bits;
        }
        #endregion

        const int NI = -1;
        
        // block.GetValueIndex() loops through all the states and values, every time.
        // We're doing this only ONCE, and manually getting all the required buffer indices to use later
        // Ideally, per-state draw data should be cached, but it's hard because of how states are implemented
        // 150 calls - 0.35ms
        readonly struct DrawValuesMap
        {
            public readonly StyleValue.Keyword kw_Padding, kw_Hovering;
            public readonly int color, opacity;
            public readonly int offsetX, offsetY;
            public readonly int border, borderWidth, borderRadius;
            public readonly int borderColor, borderColorT, borderColorR, borderColorB, borderColorL;
            public readonly int position, size, imagePath;
            
            public DrawValuesMap(in StyleBlock block)
            {
                //this = default;
                kw_Padding = kw_Hovering = StyleValue.Keyword.Invalid;
                color = opacity = offsetX = offsetY = position = size = imagePath =
                border = borderWidth = borderRadius = borderColor = borderColorT = borderColorR = borderColorB = borderColorL 
                    = NI;
                
                var states = block.states; var values = block.values;
                var sCount = states.Length; var vCount = values.Length;
                
                for (var si = 0; si < sCount; si++)
                {
                    var state = states[si];
                    if (state == StyleState.none) continue;
                    
                    for (int vi = 0; vi != vCount; ++vi)
                    {
                        ref var v = ref values[vi];
                        if (v.state != state) 
                            continue;
                        var k = v.key; var i = v.index;
                        
                        switch (v.type)
                        {
                            case SV.Type.Group:
                                if (border == NI && k == KW.border) border = i;
                                break;
                            case SV.Type.Number:
                                if (opacity == NI && k == KW.opacity) opacity = i; else
                                if (offsetX == NI && k == KW.contentImageOffsetX) offsetX = i; else
                                if (offsetY == NI && k == KW.contentImageOffsetY) offsetY = i;
                                break;
                            case SV.Type.Color:
                                if (color == NI && k == KW.backgroundColor) color = i; else
                                if (borderColor == NI && k == KW.borderColor) borderColor = i; else
                                if (borderColorT == NI && k == KW.borderTopColor) borderColorT = i; else
                                if (borderColorR == NI && k == KW.borderRightColor) borderColorR = i; else
                                if (borderColorB == NI && k == KW.borderBottomColor) borderColorB = i; else
                                if (borderColorL == NI && k == KW.borderLeftColor) borderColorL = i;
                                break;
                            case SV.Type.Keyword:
                                if (kw_Padding == SV.Keyword.Invalid && k == KW.padding) kw_Padding = (SV.Keyword)i; else
                                if (kw_Hovering == SV.Keyword.Invalid && k == k_EnableHovering) kw_Hovering = (SV.Keyword)i; 
                                break;
                            case SV.Type.Rect: 
                                if (size == NI && k == KW.backgroundSize) size = i; else
                                if (position == NI && k == KW.position) position = i; else
                                
                                if (borderWidth == NI && k == KW.borderWidth) borderWidth = i; else
                                if (borderRadius == NI && k == KW.borderRadius) borderRadius = i;
                                break; 
                            case SV.Type.Text:  
                                if (imagePath == NI && k == KW.backgroundImage) imagePath = i;
                                break;
                        }
                    }
                }
            }
        }
        
        static readonly int ToggleHash = "Toggle".GetHashCode();
        
        internal static void DrawBlock(GUIStyle gs, in StyleBlock block, Rect rect, GUIContent content, in DrawStates draw)
        {
            var userRect = rect;
            var blockId = gs.blockId;
            var id = new DrawValuesMap(block);

            var hasBackground = id.imagePath != NI;
            var hasOpacity = id.opacity != NI;
            var hasColor = id.color != NI;
            var hasWidths = id.borderWidth != NI;
            
            var color = hasColor ? Colors[id.color] : default;
            var opacity = hasOpacity ? Numbers[id.opacity] : 1f;
            var radius = id.borderRadius != NI ? Rects[id.borderRadius] : default;
            var corners = new Vector4(radius.top, radius.right, radius.bottom, radius.left);
            
            if (id.position != NI) 
            {
                var offset = Rects[id.position];
                _FastUtil.RectPadding(ref rect, offset.top, offset.right, offset.bottom, offset.left);
            }
            // Adjust width and height if enforced by style block
            var fixedW = gs.fixedWidth; var fixedH = gs.fixedHeight;
            
            if (fixedW != 0f) rect.m_Width = fixedW;
            if (fixedH != 0f) rect.m_Height = fixedH;
            
            
            GUI.get_color_Injected(out var guiColor);
            GUI.get_backgroundColor_Injected(out var guiBackgroundColor);
            
            if (!guiEnabled)
                guiColor.a *= hasOpacity ? opacity : 0.5f;
            
            var tint = guiBackgroundColor;
            _FastUtil.ColorMultiply(ref tint, guiColor);
            
            var dim = 0.81f;
            var dimTint = new Color(dim, dim, dim, 1);
            
            var image = hasBackground ? TexturesByDPIScale.GetTexture(Strings[id.imagePath], true, pixelsPerPoint) : null;
            
            if (blockId == ToggleHash)
            {
                //image = EditorGUIUtility.IconContent("Toggle Icon").image as Texture2D;
            }
            
            // Draw background color
            if (hasColor)
            {
                _FastUtil.ColorMultiply(ref color, tint);
                _FastUtil.ColorMultiply(ref color, dimTint);
                
                _GUI.DrawTexture(rect, whiteTexture, color, corners, id.borderRadius != NI);
            }
            
            // Draw background image
            if (hasBackground) 
            {
                var bgRect = GetBackgroundImageRect(block, rect, image, Rects[id.size]);
                _GUI.DrawTexture(bgRect, image, tint);
            }
            
            Rect contentRect = rect;

            if (!ReferenceEquals(content, null))
            {
                GUI.get_contentColor_Injected(out var guiContentColor);
                
                // Compute content rect
                
                //if (kw_Padding == StyleValue.Keyword.Auto)
                if (id.kw_Padding == StyleValue.Keyword.Auto)
                    GetContentCenteredRect(gs, content, ref contentRect);
                
                // Draw content (text & image)
                var hasImage = !ReferenceEquals(content.m_Image, null);
                
                var contentOpacity = hasImage && hasOpacity ? opacity : 1f;
                var offsetX = id.offsetX != NI ? Numbers[id.offsetX] : 0;
                var offsetY = id.offsetY != NI ?  Numbers[id.offsetY] : 0;
                
                var cursorColor = draw.cursorColor;
                var selectionColor = draw.selectionColor;
                var imageColor = guiContentColor;
                _FastUtil.ColorMultiply(ref imageColor, contentOpacity);
                _FastUtil.ColorMultiply(ref imageColor, 0.95f); // label dim
                
                gs.Internal_DrawContent_Injected(ref contentRect, content, draw.isHover, draw.isActive, draw.on, draw.hasKeyboardFocus,
                    draw.hasTextInput, draw.drawSelectionAsComposition, draw.cursorFirst, draw.cursorLast,
                    ref cursorColor, ref selectionColor, ref imageColor,
                    0, 0, offsetY, offsetX, false, false);

                // Handle tooltip and hovering region
                if (!string.IsNullOrEmpty(content.m_Tooltip) && _FastUtil.RectContains(contentRect, mousePos))
                //if (!string.IsNullOrEmpty(content.m_Tooltip) && contentRect.Contains(Event.current.mousePosition))
                    GUIStyle.SetMouseTooltip_Injected(content.m_Tooltip, ref contentRect);
            }
            
            
            hasWidths = false; // disable borders
            var bw = hasWidths ? Rects[id.borderWidth] : default;
            var borderColor = id.borderColor != NI ? Colors[id.borderColor] : default;
            
            if (bw.top != 0 || bw.right != 0 || bw.bottom != 0 || bw.left != 0)  
            {
                // Draw border
                // issue: tiny Matte color on corners?
                var border = new BorderColor
                {
                    t = id.borderColorT != NI ? Colors[id.borderColorT] : borderColor,
                    r = id.borderColorR != NI ? Colors[id.borderColorR] : borderColor,
                    b = id.borderColorB != NI ? Colors[id.borderColorB] : borderColor,
                    l = id.borderColorL != NI ? Colors[id.borderColorL] : borderColor
                };
                var widths = new Vector4(bw.left, bw.top, bw.right, bw.bottom);
                
                var borderTint = guiColor;
                //borderTint.a = 0.66f;
                _FastUtil.ColorMultiply(ref borderTint, dimTint);
                _FastUtil.BorderColorMultiply(ref border.t, ref border.r, ref border.b, ref border.l, borderTint);
                
                _GUI.DrawTexture(rect, whiteTexture, widths, corners, 
                    border.t, border.r, border.b, border.l);
            }
            
            if (id.kw_Hovering == StyleValue.Keyword.True)
            {
                //if (!ReferenceEquals(guiView, null) && guiView.m_CachedPtr != IntPtr.Zero)
                if (_Object.IsAlive(guiView))
                {
                    GUIClip.UnclipToWindow_Rect_Injected(ref userRect, out var hotRect);
                    guiView.MarkHotRegion_Injected(ref hotRect);
                }
                //guiView.MarkHotRegion(GUIClip.UnclipToWindow(userRect));
            }
        }
        
        
        struct BorderColor
        {
            public Color t, r, b, l;
        }
        
        static Rect GetBackgroundImageRect(in StyleBlock block, Rect pos, Texture2D bgImage, StyleRect bgSize)
        {
            var bgPos = block.GetStruct<StyleBackgroundPosition>(KW.backgroundPosition);
            if (bgSize.width == 0f && bgSize.height == 0f)
                bgSize = StyleRect.Size(bgImage.GetDataWidth(), bgImage.GetDataHeight());
            
            StyleRect anchor = StyleRect.Nil;
            if (bgPos.xEdge == KW.left) anchor.left = bgPos.xOffset;
            else if (bgPos.yEdge == KW.left) anchor.left = bgPos.yOffset;
            if (bgPos.xEdge == KW.right) anchor.right = bgPos.xOffset;
            else if (bgPos.yEdge == KW.right) anchor.right = bgPos.yOffset;
            if (bgPos.xEdge == KW.top) anchor.top = bgPos.xOffset;
            else if (bgPos.yEdge == KW.top) anchor.top = bgPos.yOffset;
            if (bgPos.xEdge == KW.bottom) anchor.bottom = bgPos.xOffset;
            else if (bgPos.yEdge == KW.bottom) anchor.bottom = bgPos.yOffset;

            var pos_xMax = pos.m_XMin + pos.m_Width;
            var pos_yMax = pos.m_YMin + pos.m_Height;
            
            var bgRect = new Rect(pos.m_XMin, pos.m_YMin, bgSize.width, bgSize.height);
            if (anchor.left < 1.0f)
                bgRect.m_XMin = pos.m_XMin + pos.m_Width * anchor.left - bgSize.width / 2f;
            else if (anchor.left >= 1.0f)
                bgRect.m_XMin = pos.m_XMin + anchor.left;

            if (anchor.top < 1.0f)
                bgRect.m_YMin = pos.m_YMin + pos.m_Height * anchor.top - bgSize.height / 2f;
            else if (anchor.top >= 1.0f)
                bgRect.m_YMin = pos.m_YMin + anchor.top;

            if (anchor.right == 0f || anchor.right >= 1.0f)
                bgRect.m_XMin = pos_xMax - bgSize.width - anchor.right;
            else if (anchor.right < 1.0f)
                bgRect.m_XMin = pos_xMax - pos.m_Width * anchor.right - bgSize.width / 2f;

            if (anchor.bottom == 0f || anchor.bottom >= 1.0f)
                bgRect.m_YMin = pos_yMax - bgSize.height - anchor.bottom;
            if (anchor.bottom < 1.0f)
                bgRect.m_YMin = pos_yMax - pos.m_Height * anchor.bottom - bgSize.height / 2f;

            bgRect.m_Width = bgSize.width;
            bgRect.m_Height = bgSize.height;
            return bgRect;
        }

        static void GetContentCenteredRect(GUIStyle gs, GUIContent content, ref Rect rect)
        {
            var r = rect;
            var size = gs.Internal_CalcSize(content);
            
            rect.m_XMin = Mathf.Max(r.m_XMin, r.m_XMin + (r.m_Width - size.x) / 2f);
            rect.m_Width = Mathf.Min(r.m_Width + r.m_XMin, rect.m_XMin + size.x);
            rect.m_YMin = Mathf.Max(r.m_YMin, r.m_YMin + (r.m_Height - size.y) / 2f);
            rect.m_Height = Mathf.Min(r.m_Height + r.m_YMin, rect.m_YMin + size.y);
        }

        private struct StyleBackgroundPosition
        {
            #pragma warning disable 0649
            public int xEdge;
            public float xOffset;
            public int yEdge;
            public float yOffset;
            #pragma warning restore 0649
        }
        
        
        static class TexturesByDPIScale
        {
            // Replaced Dictionary[scale] with flat array
            static Dictionary<string, Texture2D>[] textures = new Dictionary<string, Texture2D>[8];

            static TexturesByDPIScale() {
                for (int i = 1; i < 8; ++i) textures[i] = new Dictionary<string, Texture2D>();
            }

            public static Texture2D GetTexture(string resourcePath, bool autoScale, float systemScale)
            {
                Texture2D tex = null;
                var scale = (int)Math.Ceiling(systemScale);

                if (autoScale && systemScale > 1f)
                {
                    if (TryGetTexture(scale, resourcePath, out tex))
                        return tex;

                    string dirName = Path.GetDirectoryName(resourcePath).Replace('\\', '/');
                    string fileName = Path.GetFileNameWithoutExtension(resourcePath);
                    string fileExt = Path.GetExtension(resourcePath);
                    for (int s = scale; scale > 1; --scale)
                    {
                        string scaledResourcePath = $"{dirName}/{fileName}@{s}x{fileExt}";
                        var scaledResource = StoreTextureByScale(scale, scaledResourcePath, resourcePath, false);
                        if (scaledResource != null)
                            return scaledResource;
                    }
                }

                if (TryGetTexture(scale, resourcePath, out tex))
                    return tex;
                return StoreTextureByScale(scale, resourcePath, resourcePath, true);
            }

            static Texture2D StoreTextureByScale(int scale, string scaledPath, string resourcePath, bool logError)
            {
                var tex = EditorResources.Load<Texture2D>(scaledPath, false);
                if (tex)
                {
                    textures[scale][resourcePath] = tex;
                }
                else if (logError)
                {
                    Debug.LogFormat(LogType.Warning, LogOption.NoStacktrace, null, $"Failed to store {resourcePath} > {scaledPath}");
                }
                return tex;
            }

            static bool TryGetTexture(int scale, string path, out Texture2D tex)
            {
                return textures[scale].TryGetValue(path, out tex) && !ReferenceEquals(tex, null);
            }
        }
    }
}
#endif // [ASMDEFEX] DO NOT REMOVE THIS LINE MANUALLY.
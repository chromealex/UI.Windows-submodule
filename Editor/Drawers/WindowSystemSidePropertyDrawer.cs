using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows.Utilities;

    internal class LayoutDropdownWindow : PopupWindowContent {

        public WindowSystemSidePropertyDrawer.Layout selectedX;
        public WindowSystemSidePropertyDrawer.Layout selectedY;
        public System.Action<WindowSystemSidePropertyDrawer.Layout, WindowSystemSidePropertyDrawer.Layout> callback;
        public bool drawMiddle;
        
        public override Vector2 GetWindowSize() {
            
            return new Vector2(140f, 140f);
            
        }

        public override void OnGUI(Rect rect) {

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return) {
            
                this.editorWindow.Close();
                
            }

            const float offset = 10f;
            const float size = 40f;

            for (int x = 0; x < 3; ++x) {

                for (int y = 0; y < 3; ++y) {

                    if (this.drawMiddle == false && x == 1 && y == 1) {
                        
                        continue;
                        
                    }
                    
                    var hMode = (WindowSystemSidePropertyDrawer.Layout)x;
                    var vMode = (WindowSystemSidePropertyDrawer.Layout)y;
                    
                    var buttonRect = rect;
                    buttonRect.x = offset + size * x;
                    buttonRect.y = offset + size * y;
                    buttonRect.width = size;
                    buttonRect.height = size;
                    if (WindowSystemSidePropertyDrawer.DrawButton(buttonRect, hMode, vMode) == true) {
                        
                        if (this.callback != null) this.callback.Invoke(hMode, vMode);
                        this.editorWindow.Close();
                        
                    }

                    if (hMode == this.selectedX && vMode == this.selectedY) {
                        
                        GUILayoutExt.DrawBoxNotFilled(buttonRect, 1f, Color.white);
                        
                    }
                    
                }
                
            }
            
        }

    }

    [CustomPropertyDrawer(typeof(UnityEngine.UI.Windows.Components.Side))]
    public class WindowSystemSidePropertyDrawer : PropertyDrawer {

        public enum Layout {

            Min = 0,
            Middle = 1,
            Max = 2,
            Stretch = 3,
            Undefined = 10,

        }
        static float[] kPivotsForModes = new float[] { 0, 0.5f, 1, 0.5f, 0.5f };

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            
            return 40f;
            
        }

        public static bool DrawButton(Rect contentRect, Layout hMode, Layout vMode) {
            
            const float padding = 4f;
            const float size = 40f;

            var inner = contentRect;
            inner.x += padding;
            inner.width -= padding * 2f;
            inner.y += padding;
            inner.height -= padding * 2f;
            
            var style = new GUIStyle(EditorStyles.miniButton);
            style.stretchHeight = true;
            style.fixedHeight = 0f;
            style.normal.scaledBackgrounds[0] = null;
            style.onNormal.scaledBackgrounds[0] = Texture2D.whiteTexture;
            var oldColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, contentRect.Contains(Event.current.mousePosition) ? 0.3f : 0f);
            if (GUI.Button(contentRect, string.Empty, style) == true) {

                return true;

            }
            GUI.color = oldColor;

            GUILayoutExt.DrawBoxNotFilled(contentRect, 1f, new Color(1f, 1f, 1f, 0.3f), padding);
            GUILayoutExt.DrawBoxNotFilled(contentRect, 1f, new Color(1f, 1f, 1f, 0.5f), padding * 3f);

            var vLine = inner;
            vLine.y += (size - padding * 2f) * kPivotsForModes[(int)vMode] - 0.5f;
            vLine.height = 1f;
            vLine.x += 1f;
            vLine.width -= 2f;
            GUILayoutExt.DrawRect(vLine, new Color(0.7f, 0.3f, 0.3f, 1));

            var hLine = inner;
            hLine.x += (size - padding * 2f) * kPivotsForModes[(int)hMode] - 0.5f;
            hLine.width = 1f;
            hLine.y += 1f;
            hLine.height -= 2f;
            GUILayoutExt.DrawRect(hLine, new Color(0.7f, 0.3f, 0.3f, 1));
            
            var pivot = new Vector2(
                Mathf.Lerp(inner.xMin, inner.xMax, kPivotsForModes[(int)hMode]),
                Mathf.Lerp(inner.yMin, inner.yMax, kPivotsForModes[(int)vMode])
            );
            
            GUILayoutExt.DrawRect(new Rect(pivot.x - 1f, pivot.y - 1f, 3f, 3f), new Color(0.8f, 0.6f, 0.0f, 1));
            
            return false;

        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            var labelRect = position;
            labelRect.width = EditorGUIUtility.labelWidth;
            GUI.Label(labelRect, label);

            const float size = 40f;

            var side = (UnityEngine.UI.Windows.Components.Side)property.enumValueIndex;
            this.GetMode(side, out var hMode, out var vMode);
            
            var contentRect = position;
            contentRect.x += labelRect.width;
            contentRect.width = size;

            if (DrawButton(contentRect, hMode, vMode) == true) {
                
                var win = new LayoutDropdownWindow();
                win.drawMiddle = false;
                win.callback = (h, v) => {
                    
                    var mode = this.GetMode(h, v);
                    property.serializedObject.Update();
                    property.enumValueIndex = (int)mode;
                    property.serializedObject.ApplyModifiedProperties();

                };
                win.selectedX = hMode;
                win.selectedY = vMode;
                PopupWindow.Show(contentRect, win);
                
            }

        }

        private void GetMode(UnityEngine.UI.Windows.Components.Side side, out Layout hMode, out Layout vMode) {

            hMode = Layout.Max;
            vMode = Layout.Max;
            
            switch (side) {
                case UnityEngine.UI.Windows.Components.Side.Bottom:
                    hMode = Layout.Middle;
                    vMode = Layout.Max;
                    break;
                case UnityEngine.UI.Windows.Components.Side.Left:
                    hMode = Layout.Min;
                    vMode = Layout.Middle;
                    break;
                case UnityEngine.UI.Windows.Components.Side.Right:
                    hMode = Layout.Max;
                    vMode = Layout.Middle;
                    break;
                case UnityEngine.UI.Windows.Components.Side.Top:
                    hMode = Layout.Middle;
                    vMode = Layout.Min;
                    break;
                case UnityEngine.UI.Windows.Components.Side.Middle:
                    hMode = Layout.Middle;
                    vMode = Layout.Middle;
                    break;
                case UnityEngine.UI.Windows.Components.Side.BottomLeft:
                    hMode = Layout.Min;
                    vMode = Layout.Max;
                    break;
                case UnityEngine.UI.Windows.Components.Side.BottomRight:
                    hMode = Layout.Max;
                    vMode = Layout.Max;
                    break;
                case UnityEngine.UI.Windows.Components.Side.TopLeft:
                    hMode = Layout.Min;
                    vMode = Layout.Min;
                    break;
                case UnityEngine.UI.Windows.Components.Side.TopRight:
                    hMode = Layout.Max;
                    vMode = Layout.Min;
                    break;
            }
            
        }

        private UnityEngine.UI.Windows.Components.Side GetMode(Layout hMode, Layout vMode) {

            UnityEngine.UI.Windows.Components.Side side = UnityEngine.UI.Windows.Components.Side.Bottom;
            switch (vMode) {
                
                case Layout.Min:
                    switch (hMode) {
                        
                        case Layout.Min:
                            side = UnityEngine.UI.Windows.Components.Side.TopLeft;
                            break;

                        case Layout.Middle:
                            side = UnityEngine.UI.Windows.Components.Side.Top;
                            break;

                        case Layout.Max:
                            side = UnityEngine.UI.Windows.Components.Side.TopRight;
                            break;

                    }
                    break;

                case Layout.Middle:
                    switch (hMode) {
                        
                        case Layout.Min:
                            side = UnityEngine.UI.Windows.Components.Side.Left;
                            break;

                        case Layout.Middle:
                            side = UnityEngine.UI.Windows.Components.Side.Middle;
                            break;

                        case Layout.Max:
                            side = UnityEngine.UI.Windows.Components.Side.Right;
                            break;

                    }
                    break;

                case Layout.Max:
                    switch (hMode) {
                        
                        case Layout.Min:
                            side = UnityEngine.UI.Windows.Components.Side.BottomLeft;
                            break;

                        case Layout.Middle:
                            side = UnityEngine.UI.Windows.Components.Side.Bottom;
                            break;

                        case Layout.Max:
                            side = UnityEngine.UI.Windows.Components.Side.BottomRight;
                            break;

                    }
                    break;
                
            }

            return side;

        }
        
        
        #region RectTransformIcons
        class Styles
        {
            public Color tableHeaderColor;
            public Color tableLineColor;
            public Color parentColor;
            public Color selfColor;
            public Color simpleAnchorColor;
            public Color stretchAnchorColor;
            public Color anchorCornerColor;
            public Color pivotColor;

            public GUIStyle frame;
            public GUIStyle label = new GUIStyle(EditorStyles.miniLabel);

            public Styles()
            {
                frame = new GUIStyle();
                Texture2D tex = new Texture2D(4, 4);
                tex.SetPixels(new Color[]
                {
                    Color.white, Color.white, Color.white, Color.white,
                    Color.white, Color.clear, Color.clear, Color.white,
                    Color.white, Color.clear, Color.clear, Color.white,
                    Color.white, Color.white, Color.white, Color.white
                });
                tex.filterMode = FilterMode.Point;
                tex.Apply();
                tex.hideFlags = HideFlags.HideAndDontSave;
                frame.normal.background = tex;
                frame.border = new RectOffset(2, 2, 2, 2);

                label.alignment = TextAnchor.LowerCenter;

                if (EditorGUIUtility.isProSkin)
                {
                    tableHeaderColor = new Color(0.18f, 0.18f, 0.18f, 1);
                    tableLineColor = new Color(1, 1, 1, 0.3f);
                    parentColor = new Color(0.4f, 0.4f, 0.4f, 1);
                    selfColor = new Color(0.6f, 0.6f, 0.6f, 1);
                    simpleAnchorColor = new Color(0.7f, 0.3f, 0.3f, 1);
                    stretchAnchorColor = new Color(0.0f, 0.6f, 0.8f, 1);
                    anchorCornerColor = new Color(0.8f, 0.6f, 0.0f, 1);
                    pivotColor = new Color(0.0f, 0.6f, 0.8f, 1);
                }
                else
                {
                    tableHeaderColor = new Color(0.8f, 0.8f, 0.8f, 1);
                    tableLineColor = new Color(0, 0, 0, 0.5f);
                    parentColor = new Color(0.55f, 0.55f, 0.55f, 1);
                    selfColor = new Color(0.2f, 0.2f, 0.2f, 1);
                    simpleAnchorColor = new Color(0.8f, 0.3f, 0.3f, 1);
                    stretchAnchorColor = new Color(0.2f, 0.5f, 0.9f, 1);
                    anchorCornerColor = new Color(0.6f, 0.4f, 0.0f, 1);
                    pivotColor = new Color(0.2f, 0.5f, 0.9f, 1);
                }
            }
        }
        static Styles s_Styles;
        
        static Layout GetLayoutModeForAxis(
            SerializedProperty anchorMin,
            SerializedProperty anchorMax,
            int axis)
        {
            if (anchorMin.vector2Value[axis] == 0 && anchorMax.vector2Value[axis] == 0)
                return Layout.Min;
            if (anchorMin.vector2Value[axis] == 0.5f && anchorMax.vector2Value[axis] == 0.5f)
                return Layout.Middle;
            if (anchorMin.vector2Value[axis] == 1 && anchorMax.vector2Value[axis] == 1)
                return Layout.Max;
            if (anchorMin.vector2Value[axis] == 0 && anchorMax.vector2Value[axis] == 1)
                return Layout.Stretch;
            return Layout.Undefined;
        }

        static Layout GetLayoutModeForAxis(
            Vector2 anchorMin,
            Vector2 anchorMax,
            int axis)
        {
            if (anchorMin[axis] == 0 && anchorMax[axis] == 0)
                return Layout.Min;
            if (anchorMin[axis] == 0.5f && anchorMax[axis] == 0.5f)
                return Layout.Middle;
            if (anchorMin[axis] == 1 && anchorMax[axis] == 1)
                return Layout.Max;
            if (anchorMin[axis] == 0 && anchorMax[axis] == 1)
                return Layout.Stretch;
            return Layout.Undefined;
        }

        internal static void DrawLayoutMode(Rect rect,
                                            SerializedProperty anchorMin,
                                            SerializedProperty anchorMax,
                                            SerializedProperty position,
                                            SerializedProperty sizeDelta)
        {
            var hMode = GetLayoutModeForAxis(anchorMin, anchorMax, 0);
            var vMode = GetLayoutModeForAxis(anchorMin, anchorMax, 1);
            vMode = SwappedVMode(vMode);
            DrawLayoutMode(rect, hMode, vMode);
        }

        internal static void DrawLayoutMode(Rect rect, RectTransform rectTransform)
        {
            var hMode = GetLayoutModeForAxis(rectTransform.anchorMin, rectTransform.anchorMax, 0);
            var vMode = GetLayoutModeForAxis(rectTransform.anchorMin, rectTransform.anchorMax, 1);
            vMode = SwappedVMode(vMode);
            DrawLayoutMode(rect, hMode, vMode, rectTransform.pivot, true, false);
        }

        static Layout SwappedVMode(Layout vMode)
        {
            if (vMode == Layout.Min)
                return Layout.Max;
            else if (vMode == Layout.Max)
                return Layout.Min;
            return vMode;
        }
        
        internal static void DrawLayoutMode(Rect position, Layout hMode, Layout vMode)
        {
            DrawLayoutMode(position, hMode, vMode, Vector2.zero, false, false);
        }

        internal static void DrawLayoutMode(Rect position, Layout hMode, Layout vMode, Vector2 pivot, bool doPivot)
        {
            DrawLayoutMode(position, hMode, vMode, pivot, doPivot, false);
        }

        internal static void DrawLayoutMode(Rect position, Layout hMode, Layout vMode, Vector2 pivot, bool doPivot, bool doPosition)
        {
            if (s_Styles == null)
                s_Styles = new Styles();

            GUILayoutExt.DrawRect(position, new Color(1f, 1f, 1f, 0.02f));
            
            Color oldColor = GUI.color;

            // Make parent size the largest possible square, but enforce it's an uneven number.
            int parentWidth = (int)Mathf.Min(position.width, position.height);
            if (parentWidth % 2 == 0)
                parentWidth--;

            int selfWidth = parentWidth / 2;
            if (selfWidth % 2 == 0)
                selfWidth++;

            Vector2 parentSize = new Vector2(position.width, position.height);//parentWidth * Vector2.one;
            Vector2 selfSize = new Vector2(position.width * 0.5f, position.height * 0.5f);//selfWidth * Vector2.one;
            Vector2 padding = (position.size - parentSize) / 2;
            padding.x = Mathf.Floor(padding.x);
            padding.y = Mathf.Floor(padding.y);
            Vector2 padding2 = (position.size - selfSize) / 2;
            padding2.x = Mathf.Floor(padding2.x);
            padding2.y = Mathf.Floor(padding2.y);

            Rect outer = new Rect(position.x + padding.x, position.y + padding.y, parentSize.x, parentSize.y);
            Rect inner = new Rect(position.x + padding2.x, position.y + padding2.y, selfSize.x, selfSize.y);
            if (doPosition)
            {
                for (int axis = 0; axis < 2; axis++)
                {
                    var mode = (axis == 0 ? hMode : vMode);

                    if (mode == Layout.Min)
                    {
                        Vector2 center = inner.center;
                        center[axis] += outer.min[axis] - inner.min[axis];
                        inner.center = center;
                    }
                    if (mode == Layout.Middle)
                    {
                        // TODO
                    }
                    if (mode == Layout.Max)
                    {
                        Vector2 center = inner.center;
                        center[axis] += outer.max[axis] - inner.max[axis];
                        inner.center = center;
                    }
                    if (mode == Layout.Stretch)
                    {
                        Vector2 innerMin = inner.min;
                        Vector2 innerMax = inner.max;
                        innerMin[axis] = outer.min[axis];
                        innerMax[axis] = outer.max[axis];
                        inner.min = innerMin;
                        inner.max = innerMax;
                    }
                }
            }

            Rect anchor = new Rect();
            Vector2 min = Vector2.zero;
            Vector2 max = Vector2.zero;
            for (int axis = 0; axis < 2; axis++)
            {
                var mode = (axis == 0 ? hMode : vMode);

                if (mode == Layout.Min)
                {
                    min[axis] = outer.min[axis] + 0.5f;
                    max[axis] = outer.min[axis] + 0.5f;
                }
                if (mode == Layout.Middle)
                {
                    min[axis] = outer.center[axis];
                    max[axis] = outer.center[axis];
                }
                if (mode == Layout.Max)
                {
                    min[axis] = outer.max[axis] - 0.5f;
                    max[axis] = outer.max[axis] - 0.5f;
                }
                if (mode == Layout.Stretch)
                {
                    min[axis] = outer.min[axis] + 0.5f;
                    max[axis] = outer.max[axis] - 0.5f;
                }
            }
            anchor.min = min;
            anchor.max = max;

            // Draw parent rect
            if (Event.current.type == EventType.Repaint)
            {
                GUI.color = s_Styles.parentColor * oldColor;
                s_Styles.frame.Draw(outer, false, false, false, false);
            }

            // Draw anchor lines
            if (hMode != Layout.Undefined && hMode != Layout.Stretch)
            {
                GUI.color = s_Styles.simpleAnchorColor * oldColor;
                GUI.DrawTexture(new Rect(anchor.xMin - 0.5f, outer.y + 1, 1, outer.height - 2), EditorGUIUtility.whiteTexture);
                GUI.DrawTexture(new Rect(anchor.xMax - 0.5f, outer.y + 1, 1, outer.height - 2), EditorGUIUtility.whiteTexture);
            }
            if (vMode != Layout.Undefined && vMode != Layout.Stretch)
            {
                GUI.color = s_Styles.simpleAnchorColor * oldColor;
                GUI.DrawTexture(new Rect(outer.x + 1, anchor.yMin - 0.5f, outer.width - 2, 1), EditorGUIUtility.whiteTexture);
                GUI.DrawTexture(new Rect(outer.x + 1, anchor.yMax - 0.5f, outer.width - 2, 1), EditorGUIUtility.whiteTexture);
            }

            // Draw stretch mode arrows
            if (hMode == Layout.Stretch)
            {
                GUI.color = s_Styles.stretchAnchorColor * oldColor;
                DrawArrow(new Rect(inner.x + 1, inner.center.y - 0.5f, inner.width - 2, 1));
            }
            if (vMode == Layout.Stretch)
            {
                GUI.color = s_Styles.stretchAnchorColor * oldColor;
                DrawArrow(new Rect(inner.center.x - 0.5f, inner.y + 1, 1, inner.height - 2));
            }

            // Draw self rect
            if (Event.current.type == EventType.Repaint)
            {
                GUI.color = s_Styles.selfColor * oldColor;
                s_Styles.frame.Draw(inner, false, false, false, false);
            }

            // Draw pivot
            if (doPivot && hMode != Layout.Undefined && vMode != Layout.Undefined) {
                
                Vector2 pivotInner = new Vector2(
                    Mathf.Lerp(inner.xMin + 0.5f, inner.xMax - 0.5f, pivot.x),
                    Mathf.Lerp(inner.yMax + 0.5f, inner.yMin - 0.5f, pivot.y)
                );

                GUI.color = s_Styles.pivotColor * oldColor;
                GUI.DrawTexture(new Rect(pivotInner.x - 2.5f, pivotInner.y - 1.5f, 5, 3), EditorGUIUtility.whiteTexture);
                GUI.DrawTexture(new Rect(pivotInner.x - 1.5f, pivotInner.y - 2.5f, 3, 5), EditorGUIUtility.whiteTexture);
            }

            // Draw anchor corners
            if (hMode != Layout.Undefined && vMode != Layout.Undefined)
            {
                GUI.color = s_Styles.anchorCornerColor * oldColor;
                GUI.DrawTexture(new Rect(anchor.xMin - 1.5f, anchor.yMin - 1.5f, 2, 2), EditorGUIUtility.whiteTexture);
                GUI.DrawTexture(new Rect(anchor.xMax - 0.5f, anchor.yMin - 1.5f, 2, 2), EditorGUIUtility.whiteTexture);
                GUI.DrawTexture(new Rect(anchor.xMin - 1.5f, anchor.yMax - 0.5f, 2, 2), EditorGUIUtility.whiteTexture);
                GUI.DrawTexture(new Rect(anchor.xMax - 0.5f, anchor.yMax - 0.5f, 2, 2), EditorGUIUtility.whiteTexture);
            }

            GUI.color = oldColor;
        }
        
        static void DrawArrow(Rect lineRect)
        {
            GUI.DrawTexture(lineRect, EditorGUIUtility.whiteTexture);
            if (lineRect.width == 1)
            {
                GUI.DrawTexture(new Rect(lineRect.x - 1, lineRect.y + 1, 3, 1), EditorGUIUtility.whiteTexture);
                GUI.DrawTexture(new Rect(lineRect.x - 2, lineRect.y + 2, 5, 1), EditorGUIUtility.whiteTexture);
                GUI.DrawTexture(new Rect(lineRect.x - 1, lineRect.yMax - 2, 3, 1), EditorGUIUtility.whiteTexture);
                GUI.DrawTexture(new Rect(lineRect.x - 2, lineRect.yMax - 3, 5, 1), EditorGUIUtility.whiteTexture);
            }
            else
            {
                GUI.DrawTexture(new Rect(lineRect.x + 1, lineRect.y - 1, 1, 3), EditorGUIUtility.whiteTexture);
                GUI.DrawTexture(new Rect(lineRect.x + 2, lineRect.y - 2, 1, 5), EditorGUIUtility.whiteTexture);
                GUI.DrawTexture(new Rect(lineRect.xMax - 2, lineRect.y - 1, 1, 3), EditorGUIUtility.whiteTexture);
                GUI.DrawTexture(new Rect(lineRect.xMax - 3, lineRect.y - 2, 1, 5), EditorGUIUtility.whiteTexture);
            }
        }
        #endregion

    }

}
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI.Windows.Modules;

namespace UnityEditor.UI.Windows {

    [CustomPropertyDrawer(typeof(UnityEngine.UI.Windows.Utilities.Tweener.EaseFunction))]
    public class EaseAnimationParametersPropertyDrawer : PropertyDrawer {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

            return EditorGUIUtility.singleLineHeight;

        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            EditorGUI.BeginProperty(position, label, property);
            if (EditorGUI.DropdownButton(position, GUIContent.none, FocusType.Passive, EditorStyles.toolbarDropDown) == true) {
                PopupWindow.Show(position, new EaseEditorWindow((UnityEngine.UI.Windows.Utilities.Tweener.EaseFunction)property.enumValueIndex) {
                    onSelect = (ease) => {
                        property.serializedObject.Update();
                        property.enumValueIndex = (int)ease;
                        property.serializedObject.ApplyModifiedProperties();
                        property.serializedObject.Update();
                    },
                });
            }

            var lbl = property.enumDisplayNames[property.enumValueIndex];
            EaseEditorWindow.Item.DrawProgress(new Rect(position.x + 1f, position.y + 1f, position.width - 20f, position.height - 2f), (UnityEngine.UI.Windows.Utilities.Tweener.EaseFunction)property.enumValueIndex, -1f, lbl, false);
            EditorGUI.EndProperty();
            
        }
        
    }

    public class EaseEditorWindow : PopupWindowContent {

        private const int COLUMNS = 4;
        private const float ELEMENT_WIDTH = 100f;
        private const float ELEMENT_HEIGHT = 40f;
        private const float HEADER_HEIGHT = 20f;
        private SerializedProperty property;
        private readonly System.Collections.Generic.List<Item> items;
        public System.Action<UnityEngine.UI.Windows.Utilities.Tweener.EaseFunction> onSelect;

        public static class Styles {

            public static readonly GUIStyle headerLabel;
            public static readonly GUIStyle label;
            public static readonly GUIStyle sliderDot;

            static Styles() {
                
                headerLabel = new GUIStyle(EditorStyles.miniBoldLabel);
                headerLabel.alignment = TextAnchor.LowerLeft;
                headerLabel.normal.textColor = new Color(1f, 1f, 1f, 0.5f);
                if (EditorGUIUtility.isProSkin == false) {
                    headerLabel.normal.textColor = new Color(0f, 0f, 0f, 0.5f);
                }
                
                label = new GUIStyle(EditorStyles.miniLabel);
                label.normal.textColor = new Color(1f, 1f, 1f, 0.3f);
                label.alignment = TextAnchor.UpperLeft;
                if (EditorGUIUtility.isProSkin == false) {
                    label.normal.textColor = new Color(0f, 0f, 0f, 0.3f);
                }

                sliderDot = new GUIStyle((GUIStyle)"U2D.dragDot");

            }

        }

        public class Item {

            private readonly UnityEngine.UI.Windows.Utilities.Tweener.EaseFunction type;
            private readonly bool selected;
            private readonly int order;
            private readonly bool isGroup;
            private readonly string groupHeader;
            private double progress;
            private double prevTime;
            
            public Item(UnityEngine.UI.Windows.Utilities.Tweener.EaseFunction type, bool selected) {
                this.type = type;
                this.selected = selected;
                var memInfo = type.GetType().GetMember(type.ToString());
                {
                    var attribute = memInfo[0].GetCustomAttribute(typeof(UnityEngine.UI.Windows.Utilities.Tweener.GroupAttribute), false);
                    if (attribute is UnityEngine.UI.Windows.Utilities.Tweener.GroupAttribute groupAttribute) {
                        this.groupHeader = groupAttribute.caption;
                    }
                    this.isGroup = attribute != null;
                }
                {
                    var attribute = memInfo[0].GetCustomAttribute(typeof(UnityEngine.UI.Windows.Utilities.Tweener.InspectorOrderAttribute), false);
                    if (attribute is UnityEngine.UI.Windows.Utilities.Tweener.InspectorOrderAttribute orderAttribute) {
                        this.order = orderAttribute.order;
                    }
                }
            }

            public bool IsGroup() => this.isGroup;

            public string GetCaption() {
                return this.type.ToString();
            }

            public void OnGUI(Rect rect) {

                DrawProgress(rect, this.type, (float)this.progress, this.GetCaption(), this.selected);

            }

            public static void DrawProgress(Rect rect, UnityEngine.UI.Windows.Utilities.Tweener.EaseFunction func, float progress, string caption, bool selected) {

                const float sizeX = 10f;
                const float sizeY = 10f;
                const int precision = 100;

                var funcEase = UnityEngine.UI.Windows.Utilities.Tweener.EaseFunctions.GetEase(func);

                GUI.Box(rect, Texture2D.blackTexture);
                if (string.IsNullOrEmpty(caption) == false) GUI.Label(rect, caption, Styles.label);

                if (selected == true) {
                    GUILayoutExt.HandlesDrawBoxNotFilled(rect, 2f, new Color(0.04f, 0.97f, 1f, 0.7f));
                }

                Handles.BeginGUI();
                var selectedIndex = 0;
                var points = System.Buffers.ArrayPool<Vector3>.Shared.Rent(precision);
                var p1 = new Vector2(rect.x, rect.y + rect.height);
                for (int i = 0; i < precision; ++i) {
                    var p = i / (float)precision;
                    var p2 = new Vector2(rect.x + rect.width * p, rect.y + rect.height - funcEase.Invoke(p, 0f, 1f, 1f) * rect.height);
                    if (progress >= p) {
                        selectedIndex = i;
                    }
                    points[i] = p1;
                    p1 = p2;
                }
                Handles.color = Color.white;
                Handles.DrawAAPolyLine(2f, precision, points);
                Handles.color = Color.yellow;
                Handles.DrawAAPolyLine(4f, selectedIndex, points);
                System.Buffers.ArrayPool<Vector3>.Shared.Return(points);
                Handles.EndGUI();

                if (progress > 0f) {
                    var boxRect = new Rect(rect.x + rect.width * progress - sizeX * 0.5f, rect.y + rect.height - funcEase.Invoke(progress, 0f, 1f, 1f) * rect.height - sizeY * 0.5f, sizeX, sizeY);
                    GUI.Box(boxRect, string.Empty, Styles.sliderDot);
                }

            }
            
            public void Play() {

                var time = EditorApplication.timeSinceStartup;
                var deltaTime = time - this.prevTime;
                this.prevTime = time;

                this.progress += deltaTime;
                if (this.progress >= 1d) {
                    this.progress = 0d;
                }

            }

            public void Pause() {

                this.progress = 0d;
                this.prevTime = 0d;

            }

            public UnityEngine.UI.Windows.Utilities.Tweener.EaseFunction GetTypeValue() {
                return this.type;
            }

            public string GetGroupHeader() {
                return this.groupHeader;
            }

            public int GetOrder() {
                return this.order;
            }

        }
        
        public EaseEditorWindow(UnityEngine.UI.Windows.Utilities.Tweener.EaseFunction selected) {
            var values = System.Enum.GetValues(typeof(UnityEngine.UI.Windows.Utilities.Tweener.EaseFunction));
            this.items = new System.Collections.Generic.List<Item>();
            foreach (var value in values) {
                var func = (UnityEngine.UI.Windows.Utilities.Tweener.EaseFunction)value;
                this.items.Add(new Item(func, func == selected));
            }

            this.items = this.items.OrderBy(x => x.GetOrder()).ToList();
        }

        public override Vector2 GetWindowSize() {

            var xMax = 0;
            var yMax = 0;
            var x = 0;
            var y = 0;
            var captionsOffset = 0f;
            for (var index = 0; index < this.items.Count; ++index) {
                ++x;
                var isGroup = (index < this.items.Count - 1 && this.items[index + 1].IsGroup());
                if (isGroup == true) {
                    captionsOffset += HEADER_HEIGHT;
                }
                if (x % COLUMNS == 0 || isGroup == true) {
                    ++y;
                    xMax = Mathf.Max(xMax, x);
                    yMax = Mathf.Max(yMax, y);
                    x = 0;
                }
            }

            return new Vector2(ELEMENT_WIDTH * xMax, ELEMENT_HEIGHT * yMax + captionsOffset);
            
        }

        public override void OnGUI(Rect rect) {
            
            base.OnGUI(rect);

            const float spacing = 5f;
            const float halfSpacing = spacing * 0.5f;

            var captionsOffset = 0f;
            var x = 0;
            var y = 0;
            for (var index = 0; index < this.items.Count; ++index) {
                var item = this.items[index];
                var elementRect = new UnityEngine.Rect(ELEMENT_WIDTH * x + halfSpacing, ELEMENT_HEIGHT * y + halfSpacing + captionsOffset, ELEMENT_WIDTH - spacing, ELEMENT_HEIGHT - spacing);
                item.OnGUI(elementRect);
                if (GUI.Button(elementRect, string.Empty, GUIStyle.none) == true) {
                    if (this.onSelect != null) this.onSelect.Invoke(item.GetTypeValue());
                    this.editorWindow.Close();
                }

                if (Event.current.type == EventType.Repaint) {
                    if (elementRect.Contains(Event.current.mousePosition) == true) {
                        // Start Anim
                        item.Play();
                    } else {
                        // End Anim
                        item.Pause();
                    }
                }

                ++x;
                var isGroup = (index < this.items.Count - 1 && this.items[index + 1].IsGroup());
                if (isGroup == true) {
                    captionsOffset += HEADER_HEIGHT;
                    var headerRect = new Rect(halfSpacing, elementRect.y + elementRect.height, elementRect.width, HEADER_HEIGHT);
                    GUI.Label(headerRect, this.items[index + 1].GetGroupHeader(), Styles.headerLabel);
                }
                if (x % COLUMNS == 0 || isGroup == true) {
                    ++y;
                    x = 0;
                }
            }

            this.editorWindow.Repaint();
            
        }

    }

    [CustomPropertyDrawer(typeof(UnityEngine.UI.Windows.Modules.AnimationParameters.ShowHideParameters))]
    public class ShowHideAnimationParametersPropertyDrawer : PropertyDrawer {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            
            return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2f;
            
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            
            var duration = property.FindPropertyRelative(nameof(AnimationParameters.ShowHideParameters.duration));
            var delay = property.FindPropertyRelative(nameof(AnimationParameters.ShowHideParameters.delay));
            var ease = property.FindPropertyRelative(nameof(AnimationParameters.ShowHideParameters.ease));

            const float offset = 15f;
            const float labelWidth = 80f;
            const float easeWidth = 120f;
            var s = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            
            var headerLabel = new GUIStyle(EditorStyles.miniBoldLabel);
            headerLabel.alignment = TextAnchor.LowerCenter;
            
            EditorGUI.BeginProperty(position, label, property);
            Rect durationRect = default;
            {
                var rect = position;
                rect.height -= s;
                rect.x += labelWidth;
                rect.width -= labelWidth;
                rect.width -= easeWidth;
                rect.width /= 2f;
                durationRect = rect;
                GUI.Label(rect, "duration", headerLabel);
            }
            {
                var rect = position;
                rect.height -= s;
                rect.x += labelWidth + durationRect.width;
                rect.width -= labelWidth;
                rect.width -= easeWidth;
                rect.width /= 2f;
                GUI.Label(rect, "delay", headerLabel);
            }
            {
                var rect = position;
                rect.height -= s;
                rect.x += labelWidth + durationRect.width * 2f;
                rect.width = easeWidth;
                GUI.Label(rect, "ease", headerLabel);
            }
            {
                var rect = position;
                rect.height -= s;
                rect.y += s;
                GUI.Box(rect, string.Empty, EditorStyles.toolbar);
            }
            {
                var rect = position;
                rect.height -= s;
                rect.y += s;
                rect.width = labelWidth;
                GUI.Label(rect, label);
            }
            {
                var rect = position;
                rect.y += s;
                rect.height -= s;
                rect.x += labelWidth;
                rect.width -= labelWidth;
                rect.width -= easeWidth;
                rect.width /= 2f;
                EditorGUI.PropertyField(rect, duration, GUIContent.none);
            }
            {
                var rect = position;
                rect.y += s;
                rect.height -= s;
                rect.x += labelWidth + durationRect.width;
                rect.width -= labelWidth;
                rect.width -= easeWidth;
                rect.width /= 2f;
                EditorGUI.PropertyField(rect, delay, GUIContent.none);
            }
            {
                var rect = position;
                rect.y += s;
                rect.height -= s;
                rect.x += labelWidth + durationRect.width * 2f;
                rect.width = easeWidth;
                EditorGUI.PropertyField(rect, ease, GUIContent.none);
            }
            
            /*
            position.x += offset;
            position.width *= 2f;
            position.width -= offset;

            GUILayout.BeginArea(position);
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            {
                GUILayout.Label(string.Empty, headerLabel, GUILayout.Width(labelWidth));
                GUILayout.Label("duration", headerLabel, GUILayout.ExpandWidth(true));
                GUILayout.Label("delay", headerLabel, GUILayout.ExpandWidth(true));
                GUILayout.Label("ease", headerLabel, GUILayout.Width(easeWidth));
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            {
                GUILayout.Label(label, GUILayout.Width(labelWidth));
                EditorGUILayout.PropertyField(duration, GUIContent.none, GUILayout.ExpandWidth(true));
                EditorGUILayout.PropertyField(delay, GUIContent.none, GUILayout.ExpandWidth(true));
                EditorGUILayout.PropertyField(ease, GUIContent.none, GUILayout.Width(easeWidth));
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndArea();
            EditorGUI.EndProperty();
            */
            
        }
        
    }

    [CustomPropertyDrawer(typeof(UnityEngine.UI.Windows.Modules.AnimationParameters.RandomValue))]
    public class RandomValueAnimationParametersPropertyDrawer : PropertyDrawer {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            
            return EditorGUIUtility.singleLineHeight;
            
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            
            var randomFromTo = property.FindPropertyRelative(nameof(AnimationParameters.RandomValue.randomFromTo));
            var random = property.FindPropertyRelative(nameof(AnimationParameters.ShowHideParameters.delay.random));
            
            const float buttonSize = 20f;
            var dropdownRect = new Rect(position.x + position.width - buttonSize, position.y, buttonSize, position.height);
            
            position.width -= buttonSize;
            
            EditorGUI.BeginProperty(position, label, property);
            if (random.boolValue == true) {
                const float spacing = 12f;
                var lblStyle = new GUIStyle(EditorStyles.miniBoldLabel);
                lblStyle.alignment = TextAnchor.MiddleCenter;
                var x = EditorGUI.DelayedFloatField(new Rect(position.x, position.y, position.width * 0.5f - spacing * 0.5f, position.height), string.Empty, randomFromTo.vector2Value.x);
                GUI.Label(new Rect(position.x + position.width * 0.5f - spacing * 0.5f, position.y, spacing, position.height), "..", lblStyle);
                var y = EditorGUI.DelayedFloatField(new Rect(position.x + position.width * 0.5f + spacing * 0.5f, position.y, position.width * 0.5f, position.height), string.Empty, randomFromTo.vector2Value.y);
                if (GUI.changed == true) {
                    randomFromTo.vector2Value = this.Validate(new Vector2(x, y), x != randomFromTo.vector2Value.x);
                }
            } else {
                var newValue = EditorGUI.FloatField(position, randomFromTo.vector2Value.x);
                if (newValue != randomFromTo.vector2Value.x) {
                    var v = randomFromTo.vector2Value;
                    v.x = newValue;
                    randomFromTo.vector2Value = v;
                }
            }

            if (EditorGUI.DropdownButton(dropdownRect, GUIContent.none, FocusType.Passive, EditorStyles.toolbarDropDown) == true) {
                var genericMenu = new GenericMenu();
                genericMenu.AddItem(new GUIContent("Single"), random.boolValue == false, () => {
                    property.serializedObject.Update();
                    random.boolValue = false;
                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                });
                genericMenu.AddItem(new GUIContent("Random"), random.boolValue == true, () => {
                    property.serializedObject.Update();
                    random.boolValue = true;
                    randomFromTo.vector2Value = this.Validate(randomFromTo.vector2Value, true);
                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                });
                genericMenu.DropDown(position);
            }
            EditorGUI.EndProperty();
            
        }

        private Vector2 Validate(Vector2 value, bool xChanged) {
            if (value.x < 0f) {
                value.x = 0f;
            }
            if (xChanged == true) {
                if (value.x > value.y) value.y = value.x;
            } else {
                if (value.y < value.x) value.x = value.y;
            }
            return value;
        }

    }

}
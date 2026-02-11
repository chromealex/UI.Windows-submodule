using SafeZone = UnityEngine.UI.Windows.SafeArea;

namespace UnityEditor.UI.Windows.SafeArea {

    [CustomPropertyDrawer(typeof(SafeZone))]
    public class SafeAreaDrawer : PropertyDrawer {

        private const float PADDING = 4f;

        public override float GetPropertyHeight(SerializedProperty property, UnityEngine.GUIContent label) {

            var count = 1;
            var config = property.FindPropertyRelative(nameof(SafeZone.config));
            if (config.objectReferenceValue == null) {
                count = 3;
            }

            return PADDING + EditorGUIUtility.singleLineHeight * count + EditorGUIUtility.standardVerticalSpacing * (count - 1) + PADDING;

        }

        public override void OnGUI(UnityEngine.Rect position, SerializedProperty property, UnityEngine.GUIContent label) {

            {
                var lineRect = position;
                lineRect.height = 1f;
                lineRect.y += PADDING * 0.5f;
                EditorGUI.DrawRect(lineRect, new UnityEngine.Color(1f, 1f, 1f, 0.3f));
            }
            var config = property.FindPropertyRelative(nameof(SafeZone.config));
            var paddingType = property.FindPropertyRelative(nameof(SafeZone.paddingType));
            var customPaddings = property.FindPropertyRelative(nameof(SafeZone.customPaddings));
            {
                var rect = position;
                rect.y += PADDING;
                rect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(rect, config);
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            if (config.objectReferenceValue == null) {
                {
                    var rect = position;
                    rect.height = EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(rect, paddingType);
                }
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                {
                    var rect = position;
                    rect.height = EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(rect, customPaddings);
                }
                position.y += EditorGUIUtility.singleLineHeight;
            }
            {
                var lineRect = position;
                lineRect.height = 1f;
                lineRect.y += PADDING * 0.5f;
                EditorGUI.DrawRect(lineRect, new UnityEngine.Color(1f, 1f, 1f, 0.3f));
            }

        }

    }

}
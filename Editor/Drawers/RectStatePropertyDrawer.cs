using UnityEngine;
using UnityEngine.UI.Windows.Modules;

namespace UnityEditor.UI.Windows {

    [CustomPropertyDrawer(typeof(UnityEngine.UI.Windows.Modules.RectAnimationParameters.RectState))]
    public class RectStatePropertyDrawer : PropertyDrawer {

        private bool anchorPosState = true;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            
            if (this.anchorPosState == false) return EditorGUIUtility.singleLineHeight;

            const int lines = 3; // anchor, rotation, scale
            return EditorGUIUtility.singleLineHeight * (lines + 1) + EditorGUIUtility.standardVerticalSpacing * (lines + 1);
            
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            
            EditorGUI.BeginProperty(position, label, property);

            // Calculate foldout rect and draw it
            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            this.anchorPosState = EditorGUI.Foldout(foldoutRect, this.anchorPosState, property.displayName, true);

            if (this.anchorPosState == false) {
                EditorGUI.EndProperty();
                return;
            }

            EditorGUI.indentLevel++;
            float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            position.y += lineHeight;

            DrawVector2WithToggle(ref position, property, nameof(RectAnimationParameters.RectState.anchorPosition), nameof(RectAnimationParameters.RectState.useInitialAnchorPosition));
            DrawVector3WithToggle(ref position, property, nameof(RectAnimationParameters.RectState.rotation), nameof(RectAnimationParameters.RectState.useInitialRotation));
            DrawVector3WithToggle(ref position, property, nameof(RectAnimationParameters.RectState.scale), nameof(RectAnimationParameters.RectState.useInitialScale));

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
            
        }
        
        private static void DrawVector2WithToggle(ref Rect position, SerializedProperty parent, string valueProp, string toggleProp) {
            
            var value = parent.FindPropertyRelative(valueProp);
            var toggle = parent.FindPropertyRelative(toggleProp);

            const float toggleWidth = 20f;
            Rect fieldRect = new Rect(position.x, position.y, position.width - toggleWidth, EditorGUIUtility.singleLineHeight);
            Rect toggleRect = new Rect(position.x + position.width - toggleWidth, position.y, toggleWidth, EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginDisabledGroup(toggle.boolValue);
            value.vector2Value = EditorGUI.Vector2Field(fieldRect, ObjectNames.NicifyVariableName(value.name), value.vector2Value);
            EditorGUI.EndDisabledGroup();

            toggle.boolValue = GUI.Toggle(toggleRect, toggle.boolValue, EditorGUIUtility.IconContent(toggle.boolValue ? "d_Linked" : "d_Unlinked"), GUIStyle.none);

            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            
        }

        private static void DrawVector3WithToggle(ref Rect position, SerializedProperty parent, string valueProp, string toggleProp) {
            
            var value = parent.FindPropertyRelative(valueProp);
            var toggle = parent.FindPropertyRelative(toggleProp);

            const float toggleWidth = 20f;
            Rect fieldRect = new Rect(position.x, position.y, position.width - toggleWidth, EditorGUIUtility.singleLineHeight);
            Rect toggleRect = new Rect(position.x + position.width - toggleWidth, position.y, toggleWidth, EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginDisabledGroup(toggle.boolValue);
            value.vector3Value = EditorGUI.Vector3Field(fieldRect, ObjectNames.NicifyVariableName(value.name), value.vector3Value);
            EditorGUI.EndDisabledGroup();

            toggle.boolValue = GUI.Toggle(toggleRect, toggle.boolValue, EditorGUIUtility.IconContent(toggle.boolValue ? "d_Linked" : "d_Unlinked"), GUIStyle.none);

            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            
        }

    }

}
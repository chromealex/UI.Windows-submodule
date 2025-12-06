using UnityEngine;
using UnityEngine.UI.Windows.Modules;

namespace UnityEditor.UI.Windows {

    [CustomPropertyDrawer(typeof(UnityEngine.UI.Windows.Modules.RectAnimationParameters.RectState))]
    public class RectStatePropertyDrawer : PropertyDrawer {

        private bool anchorPosState = true;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            
            if (this.anchorPosState == false) return EditorGUIUtility.singleLineHeight;

            int lines = 0;
            var visible = (RectAnimationParameters.AnimationParameter)property.FindPropertyRelative(nameof(RectAnimationParameters.RectState.visible)).enumValueFlag;
            if ((visible & RectAnimationParameters.AnimationParameter.Position) != 0) ++lines;
            if ((visible & RectAnimationParameters.AnimationParameter.Rotation) != 0) ++lines;
            if ((visible & RectAnimationParameters.AnimationParameter.Scale) != 0) ++lines;
            if ((visible & RectAnimationParameters.AnimationParameter.Size) != 0) ++lines;
            if ((visible & RectAnimationParameters.AnimationParameter.Pivot) != 0) ++lines;
            if ((visible & RectAnimationParameters.AnimationParameter.AnchorsMin) != 0) ++lines;
            if ((visible & RectAnimationParameters.AnimationParameter.AnchorsMax) != 0) ++lines;

            return EditorGUIUtility.singleLineHeight * (lines + 1) + EditorGUIUtility.standardVerticalSpacing * (lines + 1);
            
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            
            EditorGUI.BeginProperty(position, label, property);

            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            this.anchorPosState = EditorGUI.Foldout(foldoutRect, this.anchorPosState, property.displayName, true);

            if (this.anchorPosState == false) {
                EditorGUI.EndProperty();
                return;
            }

            ++EditorGUI.indentLevel;
            float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            position.y += lineHeight;

            var parameters = property.FindPropertyRelative(nameof(RectAnimationParameters.RectState.parameters));
            var visible = (RectAnimationParameters.AnimationParameter)property.FindPropertyRelative(nameof(RectAnimationParameters.RectState.visible)).enumValueFlag;
            if ((visible & RectAnimationParameters.AnimationParameter.Position) != 0) DrawField(ref position, property, nameof(RectAnimationParameters.RectState.anchorPosition), RectAnimationParameters.AnimationParameter.Position, parameters);
            if ((visible & RectAnimationParameters.AnimationParameter.Rotation) != 0) DrawField(ref position, property, nameof(RectAnimationParameters.RectState.rotation), RectAnimationParameters.AnimationParameter.Rotation, parameters);
            if ((visible & RectAnimationParameters.AnimationParameter.Scale) != 0) DrawField(ref position, property, nameof(RectAnimationParameters.RectState.scale), RectAnimationParameters.AnimationParameter.Scale, parameters);
            if ((visible & RectAnimationParameters.AnimationParameter.Size) != 0) DrawField(ref position, property, nameof(RectAnimationParameters.RectState.size), RectAnimationParameters.AnimationParameter.Size, parameters);
            if ((visible & RectAnimationParameters.AnimationParameter.Pivot) != 0) DrawField(ref position, property, nameof(RectAnimationParameters.RectState.pivot), RectAnimationParameters.AnimationParameter.Pivot, parameters);
            if ((visible & RectAnimationParameters.AnimationParameter.AnchorsMin) != 0) DrawField(ref position, property, nameof(RectAnimationParameters.RectState.anchorsMin), RectAnimationParameters.AnimationParameter.AnchorsMin, parameters);
            if ((visible & RectAnimationParameters.AnimationParameter.AnchorsMax) != 0) DrawField(ref position, property, nameof(RectAnimationParameters.RectState.anchorsMax), RectAnimationParameters.AnimationParameter.AnchorsMax, parameters);

            --EditorGUI.indentLevel;
            EditorGUI.EndProperty();
            
        }
        
        private static void DrawField(ref Rect position, SerializedProperty parent, string valueProp, RectAnimationParameters.AnimationParameter flag, SerializedProperty parameters) {
            
            var value = parent.FindPropertyRelative(valueProp);
            var flags = (RectAnimationParameters.AnimationParameter)parameters.enumValueFlag;
            var isSet = (flags & flag) == 0;

            const float toggleWidth = 20f;
            Rect fieldRect = new Rect(position.x, position.y, position.width - toggleWidth, EditorGUIUtility.singleLineHeight);
            Rect toggleRect = new Rect(position.x + position.width - toggleWidth, position.y, toggleWidth, EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginDisabledGroup(isSet);
            EditorGUI.PropertyField(fieldRect, value);
            EditorGUI.EndDisabledGroup();

            var res = GUI.Toggle(toggleRect, isSet, EditorGUIUtility.IconContent(isSet == true ? "d_Linked" : "d_Unlinked"), GUIStyle.none);
            if (res != isSet) {
                if (res == true) {
                    parameters.enumValueFlag &= ~(int)flag;
                } else {
                    parameters.enumValueFlag |= (int)flag;
                }
            }

            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            
        }

    }

}
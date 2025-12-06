using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI.Windows.Modules;
using UnityEngine.UIElements;

namespace UnityEditor.UI.Windows {

    [CustomPropertyDrawer(typeof(UnityEngine.UI.Windows.Modules.AnimationParameters.ShowHideParameters))]
    public class ShowHideAnimationParametersPropertyDrawer : PropertyDrawer {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            
            return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2 + 10f;
            
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            
            var duration = property.FindPropertyRelative(nameof(AnimationParameters.ShowHideParameters.duration));
            var delay = property.FindPropertyRelative(nameof(AnimationParameters.ShowHideParameters.delay));
            var ease = property.FindPropertyRelative(nameof(AnimationParameters.ShowHideParameters.ease));

            const float offset = 15f;
            const float labelWidth = 120f;
            const float durationWidth = 80f;
            const float easeWidth = 120f;
            
            position.x += offset;
            position.width *= 2f;
            position.width -= offset;

            var headerLabel = new GUIStyle(EditorStyles.miniBoldLabel);
            headerLabel.alignment = TextAnchor.LowerCenter;
            
            EditorGUI.BeginProperty(position, label, property);
            GUILayout.BeginArea(position, EditorStyles.toolbar);
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            {
                GUILayout.Label(string.Empty, headerLabel, GUILayout.Width(labelWidth));
                GUILayout.Label("duration", headerLabel, GUILayout.Width(durationWidth));
                GUILayout.Label("delay", headerLabel, GUILayout.ExpandWidth(true));
                GUILayout.Label("ease", headerLabel, GUILayout.Width(easeWidth));
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            {
                GUILayout.Label(label, GUILayout.Width(labelWidth));
                EditorGUILayout.PropertyField(duration, GUIContent.none, GUILayout.Width(durationWidth));
                EditorGUILayout.PropertyField(delay, GUIContent.none, GUILayout.ExpandWidth(true));
                EditorGUILayout.PropertyField(ease, GUIContent.none, GUILayout.Width(easeWidth));
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndArea();
            EditorGUI.EndProperty();
            
        }
        
    }

    [CustomPropertyDrawer(typeof(UnityEngine.UI.Windows.Modules.AnimationParameters.Delay))]
    public class DelayAnimationParametersPropertyDrawer : PropertyDrawer {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            
            return EditorGUIUtility.singleLineHeight;
            
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            
            var randomFromTo = property.FindPropertyRelative(nameof(AnimationParameters.Delay.randomFromTo));
            var random = property.FindPropertyRelative(nameof(AnimationParameters.ShowHideParameters.delay.random));
            
            const float buttonSize = 20f;
            var dropdownRect = new Rect(position.x + position.width - buttonSize, position.y, buttonSize, position.height);
            
            position.width -= buttonSize;
            
            EditorGUI.BeginProperty(position, label, property);
            if (random.boolValue == true) {
                //EditorGUILayout.PropertyField(delayFromTo, GUIContent.none, GUILayout.ExpandWidth(true));
                var x = EditorGUI.FloatField(new Rect(position.x, position.y, position.width * 0.5f, position.height), string.Empty, randomFromTo.vector2Value.x);
                var y = EditorGUI.FloatField(new Rect(position.x + position.width * 0.5f, position.y, position.width * 0.5f, position.height), string.Empty, randomFromTo.vector2Value.y);
                if (x != randomFromTo.vector2Value.x || y != randomFromTo.vector2Value.y) {
                    randomFromTo.vector2Value = new Vector2(x, y);
                }
            } else {
                //EditorGUILayout.PropertyField(delay, GUIContent.none, GUILayout.ExpandWidth(true));
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
                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                });
                genericMenu.DropDown(position);
            }
            EditorGUI.EndProperty();
            
        }
        
    }

}
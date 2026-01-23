using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEditor.AnimatedValues;

namespace UnityEditor.UI {

    [CustomEditor(typeof(ButtonExtended))]
    public class ButtonExtendedEditor : ButtonEditor {

        private SerializedProperty transition;
        private SerializedProperty transitionExtended;

        private SerializedProperty extScaleBlockProperty;
        private SerializedProperty extAlphaBlockProperty;
        private SerializedProperty extColorBlockProperty;
        private SerializedProperty extSpriteStateProperty;
        private SerializedProperty extAnimTriggerProperty;
        private SerializedProperty extTargetGraphicProperty;
        private SerializedProperty extTargetGraphicsProperty;
        private SerializedProperty extTargetGraphicsItemsProperty;

        private AnimBool extShowScale = new AnimBool();
        private AnimBool extShowAlpha = new AnimBool();
        private AnimBool extShowColorTint = new AnimBool();
        private AnimBool extShowSpriteTrasition = new AnimBool();
        private AnimBool extShowAnimTransition = new AnimBool();
        private AnimBool extShowTargetGraphicsItems = new AnimBool();
        private bool lastInteractive;

        protected override void OnEnable() {

            base.OnEnable();

            this.transition = this.serializedObject.FindProperty("m_Transition");
            this.transitionExtended = this.serializedObject.FindProperty("transitionExtended");

            this.extScaleBlockProperty = this.serializedObject.FindProperty("m_Scale");
            this.extAlphaBlockProperty = this.serializedObject.FindProperty("m_Alpha");
            this.extColorBlockProperty = this.serializedObject.FindProperty("m_Colors");
            this.extSpriteStateProperty = this.serializedObject.FindProperty("m_SpriteState");
            this.extAnimTriggerProperty = this.serializedObject.FindProperty("m_AnimationTriggers");
            this.extTargetGraphicProperty = this.serializedObject.FindProperty("m_TargetGraphic");
            this.extTargetGraphicsProperty = this.serializedObject.FindProperty("m_TargetGraphics");
            this.extTargetGraphicsItemsProperty = this.serializedObject.FindProperty("graphicItems");

            var trans = GetTransition(this.transitionExtended);
            this.extShowScale.value = (trans & ButtonExtended.Transition.Scale) != 0;
            this.extShowAlpha.value = (trans & ButtonExtended.Transition.CanvasGroupAlpha) != 0;
            this.extShowColorTint.value = (trans & ButtonExtended.Transition.ColorTint) != 0;
            this.extShowSpriteTrasition.value = (trans & ButtonExtended.Transition.SpriteSwap) != 0;
            this.extShowAnimTransition.value = (trans & ButtonExtended.Transition.Animation) != 0;
            this.extShowTargetGraphicsItems.value = (trans & ButtonExtended.Transition.TargetGraphics) != 0;

            this.extShowScale.valueChanged.AddListener(this.Repaint);
            this.extShowAlpha.valueChanged.AddListener(this.Repaint);
            this.extShowColorTint.valueChanged.AddListener(this.Repaint);
            this.extShowSpriteTrasition.valueChanged.AddListener(this.Repaint);

        }

        protected override void OnDisable() {

            base.OnDisable();

            this.extShowScale.valueChanged.RemoveListener(this.Repaint);
            this.extShowAlpha.valueChanged.RemoveListener(this.Repaint);
            this.extShowColorTint.valueChanged.RemoveListener(this.Repaint);
            this.extShowSpriteTrasition.valueChanged.RemoveListener(this.Repaint);

        }

        private static ButtonExtended.Transition GetTransition(SerializedProperty transition) {

            return (ButtonExtended.Transition)transition.intValue;

        }

        public override void OnInspectorGUI() {

            base.OnInspectorGUI();

            this.serializedObject.Update();

            // this.transition.enumValueIndex = 0;
            if (this.transitionExtended.intValue > this.transition.intValue) {

                this.transition.intValue = 0;

            } else {

                this.transition.intValue = this.transitionExtended.intValue;

            }

            var trans = GetTransition(this.transitionExtended);
            var animator = (this.target as Selectable).GetComponent<Animator>();
            this.extShowScale.target = !this.transitionExtended.hasMultipleDifferentValues && (trans & ButtonExtended.Transition.Scale) != 0;
            this.extShowAlpha.target = !this.transitionExtended.hasMultipleDifferentValues && (trans & ButtonExtended.Transition.CanvasGroupAlpha) != 0;
            this.extShowColorTint.target = !this.transitionExtended.hasMultipleDifferentValues && (trans & ButtonExtended.Transition.ColorTint) != 0;
            this.extShowSpriteTrasition.target = !this.transitionExtended.hasMultipleDifferentValues && (trans & ButtonExtended.Transition.SpriteSwap) != 0;
            this.extShowAnimTransition.target = !this.transitionExtended.hasMultipleDifferentValues && (trans & ButtonExtended.Transition.Animation) != 0;
            this.extShowTargetGraphicsItems.target = !this.transitionExtended.hasMultipleDifferentValues && (trans & ButtonExtended.Transition.TargetGraphics) != 0;

            EditorGUILayout.PropertyField(this.transitionExtended);

            if ((trans & ButtonExtended.Transition.ColorTint) != 0 ||
                (trans & ButtonExtended.Transition.SpriteSwap) != 0) {
                EditorGUILayout.PropertyField(this.extTargetGraphicProperty);
                EditorGUILayout.PropertyField(this.extTargetGraphicsProperty, true);
                EditorGUILayout.Space();
            }

            if (EditorGUILayout.BeginFadeGroup(this.extShowColorTint.faded)) {
                EditorGUILayout.PropertyField(this.extColorBlockProperty);
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndFadeGroup();

            if (EditorGUILayout.BeginFadeGroup(this.extShowAlpha.faded)) {
                EditorGUILayout.PropertyField(this.extAlphaBlockProperty, true);
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndFadeGroup();

            if (EditorGUILayout.BeginFadeGroup(this.extShowScale.faded)) {
                EditorGUILayout.PropertyField(this.extScaleBlockProperty, true);
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndFadeGroup();

            if (EditorGUILayout.BeginFadeGroup(this.extShowSpriteTrasition.faded)) {
                EditorGUILayout.PropertyField(this.extSpriteStateProperty);
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndFadeGroup();

            if (EditorGUILayout.BeginFadeGroup(this.extShowAnimTransition.faded)) {
                EditorGUILayout.PropertyField(this.extAnimTriggerProperty);

                if (animator == null || animator.runtimeAnimatorController == null) {
                    var buttonRect = EditorGUILayout.GetControlRect();
                    buttonRect.xMin += EditorGUIUtility.labelWidth;
                }
            }

            EditorGUILayout.EndFadeGroup();

            if (EditorGUILayout.BeginFadeGroup(this.extShowTargetGraphicsItems.faded)) {
                if (this.extTargetGraphicsItemsProperty != null) {
                    EditorGUILayout.PropertyField(this.extTargetGraphicsItemsProperty, true);
                }

                EditorGUILayout.Space();
            }

            EditorGUILayout.EndFadeGroup();

            this.serializedObject.ApplyModifiedProperties();

        }

    }

}
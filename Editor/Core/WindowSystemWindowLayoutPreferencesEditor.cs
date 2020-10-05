using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;
    
    [CustomEditor(typeof(WindowLayoutPreferences), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class WindowSystemWindowLayoutPreferencesEditor : Editor {

        SerializedProperty m_UiScaleMode;
        SerializedProperty m_ScaleFactor;
        SerializedProperty m_ReferenceResolution;
        SerializedProperty m_ScreenMatchMode;
        SerializedProperty m_MatchWidthOrHeight;
        SerializedProperty m_PhysicalUnit;
        SerializedProperty m_FallbackScreenDPI;
        SerializedProperty m_DefaultSpriteDPI;
        SerializedProperty m_DynamicPixelsPerUnit;
        SerializedProperty m_ReferencePixelsPerUnit;
        
        const int kSliderEndpointLabelsHeight = 12;

        private class Styles
        {
            public GUIContent matchContent;
            public GUIContent widthContent;
            public GUIContent heightContent;
            public GUIContent uiScaleModeContent;
            public GUIStyle leftAlignedLabel;
            public GUIStyle rightAlignedLabel;

            public Styles()
            {
                matchContent = EditorGUIUtility.TrTextContent("Match");
                widthContent = EditorGUIUtility.TrTextContent("Width");
                heightContent = EditorGUIUtility.TrTextContent("Height");
                uiScaleModeContent = EditorGUIUtility.TrTextContent("UI Scale Mode");

                leftAlignedLabel = new GUIStyle(EditorStyles.label);
                rightAlignedLabel = new GUIStyle(EditorStyles.label);
                rightAlignedLabel.alignment = TextAnchor.MiddleRight;
            }
        }
        private static Styles s_Styles;
        
        private void OnEnable() {

            var scalerData = this.serializedObject.FindProperty("canvasScalerData");
            m_UiScaleMode = scalerData.FindPropertyRelative("uiScaleMode");
            m_ScaleFactor = scalerData.FindPropertyRelative("scaleFactor");
            m_ReferenceResolution = scalerData.FindPropertyRelative("referenceResolution");
            m_ScreenMatchMode = scalerData.FindPropertyRelative("screenMatchMode");
            m_MatchWidthOrHeight = scalerData.FindPropertyRelative("matchWidthOrHeight");
            m_PhysicalUnit = scalerData.FindPropertyRelative("physicalUnit");
            m_FallbackScreenDPI = scalerData.FindPropertyRelative("fallbackScreenDPI");
            m_DefaultSpriteDPI = scalerData.FindPropertyRelative("defaultSpriteDPI");
            m_DynamicPixelsPerUnit = scalerData.FindPropertyRelative("dynamicPixelsPerUnit");
            m_ReferencePixelsPerUnit = scalerData.FindPropertyRelative("referencePixelsPerUnit");
            
        }

        public override void OnInspectorGUI() {
            
            if (s_Styles == null)
                s_Styles = new Styles();

            var settings = WindowSystem.GetSettings();
            var canvasSettings = settings.canvas;
            
            //bool allAreRoot = true;
            bool showWorldDiffers = false;
            bool showWorld = (canvasSettings.renderMode == RenderMode.WorldSpace);
                
            this.serializedObject.Update();
            
            EditorGUI.showMixedValue = showWorldDiffers;
            using (new EditorGUI.DisabledScope(showWorld || showWorldDiffers))
            {
                if (showWorld || showWorldDiffers)
                {
                    EditorGUILayout.Popup(s_Styles.uiScaleModeContent.text, 0, new[] { "World" });
                }
                else
                {
                    EditorGUILayout.PropertyField(m_UiScaleMode, s_Styles.uiScaleModeContent);
                }
            }
            EditorGUI.showMixedValue = false;

            if (!showWorldDiffers && !(!showWorld && m_UiScaleMode.hasMultipleDifferentValues))
            {
                
                EditorGUILayout.Space();

                // World Canvas
                if (showWorld)
                {
                    EditorGUILayout.PropertyField(m_DynamicPixelsPerUnit);
                }
                // Constant pixel size
                else if (m_UiScaleMode.enumValueIndex == (int)UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize)
                {
                    EditorGUILayout.PropertyField(m_ScaleFactor);
                }
                // Scale with screen size
                else if (m_UiScaleMode.enumValueIndex == (int)UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize)
                {
                    EditorGUILayout.PropertyField(m_ReferenceResolution);
                    EditorGUILayout.PropertyField(m_ScreenMatchMode);
                    if (m_ScreenMatchMode.enumValueIndex == (int)UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight && !m_ScreenMatchMode.hasMultipleDifferentValues)
                    {
                        Rect r = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight + kSliderEndpointLabelsHeight);
                        DualLabeledSlider(r, m_MatchWidthOrHeight, s_Styles.matchContent, s_Styles.widthContent, s_Styles.heightContent);
                    }
                }
                // Constant physical size
                else if (m_UiScaleMode.enumValueIndex == (int)UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPhysicalSize)
                {
                    EditorGUILayout.PropertyField(m_PhysicalUnit);
                    EditorGUILayout.PropertyField(m_FallbackScreenDPI);
                    EditorGUILayout.PropertyField(m_DefaultSpriteDPI);
                }

                EditorGUILayout.PropertyField(m_ReferencePixelsPerUnit);
            }

            this.serializedObject.ApplyModifiedProperties();

        }
        
        private static void DualLabeledSlider(Rect position, SerializedProperty property, GUIContent mainLabel, GUIContent labelLeft, GUIContent labelRight)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            Rect pos = position;

            position.y += 12;
            position.xMin += EditorGUIUtility.labelWidth;
            position.xMax -= EditorGUIUtility.fieldWidth;

            GUI.Label(position, labelLeft, s_Styles.leftAlignedLabel);
            GUI.Label(position, labelRight, s_Styles.rightAlignedLabel);

            EditorGUI.PropertyField(pos, property, mainLabel);
        }
    
    }
    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;
    using UnityEngine.UI.Windows.Utilities;

    [CustomPropertyDrawer(typeof(UIWSLayer))]
    public class WindowSystemLayerPropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            var value = property.FindPropertyRelative("value");

            var settings = WindowSystem.GetSettings();
            if (settings == null) return;

            var buttonRect = position;

            EditorGUI.LabelField(position, label);

            var labelWidth = EditorGUIUtility.labelWidth;

            var normalStyle = new GUIStyle(EditorStyles.miniButton);

            var padding = 2f;
            var layersVisible = 2;

            var selectedIndex = value.intValue;
            var elementsCount = layersVisible * 2 + 1;
            buttonRect.width = (position.width - labelWidth - padding * (elementsCount - 1)) / elementsCount;
            buttonRect.x += labelWidth;
            for (int i = selectedIndex - layersVisible; i <= layersVisible + selectedIndex; ++i) {

                var layerInfo = settings.GetLayerInfo(i);

                if (i == value.intValue) {

                    GUI.Label(buttonRect, layerInfo.name, normalStyle);
                    UnityEditor.UI.Windows.GUILayoutExt.DrawBoxNotFilled(buttonRect, 1f, Color.white);

                } else {

                    if (GUI.Button(buttonRect, layerInfo.name, normalStyle) == true) {

                        property.serializedObject.Update();
                        value.intValue = i;
                        property.serializedObject.ApplyModifiedProperties();

                    }

                }

                buttonRect.x += buttonRect.width + padding;

            }

        }

    }

}
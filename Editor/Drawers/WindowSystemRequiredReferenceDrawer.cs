using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows.Utilities;

    [CustomPropertyDrawer(typeof(RequiredReferenceAttribute))]
    public class WindowSystemRequiredReferenceDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            EditorGUI.PropertyField(position, property, label);

            if (property.objectReferenceValue == null) {

                WindowSystemRequiredReferenceDrawer.DrawRequired(position, RequiredType.Error);
                
            }

        }

        public static void DrawRequired(Rect fieldRect, RequiredType requiredType) {

            if (requiredType == RequiredType.None) return;

            string iconPath = string.Empty;
            string description = string.Empty;
            Color color = Color.white;
            switch (requiredType) {
                
                case RequiredType.Warning:
                    iconPath = "warning_icon";
                    description = "Reference is desired! May be you forgot to provide a link.";
                    color = new Color(0.7f, 0.7f, 0.3f, 1f);
                    break;

                case RequiredType.Error:
                    iconPath = "error_icon";
                    description = "Reference is required! Please provide a link, otherwise this component will not be able to start properly.";
                    color = new Color(0.7f, 0.3f, 0.3f, 1f);
                    break;

            }
            
            var icon = Resources.Load<Texture>("EditorAssets/" + iconPath);
            var iconRect = fieldRect;
            iconRect.width = iconRect.height;
            iconRect.x = fieldRect.width - iconRect.width;
            using (new GUILayoutExt.GUIColorUsing(color)) {
                    
                GUI.DrawTexture(iconRect, icon);
                GUI.Label(iconRect, new GUIContent(string.Empty, description));
                    
            }

        }

    }

}

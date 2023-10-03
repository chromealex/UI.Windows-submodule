using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Windows.Essentials.Tutorial;
using UnityEngine.UI.Windows;

namespace UnityEditor.UI.Windows.Essentials.Tutorial {

    [CustomPropertyDrawer(typeof(WindowType))]
    public class WindowTypeDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            var guid = property.FindPropertyRelative("guid");
            var type = property.FindPropertyRelative("type");
            var obj = AssetDatabase.LoadAssetAtPath<WindowObject>(AssetDatabase.GUIDToAssetPath(guid.stringValue));
            var newObj = EditorGUI.ObjectField(position, obj, typeof(WindowObject), false);
            if (newObj != obj) {

                if (newObj == null) {

                    guid.stringValue = string.Empty;
                    type.stringValue = string.Empty;

                } else {

                    guid.stringValue = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(newObj));
                    type.stringValue = newObj.GetType().FullName;

                }
                
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();

            }

        }

    }

}
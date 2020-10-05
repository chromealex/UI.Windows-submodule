using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;

    [CustomPropertyDrawer(typeof(WindowSystemTargets))]
    public class WindowSystemTargetsPropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            var target = property.FindPropertyRelative("target");
            EditorGUI.PropertyField(position, target, label);

        }

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;

    [CustomPropertyDrawer(typeof(UnityEngine.UI.Windows.Utilities.AnimationParametersAttribute))]
    public class WindowSystemAnimationParametersPropertyDrawer : PropertyDrawer {

        private UnityEditorInternal.ReorderableList list;
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

            var height = (this.list == null ? 0f : this.list.GetHeight());
            return height;
            
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            if (this.list == null) {
                
                var items = property.FindPropertyRelative("items");
                this.list = new UnityEditorInternal.ReorderableList(property.serializedObject, items, true, false, true, true);
                this.list.drawHeaderCallback = (rect) => {
                    
                    GUI.Label(rect, label);
                    
                };
                this.list.drawElementCallback = (rect, index, active, focused) => {
                    
                    EditorGUI.PropertyField(rect, items.GetArrayElementAtIndex(index), label, true);
                    
                };
                this.list.onRemoveCallback = (list) => {

                    var idx = list.index;
                    var item = items.GetArrayElementAtIndex(idx);
                    if (item.objectReferenceValue != null) {

                        Object.DestroyImmediate(item.objectReferenceValue, true);

                    }
                    items.DeleteArrayElementAtIndex(idx);

                };
                
            }
            
            this.list.DoList(position);
            
        }

    }

}

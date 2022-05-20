using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;

    [CustomPropertyDrawer(typeof(UnityEngine.UI.Windows.Utilities.AnimationParametersAttribute))]
    public class WindowSystemAnimationParametersPropertyDrawer : PropertyDrawer {

        private Dictionary<string, UnityEditorInternal.ReorderableList> dicList = new Dictionary<string, UnityEditorInternal.ReorderableList>();

        private UnityEditorInternal.ReorderableList Init(SerializedProperty property, GUIContent label) {

            var key = property.propertyPath + ":" + property.serializedObject.targetObject.GetInstanceID();
            if (this.dicList.TryGetValue(key, out var list) == false) {
                
                var items = property.FindPropertyRelative("items");
                list = new UnityEditorInternal.ReorderableList(property.serializedObject, items, true, false, true, true);
                list.drawHeaderCallback = (rect) => {
                    
                    GUI.Label(rect, label);
                    
                };
                list.onAddCallback = (list) => {

                    property.serializedObject.Update();
                    items.arraySize = items.arraySize + 1;
                    items.GetArrayElementAtIndex(items.arraySize - 1).objectReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();

                };
                list.drawElementCallback = (rect, index, active, focused) => {
                    
                    EditorGUI.PropertyField(rect, items.GetArrayElementAtIndex(index), label, true);
                    
                };
                list.onRemoveCallback = (list) => {

                    property.serializedObject.Update();
                    var idx = list.index;
                    var item = items.GetArrayElementAtIndex(idx);
                    if (item.objectReferenceValue != null) {

                        Object.DestroyImmediate(item.objectReferenceValue, true);

                    }
                    items.DeleteArrayElementAtIndex(idx);
                    property.serializedObject.ApplyModifiedProperties();

                };
                this.dicList.Add(key, list);
                
            }
            
            return list;

        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

            var list = this.Init(property, label);
            var height = list.GetHeight();
            return height;
            
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            var list = this.Init(property, label);
            list.DoList(position);
            
        }

    }

}

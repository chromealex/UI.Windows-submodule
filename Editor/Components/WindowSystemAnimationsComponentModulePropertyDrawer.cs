using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets.GUI;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;

    [CustomPropertyDrawer(typeof(AnimationsComponentModule.States))]
    public class WindowSystemAnimationsComponentModulePropertyDrawer : PropertyDrawer {

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
                    var item = items.GetArrayElementAtIndex(items.arraySize - 1);
                    item.FindPropertyRelative("parameters").FindPropertyRelative("items").ClearArray();
                    property.serializedObject.ApplyModifiedProperties();

                };
                list.elementHeightCallback = (index) => {
                    var h = 0f;
                    var prop = items.GetArrayElementAtIndex(index);
                    prop.NextVisible(true);
                    var depth = prop.depth;
                    do {
                        if (prop.depth != depth) break;
                        h += EditorGUI.GetPropertyHeight(prop, true);
                    } while (prop.NextVisible(false));

                    return h;
                };
                list.drawElementCallback = (rect, index, active, focused) => {

                    property.serializedObject.Update();
                    var splitterRect = rect;
                    splitterRect.height = 1f;
                    SplitterGUI.Splitter(splitterRect);
                    var prop = items.GetArrayElementAtIndex(index);
                    prop.NextVisible(true);
                    var depth = prop.depth;
                    do {
                        if (prop.depth != depth) break;
                        var h = EditorGUI.GetPropertyHeight(prop, true);
                        rect.height = h;
                        EditorGUI.PropertyField(rect, prop, true);
                        rect.y += h;
                    } while (prop.NextVisible(false));
                    property.serializedObject.ApplyModifiedProperties();

                };
                list.onRemoveCallback = (list) => {

                    property.serializedObject.Update();
                    var idx = list.index;
                    var item = items.GetArrayElementAtIndex(idx);
                    var lbl = string.Empty;
                    var state = item.GetActualObjectForSerializedProperty<AnimationsComponentModule.State>(this.fieldInfo, ref lbl);
                    if (state.parameters.items != null) {

                        foreach (var anim in state.parameters.items) {
                            
                            if (anim != null) {

                                Object.DestroyImmediate(anim, true);

                            }
                            
                        }
                        
                    }
                    
                    items.DeleteArrayElementAtIndex(idx);
                    property.serializedObject.ApplyModifiedProperties();

                };
                this.dicList.Add(key, list);
                
            } else if (list == null || list.serializedProperty == null) {

                this.dicList.Remove(key);
                return this.Init(property, label);

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

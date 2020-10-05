using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows.Utilities;

    [CustomPropertyDrawer(typeof(SearchAssetsByTypePopupAttribute))]
    public class WindowSystemSearchAssetsByTypePopupPropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            var attr = (SearchAssetsByTypePopupAttribute)this.attribute;
            
            var target = (string.IsNullOrEmpty(attr.innerField) == true ? property : property.FindPropertyRelative(attr.innerField));
            EditorGUI.LabelField(position, label);
            position.x += EditorGUIUtility.labelWidth;
            position.width -= EditorGUIUtility.labelWidth;
            if (EditorGUI.DropdownButton(position, new GUIContent(target.objectReferenceValue != null ? EditorHelpers.StringToCaption(target.objectReferenceValue.name) : attr.noneOption), FocusType.Passive) == true) {

                var rect = position;
                var vector = GUIUtility.GUIToScreenPoint(new Vector2(rect.x, rect.y));
                rect.x = vector.x;
                rect.y = vector.y;
                
                var popup = new Popup() { title = attr.menuName, autoClose = true, screenRect = new Rect(rect.x, rect.y + rect.height, rect.width, 200f) };
                var objects = AssetDatabase.FindAssets("t:" + (attr.filterType != null ? attr.filterType.Name : "Object"), attr.filterDir == null ? null : new [] { attr.filterDir });
                if (string.IsNullOrEmpty(attr.noneOption) == false) {

                    popup.Item(attr.noneOption, null, searchable: false, action: (item) => {

                        property.serializedObject.Update();
                        target.objectReferenceValue = null;
                        property.serializedObject.ApplyModifiedProperties();

                    }, order: -1);

                }

                for (int i = 0; i < objects.Length; ++i) {

                    var path = AssetDatabase.GUIDToAssetPath(objects[i]);
                    var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                    popup.Item(EditorHelpers.StringToCaption(asset.name), () => {
                        
                        property.serializedObject.Update();
                        target.objectReferenceValue = asset;
                        property.serializedObject.ApplyModifiedProperties();
                        
                    });

                }
                popup.Show();
                
            }
            
        }

    }

    [CustomPropertyDrawer(typeof(SearchComponentsByTypePopupAttribute))]
    public class WindowSystemSearchComponentsByTypePopupPropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            var attr = (SearchComponentsByTypePopupAttribute)this.attribute;
            var searchType = attr.baseType;
            if (attr.allowClassOverrides == true && property.serializedObject.targetObject is ISearchComponentByTypeEditor searchComponentByTypeEditor) {

                searchType = searchComponentByTypeEditor.GetSearchType();

            }

            IList searchArray = null;
            var singleOnly = false;
            if (attr.allowClassOverrides == true && property.serializedObject.targetObject is ISearchComponentByTypeSingleEditor searchComponentByTypeSingleEditor) {

                searchArray = searchComponentByTypeSingleEditor.GetSearchTypeArray();
                singleOnly = true;

            }

            var target = (string.IsNullOrEmpty(attr.innerField) == true ? property : property.FindPropertyRelative(attr.innerField));
            var displayName = string.Empty;
            if (target.objectReferenceValue != null) {

                var compDisplayAttrs = target.objectReferenceValue.GetType().GetCustomAttributes(typeof(UnityEngine.UI.Windows.ComponentModuleDisplayNameAttribute), true);
                if (compDisplayAttrs.Length > 0) {

                    var compDisplayAttr = (UnityEngine.UI.Windows.ComponentModuleDisplayNameAttribute)compDisplayAttrs[0];
                    displayName = compDisplayAttr.name;

                } else {

                    displayName = EditorHelpers.StringToCaption(target.objectReferenceValue.GetType().Name);

                }

            }

            EditorGUI.LabelField(position, label);
            position.x += EditorGUIUtility.labelWidth;
            position.width -= EditorGUIUtility.labelWidth;
            if (EditorGUI.DropdownButton(position, new GUIContent(target.objectReferenceValue != null ? displayName : attr.noneOption), FocusType.Passive) == true) {

                var rect = position;
                var vector = GUIUtility.GUIToScreenPoint(new Vector2(rect.x, rect.y));
                rect.x = vector.x;
                rect.y = vector.y;
                
                var popup = new Popup() { title = attr.menuName, autoClose = true, screenRect = new Rect(rect.x, rect.y + rect.height, rect.width, 200f) };
                if (string.IsNullOrEmpty(attr.noneOption) == false) {

                    popup.Item(attr.noneOption, null, searchable: false, action: (item) => {

                        property.serializedObject.Update();
                        if (property.objectReferenceValue != null) {

                            Object.DestroyImmediate(property.objectReferenceValue, true);
                            property.objectReferenceValue = null;

                        }

                        property.serializedObject.ApplyModifiedProperties();

                    }, order: -1);

                }

                var allTypes = searchType.Assembly.GetTypes();
                foreach (var type in allTypes) {

                    if (type.IsSubclassOf(searchType) == true) {

                        var itemType = type;
                        if (singleOnly == true) {

                            var found = false;
                            foreach (var item in searchArray) {

                                if (item == null) continue;
                                if (item.GetType() == itemType) {

                                    found = true;
                                    break;
                                    
                                }
                                
                            }
                            
                            if (found == true) continue;

                        }
                        
                        var compDisplayAttrs = type.GetCustomAttributes(typeof(UnityEngine.UI.Windows.ComponentModuleDisplayNameAttribute), true);
                        if (compDisplayAttrs.Length > 0) {

                            var compDisplayAttr = (UnityEngine.UI.Windows.ComponentModuleDisplayNameAttribute)compDisplayAttrs[0];
                            displayName = compDisplayAttr.name;

                        } else {

                            displayName = EditorHelpers.StringToCaption(type.Name);

                        }
                        
                        popup.Item(displayName, () => {
                        
                            property.serializedObject.Update();
                            var go = (property.serializedObject.targetObject as Component).gameObject;
                            if (property.objectReferenceValue != null) {
                                
                                Object.DestroyImmediate(property.objectReferenceValue, true);
                                property.objectReferenceValue = null;
                                
                            }
                            property.objectReferenceValue = go.AddComponent(itemType);
                            property.serializedObject.ApplyModifiedProperties();
                        
                        });
                        
                    }
                    
                }
                
                popup.Show();
                
            }
            
        }

    }

}

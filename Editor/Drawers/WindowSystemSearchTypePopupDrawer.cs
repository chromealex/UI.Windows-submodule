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
            if (GUILayoutExt.DrawDropdown(position, new GUIContent(target.objectReferenceValue != null ? EditorHelpers.StringToCaption(target.objectReferenceValue.name) : attr.noneOption), FocusType.Passive, target.objectReferenceValue) == true) {

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

        private static bool changed;
        
        public override bool CanCacheInspectorGUI(SerializedProperty property) {
            
            return false;
            
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            
            /*if (property.propertyType == SerializedPropertyType.ManagedReference) {

                var prop = property.Copy();
                var depth = prop.depth;
                var h = EditorGUIUtility.singleLineHeight;
                var lbl = new GUIContent(prop.displayName);
                prop.isExpanded = true;
                h += EditorGUI.GetPropertyHeight(prop, lbl, true);
                return h;
                
            }*/
            
            return base.GetPropertyHeight(property, label);
            
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            WindowSystemSearchComponentsByTypePopupPropertyDrawer.DrawGUI(position, label, (SearchComponentsByTypePopupAttribute)this.attribute, property);
            
        }
        
        public static void DrawGUI(Rect position, GUIContent label, SearchComponentsByTypePopupAttribute attr, SerializedProperty property, System.Action onChanged = null, bool drawLabel = true) {

            var searchType = attr.baseType;
            var useSearchTypeOverride = false;
            if (attr.allowClassOverrides == true && property.serializedObject.targetObject is ISearchComponentByTypeEditor searchComponentByTypeEditor) {

                useSearchTypeOverride = true;
                searchType = searchComponentByTypeEditor.GetSearchType();

            }

            IList searchArray = null;
            var singleOnly = false;
            if (attr.singleOnly == true && property.serializedObject.targetObject is ISearchComponentByTypeSingleEditor searchComponentByTypeSingleEditor) {

                searchArray = searchComponentByTypeSingleEditor.GetSearchTypeArray();
                singleOnly = true;

            }

            var target = (string.IsNullOrEmpty(attr.innerField) == true ? property.Copy() : property.FindPropertyRelative(attr.innerField));
            var displayName = string.Empty;
            Object selectButtonObj = null;
            if (target.propertyType == SerializedPropertyType.ObjectReference && target.objectReferenceValue != null) {

                selectButtonObj = target.objectReferenceValue;
                var compDisplayAttrs = target.objectReferenceValue.GetType().GetCustomAttributes(typeof(UnityEngine.UI.Windows.ComponentModuleDisplayNameAttribute), true);
                if (compDisplayAttrs.Length > 0) {

                    var compDisplayAttr = (UnityEngine.UI.Windows.ComponentModuleDisplayNameAttribute)compDisplayAttrs[0];
                    displayName = compDisplayAttr.name;

                } else {

                    displayName = EditorHelpers.StringToCaption(target.objectReferenceValue.GetType().Name);

                }

            }

            if (target.propertyType == SerializedPropertyType.ManagedReference) {
                
                GetTypeFromManagedReferenceFullTypeName(target.managedReferenceFullTypename, out var type);
                displayName = EditorHelpers.StringToCaption(type != null ? type.Name : string.Empty);
                
            }

            if (drawLabel == true) EditorGUI.LabelField(position, label);
            var rectPosition = position;
            if (drawLabel == true) {
                position.x += EditorGUIUtility.labelWidth;
                position.width -= EditorGUIUtility.labelWidth;
            }
            position.height = EditorGUIUtility.singleLineHeight;
            if (GUILayoutExt.DrawDropdown(position, new GUIContent(string.IsNullOrEmpty(displayName) == false ? displayName : attr.noneOption), FocusType.Passive, selectButtonObj) == true) {

                var rect = position;
                var vector = GUIUtility.GUIToScreenPoint(new Vector2(rect.x, rect.y));
                rect.x = vector.x;
                rect.y = vector.y;
                
                var popup = new Popup() { title = attr.menuName, autoClose = true, screenRect = new Rect(rect.x, rect.y + rect.height, rect.width, 200f) };
                if (string.IsNullOrEmpty(attr.noneOption) == false) {

                    popup.Item(attr.noneOption, null, searchable: false, action: (item) => {

                        property.serializedObject.Update();
                        if (target.propertyType == SerializedPropertyType.ObjectReference) {
                                
                            if (property.objectReferenceValue != null) {

                                Object.DestroyImmediate(property.objectReferenceValue, true);
                                property.objectReferenceValue = null;

                            }
                                
                        } else if (target.propertyType == SerializedPropertyType.ManagedReference) {
                                
                            target.managedReferenceValue = null;
                            property.isExpanded = true;
                            property.serializedObject.SetIsDifferentCacheDirty();
                            GUI.changed = true;
                            changed = true;
                            onChanged?.Invoke();

                        }
                        
                        property.serializedObject.ApplyModifiedProperties();

                    }, order: -1);

                }

                var allTypes = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).ToArray();//searchType.Assembly.GetTypes();
                foreach (var type in allTypes) {

                    if (
                        (((useSearchTypeOverride == false || searchType != attr.baseType) && searchType.IsAssignableFrom(type) == true) || attr.baseType == type.BaseType) &&
                        type.IsInterface == false &&
                        type.IsAbstract == false) {

                        var itemType = type;
                        if (singleOnly == true) {

                            var found = false;
                            foreach (var item in searchArray) {

                                if (item == null) continue;
                                if (itemType.IsAssignableFrom(item.GetType()) == true) {

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
                        
                            target.serializedObject.ApplyModifiedProperties();
                            target.serializedObject.Update();
                            if (target.propertyType == SerializedPropertyType.ObjectReference) {
                                
                                var go = (target.serializedObject.targetObject as Component).gameObject;
                                if (target.objectReferenceValue != null) {

                                    Object.DestroyImmediate(target.objectReferenceValue, true);
                                    target.objectReferenceValue = null;

                                }

                                target.objectReferenceValue = go.AddComponent(itemType);
                                
                            } else if (target.propertyType == SerializedPropertyType.ManagedReference) {
                                
                                target.managedReferenceValue = System.Activator.CreateInstance(itemType);
                                property.isExpanded = true;
                                property.serializedObject.SetIsDifferentCacheDirty();
                                GUI.changed = true;
                                changed = true;
                                onChanged?.Invoke();

                            }

                            target.serializedObject.ApplyModifiedProperties();
                        
                        });
                        
                    }
                    
                }
                
                popup.Show();
                
            }

            /*if (target.propertyType == SerializedPropertyType.ManagedReference) {

                position.y += position.height;
                position.height = rectPosition.height;
                var depth = property.depth;
                
                var lbl = new GUIContent(property.displayName);
                property.isExpanded = true;
                EditorGUI.PropertyField(position, property, lbl, true);
                
            }*/

            if (changed == true) {

                GUI.changed = true;
                changed = false;

            }

        }
        
        internal static bool GetTypeFromManagedReferenceFullTypeName(string managedReferenceFullTypename, out System.Type managedReferenceInstanceType) {
            
            managedReferenceInstanceType = null;
            var parts = managedReferenceFullTypename.Split(' ');
            if (parts.Length == 2) {
                var assemblyPart = parts[0];
                var nsClassnamePart = parts[1];
                managedReferenceInstanceType = System.Type.GetType($"{nsClassnamePart}, {assemblyPart}");
            }

            return managedReferenceInstanceType != null;
            
        }

    }

}

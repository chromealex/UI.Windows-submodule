﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;
    using UnityEngine.UI.Windows.Modules;
    
    [CustomPropertyDrawer(typeof(Resource))]
    public class WindowSystemResourcesResourcePropertyDrawer : PropertyDrawer {

        public struct Result {

            public Resource resource;
            public bool changed;

        }
        
        public static Result DrawGUI(Rect position, GUIContent label, System.Reflection.FieldInfo field, Resource value, bool hasMultipleDifferentValues) {

            var result = new Result() {
                resource = value,
                changed = false
            };
            
            var requiredType = UnityEngine.UI.Windows.Utilities.RequiredType.None;
            var type = typeof(Object);
            if (field != null) {

                var attrs = field.GetCustomAttributes(typeof(ResourceTypeAttribute), inherit: false);
                if (attrs.Length > 0) {

                    var attr = (ResourceTypeAttribute)attrs[0];
                    type = attr.type;
                    requiredType = attr.required;

                }

            }

            var labelSize = 40f;

            var objRect = position;
            objRect.width -= labelSize;

            var labelPadding = 3f;
            var labelRect = position;
            labelRect.x += position.width - labelSize + labelPadding;
            labelRect.width = labelSize - labelPadding * 2f;
            labelRect.y += labelPadding;
            labelRect.height -= labelPadding * 2f;

            var obj = Resource.GetEditorRef(value.guid, value.subObjectName, type, value.objectType, value.directRef);
            var oldValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = hasMultipleDifferentValues;
            var newObj = EditorGUI.ObjectField(objRect, label, obj, type, allowSceneObjects: true);
            EditorGUI.showMixedValue = oldValue;
            if (newObj == null) WindowSystemRequiredReferenceDrawer.DrawRequired(objRect, requiredType);
            if (newObj != obj) {

                result.changed = true;
                {
                    var assetPath = AssetDatabase.GetAssetPath(newObj);
                    result.resource.guid = AssetDatabase.AssetPathToGUID(assetPath);
                    result.resource.subObjectName = (newObj != null && AssetDatabase.IsSubAsset(newObj) == true ? AssetDatabase.LoadAllAssetsAtPath(assetPath).FirstOrDefault(x => x.name == newObj.name).name : string.Empty);
                }
                
                if (newObj != null) {
                
                    // Apply objectType
                    var val = 0;
                    switch (newObj) {
                    
                        case GameObject objType:
                            val = (int)Resource.ObjectType.GameObject;
                            break;

                        case Component objType:
                            val = (int)Resource.ObjectType.Component;
                            break;

                        case ScriptableObject objType:
                            val = (int)Resource.ObjectType.ScriptableObject;
                            break;

                        case Sprite objType:
                            val = (int)Resource.ObjectType.Sprite;
                            break;

                        case Texture objType:
                            val = (int)Resource.ObjectType.Texture;
                            break;
                    
                        default:
                            val = (int)Resource.ObjectType.Unknown;
                            break;

                    }
                    result.resource.objectType = (Resource.ObjectType)val;

                }
                
                if (newObj == null) {

                    result.resource.type = Resource.Type.Manual;
                    result.resource.directRef = null;

                } else {

                    if (UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(result.resource.guid) != null) {

                        //if (loadType.enumValueIndex != (int)Resource.Type.Addressables)
                        {

                            // addressables
                            {
                                result.resource.type = Resource.Type.Addressables; //AssetDatabase.AssetPathToGUID(assetPath);
                                result.resource.directRef = null;
                                newObj.SetAddressableID(result.resource.guid);
                            }

                        }

                    } else {

                        //if (loadType.enumValueIndex != (int)Resource.Type.Direct)
                        {

                            // direct
                            {
                                result.resource.type = Resource.Type.Direct;
                                result.resource.directRef = newObj;
                            }

                        }

                    }

                }

            }

            var tooltip = "This object will be stored through GUID link.";
            if (newObj != null) {

                var typeRect = labelRect;
                //typeRect.x -= labelRect.width + labelPadding + 18f;
                var resType = result.resource.type;
                switch (resType) {

                    case Resource.Type.Manual:
                        DrawLabel(typeRect, new GUIContent("MANL", tooltip + "\nResource type is Manual."), new Color(0.7f, 0.7f, 1f, 1f));
                        break;

                    case Resource.Type.Direct:
                        DrawLabel(typeRect, new GUIContent("DRCT", tooltip + "\nResource type is Direct."), new Color(1f, 0.2f, 0.4f, 1f));
                        break;

                    case Resource.Type.Addressables:
                        DrawLabel(typeRect, new GUIContent("ADDR", tooltip + "\nResource type is Addressable."), new Color(0.3f, 1f, 0.4f, 1f));
                        break;

                }

            } else {

                DrawLabel(labelRect, new GUIContent("GUID", tooltip), new Color(0.2f, 0.6f, 1f, 0.7f));

            }

            return result;

        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            var field = UnityEditor.UI.Windows.EditorHelpers.GetFieldViaPath(property.serializedObject.targetObject.GetType(), property.propertyPath);
            var guid = property.FindPropertyRelative("guid");
            var loadType = property.FindPropertyRelative("type");
            var objectType = property.FindPropertyRelative("objectType");
            var directRef = property.FindPropertyRelative("directRef");

            var newRes = WindowSystemResourcesResourcePropertyDrawer.DrawGUI(position, label, field, new Resource() {
                guid = guid.stringValue,
                type = (Resource.Type)loadType.enumValueIndex,
                objectType = (Resource.ObjectType)objectType.enumValueIndex,
                directRef = directRef.objectReferenceValue
            }, hasMultipleDifferentValues: property.hasMultipleDifferentValues);

            if (newRes.changed == true) {

                property.serializedObject.Update();
                {
                    guid.stringValue = newRes.resource.guid;
                    loadType.enumValueIndex = (int)newRes.resource.type;
                    objectType.enumValueIndex = (int)newRes.resource.objectType;
                    directRef.objectReferenceValue = newRes.resource.directRef;
                }
                property.serializedObject.ApplyModifiedProperties();

            }

        }

        public static void DrawLabel(Rect rect, GUIContent label, Color color) {

            var c = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            var style = new GUIStyle(EditorStyles.label);
            style.fontSize = 10;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;
            GUI.Label(rect, label, style);
            GUI.color = c;

        }

    }

}
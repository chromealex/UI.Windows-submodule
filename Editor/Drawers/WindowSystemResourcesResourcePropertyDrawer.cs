using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;
    using UnityEngine.UI.Windows.Modules;
    
    [CustomPropertyDrawer(typeof(Resource))]
    public class WindowSystemResourcesResourcePropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            UnityEngine.UI.Windows.Utilities.RequiredType requiredType = UnityEngine.UI.Windows.Utilities.RequiredType.None;
            var type = typeof(Object);
            var field = UnityEditor.UI.Windows.EditorHelpers.GetFieldViaPath(property.serializedObject.targetObject.GetType(), property.propertyPath);
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

            var guid = property.FindPropertyRelative("guid");
            var loadType = property.FindPropertyRelative("type");
            var objectType = property.FindPropertyRelative("objectType");
            var directRef = property.FindPropertyRelative("directRef");
            var obj = Resource.GetEditorRef(guid.stringValue, type, (Resource.ObjectType)objectType.enumValueIndex, directRef.objectReferenceValue);
            var newObj = EditorGUI.ObjectField(objRect, label, obj, type, allowSceneObjects: true);
            if (newObj == null) WindowSystemRequiredReferenceDrawer.DrawRequired(objRect, requiredType);
            if (newObj != obj) {

                property.serializedObject.Update();
                {
                    var assetPath = AssetDatabase.GetAssetPath(newObj);
                    guid.stringValue = AssetDatabase.AssetPathToGUID(assetPath);
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
                    objectType.enumValueIndex = val;

                }
                
                if (newObj == null) {

                    loadType.enumValueIndex = (int)Resource.Type.Manual;
                    directRef.objectReferenceValue = null;

                } else {

                    if (UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(guid.stringValue) != null) {

                        //if (loadType.enumValueIndex != (int)Resource.Type.Addressables)
                        {

                            // addressables
                            {
                                loadType.enumValueIndex = (int)Resource.Type.Addressables; //AssetDatabase.AssetPathToGUID(assetPath);
                                directRef.objectReferenceValue = null;
                            }

                        }

                    } else {

                        //if (loadType.enumValueIndex != (int)Resource.Type.Direct)
                        {

                            // direct
                            {
                                loadType.enumValueIndex = (int)Resource.Type.Direct;
                                directRef.objectReferenceValue = newObj;
                            }

                        }

                    }

                }

                property.serializedObject.ApplyModifiedProperties();

            }

            var tooltip = "This object will be stored through GUID link.";
            if (newObj != null) {

                var typeRect = labelRect;
                //typeRect.x -= labelRect.width + labelPadding + 18f;
                var resType = (Resource.Type)loadType.enumValueIndex;
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
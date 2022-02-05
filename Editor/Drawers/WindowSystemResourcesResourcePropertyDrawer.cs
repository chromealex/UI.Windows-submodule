using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;
    using UnityEngine.UI.Windows.Modules;

    [CustomPropertyDrawer(typeof(Resource<>))]
    public class WindowSystemResourcesResourceGenericPropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            System.Type type = null;
            if (this.fieldInfo.FieldType.IsArray == true) {
                type = this.fieldInfo.FieldType.GetElementType().GetGenericArguments()[0];
            } else {
                type = this.fieldInfo.FieldType.GetGenericArguments()[0];
            }

            var res = property.FindPropertyRelative("data");
            WindowSystemResourcesResourcePropertyDrawer.DrawGUI(position, res, label, type, UnityEngine.UI.Windows.Utilities.RequiredType.None);
            
        }

    }

    [CustomPropertyDrawer(typeof(Resource))]
    public class WindowSystemResourcesResourcePropertyDrawer : PropertyDrawer {

        public struct Result {

            public Resource resource;
            public bool changed;

        }

        public static System.Type GetTypeByAttr(System.Reflection.FieldInfo field, out UnityEngine.UI.Windows.Utilities.RequiredType requiredType) {

            requiredType = UnityEngine.UI.Windows.Utilities.RequiredType.None;
            if (field != null) {

                var attrs = field.GetCustomAttributes(typeof(ResourceTypeAttribute), inherit: false);
                if (attrs.Length > 0) {

                    var attr = (ResourceTypeAttribute)attrs[0];
                    requiredType = attr.required;
                    return attr.type;

                }

            }

            return null;

        }
        
        public static Result DrawGUI(Rect position, GUIContent label, Resource value, bool hasMultipleDifferentValues, System.Type type = null, UnityEngine.UI.Windows.Utilities.RequiredType requiredType = UnityEngine.UI.Windows.Utilities.RequiredType.None) {

            var result = new Result() {
                resource = value,
                changed = false,
            };
            
            if (type == null) type = typeof(Object);
            
            var labelSize = 40f;

            var objRect = position;
            objRect.x -= EditorGUI.indentLevel * 14f;
            objRect.width += -labelSize + EditorGUI.indentLevel * 14f;

            var labelPadding = 3f;
            var labelRect = position;
            labelRect.x += position.width - labelSize + labelPadding;
            labelRect.width = labelSize - labelPadding * 2f;
            labelRect.y += labelPadding;
            labelRect.height -= labelPadding * 2f;

            if (type == typeof(Sprite)) {

                if (value.objectType != Resource.ObjectType.Sprite) value.validationRequired = true;
                
            } else if (type == typeof(Texture) ||
                       type == typeof(Texture2D) ||
                       type == typeof(RenderTexture)) {

                if (value.objectType != Resource.ObjectType.Texture) value.validationRequired = true;

            }

            var obj = Resource.GetEditorRef(value.guid, value.subObjectName, type, value.objectType, value.directRef);
            var oldValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = hasMultipleDifferentValues;
            var newObj = EditorGUI.ObjectField(objRect, label, obj, type, allowSceneObjects: true);
            EditorGUI.showMixedValue = oldValue;
            if (newObj == null) WindowSystemRequiredReferenceDrawer.DrawRequired(objRect, requiredType);
            if (newObj != obj || value.validationRequired == true) {

                result.changed = true;
                {
                    var assetPath = AssetDatabase.GetAssetPath(newObj);
                    result.resource.guid = AssetDatabase.AssetPathToGUID(assetPath);
                    result.resource.subObjectName = (newObj != null && AssetDatabase.IsSubAsset(newObj) == true ? AssetDatabase.LoadAllAssetsAtPath(assetPath).FirstOrDefault(x => x.name == newObj.name).name : string.Empty);
                    result.resource.directRef = null;
                    if (string.IsNullOrEmpty(assetPath) == true) {
                        result.resource.directRef = newObj;
                    }
                }
                
                WindowSystemResourcesResourcePropertyDrawer.Validate(ref result.resource, type);
                
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

        public static void DrawGUI(Rect position, SerializedProperty property, GUIContent label, System.Type type, UnityEngine.UI.Windows.Utilities.RequiredType requiredType) {
            
            var address = property.FindPropertyRelative("address");
            var guid = property.FindPropertyRelative("guid");
            var loadType = property.FindPropertyRelative("type");
            var objectType = property.FindPropertyRelative("objectType");
            var directRef = property.FindPropertyRelative("directRef");
            var validationRequired = property.FindPropertyRelative("validationRequired");

            var newRes = WindowSystemResourcesResourcePropertyDrawer.DrawGUI(position, label, new Resource() {
                guid = guid.stringValue,
                type = (Resource.Type)loadType.enumValueIndex,
                objectType = (Resource.ObjectType)objectType.enumValueIndex,
                directRef = directRef.objectReferenceValue,
                validationRequired = validationRequired.boolValue,
            }, hasMultipleDifferentValues: property.hasMultipleDifferentValues, type: type, requiredType: requiredType);

            if (validationRequired.boolValue == true || newRes.changed == true) {

                property.serializedObject.Update();
                {
                    guid.stringValue = newRes.resource.guid;
                    address.stringValue = newRes.resource.address;
                    loadType.enumValueIndex = (int)newRes.resource.type;
                    objectType.enumValueIndex = (int)newRes.resource.objectType;
                    directRef.objectReferenceValue = newRes.resource.directRef;
                    validationRequired.boolValue = false;
                }
                property.serializedObject.ApplyModifiedProperties();

            }

        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            var type = WindowSystemResourcesResourcePropertyDrawer.GetTypeByAttr(this.fieldInfo, out var requiredType);
            WindowSystemResourcesResourcePropertyDrawer.DrawGUI(position, property, label, type, requiredType);

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

        public static void SetValue(Object value, ref Resource resource, System.Type type = null) {
            
            {
                var assetPath = AssetDatabase.GetAssetPath(value);
                resource.guid = AssetDatabase.AssetPathToGUID(assetPath);
                resource.subObjectName = (value != null && AssetDatabase.IsSubAsset(value) == true ? AssetDatabase.LoadAllAssetsAtPath(assetPath).FirstOrDefault(x => x.name == value.name).name : string.Empty);
                resource.directRef = null;
                if (string.IsNullOrEmpty(assetPath) == true) {
                    resource.directRef = value;
                }
            }
                
            WindowSystemResourcesResourcePropertyDrawer.Validate(ref resource, type);
            
        }

        public static void Validate(ref Resource resource, System.Type type = null) {
            
            if (type == null) type = typeof(Object);
            if (type == typeof(Sprite)) {
                
                var objType = resource.objectType;
                if (objType == Resource.ObjectType.Texture) {
                    
                    resource.objectType = Resource.ObjectType.Sprite;
                    
                }

            }

            var newObj = Resource.GetEditorRef(resource.guid, resource.subObjectName, type, resource.objectType, resource.directRef);

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

                resource.objectType = (Resource.ObjectType)val;

            }

            if (newObj == null) {

                resource.type = Resource.Type.Manual;
                resource.directRef = null;

            } else {

                var entry = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(resource.guid, includeImplicit: true);
                if (entry != null) {

                    //if (loadType.enumValueIndex != (int)Resource.Type.Addressables)
                    {

                        // addressables
                        {
                            resource.type = Resource.Type.Addressables; //AssetDatabase.AssetPathToGUID(assetPath);
                            resource.directRef = null;
                            resource.address = entry.address;
                            //newObj.SetAddressableID(resource.guid);
                        }

                    }

                } else {

                    //if (loadType.enumValueIndex != (int)Resource.Type.Direct)
                    {

                        // direct
                        {
                            resource.type = Resource.Type.Direct;
                            resource.directRef = newObj;
                        }

                    }

                }

            }
            
            if (type == typeof(Sprite)) {
                
                var objType = resource.objectType;
                if (objType == Resource.ObjectType.Texture) {
                    
                    resource.objectType = Resource.ObjectType.Sprite;
                    
                }

            }

        }

    }

}
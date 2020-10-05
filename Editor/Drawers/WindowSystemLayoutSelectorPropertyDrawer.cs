using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;

    [CustomPropertyDrawer(typeof(UnityEngine.UI.Windows.WindowTypes.LayoutSelectorAttribute))]
    public class WindowSystemLayoutSelectorPropertyDrawer : PropertyDrawer {

        private struct Attr {

            public string menuName;
            public string noneOption;
            public string filterType;
            public string filterDir;

        }
        
        private Attr attr = new Attr() {
            menuName = "Screen Layouts",
            noneOption = "None",
            filterType = "Prefab",
            filterDir = "",
        };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            //EditorGUI.PropertyField(position, property, label);

            var attr = this.attr;
            var go = (property.serializedObject.targetObject as Component).gameObject;
            if (go == null) return;

            var pth = AssetDatabase.GetAssetPath(go);
            if (string.IsNullOrEmpty(pth) == true) return;
            
            var assetPath = System.IO.Path.GetDirectoryName(pth);
            assetPath = assetPath.Replace("/Screens", "/Layouts");
            attr.filterDir = assetPath;
            
            var target = property;
            EditorGUI.LabelField(position, label);
            position.x += EditorGUIUtility.labelWidth;
            position.width -= EditorGUIUtility.labelWidth;
            
            if (EditorGUI.DropdownButton(position, new GUIContent(target.objectReferenceValue != null ? EditorHelpers.StringToCaption(target.objectReferenceValue.name) : attr.noneOption), FocusType.Passive) == true) {

                var rect = position;
                var vector = GUIUtility.GUIToScreenPoint(new Vector2(rect.x, rect.y));
                rect.x = vector.x;
                rect.y = vector.y;
                
                var popup = new Popup() { title = attr.menuName, autoClose = true, screenRect = new Rect(rect.x, rect.y + rect.height, rect.width, 200f) };
                var objects = AssetDatabase.FindAssets("t:" + (attr.filterType != null ? attr.filterType : "Object"), attr.filterDir == null ? null : new [] { attr.filterDir });

                var allObjects = Resources.LoadAll<GameObject>("").Where(x => x.GetComponent<WindowLayout>() != null && x.GetComponent<TemplateMarker>() != null).ToArray();
                for (int i = 0; i < allObjects.Length; ++i) {

                    var idx = i;
                    popup.Item("Clone Template/" + allObjects[i].name, null, searchable: true, action: (item) => {

                        var p = AssetDatabase.GetAssetPath(allObjects[idx]);
                        var newPath = AssetDatabase.GenerateUniqueAssetPath(assetPath + "/" + allObjects[idx].name + ".prefab");
                        AssetDatabase.CopyAsset(p, newPath);
                        AssetDatabase.ImportAsset(newPath, ImportAssetOptions.ForceUpdate);
                        var newGo = AssetDatabase.LoadAssetAtPath<GameObject>(newPath);
                        Object.DestroyImmediate(newGo.GetComponent<TemplateMarker>(), true);
                        
                        property.serializedObject.Update();
                        target.objectReferenceValue = newGo.GetComponent<WindowLayout>();
                        property.serializedObject.ApplyModifiedProperties();

                    }, order: -1);

                }

                if (string.IsNullOrEmpty(attr.noneOption) == false) {

                    popup.Item(attr.noneOption, null, searchable: false, action: (item) => {

                        property.serializedObject.Update();
                        target.objectReferenceValue = null;
                        property.serializedObject.ApplyModifiedProperties();

                    }, order: -1);

                }

                for (int i = 0; i < objects.Length; ++i) {

                    var path = AssetDatabase.GUIDToAssetPath(objects[i]);
                    var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (asset.GetComponent<WindowLayout>() == null) continue;
                    
                    popup.Item(EditorHelpers.StringToCaption(asset.name), () => {
                        
                        property.serializedObject.Update();
                        target.objectReferenceValue = asset.GetComponent<WindowLayout>();
                        property.serializedObject.ApplyModifiedProperties();
                        
                    });

                }
                popup.Show();
                
            }
            
        }

    }

}

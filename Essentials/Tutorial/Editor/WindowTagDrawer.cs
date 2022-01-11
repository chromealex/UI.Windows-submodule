using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI.Windows.Essentials.Tutorial;
using UnityEngine.UI.Windows;

namespace UnityEditor.UI.Windows.Essentials.Tutorial {
    
    public struct TagData {

        public int tagId;
        public WindowLayoutElement element;

    }

    [CustomPropertyDrawer(typeof(Tag))]
    public class WindowTagDrawer : PropertyDrawer {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            
            return EditorGUIUtility.singleLineHeight * 2f;
            
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            var windowType = property.serializedObject.FindProperty("forWindowType");
            var windowGuid = windowType.FindPropertyRelative("guid");

            var tagId = property.FindPropertyRelative("id");
            var listIndex = property.FindPropertyRelative("listIndex");

            var window = (UnityEngine.UI.Windows.WindowTypes.LayoutWindowType)AssetDatabase.LoadAssetAtPath<WindowBase>(AssetDatabase.GUIDToAssetPath(windowGuid.stringValue));
            if (window != null) {

                var height = EditorGUIUtility.singleLineHeight;
                var windowLayout = ((UnityEngine.UI.Windows.WindowTypes.LayoutWindowType)window).layouts.items[0].windowLayout;
                var tags = windowLayout.layoutElements.Select(x => new TagData() { tagId = x.tagId, element = x });
                var data = tags.Select(x => x.tagId).ToArray();
                var options = tags.Select(x => x.element.name).ToArray();
                position.height = height;
                tagId.intValue = EditorGUI.IntPopup(position, "Tag", tagId.intValue, options, data);

                var components = window.layouts.items[0].components;
                for (int i = 0; i < components.Length; ++i) {

                    if (components[i].tag == tagId.intValue) {

                        var editorRef = Resource.GetEditorRef<WindowComponent>(components[i].component);
                        if (editorRef is UnityEngine.UI.Windows.Components.ListBaseComponent) {

                            property.FindPropertyRelative("isList").boolValue = true;
                            position.y += height;
                            listIndex.intValue = EditorGUI.IntField(position, "List Index", listIndex.intValue);

                        } else {
                            
                            property.FindPropertyRelative("isList").boolValue = false;
                            
                        }
                        break;
                        
                    }
                    
                }

            }
            
        }

    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;
    using UnityEngine.UI.Windows.WindowTypes;
    
    [CustomPropertyDrawer(typeof(Layouts))]
    public class WindowSystemLayoutsPropertyDrawer : PropertyDrawer {

        private int selectedTab {
            get {
                return EditorPrefs.GetInt("UnityEditor.UI.Windows.LayoutsProperty.TabIndex");
            }
            set {
                EditorPrefs.SetInt("UnityEditor.UI.Windows.LayoutsProperty.TabIndex", value);
            }
        }

        private Vector2 tabScrollPosition {
            get {
                return new Vector2(
                    EditorPrefs.GetFloat("UnityEditor.UI.Windows.LayoutsProperty.TabScrollPosition.X"),
                    EditorPrefs.GetFloat("UnityEditor.UI.Windows.LayoutsProperty.TabScrollPosition.Y")
                    );
            }
            set {
                EditorPrefs.SetFloat("UnityEditor.UI.Windows.LayoutsProperty.TabScrollPosition.X", value.x);
                EditorPrefs.SetFloat("UnityEditor.UI.Windows.LayoutsProperty.TabScrollPosition.Y", value.y);
            }
        }

        private UnityEditorInternal.ReorderableList list;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            
            return 0f;
            
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            var evenStyle = new GUIStyle(GUIStyle.none);
            evenStyle.normal.background = Texture2D.whiteTexture;
            var tagStyle = new GUIStyle(EditorStyles.label);
            tagStyle.alignment = TextAnchor.MiddleRight;
            var innerLayoutStyle = new GUIStyle(EditorStyles.miniLabel);
            innerLayoutStyle.alignment = TextAnchor.UpperLeft;
            innerLayoutStyle.stretchHeight = false;
            
            var items = property.FindPropertyRelative("items");
            if (items.arraySize == 0) {

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add Layout", GUILayout.Height(30f), GUILayout.Width(120f)) == true) {

                    ++items.arraySize;
                    this.selectedTab = 0;

                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(20f);
                return;

            }
            
            var arr = new GUITab[items.arraySize + 1];
            this.selectedTab = Mathf.Clamp(this.selectedTab, 0, arr.Length - 2);
            var i = 0;
            for (i = 0; i < items.arraySize; ++i) {

                var idx = i;
                var prop = items.GetArrayElementAtIndex(i);
                var objRef = prop.FindPropertyRelative("windowLayout").objectReferenceValue;
                var caption = (objRef != null ? EditorHelpers.StringToCaption(objRef.name) : "Layout (Empty)");
                arr[i] = new GUITab(caption, () => {

                    GUILayout.BeginHorizontal();
                    {
                        
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Remove", GUILayout.Width(60f)) == true) {

                            if (EditorUtility.DisplayDialog("Delete Layout Reference", "Are you sure?", "Yes", "No") == true) {

                                items.DeleteArrayElementAtIndex(idx);
                                return;

                            }

                        }
                        
                    }
                    GUILayout.EndHorizontal();

                    if (idx > 0) {

                        GUILayout.Space(6f);

                        EditorGUI.BeginChangeCheck();
                        var targets = prop.FindPropertyRelative("targets");
                        EditorGUILayout.PropertyField(targets);
                        if (EditorGUI.EndChangeCheck() == true) {

                            EditorHelpers.SetDirtyAndValidate(property);

                        }

                    } else {
                        
                        EditorGUILayout.HelpBox("This is default layout. Target Filter couldn't been attach here.", MessageType.Info);
                        
                    }

                    GUILayout.Space(6f);

                    EditorGUI.BeginChangeCheck();
                    var windowLayout = prop.FindPropertyRelative("windowLayout");
                    EditorGUILayout.PropertyField(windowLayout);
                    if (EditorGUI.EndChangeCheck() == true) {

                        EditorHelpers.SetDirtyAndValidate(property);

                    }

                    var layout = windowLayout.objectReferenceValue as WindowLayout;
                    if (layout == null) {

                        return;

                    }

                    EditorGUI.BeginChangeCheck();
                    var layoutPreferences = prop.FindPropertyRelative("layoutPreferences");
                    EditorGUILayout.PropertyField(layoutPreferences);
                    if (layoutPreferences.objectReferenceValue == null) {
                        
                        EditorGUILayout.HelpBox("Layout Preferences are CanvasScaler override parameters. It's highly recommended to use override here.", MessageType.Warning);
                        
                    }

                    if (layoutPreferences.objectReferenceValue is WindowLayoutPreferences windowLayoutPreferences) {

                        windowLayoutPreferences.Apply(layout.canvasScaler);

                        try {

                            EditorGUI.BeginDisabledGroup(true);
                            var editorCanvasScaler = Editor.CreateEditor(layout.canvasScaler);
                            editorCanvasScaler.OnInspectorGUI();
                            EditorGUI.EndDisabledGroup();

                        } catch (System.Exception) {}

                    }

                    if (EditorGUI.EndChangeCheck() == true) {

                        EditorHelpers.SetDirtyAndValidate(property);

                    }

                    GUILayout.Space(2f);
                    GUILayoutExt.Separator();
                    GUILayout.Space(2f);
                    
                    if (this.selectedTab != idx || this.list == null) {
                
                        var componentsProp = prop.FindPropertyRelative("components");
                        this.list = new UnityEditorInternal.ReorderableList(property.serializedObject, componentsProp, true, true, false, false);
                        this.list.elementHeight = 40f;
                        this.list.onAddCallback = (rList) => {

                            if (rList.serializedProperty != null) {
                        
                                ++rList.serializedProperty.arraySize;
                                rList.index = rList.serializedProperty.arraySize - 1;
                                
                            }

                        };
                        this.list.drawElementBackgroundCallback = (rect, index, active, focused) => {

                            if (focused == true) {

                                GUILayoutExt.DrawRect(rect, new Color(0.1f, 0.4f, 0.7f, 1f));

                            } else {

                                GUILayoutExt.DrawRect(rect, new Color(1f, 1f, 1f, index % 2 == 0 ? 0.05f : 0f));

                            }

                        }; 
                        this.list.drawElementCallback = (rect, index, active, focused) => {
                
                            //EditorGUI.PropertyField(rect, componentsProp.GetArrayElementAtIndex(index));

                            EditorGUI.BeginChangeCheck();
                            {
                                var captionRect = new Rect(rect.x, rect.y, rect.width, 18f);
                                var tagRect = captionRect;
                                var layoutRect = new Rect(tagRect.x + 140f, tagRect.y, tagRect.width, tagRect.height);
                                var objectRect = new Rect(captionRect.x, captionRect.y + 18f, captionRect.width, captionRect.height);
                                
                                var compProp = componentsProp.GetArrayElementAtIndex(index);
                                var component = compProp.FindPropertyRelative("component");
                                
                                var localTagId = compProp.FindPropertyRelative("localTag").intValue;
                                var windowLayoutInner = (WindowLayout)compProp.FindPropertyRelative("windowLayout").objectReferenceValue;
                                string layoutName = string.Empty;
                                if (windowLayoutInner != null) {

                                    var tagId = compProp.FindPropertyRelative("tag").intValue;
                                    var layoutElement = windowLayoutInner.GetLayoutElementByTagId(tagId);
                                    if (layoutElement != null) layoutName = layoutElement.name;

                                }
                                
                                using (GUILayoutExt.GUIColor(new Color(1f, 1f, 1f, 0.4f))) {
                                    if (windowLayoutInner != null && windowLayoutInner != layout)
                                        GUI.Label(layoutRect, "(" + EditorHelpers.StringToCaption(windowLayoutInner.name) + ")", innerLayoutStyle);
                                }
                                
                                GUI.Label(captionRect, EditorHelpers.StringToCaption(layoutName), EditorStyles.boldLabel);
                                GUI.Label(tagRect, "Tag: " + localTagId.ToString(), tagStyle);
                                EditorGUI.PropertyField(objectRect, component, new GUIContent(string.Empty));
                            }
                            
                            if (EditorGUI.EndChangeCheck() == true) {

                                EditorHelpers.SetDirtyAndValidate(property);

                            }
                            
                        }; 
                        this.list.drawHeaderCallback = (rect) => {
                
                            GUI.Label(rect, "Components");
                            var buttonRect = rect;
                            var width = 80f;
                            buttonRect.x = rect.width - width + 40f;
                            buttonRect.width = width;
                            if (GUI.Button(buttonRect, "Refresh") == true) {

                                if (componentsProp.serializedObject.targetObject is LayoutWindowType layoutWindowType) {

                                    var helper = new UnityEngine.UI.Windows.Utilities.DirtyHelper(layoutWindowType);
                                    EditorHelpers.UpdateLayoutWindow(helper, layoutWindowType);
                                    helper.Apply();
                                    
                                }

                                (componentsProp.serializedObject.targetObject as WindowObject).ValidateEditor();
                                EditorHelpers.SetDirtyAndValidate(property);
                                
                            }
                
                        };
                
                    }
            
                    this.list.DoLayoutList();
                    
                    /*
                    EditorGUI.BeginChangeCheck();
                    var components = prop.FindPropertyRelative("components");
                    for (int j = 0; j < components.arraySize; ++j) {

                        var compProp = components.GetArrayElementAtIndex(j);
                        var component = compProp.FindPropertyRelative("component");

                        var c = GUI.color;
                        GUI.color = new Color(1f, 1f, 1f, 0.1f);
                        GUILayout.BeginVertical(j % 2 == 0 ? evenStyle : oddStyle);
                        GUI.color = c;
                        {
                            
                            GUILayout.Space(4f);
                            GUILayout.BeginHorizontal();
                            {
                                var localRagId = compProp.FindPropertyRelative("localTag").intValue;
                                var windowLayoutInner = (WindowLayout)compProp.FindPropertyRelative("windowLayout").objectReferenceValue;
                                string layoutName = string.Empty;
                                if (windowLayoutInner != null) {

                                    var tagId = compProp.FindPropertyRelative("tag").intValue;
                                    var layoutElement = windowLayoutInner.GetLayoutElementByTagId(tagId);
                                    if (layoutElement != null) layoutName = layoutElement.name;
                                    
                                }
                                GUILayout.Label(EditorHelpers.StringToCaption(layoutName), EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
                                using (GUILayoutExt.GUIColor(new Color(1f, 1f, 1f, 0.4f))) {
                                    if (windowLayoutInner != null && windowLayoutInner != layout) GUILayout.Label("(" + EditorHelpers.StringToCaption(windowLayoutInner.name) + ")", innerLayoutStyle, GUILayout.ExpandWidth(false));
                                }

                                GUILayout.FlexibleSpace();
                                GUILayout.Label("Tag: " + localRagId.ToString(), tagStyle, GUILayout.ExpandWidth(false));
                            }
                            GUILayout.EndHorizontal();
                            EditorGUILayout.PropertyField(component);
                            
                            GUILayout.Space(4f);
                            GUILayoutExt.Separator();

                        }
                        GUILayout.EndVertical();

                    }
                    if (EditorGUI.EndChangeCheck() == true) {

                        EditorHelpers.SetDirtyAndValidate(property);

                    }*/

                });

            }
            
            arr[i] = new GUITab("+", () => {
                
                
                
            }, 40f);

            var scroll = this.tabScrollPosition;
            var newTab = GUILayoutExt.DrawTabs(
                this.selectedTab,
                ref scroll,
                arr
            );
            this.tabScrollPosition = scroll;

            if (newTab != this.selectedTab) {

                if (newTab == i) {
                    
                    // Add item
                    ++items.arraySize;
                    this.selectedTab = i;

                } else {

                    this.selectedTab = newTab;

                }

            }

        }

    }

}

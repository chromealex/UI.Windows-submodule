using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;
    using UnityEngine.UI.Windows.Modules;

    [CustomEditor(typeof(WindowBase), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class WindowSystemWindowBaseEditor : Editor {

        private SerializedProperty createPool;
        private SerializedProperty objectState;
        private SerializedProperty focusState;

        private SerializedProperty preferences;
        private SerializedProperty modules;
        private SerializedProperty layouts;
        
        private int selectedTab {
            get {
                return EditorPrefs.GetInt("UnityEditor.UI.Windows.WindowBase.TabIndex");
            }
            set {
                EditorPrefs.SetInt("UnityEditor.UI.Windows.WindowBase.TabIndex", value);
            }
        }

        private Vector2 tabScrollPosition {
            get {
                return new Vector2(
                    EditorPrefs.GetFloat("UnityEditor.UI.Windows.WindowBase.TabScrollPosition.X"),
                    EditorPrefs.GetFloat("UnityEditor.UI.Windows.WindowBase.TabScrollPosition.Y")
                );
            }
            set {
                EditorPrefs.SetFloat("UnityEditor.UI.Windows.WindowBase.TabScrollPosition.X", value.x);
                EditorPrefs.SetFloat("UnityEditor.UI.Windows.WindowBase.TabScrollPosition.Y", value.y);
            }
        }

        private UnityEditorInternal.ReorderableList listModules;

        public void OnEnable() {

            try {

                #pragma warning disable
                var _ = this.serializedObject;
                #pragma warning restore

            } catch (System.Exception) {

                return;

            }

            this.createPool = this.serializedObject.FindProperty("createPool");
            
            this.objectState = this.serializedObject.FindProperty("objectState");
            this.focusState = this.serializedObject.FindProperty("focusState");
            
            this.preferences = this.serializedObject.FindProperty("preferences");
            this.modules = this.serializedObject.FindProperty("modules");
            this.layouts = this.serializedObject.FindProperty("layouts");

            var moduleItems = this.modules.FindPropertyRelative("modules");
            this.listModules = new UnityEditorInternal.ReorderableList(this.serializedObject, moduleItems, false, false, true, true);
            const float elementHeight = 64f;
            this.listModules.headerHeight = 1f;
            this.listModules.elementHeight = elementHeight;
            this.listModules.drawElementCallback += (rect, index, active, focus) => {
                
                var item = moduleItems.GetArrayElementAtIndex(index);
                var prevValue = item.FindPropertyRelative("module").FindPropertyRelative("guid");
                rect.y += 2f;
                rect.height = 18f;
                EditorGUI.BeginChangeCheck();
                GUILayoutExt.DrawProperty(rect, item, 20f);
                GUILayoutExt.DrawRect(new Rect(rect.x, rect.y + elementHeight - 3f, rect.width, 1f), new Color(1f, 1f, 1f, 0.1f));
                if (EditorGUI.EndChangeCheck() == true) {

                    var newValue = item.FindPropertyRelative("module").FindPropertyRelative("guid");
                    if (prevValue != newValue && string.IsNullOrEmpty(newValue.stringValue) == false) {

                        var guid = newValue.stringValue;
                        var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        var obj = AssetDatabase.LoadAssetAtPath<WindowModule>(assetPath);
                        
                        var module = obj;
                        item.FindPropertyRelative("order").intValue = module.defaultOrder;

                    }
                    
                }
                //EditorGUI.PropertyField(rect, item);

            };

            EditorHelpers.SetFirstSibling(this.targets);

        }

        public override GUIContent GetPreviewTitle() {
            
            return new GUIContent("Layout");
            
        }

        public override bool HasPreviewGUI() {

            if (this.target is UnityEngine.UI.Windows.WindowTypes.LayoutWindowType) {
                
                return true;

            }

            return false;
            
        }

        private int selectedIndexAspect = 0;
        private int selectedIndexInner = 0;
        private int selectedType = 0;
        private Vector2 tabsScrollPosition;
        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background) {

            if (this.target is UnityEngine.UI.Windows.WindowTypes.LayoutWindowType layoutWindowType) {

                var windowLayout = layoutWindowType.layouts.GetActive().windowLayout;
                WindowLayoutUtilities.DrawLayout(this.selectedIndexAspect, this.selectedIndexInner, this.selectedType, (type, idx, inner) => {
                    
                    this.selectedType = type;
                    this.selectedIndexAspect = idx;
                    this.selectedIndexInner = inner;
                    
                }, ref this.tabsScrollPosition, windowLayout, r, drawComponents: layoutWindowType);

            }
            
        }

        public override void OnInspectorGUI() {

            this.serializedObject.Update();
            
            GUILayoutExt.DrawComponentHeader(this.serializedObject, "S", () => {
                
                GUILayoutExt.DrawComponentHeaderItem("State", GUILayoutExt.GetPropertyToString(this.objectState));
                GUILayoutExt.DrawComponentHeaderItem("Focus", GUILayoutExt.GetPropertyToString(this.focusState));
                
                GUILayout.FlexibleSpace();
                
            }, new Color(0f, 0.6f, 0f, 0.4f));
            
            GUILayout.Space(5f);
            
            var scroll = this.tabScrollPosition;
            this.selectedTab = GUILayoutExt.DrawTabs(
                this.selectedTab,
                ref scroll,
                new GUITab("Preferences", () => {

                    GUILayoutExt.DrawProperty(this.preferences);
                    EditorGUILayout.PropertyField(this.createPool);

                }),
                new GUITab("Modules (" + this.listModules.count.ToString() + ")", () => {

                    this.listModules.DoLayoutList();
                    
                }),
                new GUITab("Layouts", () => {
                    
                    EditorGUILayout.PropertyField(this.layouts);

                })
                );
            this.tabScrollPosition = scroll;
        
            GUILayout.Space(10f);

            var iter = this.serializedObject.GetIterator();
            iter.NextVisible(true);
            do {

                if (EditorHelpers.IsFieldOfTypeBeneath(this.serializedObject.targetObject.GetType(), typeof(UnityEngine.UI.Windows.WindowTypes.LayoutWindowType), iter.propertyPath) == true) {

                    EditorGUILayout.PropertyField(iter);

                }

            } while (iter.NextVisible(false) == true);

            this.serializedObject.ApplyModifiedProperties();

        }

    }

}

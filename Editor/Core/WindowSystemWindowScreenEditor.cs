using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        
        private SerializedProperty audioEvents;

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

            this.audioEvents = this.serializedObject.FindProperty("audioEvents");

            var moduleItems = this.modules.FindPropertyRelative("modules");
            this.listModules = new UnityEditorInternal.ReorderableList(this.serializedObject, moduleItems, false, false, true, true);
            this.listModules.headerHeight = 1f;
            this.listModules.elementHeightCallback += (index) => {
                
                var item = moduleItems.GetArrayElementAtIndex(index);
                var h = 0f;
                h += EditorGUI.GetPropertyHeight(item.FindPropertyRelative("targets"));
                h += 2f;
                h += EditorGUI.GetPropertyHeight(item.FindPropertyRelative("module"));
                h += 2f;
                h += 20f;
                var parameters = item.FindPropertyRelative("parameters");
                if (parameters.hasVisibleChildren == true) {

                    var px = parameters.Copy();
                    px.NextVisible(true);
                    var depth = px.depth;
                    while (px.depth >= depth) {

                        h += EditorGUI.GetPropertyHeight(px, true);
                        if (px.NextVisible(false) == false) break;

                    }
                    h += 4f;

                }
                h += 4f;
                return h;
                
            };
            this.listModules.drawElementCallback += (rect, index, active, focus) => {
                
                var item = moduleItems.GetArrayElementAtIndex(index);
                var prevValue = item.FindPropertyRelative("module").FindPropertyRelative("guid").stringValue;
                rect.y += 2f;
                rect.height = 18f;
                EditorGUI.BeginChangeCheck();
                //GUILayoutExt.DrawProperty(rect, item, 20f);
                EditorGUI.PropertyField(rect, item.FindPropertyRelative("targets"));
                rect.y += EditorGUI.GetPropertyHeight(item.FindPropertyRelative("targets"), true);
                rect.y += 2f;
                EditorGUI.PropertyField(rect, item.FindPropertyRelative("module"));
                rect.y += EditorGUI.GetPropertyHeight(item.FindPropertyRelative("module"), true);
                rect.y += 2f;
                EditorGUI.LabelField(rect, "Parameters", EditorStyles.centeredGreyMiniLabel);
                rect.y += 20f;

                var parameters = item.FindPropertyRelative("parameters");
                {
                    if (string.IsNullOrEmpty(parameters.managedReferenceFullTypename) == true) {

                        parameters.managedReferenceValue = new WindowModule.Parameters();

                    }

                    if (parameters.hasVisibleChildren == true) {

                        var px = parameters.Copy();
                        px.NextVisible(true);
                        var depth = px.depth;
                        while (px.depth >= depth) {

                            var h = EditorGUI.GetPropertyHeight(px, true);
                            EditorGUI.PropertyField(rect, px, true);
                            rect.y += h;
                            rect.y += 2f;
                            if (px.NextVisible(false) == false) break;

                        }

                    }
                    
                    rect.y += 4f;

                    /*
                    var p = parameters.Copy();
                    if (p.CountInProperty() > 0) {
                        var px = parameters.Copy();
                        px.NextVisible(true);
                        var h = EditorGUI.GetPropertyHeight(px, true);
                        EditorGUI.PropertyField(rect, px, true);
                        rect.y += h;
                        rect.y += 2f;
                    }*/
                    
                }

                GUILayoutExt.DrawRect(new Rect(rect.x, rect.y - 3f, rect.width, 1f), new Color(1f, 1f, 1f, 0.1f));
                rect.y += 4f;
                if (EditorGUI.EndChangeCheck() == true) {

                    var newValue = item.FindPropertyRelative("module").FindPropertyRelative("guid").stringValue;
                    if (prevValue != newValue && string.IsNullOrEmpty(newValue) == false) {

                        var guid = newValue;
                        var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        var module = AssetDatabase.LoadAssetAtPath<WindowModule>(assetPath);
                        var sourceParameters = module.parameters;
                        parameters.managedReferenceValue = sourceParameters;

                    }
                    
                }
                
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
                if (windowLayout == null) return;
                
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

                }),
                new GUITab("Audio", () => {
                    
                    GUILayoutExt.DrawHeader("Events");
                    var enterChildren = true;
                    var prop = this.audioEvents.Copy();
                    var depth = prop.depth + 1;
                    while (prop.NextVisible(enterChildren) == true && prop.depth >= depth) {
                    
                        EditorGUILayout.PropertyField(prop, true);
                        enterChildren = false;

                    }
                    
                }),
                new GUITab("Tools", () => {

                    GUILayoutExt.Box(4f, 4f, () => {
                        
                        if (GUILayout.Button("Collect Images", GUILayout.Height(30f)) == true) {

                            var images = new List<ImageCollectionItem>();
                            if (this.target is UnityEngine.UI.Windows.WindowTypes.LayoutWindowType layoutWindowType) {
                                
                                var components = new List<Object>();
                                var resources = layoutWindowType.layouts.items.SelectMany(x => x.components.Select(x => x.component)).ToArray();
                                var type = typeof(WindowComponent);
                                foreach (var resource in resources) {
                                    
                                    var editorRef = Resource.GetEditorRef(resource.guid, resource.subObjectName, type, resource.objectType, resource.directRef);
                                    if (editorRef != null) components.Add(editorRef);
                                    
                                }
                                this.lastImagesPreview = EditorHelpers.CollectImages(components.ToArray(), images);
                                this.lastImages = images;
                                
                            }

                        }

                        GUILayoutExt.DrawImages(this.lastImagesPreview, this.lastImages);
                        
                    });
                    
                })

                );
            this.tabScrollPosition = scroll;
        
            GUILayout.Space(10f);

            GUILayoutExt.DrawFieldsBeneath(this.serializedObject, typeof(UnityEngine.UI.Windows.WindowTypes.LayoutWindowType));

            this.serializedObject.ApplyModifiedProperties();

        }

        private Texture2D lastImagesPreview;
        private List<ImageCollectionItem> lastImages;

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;

    [CustomEditor(typeof(WindowComponent), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class WindowSystemWindowComponentEditor : Editor {

        private SerializedProperty createPool;
        
        private SerializedProperty animationParameters;
        private SerializedProperty subObjects;
        private SerializedProperty componentModules;

        private SerializedProperty renderBehaviourOnHidden;
        
        private SerializedProperty allowRegisterInRoot;
        private SerializedProperty autoRegisterSubObjects;
        private SerializedProperty hiddenByDefault;
        
        private SerializedProperty objectCanvas;
        private SerializedProperty canvasSortingOrderDelta;

        private SerializedProperty audioEvents;

        private SerializedProperty editorRefLocks;

        private UnityEditorInternal.ReorderableList listModules;

        private int selectedTab {
            get {
                return EditorPrefs.GetInt("UnityEditor.UI.Windows.WindowComponent.TabIndex");
            }
            set {
                EditorPrefs.SetInt("UnityEditor.UI.Windows.WindowComponent.TabIndex", value);
            }
        }

        private Vector2 tabScrollPosition {
            get {
                return new Vector2(
                    EditorPrefs.GetFloat("UnityEditor.UI.Windows.WindowComponent.TabScrollPosition.X"),
                    EditorPrefs.GetFloat("UnityEditor.UI.Windows.WindowComponent.TabScrollPosition.Y")
                );
            }
            set {
                EditorPrefs.SetFloat("UnityEditor.UI.Windows.WindowComponent.TabScrollPosition.X", value.x);
                EditorPrefs.SetFloat("UnityEditor.UI.Windows.WindowComponent.TabScrollPosition.Y", value.y);
            }
        }

        public virtual void OnEnable() {

            try {

                #pragma warning disable
                var _ = this.serializedObject;
                #pragma warning restore

            } catch (System.Exception) {

                return;

            }
            
            this.createPool = this.serializedObject.FindProperty("createPool");

            this.animationParameters = this.serializedObject.FindProperty("animationParameters");
            this.renderBehaviourOnHidden = this.serializedObject.FindProperty("renderBehaviourOnHidden");

            this.subObjects = this.serializedObject.FindProperty("subObjects");
            this.componentModules = this.serializedObject.FindProperty("componentModules");

            this.allowRegisterInRoot = this.serializedObject.FindProperty("allowRegisterInRoot");
            this.autoRegisterSubObjects = this.serializedObject.FindProperty("autoRegisterSubObjects");
            this.hiddenByDefault = this.serializedObject.FindProperty("hiddenByDefault");
            
            this.objectCanvas = this.serializedObject.FindProperty("objectCanvas");
            this.canvasSortingOrderDelta = this.serializedObject.FindProperty("canvasSortingOrderDelta");
            
            this.audioEvents = this.serializedObject.FindProperty("audioEvents");
            
            this.editorRefLocks = this.serializedObject.FindProperty("editorRefLocks");

            if (this.listModules == null) {
                
                var componentsProp = this.componentModules.FindPropertyRelative("modules");
                
                if (this.componentModules.FindPropertyRelative("windowComponent").objectReferenceValue != this.componentModules.serializedObject.targetObject) {
                    this.serializedObject.Update();
                    this.componentModules.FindPropertyRelative("windowComponent").objectReferenceValue = this.componentModules.serializedObject.targetObject;
                    this.serializedObject.ApplyModifiedProperties();
                }
                
                this.listModules = new UnityEditorInternal.ReorderableList(componentsProp.serializedObject, componentsProp, true, true, true, true);
                this.listModules.elementHeight = 40f;
                this.listModules.onAddCallback = (rList) => {

                    if (rList.serializedProperty != null) {

                        ++rList.serializedProperty.arraySize;
                        rList.index = rList.serializedProperty.arraySize - 1;
                        var idx = rList.index;
                        var prop = componentsProp.GetArrayElementAtIndex(idx);
                        prop.objectReferenceValue = null;
                        
                    }

                };
                this.listModules.onRemoveCallback = (rList) => {

                    var idx = this.listModules.index;
                    var prop = componentsProp.GetArrayElementAtIndex(idx);
                    if (prop.objectReferenceValue != null) Object.DestroyImmediate(prop.objectReferenceValue, true);
                    componentsProp.DeleteArrayElementAtIndex(idx);

                };
                this.listModules.drawElementBackgroundCallback = (rect, index, active, focused) => {

                    if (focused == true || active == true) {

                        GUILayoutExt.DrawRect(rect, new Color(0.1f, 0.4f, 0.7f, 1f));

                    } else {

                        GUILayoutExt.DrawRect(rect, new Color(1f, 1f, 1f, index % 2 == 0 ? 0.05f : 0f));

                    }

                };
                this.listModules.elementHeightCallback = (index) => {

                    if (componentsProp.arraySize > 0) {

                        var prop = componentsProp.GetArrayElementAtIndex(index);
                        if (prop.objectReferenceValue != null) {

                            var height = 0f;
                            var so = new SerializedObject(prop.objectReferenceValue);
                            var iterator = so.GetIterator();
                            iterator.NextVisible(true);

                            EditorGUI.indentLevel += 1;
                            int indent = EditorGUI.indentLevel;

                            while (true) {

                                if (EditorHelpers.IsFieldOfTypeBeneath(prop.objectReferenceValue.GetType(), typeof(WindowComponentModule), iterator.propertyPath) == true) {

                                    height += EditorGUI.GetPropertyHeight(iterator, true);
                                    
                                }

                                if (!iterator.NextVisible(false)) {//iterator.isExpanded)) {
                                    break;
                                }

                            }

                            EditorGUI.indentLevel = indent;
                            EditorGUI.indentLevel -= 1;

                            return 40f + height;

                        }

                    }

                    return 40f;

                };
                this.listModules.drawElementCallback = (rect, index, active, focused) => {

                    var padding = 10f;
                    rect.x += padding;
                    rect.y += padding;
                    rect.width -= padding * 2f;
                    rect.height = 18f;
                    EditorGUI.PropertyField(rect, componentsProp.GetArrayElementAtIndex(index), new GUIContent("Module"));
                    rect.y += 18f;
                    rect.y += padding;

                    var prop = componentsProp.GetArrayElementAtIndex(index);
                    if (prop.objectReferenceValue != null) {
                        
                        var so = new SerializedObject(prop.objectReferenceValue);
                        so.Update();
                        
                        so.FindProperty("windowComponent").objectReferenceValue = this.serializedObject.targetObject;

                        var iterator = so.GetIterator();
                        iterator.NextVisible(true);

                        EditorGUI.indentLevel += 1;
                        
                        while (true) {

                            if (EditorHelpers.IsFieldOfTypeBeneath(prop.objectReferenceValue.GetType(), typeof(WindowComponentModule), iterator.propertyPath) == true) {

                                rect.height = EditorGUI.GetPropertyHeight(iterator, true);

                                //totalHeight += rect.height;
                                EditorGUI.PropertyField(rect, iterator, true);
                                rect.y += rect.height;

                            }

                            if (!iterator.NextVisible(false)) {//iterator.isExpanded)) {
                                break;
                            }
                            
                        }

                        EditorGUI.indentLevel -= 1;
                        
                        so.ApplyModifiedProperties();

                    }

                };
                this.listModules.drawHeaderCallback = (rect) => { GUI.Label(rect, "Modules"); };
                
            }
    
            EditorHelpers.SetFirstSibling(this.targets);

        }
        
        public virtual void OnDisable() {}

        public override GUIContent GetPreviewTitle() {
            
            return new GUIContent("Component Preview");
            
        }

        public override bool HasPreviewGUI() {
            
            return this.targets.Length == 1;
            
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background) {
            
            WindowLayoutUtilities.DrawComponent(r, this.target as WindowComponent, 0);
            
        }

        private void DrawCanvas() {

            if (this.objectCanvas.objectReferenceValue != null) {
                
                GUILayoutExt.DrawHeader("Canvas Options");
                EditorGUILayout.PropertyField(this.canvasSortingOrderDelta, new GUIContent("Canvas Order Delta"));
                
            }
            
        }

        public override void OnInspectorGUI() {

            this.serializedObject.Update();
            
            GUILayoutExt.DrawComponentHeader(this.serializedObject, "C", () => {
                
                GUILayoutExt.DrawComponentHeaderItem("State", ((WindowObject)this.target).GetState().ToString());

            });
            
            GUILayout.Space(5f);
            
            var scroll = this.tabScrollPosition;
            this.selectedTab = GUILayoutExt.DrawTabs(
                this.selectedTab,
                ref scroll,
                new GUITab("Basic", () => {
                    
                    GUILayoutExt.DrawHeader("Main");
                    GUILayoutExt.PropertyField(this.hiddenByDefault, (reg) => reg.holdHiddenByDefault);
                    EditorGUILayout.PropertyField(this.subObjects);
                    
                    GUILayoutExt.DrawHeader("Animations");
                    EditorGUILayout.PropertyField(this.animationParameters);

                    GUILayoutExt.DrawHeader("Performance Options");
                    EditorGUILayout.PropertyField(this.createPool);

                    this.DrawCanvas();

                }),
                new GUITab("Advanced", () => {
                    
                    GUILayoutExt.DrawHeader("Render Behaviour");
                    EditorGUILayout.PropertyField(this.renderBehaviourOnHidden);

                    GUILayoutExt.DrawHeader("Animations");
                    EditorGUILayout.PropertyField(this.animationParameters);

                    GUILayoutExt.DrawHeader("Graph");
                    GUILayoutExt.PropertyField(this.allowRegisterInRoot, (reg) => reg.holdAllowRegisterInRoot);
                    EditorGUILayout.PropertyField(this.autoRegisterSubObjects);
                    GUILayoutExt.PropertyField(this.hiddenByDefault, (reg) => reg.holdHiddenByDefault);
                    EditorGUILayout.PropertyField(this.subObjects);

                    GUILayoutExt.DrawHeader("Performance Options");
                    EditorGUILayout.PropertyField(this.createPool);
                    
                    this.DrawCanvas();

                }),
                new GUITab("Modules (" + this.componentModules.FindPropertyRelative("modules").arraySize + ")", () => {
                    
                    this.listModules.DoLayoutList();

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

                        EditorGUILayout.PropertyField(this.editorRefLocks);
                        
                    });

                    GUILayoutExt.Box(4f, 4f, () => {
                        
                        if (GUILayout.Button("Collect Images", GUILayout.Height(30f)) == true) {

                            var images = new List<ImageCollectionItem>();
                            this.lastImagesPreview = EditorHelpers.CollectImages(this.target, images);
                            this.lastImages = images;

                        }

                        GUILayoutExt.DrawImages(this.lastImagesPreview, this.lastImages);
                        
                    });
                    
                })
                );
            this.tabScrollPosition = scroll;

            GUILayout.Space(10f);

            GUILayoutExt.DrawFieldsBeneath(this.serializedObject, typeof(WindowComponent));
            
            this.serializedObject.ApplyModifiedProperties();

        }

        private List<ImageCollectionItem> lastImages;
        private Texture2D lastImagesPreview;

    }

}

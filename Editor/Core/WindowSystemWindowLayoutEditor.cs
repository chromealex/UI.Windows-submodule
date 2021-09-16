using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;
    
    [CustomEditor(typeof(WindowLayout), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class WindowSystemWindowLayoutEditor : Editor {

        private SerializedProperty createPool;
        
        private SerializedProperty objectState;
        private SerializedProperty animationParameters;
        private SerializedProperty subObjects;

        private SerializedProperty renderBehaviourOnHidden;
        
        private SerializedProperty allowRegisterInRoot;
        private SerializedProperty autoRegisterSubObjects;
        private SerializedProperty hiddenByDefault;
        
        private SerializedProperty useSafeZone;
        private SerializedProperty safeZone;

        private int selectedTab {
            get {
                return EditorPrefs.GetInt("UnityEditor.UI.Windows.WindowLayout.TabIndex");
            }
            set {
                EditorPrefs.SetInt("UnityEditor.UI.Windows.WindowLayout.TabIndex", value);
            }
        }

        private Vector2 tabScrollPosition {
            get {
                return new Vector2(
                    EditorPrefs.GetFloat("UnityEditor.UI.Windows.WindowLayout.TabScrollPosition.X"),
                    EditorPrefs.GetFloat("UnityEditor.UI.Windows.WindowLayout.TabScrollPosition.Y")
                );
            }
            set {
                EditorPrefs.SetFloat("UnityEditor.UI.Windows.WindowLayout.TabScrollPosition.X", value.x);
                EditorPrefs.SetFloat("UnityEditor.UI.Windows.WindowLayout.TabScrollPosition.Y", value.y);
            }
        }

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
            this.animationParameters = this.serializedObject.FindProperty("animationParameters");
            this.renderBehaviourOnHidden = this.serializedObject.FindProperty("renderBehaviourOnHidden");

            this.subObjects = this.serializedObject.FindProperty("subObjects");

            this.allowRegisterInRoot = this.serializedObject.FindProperty("allowRegisterInRoot");
            this.autoRegisterSubObjects = this.serializedObject.FindProperty("autoRegisterSubObjects");
            this.hiddenByDefault = this.serializedObject.FindProperty("hiddenByDefault");
        
            this.useSafeZone = this.serializedObject.FindProperty("useSafeZone");
            this.safeZone = this.serializedObject.FindProperty("safeZone");
            
            EditorHelpers.SetFirstSibling(this.targets);

            EditorApplication.update += this.Repaint;
            
        }

        public void OnDisable() {
            
            EditorApplication.update -= this.Repaint;
            
        }

        public override GUIContent GetPreviewTitle() {
            
            return new GUIContent("Layout");
            
        }

        private int selectedIndexAspect = 0;
        private int selectedIndexInner = 0;
        private int selectedType = 0;
        private Vector2 tabsScrollPosition;
        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background) {

            var windowLayout = this.target as WindowLayout;
            WindowLayoutUtilities.DrawLayout(this.selectedIndexAspect, this.selectedIndexInner, this.selectedType, (type, idx, inner) => {
                    
                this.selectedType = type;
                this.selectedIndexAspect = idx;
                this.selectedIndexInner = inner;
                
            }, ref this.tabsScrollPosition, windowLayout, r, drawComponents: null);
            
        }

        public override bool HasPreviewGUI() {
            
            return this.targets.Length == 1;
            
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background) {

        }

        public override void OnInspectorGUI() {

            this.serializedObject.Update();
            
            GUILayoutExt.DrawComponentHeader(this.serializedObject, "L", () => {
                
                GUILayoutExt.DrawComponentHeaderItem("State", GUILayoutExt.GetPropertyToString(this.objectState));

            }, new Color(1f, 0.6f, 0f, 0.4f));
            
            GUILayout.Space(5f);
            
            var scroll = this.tabScrollPosition;
            this.selectedTab = GUILayoutExt.DrawTabs(
                this.selectedTab,
                ref scroll,
                new GUITab("Basic", () => {

                    GUILayoutExt.DrawHeader("Main");
                    EditorGUILayout.PropertyField(this.hiddenByDefault);
                    EditorGUILayout.PropertyField(this.subObjects);
                    
                    GUILayoutExt.DrawHeader("Animations");
                    EditorGUILayout.PropertyField(this.animationParameters);

                    GUILayoutExt.DrawHeader("Performance Options");
                    EditorGUILayout.PropertyField(this.createPool);
                    
                }),
                new GUITab("Advanced", () => {
                    
                    GUILayoutExt.DrawHeader("Render Behaviour");
                    EditorGUILayout.PropertyField(this.renderBehaviourOnHidden);

                    GUILayoutExt.DrawHeader("Animations");
                    EditorGUILayout.PropertyField(this.animationParameters);

                    GUILayoutExt.DrawHeader("Graph");
                    EditorGUILayout.PropertyField(this.allowRegisterInRoot);
                    EditorGUILayout.PropertyField(this.autoRegisterSubObjects);
                    EditorGUILayout.PropertyField(this.hiddenByDefault);
                    EditorGUILayout.PropertyField(this.subObjects);

                    GUILayoutExt.DrawHeader("Performance Options");
                    EditorGUILayout.PropertyField(this.createPool);

                }),
                new GUITab("Tools", () => {

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

            if (this.targets.Length == 1) GUILayoutExt.DrawSafeAreaFields(this.target, this.useSafeZone, this.safeZone);
            
            GUILayout.Space(10f);

            GUILayoutExt.DrawFieldsBeneath(this.serializedObject, typeof(WindowLayout));

            this.serializedObject.ApplyModifiedProperties();

        }
        
        private Texture2D lastImagesPreview;
        private List<ImageCollectionItem> lastImages;

    }

}

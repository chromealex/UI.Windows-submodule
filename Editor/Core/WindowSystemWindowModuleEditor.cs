using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;
    using UnityEngine.UI.Windows.Modules;
    
    [CustomEditor(typeof(WindowModule), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class WindowSystemWindowModuleEditor : Editor {

        private SerializedProperty createPool;
        
        private SerializedProperty animationParameters;
        private SerializedProperty subObjects;
        private SerializedProperty hideBehaviour;
        private SerializedProperty showBehaviour;

        private SerializedProperty renderBehaviourOnHidden;
        
        private SerializedProperty allowRegisterInRoot;
        private SerializedProperty autoRegisterSubObjects;
        private SerializedProperty hiddenByDefault;

        private SerializedProperty parameters;

        private SerializedProperty useSafeZone;
        private SerializedProperty safeZone;

        private int selectedTab {
            get {
                return EditorPrefs.GetInt("UnityEditor.UI.Windows.WindowModule.TabIndex");
            }
            set {
                EditorPrefs.SetInt("UnityEditor.UI.Windows.WindowModule.TabIndex", value);
            }
        }

        private Vector2 tabScrollPosition {
            get {
                return new Vector2(
                    EditorPrefs.GetFloat("UnityEditor.UI.Windows.WindowModule.TabScrollPosition.X"),
                    EditorPrefs.GetFloat("UnityEditor.UI.Windows.WindowModule.TabScrollPosition.Y")
                );
            }
            set {
                EditorPrefs.SetFloat("UnityEditor.UI.Windows.WindowModule.TabScrollPosition.X", value.x);
                EditorPrefs.SetFloat("UnityEditor.UI.Windows.WindowModule.TabScrollPosition.Y", value.y);
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
            
            this.animationParameters = this.serializedObject.FindProperty("animationParameters");
            this.renderBehaviourOnHidden = this.serializedObject.FindProperty("renderBehaviourOnHidden");

            this.subObjects = this.serializedObject.FindProperty("subObjects");
            this.hideBehaviour = this.serializedObject.FindProperty("hideBehaviour");
            this.showBehaviour = this.serializedObject.FindProperty("showBehaviour");

            this.allowRegisterInRoot = this.serializedObject.FindProperty("allowRegisterInRoot");
            this.autoRegisterSubObjects = this.serializedObject.FindProperty("autoRegisterSubObjects");
            this.hiddenByDefault = this.serializedObject.FindProperty("hiddenByDefault");
            
            this.parameters = this.serializedObject.FindProperty("parameters");
            this.ValidateParameters();
        
            this.useSafeZone = this.serializedObject.FindProperty("useSafeZone");
            this.safeZone = this.serializedObject.FindProperty("safeZone");

            EditorHelpers.SetFirstSibling(this.targets);

        }

        private void ValidateParameters() {
            
            if (string.IsNullOrEmpty(this.parameters.managedReferenceFullTypename) == true) {
                
                this.parameters.managedReferenceValue = new WindowModule.Parameters();
                
            }
            
        }

        private void DrawParameters() {

            this.ValidateParameters();
            
            var p = this.parameters.Copy();
            if (p.hasVisibleChildren == true) {

                GUILayoutExt.DrawHeader("Module Options (Default)");

                var px = this.parameters.Copy();
                px.NextVisible(true);
                var depth = px.depth;
                while (px.depth >= depth) {
                    
                    EditorGUILayout.PropertyField(px, true);
                    if (px.NextVisible(false) == false) break;
                    
                }

            }

        }

        public override void OnInspectorGUI() {

            this.serializedObject.Update();
            
            GUILayoutExt.DrawComponentHeader(this.serializedObject, "M", () => {
                
                GUILayoutExt.DrawComponentHeaderItem("State", ((WindowObject)this.target).GetState().ToString());

            }, new Color(1f, 0.6f, 1f, 0.4f));
            
            GUILayout.Space(5f);
            
            var scroll = this.tabScrollPosition;
            this.selectedTab = GUILayoutExt.DrawTabs(
                this.selectedTab,
                ref scroll,
                new GUITab("Basic", () => {
                    
                    GUILayoutExt.DrawHeader("Main");
                    EditorGUILayout.PropertyField(this.hiddenByDefault);
                    EditorGUILayout.PropertyField(this.animationParameters);
                    EditorGUILayout.PropertyField(this.subObjects);
                    
                    GUILayoutExt.DrawHeader("Animations");
                    EditorGUILayout.PropertyField(this.animationParameters);

                    GUILayoutExt.DrawHeader("Performance Options");
                    EditorGUILayout.PropertyField(this.createPool);
                    
                    this.DrawParameters();

                }),
                new GUITab("Advanced", () => {
                    
                    GUILayoutExt.DrawHeader("Render Behaviour");
                    EditorGUILayout.PropertyField(this.renderBehaviourOnHidden);

                    GUILayoutExt.DrawHeader("Animations");
                    EditorGUILayout.PropertyField(this.animationParameters);
                    EditorGUILayout.PropertyField(this.hideBehaviour);
                    EditorGUILayout.PropertyField(this.showBehaviour);

                    GUILayoutExt.DrawHeader("Graph");
                    EditorGUILayout.PropertyField(this.allowRegisterInRoot);
                    EditorGUILayout.PropertyField(this.autoRegisterSubObjects);
                    EditorGUILayout.PropertyField(this.hiddenByDefault);
                    EditorGUILayout.PropertyField(this.subObjects);

                    GUILayoutExt.DrawHeader("Performance Options");
                    EditorGUILayout.PropertyField(this.createPool);

                    this.DrawParameters();

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
            
            GUILayoutExt.DrawFieldsBeneath(this.serializedObject, typeof(WindowModule));

            this.serializedObject.ApplyModifiedProperties();

        }

        private Texture2D lastImagesPreview;
        private List<ImageCollectionItem> lastImages;

    }

}

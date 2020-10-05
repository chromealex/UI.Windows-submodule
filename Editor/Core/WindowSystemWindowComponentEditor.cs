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
        
        private SerializedProperty objectState;
        private SerializedProperty animationParameters;
        private SerializedProperty subObjects;
        private SerializedProperty componentModules;

        private SerializedProperty renderBehaviourOnHidden;
        
        private SerializedProperty allowRegisterInRoot;
        private SerializedProperty autoRegisterSubObjects;
        private SerializedProperty hiddenByDefault;

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

            this.objectState = this.serializedObject.FindProperty("objectState");
            this.animationParameters = this.serializedObject.FindProperty("animationParameters");
            this.renderBehaviourOnHidden = this.serializedObject.FindProperty("renderBehaviourOnHidden");

            this.subObjects = this.serializedObject.FindProperty("subObjects");
            this.componentModules = this.serializedObject.FindProperty("componentModules");

            this.allowRegisterInRoot = this.serializedObject.FindProperty("allowRegisterInRoot");
            this.autoRegisterSubObjects = this.serializedObject.FindProperty("autoRegisterSubObjects");
            this.hiddenByDefault = this.serializedObject.FindProperty("hiddenByDefault");

            if (this.listModules == null) {
                
                var componentsProp = this.componentModules.FindPropertyRelative("modules");
                //if (componentsProp.arraySize != 0) {

                    this.serializedObject.Update();
                    this.componentModules.FindPropertyRelative("windowComponent").objectReferenceValue = this.componentModules.serializedObject.targetObject;
                    this.serializedObject.ApplyModifiedProperties();
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

                        var prop = componentsProp.GetArrayElementAtIndex(index);
                        if (prop.objectReferenceValue != null) {

                            var height = 0f;
                            var so = new SerializedObject(prop.objectReferenceValue);
                            var iterator = so.GetIterator();
                            iterator.NextVisible(true);

                            while (true) {

                                if (EditorHelpers.IsFieldOfTypeBeneath(prop.objectReferenceValue.GetType(), typeof(WindowComponentModule), iterator.propertyPath) == true) {

                                    height += EditorGUI.GetPropertyHeight(iterator, new GUIContent(iterator.displayName), false);

                                }

                                if (!iterator.NextVisible(iterator.isExpanded)) {
                                    break;
                                }
                                
                            }
                            
                            return 40f + height;

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
                            int indent = EditorGUI.indentLevel;
                            while (true) {

                                if (EditorHelpers.IsFieldOfTypeBeneath(prop.objectReferenceValue.GetType(), typeof(WindowComponentModule), iterator.propertyPath) == true) {

                                    rect.height = EditorGUI.GetPropertyHeight(iterator, new GUIContent(iterator.displayName), false);

                                    //totalHeight += rect.height;
                                    EditorGUI.indentLevel = indent + iterator.depth;
                                    EditorGUI.PropertyField(rect, iterator);
                                    rect.y += rect.height;

                                }

                                if (!iterator.NextVisible(iterator.isExpanded)) {
                                    break;
                                }
                                
                            }

                            EditorGUI.indentLevel = indent;
                            EditorGUI.indentLevel -= 1;
                            
                            /*
                            var iter = so.GetIterator();
                            while (iter.NextVisible(true) == true) {

                                if (iter.hasVisibleChildren == true) {

                                    iter.isExpanded = EditorGUI.Foldout(rect, iter.isExpanded, iter.displayName);

                                    if (iter.isExpanded == false) continue;

                                }
                                
                                if (EditorHelpers.IsFieldOfTypeBeneath(prop.objectReferenceValue.GetType(), typeof(WindowComponentModule), iter.propertyPath) == true) {

                                    rect.height = EditorGUI.GetPropertyHeight(iter);
                                    EditorGUI.PropertyField(rect, iter);
                                    rect.y += rect.height;

                                }

                            }*/

                            so.ApplyModifiedProperties();

                        }

                    };
                    this.listModules.drawHeaderCallback = (rect) => { GUI.Label(rect, "Modules"); };

                //}

            }
    
            EditorHelpers.SetFirstSibling(this.targets);

        }

        public override GUIContent GetPreviewTitle() {
            
            return new GUIContent("Component Preview");
            
        }

        public override bool HasPreviewGUI() {
            
            return this.targets.Length == 1;
            
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background) {
            
            WindowLayoutUtilities.DrawComponent(r, this.target as WindowComponent, 0);
            
        }

        public override void OnInspectorGUI() {

            this.serializedObject.Update();
            
            GUILayoutExt.DrawComponentHeader(this.serializedObject, "C", () => {
                
                GUILayoutExt.DrawComponentHeaderItem("State", GUILayoutExt.GetPropertyToString(this.objectState));

            });
            
            GUILayout.Space(5f);
            
            var scroll = this.tabScrollPosition;
            this.selectedTab = GUILayoutExt.DrawTabs(
                this.selectedTab,
                ref scroll,
                new GUITab("Basic", () => {
                    
                    GUILayoutExt.DrawHeader("Main");
                    GUILayoutExt.PropertyField(this.hiddenByDefault, (reg) => reg.hiddenByDefault == true ? reg.hiddenByDefaultDescription : string.Empty);
                    EditorGUILayout.PropertyField(this.animationParameters);
                    EditorGUILayout.PropertyField(this.subObjects);

                    GUILayoutExt.DrawHeader("Performance Options");
                    EditorGUILayout.PropertyField(this.createPool);

                }),
                new GUITab("Advanced", () => {
                    
                    GUILayoutExt.DrawHeader("Render Behaviour");
                    EditorGUILayout.PropertyField(this.renderBehaviourOnHidden);

                    GUILayoutExt.DrawHeader("Animation");
                    EditorGUILayout.PropertyField(this.animationParameters);

                    GUILayoutExt.DrawHeader("Graph");
                    GUILayoutExt.PropertyField(this.allowRegisterInRoot, (reg) => reg.allowRegisterInRoot == true ? reg.allowRegisterInRootDescription : string.Empty);
                    EditorGUILayout.PropertyField(this.autoRegisterSubObjects);
                    GUILayoutExt.PropertyField(this.hiddenByDefault, (reg) => reg.hiddenByDefault == true ? reg.hiddenByDefaultDescription : string.Empty);
                    EditorGUILayout.PropertyField(this.subObjects);

                    GUILayoutExt.DrawHeader("Performance Options");
                    EditorGUILayout.PropertyField(this.createPool);

                }),
                this.listModules == null ? GUITab.none : new GUITab("Modules (" + this.listModules.count + ")", () => {
                    
                    this.listModules.DoLayoutList();

                })
                );
            this.tabScrollPosition = scroll;

            GUILayout.Space(10f);

            var iter = this.serializedObject.GetIterator();
            iter.NextVisible(true);
            do {

                if (EditorHelpers.IsFieldOfTypeBeneath(this.serializedObject.targetObject.GetType(), typeof(WindowComponent), iter.propertyPath) == true) {

                    EditorGUILayout.PropertyField(iter);

                }

            } while (iter.NextVisible(false) == true);
            
            this.serializedObject.ApplyModifiedProperties();

        }

    }

}

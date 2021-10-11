using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;
    using UnityEngine.UI.Windows.Modules;

    [CustomEditor(typeof(WindowSystemBreadcrumbs))]
    public class WindowSystemBreadcrumbsEditor : Editor {

        private int selectedTabIndex;
        private Vector2 tabScrollPosition;
        
        public void OnEnable() {

            EditorApplication.update += this.Repaint;

        }

        public override void OnInspectorGUI() {

            GUILayoutExt.DrawComponentHeader(this.serializedObject, "EXT", () => {
                
                GUILayout.Label("WindowSystem Internal module.\nBreadcrumbs system.", GUILayout.Height(36f));
                
            }, new Color(0.4f, 0.2f, 0.7f, 1f));

            var target = this.target as WindowSystemBreadcrumbs;

            GUILayout.Space(5f);

            var main = target.GetMain();
            this.selectedTabIndex = GUILayoutExt.DrawTabs(
                this.selectedTabIndex,
                ref this.tabScrollPosition,
                new GUITab() {
                    caption = $"Main ({(main.history != null ? main.history.Count : 0)})",
                    onDraw = () => {

                        if (main.history == null || main.history.Count == 0) {

                            GUILayout.Space(10f);
                            GUILayout.Label("There are no breadcrumbs history yet.", EditorStyles.centeredGreyMiniLabel);
                            GUILayout.Space(10f);

                        } else {

                            var idx = 1;
                            foreach (var item in main.history) {

                                GUILayoutExt.DrawHeader($"Index: {idx}");
                                EditorGUILayout.ObjectField("Prefab", item.prefab, typeof(Object), allowSceneObjects: true);
                                EditorGUILayout.ObjectField("Instance", item.instance, typeof(Object), allowSceneObjects: true);
                                ++idx;

                            }

                        }

                    },
                });

        }

    }

}
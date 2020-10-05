using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;
    using UnityEngine.UI.Windows.Modules;

    [CustomEditor(typeof(WindowSystemPools))]
    public class WindowSystemPoolsEditor : Editor {

        public override void OnInspectorGUI() {

            GUILayoutExt.DrawComponentHeader(this.serializedObject, "EXT", () => {
                
                GUILayout.Label("WindowSystem Internal module.\nPooling system.", GUILayout.Height(36f));
                
            }, new Color(0.4f, 0.2f, 0.7f, 1f));

            var target = this.target as WindowSystemPools;

            UnityEditor.UI.Windows.GUILayoutExt.DrawHeader("Registered prefabs");
            foreach (var item in target.registeredPrefabs) {

                GUILayout.Label(item.ToString());

            }

            UnityEditor.UI.Windows.GUILayoutExt.DrawHeader("Instances on scene");
            foreach (var item in target.instanceOnSceneToPrefab) {

                UnityEditor.UI.Windows.GUILayoutExt.Box(2f, 2f, () => {

                    GUILayout.Label("Prefab: " + item.Value);
                    EditorGUILayout.ObjectField("Object", item.Key, typeof(Object), allowSceneObjects: true);

                });

            }

            UnityEditor.UI.Windows.GUILayoutExt.DrawHeader("Instances in pool");
            foreach (var item in target.prefabToPooledInstances) {

                UnityEditor.UI.Windows.GUILayoutExt.Box(2f, 2f, () => {

                    GUILayout.Label("Prefab: " + item.Key);
                    foreach (var comp in item.Value) {

                        EditorGUILayout.ObjectField(comp, typeof(Object), true);

                    }

                });

            }

        }

    }

}
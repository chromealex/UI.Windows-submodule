using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;
    using UnityEngine.UI.Windows.Modules;

    [CustomEditor(typeof(WindowSystemEvents))]
    public class WindowSystemEventsEditor : Editor {

        public void OnEnable() {

            EditorApplication.update += this.Repaint;

        }

        public override void OnInspectorGUI() {

            GUILayoutExt.DrawComponentHeader(this.serializedObject, "EXT", () => {
                
                GUILayout.Label("WindowSystem Internal module.\nEvents system.", GUILayout.Height(36f));
                
            }, new Color(0.4f, 0.2f, 0.7f, 1f));

            var target = this.target as WindowSystemEvents;

            UnityEditor.UI.Windows.GUILayoutExt.DrawHeader("Registered Events");
            this.DrawRegistry(target.registry);
            foreach (var item in target.registriesGeneric) {
                this.DrawRegistry(item);
            }

        }

        private void DrawRegistry(WindowSystemEvents.RegistryBase registry) {
            
            UnityEditor.UI.Windows.GUILayoutExt.DrawHeader($"Registry: {registry}");
            foreach (var item in registry.GetObjects()) {

                UnityEditor.UI.Windows.GUILayoutExt.Box(2f, 2f, () => {

                    if (item.Value.instance == null) {
                        
                        GUILayout.Label($"Object: {item.Value.name}");
                        
                    } else {

                        EditorGUILayout.ObjectField("Object", item.Value.instance, typeof(Object), allowSceneObjects: true);

                    }

                    UnityEngine.UI.Windows.Utilities.UIWSMath.GetKey(item.Key, out var hash, out var evt);
                    GUILayout.Label($"Event: {(WindowEvent)evt} ({(registry.ContainsCache(item.Key) == true ? "Multiple" : "Once")})");

                });

            }
            
        }

    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;
    using UnityEngine.UI.Windows.Modules;

    [CustomEditor(typeof(WindowSystemResources))]
    public class WindowSystemResourcesEditor : Editor {

        public void OnEnable() {

            EditorApplication.update += this.Repaint;

        }

        public override void OnInspectorGUI() {

            GUILayoutExt.DrawComponentHeader(this.serializedObject, "EXT", () => {
                
                GUILayout.Label("WindowSystem Internal module.\nWorking with resources.", GUILayout.Height(36f));
                
            }, new Color(0.4f, 0.2f, 0.7f, 1f));
            
            var target = this.target as WindowSystemResources;

            var allObjects = target.GetAllObjects();

            GUILayoutExt.Box(2f, 2f, () => {
                
                GUILayout.Label("Resources: " + target.GetAllocatedCount());
                foreach (var item in allObjects) {

                    //EditorGUILayout.ObjectField("Handler", item.Key as Object, typeof(Object), allowSceneObjects: true);
                    EditorGUILayout.LabelField("Handler", item.Key.ToString());

                    ++EditorGUI.indentLevel;
                    foreach (var resItem in item.Value) {

                        EditorGUILayout.LabelField(resItem.resourceId.ToString());
                        if (resItem.resource is Object) {
                         
                            EditorGUILayout.ObjectField((Object)resItem.resource, typeof(Object), allowSceneObjects: true);
   
                        } else {

                            EditorGUILayout.LabelField(resItem.resource.ToString());

                        }

                    }
                    --EditorGUI.indentLevel;

                }

            });
            
        }

    }

}
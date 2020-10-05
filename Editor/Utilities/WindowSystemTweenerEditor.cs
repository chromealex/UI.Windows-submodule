using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;
    using UnityEngine.UI.Windows.Modules;
    using UnityEngine.UI.Windows.Utilities;
    
    [CustomEditor(typeof(Tweener), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class WindowSystemTweenerEditor : Editor {

        public void OnEnable() {

            EditorApplication.update += this.Repaint;

        }

        public override void OnInspectorGUI() {

            var target = (Tweener)this.target;
            
            foreach (var tween in target.tweens) {

                GUILayoutExt.Box(2f, 2f, () => {

                    var data = (Tweener.ITweenInternal)tween;
                    EditorGUILayout.ObjectField("Tag", data.GetTag() as Object, typeof(Object), allowSceneObjects: true);
                    if (data.GetDelay() > 0f) EditorGUILayout.LabelField("Delay", data.GetDelay().ToString("mm:ss"));
                    EditorGUILayout.Slider(new GUIContent("Timer"), data.GetTimer(), 0f, 1f);

                });

            }
                
        }

    }

}

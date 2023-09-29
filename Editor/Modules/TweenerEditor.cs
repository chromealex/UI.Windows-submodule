using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;
    using UnityEngine.UI.Windows.Modules;

    [CustomEditor(typeof(UnityEngine.UI.Windows.Utilities.Tweener))]
    public class WindowSystemTweenerEditor : Editor {

        public void OnEnable() {

            EditorApplication.update += this.Repaint;

        }

        public override void OnInspectorGUI() {

            GUILayoutExt.DrawComponentHeader(this.serializedObject, "EXT", () => {
                
                GUILayout.Label("WindowSystem Internal module.\nTweener system.", GUILayout.Height(36f));
                
            }, new Color(0.4f, 0.2f, 0.7f, 1f));

            var target = this.target as UnityEngine.UI.Windows.Utilities.Tweener;
            
            var leftStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
            leftStyle.alignment = TextAnchor.MiddleLeft;
            var rightStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
            rightStyle.alignment = TextAnchor.MiddleRight;
            
            UnityEditor.UI.Windows.GUILayoutExt.DrawHeader("Tweens");
            foreach (var tween in target.tweens) {

                GUILayoutExt.Box(2f, 2f, () => {

                    var data = (UnityEngine.UI.Windows.Utilities.Tweener.ITweenInternal)tween;
                    EditorGUILayout.ObjectField("Tag", data.GetTag() as Object, typeof(Object), allowSceneObjects: true);
                    if (data.GetDelay() > 0f) EditorGUILayout.LabelField("Delay", System.TimeSpan.FromSeconds(data.GetDelay()).ToString(@"c"));
                    if (data.GetTimer() > 0f) EditorGUILayout.LabelField("Timer", System.TimeSpan.FromSeconds(data.GetTimer() * data.GetDuration()).ToString(@"c"));
                    if (data.GetDuration() > 0f) EditorGUILayout.LabelField("Duration", System.TimeSpan.FromSeconds(data.GetDuration()).ToString(@"c"));
                    
                    var rect = GUILayoutExt.ProgressBar(data.GetTimer() * data.GetDuration(), data.GetDuration(), height: 10f);

                    EditorGUI.LabelField(rect, data.GetFrom().ToString(), leftStyle);
                    EditorGUI.LabelField(rect, data.GetTo().ToString(), rightStyle);
                    
                });

            }

        }

    }

}
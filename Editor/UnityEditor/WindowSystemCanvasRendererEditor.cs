using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    [CustomEditor(typeof(CanvasRenderer), editorForChildClasses: true)]
    public class WindowSystemCanvasRendererEditor : Editor {

        public override void OnInspectorGUI() {

            if (this.targets.Length == 1) {

                var ren = (CanvasRenderer)this.target;
                var newCull = EditorGUILayout.Toggle("Cull", ren.cull);
                if (newCull != ren.cull) {

                    this.serializedObject.Update();
                    ren.cull = newCull;
                    this.serializedObject.ApplyModifiedProperties();

                }
            
            }

            this.DrawDefaultInspector();

        }

    }

}

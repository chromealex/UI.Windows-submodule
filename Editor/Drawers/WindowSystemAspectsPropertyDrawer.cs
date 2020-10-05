using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;

    [CustomPropertyDrawer(typeof(WindowSystemTarget.Aspects))]
    public class WindowSystemAspectsPropertyDrawer : PropertyDrawer {

        private UnityEditorInternal.ReorderableList list;
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

            var propertyArr = property.FindPropertyRelative("items");
            var h = propertyArr.arraySize * (120f + 4f) + 20f + 20f;
            if (propertyArr.arraySize == 0) h += 120f;
            return h;

        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            var propertyArr = property.FindPropertyRelative("items");

            if (this.list == null) {
                
                this.list = new UnityEditorInternal.ReorderableList(property.serializedObject, propertyArr, false, true, true, true);
                this.list.elementHeight = 120f;
                this.list.onAddCallback = (rList) => {

                    if (rList.serializedProperty != null) {
                        
                        ++rList.serializedProperty.arraySize;
                        rList.index = rList.serializedProperty.arraySize - 1;

                        var prop = rList.serializedProperty.GetArrayElementAtIndex(rList.index);
                        prop.FindPropertyRelative("aspectFrom").vector2Value = new Vector2(16f, 9f);
                        prop.FindPropertyRelative("aspectTo").vector2Value = new Vector2(4f, 3f);

                    }

                }; 
                this.list.drawElementCallback = (rect, index, active, focused) => {
                
                    EditorGUI.PropertyField(rect, propertyArr.GetArrayElementAtIndex(index));
                
                }; 
                this.list.drawHeaderCallback = (rect) => {
                
                    GUI.Label(rect, "Aspects");
                
                };
                
            }
            
            this.list.DoList(position);
            
        }

    }

    [CustomPropertyDrawer(typeof(WindowSystemTarget.AspectItem))]
    public class WindowSystemAspectItemPropertyDrawer : PropertyDrawer {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            
            return 120f;
            
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            var aspectFromProp = property.FindPropertyRelative("aspectFrom");
            var aspectToProp = property.FindPropertyRelative("aspectTo");
            var aspectFrom = aspectFromProp.vector2Value;
            var aspectTo = aspectToProp.vector2Value;

            var offset = EditorGUI.indentLevel * 24f;
            var defPosition = position;
            var aspectRect = new Rect(position.x + offset, position.y, EditorGUIUtility.labelWidth, position.height);
            
            var fillColor = new Color(0.5f, 0.3f, 0.3f, 0.2f);
            var borderColor = fillColor;
            borderColor.a = 0.5f;

            position.x += EditorGUIUtility.labelWidth;
            position.width -= EditorGUIUtility.labelWidth;

            GUILayoutExt.DrawRect(position, fillColor);
            GUILayoutExt.DrawBoxNotFilled(position, 1f, borderColor);

            var padding = 4f;
            var size = position.height - padding * 2f;
            var rect = new Rect(position.x + position.width * 0.5f - size * 0.5f, position.y + padding, size, size);
            GUILayoutExt.DrawRect(rect, fillColor);

            var fromRect = new Rect(0f, 0f, aspectFrom.x, aspectFrom.y);
            var toRect = new Rect(0f, 0f, aspectTo.x, aspectTo.y);
            fromRect = EditorHelpers.FitRect(fromRect, rect);
            toRect = EditorHelpers.FitRect(toRect, rect);
            GUILayoutExt.DrawBoxNotFilled(fromRect, 2f, new Color(0.4f, 0.4f, 1f, 0.6f));
            GUILayoutExt.DrawBoxNotFilled(toRect, 2f, new Color(1f, 0.4f, 1f, 0.6f));

            {

                GUILayout.BeginArea(aspectRect);
                //GUILayoutExt.Box(4f, 4f, () => {

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(defPosition.x + 10f);

                    GUILayout.BeginVertical();
                    {
                        
                        GUILayout.Label("From");
                        GUILayout.BeginHorizontal();
                        aspectFrom.x = EditorGUILayout.FloatField(aspectFrom.x);
                        GUILayout.Label(":", GUILayout.Width(10f));
                        aspectFrom.y = EditorGUILayout.FloatField(aspectFrom.y);
                        aspectFromProp.vector2Value = aspectFrom;
                        GUILayout.EndHorizontal();

                        //}, GUIStyle.none);
                        //GUILayoutExt.Box(4f, 4f, () => {

                        GUILayout.Label("To");
                        GUILayout.BeginHorizontal();
                        aspectTo.x = EditorGUILayout.FloatField(aspectTo.x);
                        GUILayout.Label(":", GUILayout.Width(10f));
                        aspectTo.y = EditorGUILayout.FloatField(aspectTo.y);
                        aspectToProp.vector2Value = aspectTo;
                        GUILayout.EndHorizontal();

                    }
                    GUILayout.EndVertical();

                }
                GUILayout.EndHorizontal();
                
                //}, GUIStyle.none);
                GUILayout.EndArea();

            }

        }

    }

}

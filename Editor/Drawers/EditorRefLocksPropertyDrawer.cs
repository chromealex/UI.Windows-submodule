namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;
    using UnityEngine;
    
    [CustomPropertyDrawer(typeof(EditorRefLocks))]
    public class EditorRefLocksPropertyDrawer : PropertyDrawer {

        private UnityEditorInternal.ReorderableList editorRefLocksList;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

            this.Validate(property);
            return this.editorRefLocksList.GetHeight();
            
        }

        public override void OnGUI(UnityEngine.Rect position, SerializedProperty property, UnityEngine.GUIContent label) {
            
            this.Validate(property);
            
            this.editorRefLocksList.DoList(position);
            
        }

        public void Validate(SerializedProperty property) {
            
            if (this.editorRefLocksList == null) {
                
                var componentsProp = property.FindPropertyRelative("directories");
                this.editorRefLocksList = new UnityEditorInternal.ReorderableList(componentsProp.serializedObject, componentsProp, true, true, true, true);
                this.editorRefLocksList.elementHeight = 40f;
                this.editorRefLocksList.onAddCallback = (rList) => {

                    if (rList.serializedProperty != null) {

                        ++rList.serializedProperty.arraySize;
                        rList.index = rList.serializedProperty.arraySize - 1;
                        var idx = rList.index;
                        var prop = componentsProp.GetArrayElementAtIndex(idx);
                        prop.stringValue = null;
                        
                    }

                };
                this.editorRefLocksList.drawElementBackgroundCallback = (rect, index, active, focused) => {

                    if (focused == true || active == true) {

                        GUILayoutExt.DrawRect(rect, new Color(0.1f, 0.4f, 0.7f, 1f));

                    } else {

                        GUILayoutExt.DrawRect(rect, new Color(1f, 1f, 1f, index % 2 == 0 ? 0.05f : 0f));

                    }

                };
                this.editorRefLocksList.elementHeightCallback = (index) => {

                    return EditorGUIUtility.singleLineHeight;

                };
                this.editorRefLocksList.drawElementCallback = (rect, index, active, focused) => {

                    var prop = componentsProp.GetArrayElementAtIndex(index);
                    var val = prop.stringValue;
                    Object dir = null;
                    if (string.IsNullOrEmpty(val) == false) {
                        var path = AssetDatabase.GUIDToAssetPath(val);
                        dir = AssetDatabase.LoadAssetAtPath<Object>(path);
                    }

                    var newDir = EditorGUI.ObjectField(rect, dir, typeof(Object), allowSceneObjects: false);
                    if (newDir != dir) {

                        var path = (newDir == null ? null : AssetDatabase.GetAssetPath(newDir));
                        componentsProp.serializedObject.Update();
                        prop.stringValue = (string.IsNullOrEmpty(path) == false ? AssetDatabase.AssetPathToGUID(path) : null);
                        componentsProp.serializedObject.ApplyModifiedProperties();

                    }

                };
                this.editorRefLocksList.headerHeight = 32f;
                UnityEditorInternal.ReorderableList.defaultBehaviours.headerBackground.fixedHeight = 0f;
                this.editorRefLocksList.drawHeaderCallback = (rect) => {
                    rect.height = EditorGUIUtility.singleLineHeight;
                    GUI.Label(rect, "Resource Directories");
                    rect.y += rect.height;
                    rect.height = 10f;
                    #if USE_PROPERTY_DRAWERS_OVERRIDE
                    GUI.Label(rect, "Only this directories must be used for resources.", EditorStyles.miniLabel);
                    #else
                    GUI.Label(rect, "Only this directories must be used for resources. Add USE_PROPERTY_DRAWERS_OVERRIDE define to use this feature.", EditorStyles.miniLabel);
                    #endif
                };
            
            }
            
        }

    }

}
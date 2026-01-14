using System.Linq;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;
    using UnityEngine;
    
    public static class EditorRefLocksPropertyDrawer {

        private static UnityEditorInternal.ReorderableList editorRefLocksList;
        private static SerializedObject serializedObject;

        public static void Draw(SerializedObject serializedObject, bool core = false) {
            
            if (editorRefLocksList == null || EditorRefLocksPropertyDrawer.serializedObject != serializedObject) {

                EditorRefLocksPropertyDrawer.serializedObject = serializedObject;
                
                var component = serializedObject.targetObject;
                if (component == null) return;

                var componentsProp = WindowSystemEditor.GetRefLockProperty(core == true ? null : component);
                editorRefLocksList = new UnityEditorInternal.ReorderableList(componentsProp.serializedObject, componentsProp, true, true, true, true);
                editorRefLocksList.elementHeight = 40f;
                editorRefLocksList.onAddCallback = (rList) => {

                    if (rList.serializedProperty != null) {

                        ++componentsProp.arraySize;
                        rList.index = componentsProp.arraySize - 1;
                        var idx = rList.index;
                        var prop = componentsProp.GetArrayElementAtIndex(idx);
                        prop.stringValue = null;
                        
                    }

                };
                editorRefLocksList.onRemoveCallback = list => {

                    foreach (var index in list.selectedIndices.Reverse()) {
                        componentsProp.DeleteArrayElementAtIndex(index);
                    }
                    componentsProp.serializedObject.ApplyModifiedProperties();
                    componentsProp.serializedObject.Update();
                    WindowSystemEditor.ValidateRefLock();
                    
                };
                editorRefLocksList.drawElementBackgroundCallback = (rect, index, active, focused) => {

                    if (focused == true || active == true) {

                        GUILayoutExt.DrawRect(rect, new Color(0.1f, 0.4f, 0.7f, 1f));

                    } else {

                        GUILayoutExt.DrawRect(rect, new Color(1f, 1f, 1f, index % 2 == 0 ? 0.05f : 0f));

                    }

                };
                editorRefLocksList.elementHeightCallback = (index) => {

                    return EditorGUIUtility.singleLineHeight;

                };
                editorRefLocksList.drawElementCallback = (rect, index, active, focused) => {

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
                        prop = componentsProp.GetArrayElementAtIndex(index);
                        prop.stringValue = (string.IsNullOrEmpty(path) == false ? AssetDatabase.AssetPathToGUID(path) : null);
                        componentsProp.serializedObject.ApplyModifiedProperties();
                        componentsProp.serializedObject.Update();
                        
                    }

                };
                editorRefLocksList.headerHeight = 32f;
                UnityEditorInternal.ReorderableList.defaultBehaviours.headerBackground.fixedHeight = 0f;
                editorRefLocksList.drawHeaderCallback = (rect) => {
                    rect.height = EditorGUIUtility.singleLineHeight;
                    GUI.Label(rect, "Resource Directories");
                    rect.y += rect.height;
                    rect.height = 10f;
                    #if USE_PROPERTY_DRAWERS_OVERRIDE
                    GUI.Label(rect, "Only these directories must be used for resources.", EditorStyles.miniLabel);
                    #else
                    GUI.Label(rect, "Only these directories must be used for resources. Add USE_PROPERTY_DRAWERS_OVERRIDE define to use this feature.", EditorStyles.miniLabel);
                    #endif
                };
            
            }
            
            editorRefLocksList.DoLayoutList();

            
        }

    }

}
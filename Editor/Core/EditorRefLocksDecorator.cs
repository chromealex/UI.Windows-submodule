namespace UnityEditor.UI.Windows {
    
    #if UNITY_2021_2_OR_NEWER
    using EditorSceneManagement = UnityEditor.SceneManagement;
    #else
    using EditorSceneManagement = UnityEditor.Experimental.SceneManagement;
    #endif
    
    using UnityEngine;
    using UnityEngine.UI.Windows;

    #if USE_PROPERTY_DRAWERS_OVERRIDE
    [CustomPropertyDrawer(typeof(UnityEngine.Sprite), true)]
    public class SpriteDrawer : ObjDrawer {

    }

    [CustomPropertyDrawer(typeof(UnityEngine.Texture), true)]
    public class TextureDrawer : ObjDrawer {

    }

    [CustomPropertyDrawer(typeof(UnityEngine.Material), true)]
    public class MaterialDrawer : ObjDrawer {

    }
    #endif

    public class ObjDrawer : PropertyDrawer {

        private System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
        private GameObject prevSelected;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

            var dirs = this.GetDirectories(property);
            var h = EditorGUI.GetPropertyHeight(property, label);
            if (dirs == null) {

                return h;

            } else {

                var isValid = true;
                var val = property.objectReferenceValue;
                if (val != null) {

                    isValid = false;
                    var path = AssetDatabase.GetAssetPath(val);
                    for (int i = 0; i < dirs.Count; ++i) {

                        var dirPath = AssetDatabase.GUIDToAssetPath(dirs[i]);
                        if (string.IsNullOrEmpty(dirPath) == true) continue;
                        if (path.StartsWith(dirPath) == true) {

                            isValid = true;
                            break;
                            
                        }
                        
                    }

                }

                if (isValid == true) {

                    return h;

                } else {

                    return h + 3f;

                }

            }
            
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            var source = position;
            position.height = EditorGUI.GetPropertyHeight(property, label);
            var dirs = this.GetDirectories(property);
            if (dirs == null) {
                
                EditorGUI.PropertyField(position, property, label);
                
            } else {

                var isValid = true;
                var val = property.objectReferenceValue;
                if (val != null) {

                    isValid = false;
                    var path = AssetDatabase.GetAssetPath(val);
                    for (int i = 0; i < dirs.Count; ++i) {

                        var dirPath = AssetDatabase.GUIDToAssetPath(dirs[i]);
                        if (string.IsNullOrEmpty(dirPath) == true) continue;
                        if (path.StartsWith(dirPath) == true) {

                            isValid = true;
                            break;
                            
                        }
                        
                    }

                }

                if (isValid == true) {

                    EditorGUI.PropertyField(position, property, label);

                } else {

                    GUILayoutExt.DrawRect(new Rect(source.x, source.y + source.height - 2f, source.width, 2f), Color.red); 
                    EditorGUI.PropertyField(position, property, new GUIContent(label.text, label.image, "This resource path is invalid. Check `Resource Directories` section."));
                    
                }

            }

        }

        public System.Collections.Generic.List<string> GetDirectories(SerializedProperty property) {
            
            var activeObject = Selection.activeObject as GameObject;
            var usePrefabMode = true;
            if (activeObject == null ||
                (EditorSceneManagement.PrefabStageUtility.GetCurrentPrefabStage() == null &&
                 activeObject != null &&
                 activeObject.scene.IsValid() == true)) {
                activeObject = (property.serializedObject.targetObject as Component)?.gameObject;
                usePrefabMode = false;
                if (activeObject == null) return null;
            }

            if (this.prevSelected == activeObject) return this.list.Count > 0 ? this.list : null;
            this.prevSelected = activeObject;
            
            var components = activeObject.GetComponentsInParent<WindowObject>(true);
            if (components.Length > 0) {

                var path = string.Empty;
                if (usePrefabMode == true) {

                    var prefabMode = EditorSceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
                    if (prefabMode != null && prefabMode.IsPartOfPrefabContents(activeObject) == true) {

                        path = prefabMode.assetPath;

                    } else {

                        path = AssetDatabase.GetAssetPath(activeObject);

                    }

                    if (string.IsNullOrEmpty(path) == true) return null;

                }

                this.list.Clear();
                foreach (var component in components) {

                    if (component.editorRefLocks.directories != null) this.list.AddRange(component.editorRefLocks.directories);

                }

                if (usePrefabMode == true) {
                    
                    var splitted = path.Split(new[] { "/Components" }, System.StringSplitOptions.RemoveEmptyEntries);
                    var rootPath = splitted[0] + "/Screens";
                    var objs = AssetDatabase.FindAssets("t:prefab", new[] { rootPath });
                    foreach (var obj in objs) {

                        var screenPath = AssetDatabase.GUIDToAssetPath(obj);
                        var screen = AssetDatabase.LoadAssetAtPath<GameObject>(screenPath);
                        if (screen != null) {

                            var window = screen.GetComponent<WindowBase>();
                            if (window.editorRefLocks.directories != null) this.list.AddRange(window.editorRefLocks.directories);

                        }

                    }

                }

                //EditorGUI.LabelField(position, "OBJ: " + component + " :: " + list.Count);
                return this.list.Count > 0 ? this.list : null;

            }

            return null;

        }

    }

    /*[CustomPropertyDrawer(typeof(UnityEngine.Object), true)]
    public class EditorRefLocksDecorator : DecoratorDrawer {

        public override void OnGUI(UnityEngine.Rect position) {
            
            var activeObject = Selection.activeObject as GameObject;
            if (activeObject == null) return;
            
            var component = activeObject.GetComponent<WindowComponent>();
            if (component != null) {

                var path = AssetDatabase.GetAssetPath(activeObject);
                if (string.IsNullOrEmpty(path) == true) return;
                
                var list = new System.Collections.Generic.List<string>();
                if (component.editorRefLocks.enabled == true) {
                    
                    if (component.editorRefLocks.directories != null) list.AddRange(component.editorRefLocks.directories);
                    
                }
                
                var splitted = path.Split(new [] { "/Components" }, System.StringSplitOptions.RemoveEmptyEntries);
                var rootPath = splitted[0] + "/Screens";
                var objs = AssetDatabase.FindAssets("t:prefab", new[] { rootPath });
                foreach (var obj in objs) {

                    var screenPath = AssetDatabase.GUIDToAssetPath(obj);
                    var screen = AssetDatabase.LoadAssetAtPath<GameObject>(screenPath);
                    if (screen != null) {
                        
                        var window = screen.GetComponent<WindowBase>();
                        if (window.editorRefLocks.enabled == true) {
                            
                            if (window.editorRefLocks.directories != null) list.AddRange(window.editorRefLocks.directories);
                            
                        }
                        
                    }

                }
                EditorGUI.LabelField(position, "OBJ: " + component + " :: " + list.Count);
                
            }

        }

    }*/

}
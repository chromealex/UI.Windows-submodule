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
    
    [CustomPropertyDrawer(typeof(UnityEngine.Font), true)]
    public class FontDrawer : ObjDrawer {

    }
    
    #if TEXTMESHPRO_SUPPORT
    [CustomPropertyDrawer(typeof(TMPro.TMP_FontAsset), true)]
    public class TmpFontDrawer : ObjDrawer {

    }
    #endif
    #endif

    public struct ObjDrawerCache {

        public System.Collections.Generic.List<string> list;
        public GameObject go;

    }

    public class ObjDrawer : PropertyDrawer {

        private ObjDrawerCache prevSelected;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

            var h = EditorGUI.GetPropertyHeight(property, label);
            if (IsValid(property, ref this.prevSelected) == true) {
                return h;
            }
            
            return h + 3f;
            
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            position.height = EditorGUI.GetPropertyHeight(property, label);
            if (IsValid(property, ref this.prevSelected) == true) {
                EditorGUI.PropertyField(position, property, label);
            } else {
                GUILayoutExt.DrawInvalid(position);
                EditorGUI.PropertyField(position, property, new GUIContent(label.text, label.image, "This resource path is invalid. Check `Resource Directories` section."));
            }

        }

        public static bool IsValid(SerializedProperty property, ref ObjDrawerCache prevSelected) {
            
            var dirs = GetDirectories(property, ref prevSelected);
            if (dirs == null) {
                return true;
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

                return isValid;

            }
            
        }

        public static bool IsValid(GameObject obj, Object targetObj, ref ObjDrawerCache prevSelected) {

            if (targetObj is Component comp) {
                var prefabType = PrefabUtility.GetPrefabAssetType(comp.gameObject);
                if (prefabType == PrefabAssetType.NotAPrefab) {
                    return true;
                }
            }

            var dirs = GetDirectories(obj, ref prevSelected);
            if (dirs == null) {
                return true;
            } else {

                var isValid = true;
                var val = targetObj;
                if (val != null) {

                    isValid = false;
                    if (PrefabUtility.IsPartOfPrefabInstance(val) == true) {
                        val = PrefabUtility.GetCorrespondingObjectFromSource(val);
                    }
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

                return isValid;

            }
            
        }

        public static System.Collections.Generic.List<string> GetDirectories(SerializedProperty property, ref ObjDrawerCache prevSelected) {
            return GetDirectories((property.serializedObject.targetObject as Component)?.gameObject, ref prevSelected);
        }

        public static System.Collections.Generic.List<string> GetDirectories(GameObject go, ref ObjDrawerCache prevSelected) {

            if (prevSelected.list == null) {
                prevSelected.list = new System.Collections.Generic.List<string>();
            }
            
            var activeObject = Selection.activeObject as GameObject;
            var usePrefabMode = true;
            if (activeObject == null ||
                (EditorSceneManagement.PrefabStageUtility.GetCurrentPrefabStage() == null &&
                 activeObject != null &&
                 activeObject.scene.IsValid() == true)) {
                activeObject = go;
                usePrefabMode = false;
                if (activeObject == null) return null;
            }

            if (prevSelected.go == activeObject) return prevSelected.list.Count > 0 ? prevSelected.list : null;
            prevSelected.go = activeObject;
            
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

                prevSelected.list.Clear();
                foreach (var component in components) {

                    var list = WindowSystemEditor.GetRefLock(component);
                    if (list != null) {
                        prevSelected.list.AddRange(list);
                    }

                }

                {
                    var list = WindowSystemEditor.GetRefLock("UIWS");
                    if (list != null) {
                        prevSelected.list.AddRange(list);
                    }
                }

                if (usePrefabMode == true) {
                    
                    var splitted = path.Split(new[] { "/Components" }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (splitted.Length == 2) {
                        var rootPath = $"{splitted[0]}/Screens";
                        var objs = AssetDatabase.FindAssets("t:prefab", new[] { rootPath });
                        foreach (var obj in objs) {
                            var screenPath = AssetDatabase.GUIDToAssetPath(obj);
                            var screen = AssetDatabase.LoadAssetAtPath<GameObject>(screenPath);
                            if (screen != null) {
                                var window = screen.GetComponent<WindowBase>();
                                var list = WindowSystemEditor.GetRefLock(window);
                                if (list != null) {
                                    prevSelected.list.AddRange(list);
                                }
                            }
                        }
                    }

                }

                //EditorGUI.LabelField(position, "OBJ: " + component + " :: " + list.Count);
                return prevSelected.list.Count > 0 ? prevSelected.list : null;

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
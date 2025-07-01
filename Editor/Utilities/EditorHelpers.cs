using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnityEditor.UI.Windows {

    public readonly struct EditPrefabAssetScope : System.IDisposable {
 
        public readonly string assetPath;
        public readonly GameObject prefabRoot;
 
        public EditPrefabAssetScope(string assetPath) {
            this.assetPath = assetPath;
            this.prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);
        }
 
        public void Dispose() {
            PrefabUtility.SaveAsPrefabAsset(this.prefabRoot, this.assetPath);
            PrefabUtility.UnloadPrefabContents(this.prefabRoot);
        }
    }

    public static class StringExtensions {
        
        public static string ToSentenceCase(this string str) {

            if (str == null) return string.Empty;
            return System.Text.RegularExpressions.Regex.Replace(str, "[a-z][A-Z]", m => $"{m.Value[0]} {char.ToLower(m.Value[1])}");
		
        }
	
        public static string UppercaseWords(this string value) {

            var chars = new char[] { ' ', '_', '?', '.', ',', '!', '@', '#', '$', '%', '^', '&', '*', '(', '{', '[', '/', '\\' };

            char[] array = value.ToCharArray();
            // Handle the first letter in the string.
            if (array.Length >= 1) {

                if (char.IsLower(array[0])) {

                    array[0] = char.ToUpper(array[0]);

                }

            }

            // Scan through the letters, checking for spaces.
            // ... Uppercase the lowercase letters following spaces.
            for (int i = 1; i < array.Length; ++i) {

                if (System.Array.IndexOf(chars, array[i - 1]) >= 0) {

                    if (char.IsLower(array[i])) {

                        array[i] = char.ToUpper(array[i]);

                    }

                }

            }

            return new string(array);

        }

    }

    public struct ImageCollectionItem {

        public Component holder;
        public Object obj;

    }

    public static class EditorHelpers {

        public static Texture2D CollectImages(Object target, List<ImageCollectionItem> images) {

            return CollectImages(new[] { target }, images);

        }

        public static Texture2D CollectImages(Object[] targets, List<ImageCollectionItem> images) {
            
            var used = new HashSet<Object>();
            var visited = new HashSet<object>();
            var visitedFonts = new HashSet<object>();
            foreach (var target in targets) {

                var components = ((UnityEngine.Component)target).gameObject.GetComponentsInChildren<Component>(true);
                foreach (var component in components) {

                    EditorHelpers.FindType(component, new[] { typeof(Sprite), typeof(Texture), typeof(Texture2D) }, (fieldInfo, obj) => {

                        if (obj is Object texObj && texObj != null) {
                            images.Add(new ImageCollectionItem() {
                                holder = component,
                                obj = texObj,
                            });
                        }

                        return obj;

                    }, visited, includeUnityObjects: true, ignoreTypes: new[] { typeof(TMPro.TMP_FontAsset), typeof(TMPro.TextMeshProUGUI) });

                    EditorHelpers.FindType(component, new[] { typeof(Font), typeof(TMPro.TMP_FontAsset) }, (fieldInfo, obj) => {

                        if (obj is Object texObj && texObj != null) {
                            images.Add(new ImageCollectionItem() {
                                holder = component,
                                obj = texObj,
                            });
                        }

                        return obj;

                    }, visitedFonts, includeUnityObjects: true);

                }

            }

            {
                var preview = new List<Texture2D>();
                foreach (var img in images) {

                    if (used.Contains(img.obj) == true) continue;

                    used.Add(img.obj);
                    var tex = img.obj as Texture2D;
                    if (img.obj is Sprite sprite) {

                        tex = UnityEditor.Sprites.SpriteUtility.GetSpriteTexture(sprite, false);

                    }

                    if (tex != null) {

                        var copy = EditorHelpers.CopyTexture(tex);
                        preview.Add(copy);

                    }

                }

                var previewTexture = new Texture2D(10, 10, TextureFormat.RGBA32, false);
                previewTexture.PackTextures(preview.ToArray(), 0, 4096, false);
                return previewTexture;
            }
            
        }

        public static Texture2D CopyTexture(Texture2D texture) {
        
            RenderTexture tmp = RenderTexture.GetTemporary( 
                texture.width,
                texture.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);
            Graphics.Blit(texture, tmp);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tmp;
            Texture2D myTexture2D = new Texture2D(texture.width, texture.height);
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmp);

            return myTexture2D;

        }

        public static void FindType(object root, System.Type[] searchTypes, System.Func<FieldInfo, object, object> del, HashSet<object> visited = null, bool includeUnityObjects = false, System.Type[] ignoreTypes = null) {
            
            if (root == null) return;
            if (visited == null) visited = new HashSet<object>();

            if (visited.Contains(root) == true) return;
            visited.Add(root);

            var rootType = root.GetType();
            if (ignoreTypes != null) {

                if (System.Array.IndexOf(ignoreTypes, rootType) >= 0) return;

            }

            var fields = rootType.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            foreach (var field in fields) {

                if (field.FieldType.IsPrimitive == true) continue;
                if (field.FieldType.IsPointer == true) continue;
                if (typeof(Component).IsAssignableFrom(field.FieldType) == true) continue;
                
                if (System.Array.IndexOf(searchTypes, field.FieldType) >= 0) {

                    var obj = field.GetValue(root);
                    field.SetValue(root, del.Invoke(field, obj));

                } else if (field.FieldType.IsArray == true) {
                    
                    var arr = (System.Array)field.GetValue(root);
                    if (arr != null) {
                        for (int i = 0; i < arr.Length; ++i) {
                            var r = arr.GetValue(i);
                            if (r != null) {
                                if (System.Array.IndexOf(searchTypes, r.GetType()) >= 0) {
                                    arr.SetValue(del.Invoke(field, r), i);
                                } else {
                                    if (includeUnityObjects == true || (r is Object) == false) EditorHelpers.FindType(r, searchTypes, del, visited);
                                    arr.SetValue(r, i);
                                }
                            }
                        }
                    }

                } else {
                    
                    var obj = field.GetValue(root);
                    if (includeUnityObjects == true || (obj is Object) == false) EditorHelpers.FindType(obj, searchTypes, del, visited);
                    field.SetValue(root, obj);
                    
                }

            }

        }

        public static bool FindType(object root, System.Type searchType, System.Func<MemberInfo, object, object> del, HashSet<object> visited = null, bool includeUnityObjects = false, System.Type[] ignoreTypes = null, bool getProperties = false) {

            var result = false;
            
            if (root == null) return result;
            if (visited == null) visited = new HashSet<object>();

            if (root.GetType().IsValueType == false && root.GetType().IsPrimitive == false) {
                                        
                if (visited.Add(root) == false) return result;

            }

            var isGeneric = searchType.IsGenericType;
            
            System.Func<System.Type, System.Type, bool> check = (t1, search) => {

                if (isGeneric == true) {

                    if (t1.IsGenericType == false) return false;
                    return t1.GetGenericTypeDefinition() == search;

                }
                
                return t1 == search;
                
            };

            var rootType = root.GetType();
            if (ignoreTypes != null) {

                if (System.Array.IndexOf(ignoreTypes, rootType) >= 0) return result;

            }

            var fields = rootType.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (getProperties == true) {
                
                var props = rootType.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                foreach (var field in props) {

                    if (field.GetMethod == null || field.SetMethod == null) continue;
                    if (field.CanRead == false || field.CanWrite == false) continue;

                    if (field.PropertyType.IsPrimitive == true) continue;
                    if (field.PropertyType.IsPointer == true) continue;
                    if (typeof(Component).IsAssignableFrom(field.PropertyType) == true) continue;

                    if (check.Invoke(field.PropertyType, searchType) == true) {

                        var obj = field.GetValue(root);
                        field.SetValue(root, del.Invoke(field, obj));
                        result = true;

                    } else if (field.PropertyType.IsArray == true) {

                        var arr = (System.Array)field.GetValue(root);
                        if (arr != null) {
                            for (int i = 0; i < arr.Length; ++i) {
                                var r = arr.GetValue(i);
                                if (r != null) {
                                    if (check.Invoke(r.GetType(), searchType) == true) {
                                        arr.SetValue(del.Invoke(field, r), i);
                                    } else {
                                        var localResult = false;
                                        if (includeUnityObjects == true || (r is Object) == false) localResult = EditorHelpers.FindType(r, searchType, del, visited);
                                        if (localResult == true) {
                                            arr.SetValue(r, i);
                                            result = true;
                                        }
                                    }
                                }
                            }
                        }

                    } else {

                        var localResult = false;
                        var obj = field.GetValue(root);
                        if (includeUnityObjects == true || (obj is Object) == false) localResult = EditorHelpers.FindType(obj, searchType, del, visited);
                        if (localResult == true) {
                            field.SetValue(root, obj);
                            result = true;
                        }

                    }

                }
                
            }
            foreach (var field in fields) {

                if (field.FieldType.IsPrimitive == true) continue;
                if (field.FieldType.IsPointer == true) continue;
                if (typeof(Component).IsAssignableFrom(field.FieldType) == true) continue;
                
                if (check.Invoke(field.FieldType, searchType) == true) {

                    var obj = field.GetValue(root);
                    field.SetValue(root, del.Invoke(field, obj));

                } else if (field.FieldType.IsArray == true) {
                    
                    var arr = (System.Array)field.GetValue(root);
                    if (arr != null) {
                        for (int i = 0; i < arr.Length; ++i) {
                            var r = arr.GetValue(i);
                            if (r != null) {
                                if (check.Invoke(r.GetType(), searchType) == true) {
                                    arr.SetValue(del.Invoke(field, r), i);
                                } else {
                                    var localResult = false;
                                    if (includeUnityObjects == true || (r is Object) == false) localResult = EditorHelpers.FindType(r, searchType, del, visited);
                                    if (localResult == true) {
                                        arr.SetValue(r, i);
                                        result = true;
                                    }
                                }
                            }
                        }
                    }

                } else {
                    
                    var localResult = false;
                    var obj = field.GetValue(root);
                    if (includeUnityObjects == true || (obj is Object) == false) localResult = EditorHelpers.FindType(obj, searchType, del, visited);
                    if (localResult == true) {
                        field.SetValue(root, obj);
                        result = true;
                    }
                }

            }
            
            return result;

        }

        public static void UpdateLayoutWindow(UnityEngine.UI.Windows.Utilities.DirtyHelper helper, UnityEngine.UI.Windows.WindowTypes.LayoutWindowType layoutWindowType) {
        
            var itemsLayout = layoutWindowType.layouts.items;
            if (itemsLayout != null) {

                for (int j = 0; j < itemsLayout.Length; ++j) {

                    ref var layoutItem = ref itemsLayout[j];
                    if (layoutItem == null) {
                        layoutItem = new UnityEngine.UI.Windows.WindowTypes.LayoutItem();
                    }

                    layoutItem.Validate(helper);

                    var windowLayoutType = layoutItem.windowLayout;
                    if (windowLayoutType != null) {

                        windowLayoutType.ValidateEditor();

                        { // Validate components list

                            for (int c = 0; c < layoutItem.components.Length; ++c) {

                                ref var com = ref layoutItem.components[c];
                                WindowSystemResourcesResourcePropertyDrawer.Validate(ref com.component, typeof(UnityEngine.UI.Windows.WindowComponent));

                            }

                        }

                    }

                }

            }

        }

        public static void AddSafeZone(Transform root) {
            
            var safeGo = new GameObject("SafeZone", typeof(UnityEngine.UI.Windows.WindowLayoutSafeZone));
            safeGo.transform.SetParent(root);
            safeGo.GetComponent<UnityEngine.UI.Windows.WindowLayoutSafeZone>().ValidateEditor();
            safeGo.GetComponent<UnityEngine.UI.Windows.WindowLayoutSafeZone>().SetTransformFullRect();
                                
            for (int i = root.transform.childCount - 1; i >= 0; --i) {

                var child = root.transform.GetChild(i);
                var scale = child.localScale;
                child.SetParent(safeGo.transform, worldPositionStays: true);
                child.localScale = scale;

            }

            root.GetComponent<UnityEngine.UI.Windows.WindowLayout>().safeZone = safeGo.GetComponent<UnityEngine.UI.Windows.WindowLayoutSafeZone>();

        }
        
        public static void SetFirstSibling(Object[] objects, int siblingIndexTarget = 0) {

            foreach (var obj in objects) {

                var comp = obj as Component;
                var comps = comp.GetComponents<Component>();
                var countPrev = 0;
                foreach (var c in comps) {

                    if (c == obj) break;
                    ++countPrev;

                }

                --countPrev;
                var target = countPrev - siblingIndexTarget;
                
                for (int i = 0; i < target; ++i) {

                    UnityEditorInternal.ComponentUtility.MoveComponentUp(comp);

                }

            }
            
        }
        
        public static Rect FitRect(Rect rect, Rect root) {

            if (rect.width > rect.height) {

                rect.height = rect.height / rect.width * root.height;
                rect.width = root.width;

                rect.x = root.x;
                rect.y = root.y + root.height * 0.5f - rect.height * 0.5f;

            } else {
                
                rect.width = rect.width / rect.height * root.width;
                rect.height = root.height;

                rect.x = root.x + root.width * 0.5f - rect.width * 0.5f;
                rect.y = root.y;

            }

            return rect;

        }
        
        public static string StringToCaption(string str) {

            return System.Text.RegularExpressions.Regex.Replace(str, "[A-Z]", (match) => { return " " + match.Value.Trim(); }).Trim();

        }

        public static System.Reflection.FieldInfo GetFieldViaPath(System.Type type, string path) {

            return EditorHelpers.GetFieldViaPath(type, path, out _);

        }

        public static bool IsFieldOfTypeBeneath(System.Type type, System.Type baseType, string path) {

            while (type != baseType) {

                var parent = type;
                var fi = parent.GetField(path);
                var paths = path.Split('.');

                for (var i = 0; i < (path.Length > 0 ? 1 : paths.Length); i++) {

                    fi = parent.GetField(paths[i]);
                    if (fi == null) {
                        return false;
                    }

                    // there are only two container field type that can be serialized:
                    // Array and List<T>
                    if (fi.FieldType.IsArray) {
                        parent = fi.FieldType.GetElementType();
                        i += 2;
                        continue;
                    }

                    if (fi.FieldType.IsGenericType) {
                        parent = fi.FieldType.GetGenericArguments()[0];
                        i += 2;
                        continue;
                    }

                    parent = fi.FieldType;

                }

                if (fi.DeclaringType == type) {

                    return true;

                }

                type = type.BaseType;

            }

            return false;

        }
        
        public static System.Reflection.FieldInfo GetFieldViaPath(System.Type type, string path, out System.Type parent) {

            parent = type;
            var fi = parent.GetField(path);
            var paths = path.Split('.');

            for (var i = 0; i < paths.Length; i++) {

                fi = parent.GetField(paths[i]);
                if (fi == null) {
                    return null;
                }

                // there are only two container field type that can be serialized:
                // Array and List<T>
                if (fi.FieldType.IsArray) {
                    parent = fi.FieldType.GetElementType();
                    i += 2;
                    continue;
                }

                if (fi.FieldType.IsGenericType) {
                    parent = fi.FieldType.GetGenericArguments()[0];
                    i += 2;
                    continue;
                }

                parent = fi.FieldType;
                
            }

            return fi;

        }

        public static void SetDirtyAndValidate(SerializedProperty property) {

            for (int i = 0; i < property.serializedObject.targetObjects.Length; ++i) {
                
                EditorUtility.SetDirty(property.serializedObject.targetObjects[i]);
                
            }
            
        }

    }

}
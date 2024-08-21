using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.U2D;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;
    using UnityEngine.UI.Windows.Modules;
    using UnityEngine.UI.Windows.Utilities;

    [CustomEditor(typeof(WindowSystem), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class WindowSystemEditor : Editor {

        public SerializedProperty audio;
        public SerializedProperty breadcrumbs;
        public SerializedProperty events;
        public SerializedProperty resources;
        public SerializedProperty pools;
        public SerializedProperty tweener;
        
        public SerializedProperty modules;

        public SerializedProperty settings;
        
        private SerializedProperty emulatePlatform;
        private SerializedProperty emulateRuntimePlatform;
        
        private SerializedProperty registeredPrefabs;

        private SerializedProperty showRootOnStart;
        private SerializedProperty rootScreen;

        private SerializedProperty loaderScreen;
        
        private int selectedTab {
            get {
                return EditorPrefs.GetInt("UnityEditor.UI.Windows.WindowSystem.TabIndex");
            }
            set {
                EditorPrefs.SetInt("UnityEditor.UI.Windows.WindowSystem.TabIndex", value);
            }
        }

        private Vector2 tabScrollPosition {
            get {
                return new Vector2(
                    EditorPrefs.GetFloat("UnityEditor.UI.Windows.WindowSystem.TabScrollPosition.X"),
                    EditorPrefs.GetFloat("UnityEditor.UI.Windows.WindowSystem.TabScrollPosition.Y")
                );
            }
            set {
                EditorPrefs.SetFloat("UnityEditor.UI.Windows.WindowSystem.TabScrollPosition.X", value.x);
                EditorPrefs.SetFloat("UnityEditor.UI.Windows.WindowSystem.TabScrollPosition.Y", value.y);
            }
        }

        private UnityEditorInternal.ReorderableList listModules;
        private System.Type textureUtils;
        private System.Reflection.MethodInfo getTextureSizeMethod;

        public void OnEnable() {

            this.textureUtils = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.TextureUtil");
            this.getTextureSizeMethod = this.textureUtils.GetMethod("GetStorageMemorySize", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            this.emulatePlatform = this.serializedObject.FindProperty("emulatePlatform");
            this.emulateRuntimePlatform = this.serializedObject.FindProperty("emulateRuntimePlatform");
            this.registeredPrefabs = this.serializedObject.FindProperty("registeredPrefabs");
            this.showRootOnStart = this.serializedObject.FindProperty("showRootOnStart");
            this.rootScreen = this.serializedObject.FindProperty("rootScreen");
            
            this.loaderScreen = this.serializedObject.FindProperty("loaderScreen");

            this.settings = this.serializedObject.FindProperty("settings");

            { // Modules
                this.audio = this.serializedObject.FindProperty("audio");
                this.breadcrumbs = this.serializedObject.FindProperty("breadcrumbs");
                this.events = this.serializedObject.FindProperty("events");
                this.resources = this.serializedObject.FindProperty("resources");
                this.pools = this.serializedObject.FindProperty("pools");
                this.tweener = this.serializedObject.FindProperty("tweener");
            }
            
            this.modules = this.serializedObject.FindProperty("modules");
            
            EditorHelpers.SetFirstSibling(this.targets);

        }

        private readonly struct UsedResource {

            public readonly WindowBase screen;
            public readonly Component component;
            public readonly Object resource;

            public UsedResource(WindowBase screen, Component component, Object resource) {
                
                this.screen = screen;
                this.component = component;
                this.resource = resource;
                
            }

            public override int GetHashCode() {
                
                return this.resource.GetHashCode();
                
            }

            public override string ToString() {
                
                return $"Screen: {this.screen}, Component: {this.component}, Resource: {this.resource}";
                
            }

            public string ToSmallString() {
                
                return $"Component: {this.component}, Resource: {this.resource}";
                
            }

        }

        public struct AtlasData {

            public UnityEngine.U2D.SpriteAtlas atlas;
            public Texture2D[] previews;

        }
        
        private Dictionary<WindowBase, HashSet<UsedResource>> usedResources = new Dictionary<WindowBase, HashSet<UsedResource>>();
        private HashSet<AtlasData> usedAtlases = new HashSet<AtlasData>();
        private bool dependenciesState;

        public override void OnInspectorGUI() {

            this.serializedObject.Update();
            
            GUILayoutExt.DrawComponentHeader(this.serializedObject, "UI", () => {
                
                GUILayout.Label("Window System", GUILayout.Height(36f));
                
            }, new Color(0.3f, 0.4f, 0.6f, 0.4f));
            
            GUILayout.Space(5f);
            
            var scroll = this.tabScrollPosition;
            this.selectedTab = GUILayoutExt.DrawTabs(
                this.selectedTab,
                ref scroll,
                new GUITab("Start Up", () => {

                    EditorGUILayout.PropertyField(this.emulatePlatform);
                    EditorGUILayout.PropertyField(this.emulateRuntimePlatform);
                    
                    GUILayout.Space(10f);
                    
                    EditorGUILayout.PropertyField(this.showRootOnStart);
                    EditorGUILayout.PropertyField(this.rootScreen);

                    GUILayout.Space(10f);

                    EditorGUILayout.PropertyField(this.loaderScreen);

                    GUILayout.Space(10f);

                    EditorGUILayout.PropertyField(this.settings);

                }),
                new GUITab("Modules", () => {

                    EditorGUILayout.PropertyField(this.breadcrumbs);
                    EditorGUILayout.PropertyField(this.events);
                    EditorGUILayout.PropertyField(this.resources);
                    EditorGUILayout.PropertyField(this.pools);
                    EditorGUILayout.PropertyField(this.tweener);
                    EditorGUILayout.PropertyField(this.audio);
                    
                    GUILayoutExt.DrawHeader("Custom Modules");
                    EditorGUILayout.PropertyField(this.modules);
                    
                }),
                new GUITab("Windows", () => {

                    var count = this.registeredPrefabs.arraySize;
                    EditorGUILayout.PropertyField(this.registeredPrefabs, new GUIContent("Registered Prefabs (" + count + ")"));

                    GUILayout.Space(10f);
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Collect prefabs", GUILayout.Width(200f), GUILayout.Height(30f)) == true) {

                        var list = new List<WindowBase>();
                        var gameObjects = AssetDatabase.FindAssets("t:GameObject");
                        foreach (var guid in gameObjects) {

                            var path = AssetDatabase.GUIDToAssetPath(guid);
                            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                            var win = asset.GetComponent<WindowBase>();
                            if (win != null) {
                                
                                list.Add(win);
                                
                            }

                        }
                        
                        this.registeredPrefabs.ClearArray();
                        this.registeredPrefabs.arraySize = list.Count;
                        for (int i = 0; i < list.Count; ++i) {

                            this.registeredPrefabs.GetArrayElementAtIndex(i).objectReferenceValue = list[i];


                        }

                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                }),
                new GUITab() {
                    caption = "Tools",
                    onDraw = () => {
                        
                        GUILayout.Space(10f);
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Make Addressables", GUILayout.Width(200f), GUILayout.Height(30f)) == true) {

                            try {

                                for (int i = 0; i < this.registeredPrefabs.arraySize; ++i) {

                                    var element = this.registeredPrefabs.GetArrayElementAtIndex(i);
                                    var window = element.objectReferenceValue as WindowBase;
                                    if (window != null) {

                                        EditorUtility.DisplayProgressBar("Updating Addressables", window.ToString(), i / (float)this.registeredPrefabs.arraySize);

                                        var path = AssetDatabase.GetAssetPath(window);
                                        var sourceDir = System.IO.Path.GetDirectoryName(path);
                                        var dir = sourceDir.Replace("/Screens", "");
                                        dir = dir.Replace("\\Screens", "");
                                        var mainDir = dir;

                                        if (System.IO.File.Exists(mainDir + "/UIWS-IgnoreAddressables.txt") == true) continue;
                                        if (System.IO.File.Exists(mainDir + "\\UIWS-IgnoreAddressables.txt") == true) continue;

                                        var name = $"UIWS-{window.name}-AddressablesGroup";
                                        UnityEditor.AddressableAssets.Settings.AddressableAssetSettings aaSettings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
                                        var groupPath = $"{mainDir}/{name}.asset";
                                        UnityEditor.AddressableAssets.Settings.AddressableAssetGroup group;
                                        if (System.IO.File.Exists(groupPath) == false) {
                                            
                                            group = aaSettings.CreateGroup(name, false, false, true, null);
                                            var scheme = group.AddSchema<UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema>();
                                            var schemeInstance = WindowSystemEditor.Instantiate(scheme);
                                            schemeInstance.name = "BundledAssetGroupSchema";
                                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(scheme));
                                            AssetDatabase.AddObjectToAsset(schemeInstance, group);
                                            group.Schemas.Clear();
                                            group.Schemas.Add(schemeInstance);
                                            AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(group), groupPath);
                                            
                                        }
                                        group = AssetDatabase.LoadAssetAtPath<UnityEditor.AddressableAssets.Settings.AddressableAssetGroup>(groupPath);
                                        
                                        dir = sourceDir.Replace("/Screens", "/Components");
                                        dir = dir.Replace("\\Screens", "\\Components");
                                        var components = AssetDatabase.FindAssets("t:GameObject", new string[] { dir });
                                        foreach (var guid in components) {

                                            var p = AssetDatabase.GUIDToAssetPath(guid);
                                            var componentGo = AssetDatabase.LoadAssetAtPath<GameObject>(p);
                                            var component = componentGo.GetComponent<WindowComponent>();
                                            if (component != null) {

                                                componentGo.SetAddressableID(p, group);
                                                EditorUtility.SetDirty(componentGo);
                                                
                                            }

                                        }

                                    }

                                    if (window is UnityEngine.UI.Windows.WindowTypes.LayoutWindowType layoutWindowType) {

                                        var helper = new DirtyHelper(layoutWindowType);
                                        EditorHelpers.UpdateLayoutWindow(helper, layoutWindowType);
                                        helper.Apply();

                                    }

                                }

                            } catch (System.Exception ex) {
                                Debug.LogException(ex);
                            }

                            EditorUtility.ClearProgressBar();
                            
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                        GUILayout.Label("Make all component in all registered screens as Addressables", EditorStyles.centeredGreyMiniLabel);

                        GUILayout.Space(10f);
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Validate Resources", GUILayout.Width(200f), GUILayout.Height(30f)) == true) {

                            EditorApplication.delayCall += () => {

                                try {

                                    var isBreak = false;
                                    var markDirtyCount = 0;
                                    var gos = AssetDatabase.FindAssets("t:GameObject");
                                    var visited = new HashSet<object>();
                                    var visitedGeneric = new HashSet<object>();
                                    var i = 0;
                                    foreach (var guid in gos) {

                                        var path = AssetDatabase.GUIDToAssetPath(guid);
                                        var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                                        if (EditorUtility.DisplayCancelableProgressBar("Validating Resources 1 / 2", path, i / (float)gos.Length) == true) {

                                            isBreak = true;
                                            break;

                                        }

                                        {
                                            var allComponents = go.GetComponentsInChildren<Component>(true);
                                            foreach (var component in allComponents) {

                                                EditorHelpers.FindType(component, typeof(Resource), (fieldInfo, res) => {

                                                    System.Type resType = null;
                                                    var resTypeAttrs = fieldInfo.GetCustomAttributes(typeof(ResourceTypeAttribute), true);
                                                    if (resTypeAttrs.Length > 0) {
                                                        resType = ((ResourceTypeAttribute)resTypeAttrs[0]).type;
                                                    }

                                                    var r = (Resource)res;
                                                    WindowSystemResourcesResourcePropertyDrawer.Validate(ref r, resType);
                                                    ++markDirtyCount;
                                                    EditorUtility.SetDirty(component.gameObject);
                                                    return r;

                                                }, visited);

                                            }

                                            foreach (var component in allComponents) {
                                            
                                                EditorHelpers.FindType(component, typeof(Resource<>), (fieldInfo, res) => {

                                                    try {

                                                        Debug.Log($"Validating reference for asset {path}, {guid}");

                                                        var rField =
                                                            (res.GetType().GetField("data",
                                                                System.Reflection.BindingFlags.Instance |
                                                                System.Reflection.BindingFlags.NonPublic));
                                                        var r = (Resource) rField.GetValue(res);
                                                        System.Type type = null;
                                                        var _fieldInfo = (System.Reflection.FieldInfo) fieldInfo;
                                                        if (_fieldInfo.FieldType.IsArray == true) {
                                                            type = _fieldInfo.FieldType.GetElementType()
                                                                .GetGenericArguments()[0];
                                                        }
                                                        else {
                                                            type = _fieldInfo.FieldType.GetGenericArguments()[0];
                                                        }

                                                        WindowSystemResourcesResourcePropertyDrawer.Validate(ref r,
                                                            type);
                                                        ++markDirtyCount;
                                                        rField.SetValue(res, r);
                                                        EditorUtility.SetDirty(component.gameObject);
                                                        return res;

                                                    }
                                                    catch (System.Exception ex) {
                                                        Debug.LogException(ex);
                                                        throw;
                                                    }

                                                }, visitedGeneric);
                                                
                                            }
                                            
                                        }

                                        ++i;

                                    }

                                    if (isBreak == false) {

                                        var sos = AssetDatabase.FindAssets("t:ScriptableObject");
                                        i = 0;
                                        foreach (var guid in sos) {

                                            var path = AssetDatabase.GUIDToAssetPath(guid);
                                            var go = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                                            if (EditorUtility.DisplayCancelableProgressBar("Validating Resources 2 / 2", path, i / (float)sos.Length) == true) {

                                                isBreak = true;
                                                break;

                                            }

                                            {
                                                EditorHelpers.FindType(go, typeof(Resource), (fieldInfo, res) => {

                                                    System.Type resType = null;
                                                    var resTypeAttrs = fieldInfo.GetCustomAttributes(typeof(ResourceTypeAttribute), true);
                                                    if (resTypeAttrs.Length > 0) {
                                                        resType = ((ResourceTypeAttribute)resTypeAttrs[0]).type;
                                                    }

                                                    var r = (Resource)res;
                                                    WindowSystemResourcesResourcePropertyDrawer.Validate(ref r, resType);
                                                    ++markDirtyCount;
                                                    EditorUtility.SetDirty(go);
                                                    return r;

                                                }, visited);
                                                EditorHelpers.FindType(go, typeof(Resource<>), (fieldInfo, res) => {

                                                    var rField =
                                                        (res.GetType().GetField("data", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));
                                                    var r = (Resource)rField.GetValue(res);
                                                    var _fieldInfo = (System.Reflection.FieldInfo)fieldInfo;
                                                    System.Type type = null;
                                                    if (_fieldInfo.FieldType.IsArray == true) {
                                                        type = _fieldInfo.FieldType.GetElementType().GetGenericArguments()[0];
                                                    } else {
                                                        type = _fieldInfo.FieldType.GetGenericArguments()[0];
                                                    }

                                                    WindowSystemResourcesResourcePropertyDrawer.Validate(ref r, type);
                                                    ++markDirtyCount;
                                                    rField.SetValue(res, r);
                                                    EditorUtility.SetDirty(go);
                                                    return res;

                                                }, visitedGeneric);
                                            }

                                            ++i;

                                        }

                                        Debug.Log("Done. Updated: " + markDirtyCount);

                                    }

                                    if (isBreak == true) {
                                        
                                        Debug.Log("Break. Updated: " + markDirtyCount);
                                        
                                    }

                                } catch (System.Exception ex) {
                                    Debug.LogException(ex);
                                }

                                EditorUtility.ClearProgressBar();

                            };

                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                        GUILayout.Label("Find and Validate all Resource objects.", EditorStyles.centeredGreyMiniLabel);
                        
                        GUILayout.Space(10f);
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Validate WindowObjects", GUILayout.Width(200f), GUILayout.Height(30f)) == true) {

                            EditorApplication.delayCall += () => {

                                try {

                                    var isBreak = false;
                                    var markDirtyCount = 0;
                                    var gos = AssetDatabase.FindAssets("t:GameObject");
                                    var i = 0;
                                    foreach (var guid in gos) {

                                        var path = AssetDatabase.GUIDToAssetPath(guid);
                                        var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                                        if (EditorUtility.DisplayCancelableProgressBar("Validating Objects", path, i / (float)gos.Length) == true) {

                                            isBreak = true;
                                            break;

                                        }

                                        {
                                            var allComponents = go.GetComponentsInChildren<WindowObject>(true);
                                            foreach (var component in allComponents) {

                                                component.ValidateEditor();
                                                EditorUtility.SetDirty(component);
                                                EditorUtility.SetDirty(go);
                                                ++markDirtyCount;

                                            }
                                        }

                                        ++i;

                                    }

                                    if (isBreak == true) {
                                        
                                        Debug.Log("Break. Updated: " + markDirtyCount);
                                        
                                    } else {

                                        Debug.Log("Done. Updated: " + markDirtyCount);

                                    }

                                } catch (System.Exception ex) {
                                    Debug.LogException(ex);
                                }

                                EditorUtility.ClearProgressBar();

                            };

                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                        GUILayout.Label("Find and Validate all WindowObject objects.", EditorStyles.centeredGreyMiniLabel);
                        
                        GUILayout.Space(10f);
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Find Direct References", GUILayout.Width(200f), GUILayout.Height(30f)) == true) {

                            try {
                                
                                /*var listAtlases = new List<UnityEngine.U2D.SpriteAtlas>();
                                System.Action<UnityEngine.U2D.SpriteAtlas> action = (atlas) => {
                                    listAtlases.Add(atlas);
                                    Debug.Log("Reg atlas: " + atlas);
                                };*/
                                UnityEditor.U2D.SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget);
                                
                                var listAtlases = new List<AtlasData>();
                                var atlasesGUID = AssetDatabase.FindAssets("t:spriteatlas");
                                foreach (var atlasGUID in atlasesGUID) {

                                    var atlas = AssetDatabase.LoadAssetAtPath<UnityEngine.U2D.SpriteAtlas>(AssetDatabase.GUIDToAssetPath(atlasGUID));
                                    if (atlas != null) {
                                        
                                        var previews = typeof(SpriteAtlasExtensions).GetMethod("GetPreviewTextures", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                                        listAtlases.Add(new AtlasData() {
                                            atlas = atlas,
                                            previews = (Texture2D[])previews.Invoke(null, new [] { atlas }),
                                        });
                                        
                                    }
                                    
                                }
                                
                                this.usedAtlases.Clear();
                                this.usedResources.Clear();
                                for (int i = 0; i < this.registeredPrefabs.arraySize; ++i) {

                                    var element = this.registeredPrefabs.GetArrayElementAtIndex(i);
                                    var window = element.objectReferenceValue as WindowBase;
                                    if (window != null) {

                                        var path = AssetDatabase.GetAssetPath(window);
                                        EditorUtility.DisplayProgressBar("Validating Resources", path, i / (float)this.registeredPrefabs.arraySize);

                                        var dir = System.IO.Path.GetDirectoryName(path);
                                        var componentsDir = dir.Replace("/Screens", "/Components");
                                        var components = AssetDatabase.FindAssets("t:GameObject", new string[] { componentsDir });
                                        foreach (var guid in components) {

                                            var p = AssetDatabase.GUIDToAssetPath(guid);
                                            var componentGo = AssetDatabase.LoadAssetAtPath<GameObject>(p);
                                            var component = componentGo.GetComponent<WindowComponent>();
                                            if (component != null) {

                                                EditorHelpers.FindType(component, new [] { typeof(Sprite), typeof(Texture), typeof(Texture2D) }, (fieldInfo, res) => {

                                                    var r = (Object)res;
                                                    if (r != null) {

                                                        if (res is Sprite sprite) {

                                                            if (sprite != null) {

                                                                foreach (var atlas in listAtlases) {

                                                                    if (atlas.atlas.CanBindTo(sprite) == true) {

                                                                        if (this.usedAtlases.Contains(atlas) == false) this.usedAtlases.Add(atlas);
                                                                        break;

                                                                    }

                                                                }

                                                            }

                                                        }
                                                        
                                                        var usedRes = new UsedResource(window, component, r);
                                                        if (this.usedResources.TryGetValue(window, out var hs) == true) {
                                                            
                                                            if (hs.Contains(usedRes) == false) hs.Add(usedRes);
                                                            
                                                        } else {
                                                            
                                                            hs = new HashSet<UsedResource>();
                                                            hs.Add(usedRes);
                                                            this.usedResources.Add(window, hs);
                                                            
                                                        }
                                                        
                                                    }

                                                    return r;

                                                });
                                                
                                            }

                                        }

                                    }
                                    
                                }
                                
                            } catch (System.Exception ex) {
                                Debug.LogException(ex);
                            }

                            EditorUtility.ClearProgressBar();

                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                        GUILayout.Label("Find all used direct reference resources and group them by screen.", EditorStyles.centeredGreyMiniLabel);

                        if (this.usedResources.Count > 0) {

                            GUILayoutExt.FoldOut(ref this.dependenciesState, "Direct References", () => {

                                GUILayout.Space(10f);
                                GUILayout.Label($"Used atlases ({this.usedAtlases.Count}):");
                                var usedSize = 0;
                                foreach (var atlas in this.usedAtlases) {

                                    GUILayout.BeginHorizontal();
                                    EditorGUILayout.ObjectField(atlas.atlas, typeof(Object), allowSceneObjects: false);
                                    var atlasSize = 0;
                                    foreach (var tex in atlas.previews) {

                                        var size = (int)this.getTextureSizeMethod.Invoke(null, new[] { (Texture)tex });
                                        atlasSize += size;
                                        usedSize += size;

                                    }

                                    var str = EditorUtility.FormatBytes(atlasSize);
                                    GUILayout.Label(str, GUILayout.Width(70f));
                                    GUILayout.EndHorizontal();

                                }

                                GUILayout.BeginHorizontal();
                                GUILayout.FlexibleSpace();
                                GUILayout.Label($"Size: {EditorUtility.FormatBytes(usedSize)}");
                                GUILayout.EndHorizontal();

                                GUILayout.Space(10f);
                                GUILayout.Label("Used resources:");
                                foreach (var kv in this.usedResources) {

                                    GUILayoutExt.DrawHeader(kv.Key.name);
                                    foreach (var res in kv.Value) {

                                        GUILayout.BeginHorizontal();
                                        EditorGUILayout.ObjectField(res.component, typeof(Object), allowSceneObjects: false);
                                        EditorGUILayout.ObjectField(res.resource, typeof(Object), allowSceneObjects: false);
                                        GUILayout.EndHorizontal();

                                    }

                                }

                            });

                        }

                    },
                }
                );
            this.tabScrollPosition = scroll;

            /*
            GUILayout.Space(10f);

            var iter = this.serializedObject.GetIterator();
            iter.NextVisible(true);
            do {

                if (EditorHelpers.IsFieldOfType(typeof(WindowSystem), iter.propertyPath) == true) {

                    EditorGUILayout.PropertyField(iter);

                }

            } while (iter.NextVisible(false) == true);*/

            this.serializedObject.ApplyModifiedProperties();

        }

    }

}

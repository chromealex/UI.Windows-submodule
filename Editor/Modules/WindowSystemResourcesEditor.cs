using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;
    using UnityEngine.UI.Windows.Modules;

    [CustomEditor(typeof(WindowSystemResources))]
    public class WindowSystemResourcesEditor : Editor {

        private int selectedTabIndex;
        private Vector2 tabScrollPosition;

        public void OnEnable() {

            EditorApplication.update += this.Repaint;

        }

        public override void OnInspectorGUI() {

            GUILayoutExt.DrawComponentHeader(this.serializedObject, "EXT", () => {
                
                GUILayout.Label("WindowSystem Resources Internal module", GUILayout.Height(36f));
                
            }, new Color(0.4f, 0.2f, 0.7f, 1f));
            
            var target = this.target as WindowSystemResources;
            var allObjects = target.GetAllObjects();
            var loadedBundles = AssetBundle.GetAllLoadedAssetBundles().ToList();
            var tasks = target.GetTasks();
                                    
            GUILayout.Space(5f);

            this.selectedTabIndex = GUILayoutExt.DrawTabs(
                this.selectedTabIndex,
                ref this.tabScrollPosition,
                new GUITab() {
                    caption = $"Tasks ({tasks.Count})",
                    onDraw = () => {

                        if (tasks.Count == 0) {

                            GUILayout.Space(10f);
                            GUILayout.Label("There are no tasks in loading stage yet.", EditorStyles.centeredGreyMiniLabel);
                            GUILayout.Space(10f);

                        } else {

                            foreach (var task in tasks) {

                                var internalTask = task.Key;
                                GUILayoutExt.DrawHeader($"Resource Id: {internalTask.resourceId}");
                                EditorGUILayout.LabelField($"Pending count: {task.Value?.GetInvocationList().Length}");
                                EditorGUILayout.SelectableLabel(internalTask.resourceSource.ToString());
                                
                            }

                        }

                    },
                },
                new GUITab() {
                    caption = $"Resources ({target.GetAllocatedCount()})",
                    onDraw = () => {
                        
                        if (allObjects.Count == 0) {

                            GUILayout.Space(10f);
                            GUILayout.Label("There are no loaded resources yet.", EditorStyles.centeredGreyMiniLabel);
                            GUILayout.Space(10f);

                        } else {

                            foreach (var item in allObjects) {

                                //EditorGUILayout.ObjectField("Handler", item.Value as Object, typeof(Object), allowSceneObjects: true);
                                GUILayoutExt.DrawHeader($"Handler: {item.Key} (Count: {item.Value.Count})");

                                ++EditorGUI.indentLevel;
                                GUILayoutExt.Box(2f, 2f, () => {

                                    foreach (var resItem in item.Value) {

                                        EditorGUILayout.SelectableLabel(resItem.resourceSource.ToString());
                                        EditorGUILayout.LabelField("Resource ID:", resItem.resourceId.ToString());
                                        if (resItem.handler != null) {

                                            if (resItem.handler is Object handler) {

                                                EditorGUILayout.ObjectField("Handler:", handler, typeof(Object), allowSceneObjects: true);

                                            } else {

                                                EditorGUILayout.LabelField("Handler:", resItem.handler.ToString());

                                            }

                                        }

                                        if (resItem.resource is Object obj) {

                                            EditorGUILayout.ObjectField(obj, typeof(Object), allowSceneObjects: true);

                                        } else {

                                            EditorGUILayout.LabelField(resItem.resource.ToString());

                                        }

                                    }

                                });
                                --EditorGUI.indentLevel;

                            }

                        }

                    },
                }, new GUITab() {
                    caption = $"Bundles ({loadedBundles.Count})",
                    onDraw = () => {

                        if (loadedBundles.Count == 0) {
                            
                            GUILayout.Space(10f);
                            if (UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings.ActivePlayModeDataBuilder is UnityEditor.AddressableAssets.Build
                                    .DataBuilders.BuildScriptVirtualMode ||
                                UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings.ActivePlayModeDataBuilder is UnityEditor.AddressableAssets.Build
                                    .DataBuilders.BuildScriptFastMode) {
                                
                                GUILayout.Label("Seems like there are no bundles loaded yet.\nYou run with VirtualMode now, change mode to use Addressables as in build version.", EditorStyles.centeredGreyMiniLabel);

                            } else {

                                GUILayout.Label("Seems like there are no bundles loaded yet.\nMay be you forgot to turn bundles simulation on.", EditorStyles.centeredGreyMiniLabel);

                            }

                            GUILayout.Space(10f);
                            
                        } else {

                            GUILayoutExt.Box(2f, 2f, () => {

                                const float boxSize = 12f;

                                GUILayout.BeginHorizontal();
                                {
                                    GUILayoutExt.Box(0f, 1f, () => {

                                        var rect = EditorGUILayout.GetControlRect(GUILayout.Width(boxSize), GUILayout.Height(boxSize));
                                        rect.x += 1f;
                                        rect.y += 1f;
                                        EditorGUI.DrawRect(rect, Color.yellow);

                                    }, null, GUILayout.Width(boxSize), GUILayout.Height(boxSize));
                                    GUILayout.Label("Used from bundle because of direct reference dependency", EditorStyles.miniLabel, GUILayout.ExpandWidth(true));
                                }
                                GUILayout.EndHorizontal();

                                GUILayout.Space(4f);

                                GUILayout.BeginHorizontal();
                                {
                                    GUILayoutExt.Box(0f, 1f, () => {

                                        var rect = EditorGUILayout.GetControlRect(GUILayout.Width(boxSize), GUILayout.Height(boxSize));
                                        rect.x += 1f;
                                        rect.y += 1f;
                                        EditorGUI.DrawRect(rect, Color.green);

                                    }, null, GUILayout.Width(boxSize), GUILayout.Height(boxSize));
                                    GUILayout.Label("Used from bundle", EditorStyles.miniLabel, GUILayout.ExpandWidth(true));
                                }
                                GUILayout.EndHorizontal();

                                GUILayout.Space(4f);

                                GUILayout.BeginHorizontal();
                                {
                                    GUILayoutExt.Box(0f, 1f, () => {

                                        var rect = EditorGUILayout.GetControlRect(GUILayout.Width(boxSize), GUILayout.Height(boxSize));
                                        rect.x += 1f;
                                        rect.y += 1f;
                                        EditorGUI.DrawRect(rect, Color.grey);

                                    }, null, GUILayout.Width(boxSize), GUILayout.Height(boxSize));
                                    GUILayout.Label("Don't used", EditorStyles.miniLabel, GUILayout.ExpandWidth(true));
                                }
                                GUILayout.EndHorizontal();

                            });

                            GUILayoutExt.Box(2f, 2f, () => {

                                foreach (var bundle in loadedBundles) {

                                    var state = EditorPrefs.GetBool($"UIWR.FoldOut.Bundles.{bundle.name}", false);
                                    GUILayout.BeginHorizontal();

                                    state = EditorGUILayout.Foldout(state, bundle.name);
                                    if (GUILayout.Button("Unload", EditorStyles.miniButton) == true) {

                                        bundle.Unload(true);
                                        GUILayout.EndHorizontal();
                                        return;

                                    }

                                    GUILayout.EndHorizontal();

                                    EditorPrefs.SetBool($"UIWR.FoldOut.Bundles.{bundle.name}", state);
                                    if (state == true) {

                                        EditorGUILayout.SelectableLabel(bundle.name, EditorStyles.boldLabel, GUILayout.Height(18f));

                                        GUILayoutExt.Box(2f, 8f, () => {

                                            var c1 = new Color(1f, 1f, 1f, 0f);
                                            var c2 = new Color(1f, 1f, 1f, 0.02f);
                                            var prevColor = GUI.color;
                                            var assets = bundle.GetAllAssetNames();
                                            var i = 0;
                                            foreach (var asset in assets) {

                                                GUI.color = Color.white;
                                                if (this.IsLoaded(asset, allObjects, out var hasDirectRef) == true) {
                                                    if (hasDirectRef == true) {
                                                        GUI.color = Color.yellow;
                                                    } else {
                                                        GUI.color = Color.green;
                                                    }
                                                }

                                                var rect = EditorGUILayout.GetControlRect(GUILayout.Height(14f), GUILayout.ExpandWidth(true));
                                                GUILayoutExt.DrawRect(rect, ++i % 2 == 0 ? c1 : c2);
                                                var path = AssetDatabase.GUIDToAssetPath(asset);
                                                if (string.IsNullOrEmpty(path) == false) {

                                                    EditorGUI.SelectableLabel(rect, path, EditorStyles.miniLabel);

                                                } else {

                                                    EditorGUI.SelectableLabel(rect, asset, EditorStyles.miniLabel);

                                                }

                                            }

                                            GUI.color = prevColor;

                                        });

                                    }

                                }

                            });

                        }

                    },
                });

        }

        private bool IsLoaded(string asset, Dictionary<int, HashSet<WindowSystemResources.InternalResourceItem>> allObjects, out bool hasDirectRef) {

            hasDirectRef = false;
            foreach (var obj in allObjects) {

                foreach (var item in obj.Value) {

                    var unityObj = item.resource as Object;
                    if (unityObj == null) continue;

                    hasDirectRef = (item.resourceSource.type == Resource.Type.Direct);
                    
                    var unityGuid = item.resourceSource.guid;
                    var unityPath = AssetDatabase.GUIDToAssetPath(unityGuid);
                    if (unityPath == asset || unityGuid == asset) return true;
                    
                    var path = AssetDatabase.GetAssetPath(unityObj);
                    if (string.IsNullOrEmpty(path) == true) continue;
                    
                    var guid = AssetDatabase.AssetPathToGUID(path);
                    if (guid == asset || path == asset) return true;

                }
                
            }

            return false;

        }

    }

}
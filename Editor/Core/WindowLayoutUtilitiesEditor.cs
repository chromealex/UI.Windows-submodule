using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;
    
    public class WindowLayoutUtilities {

        private struct Item {

            public string name;
            public float value;
            public DeviceInfo.ScreenData screenData;
            public DeviceInfo.OrientationData data;

        }

        private static Dictionary<long, Texture> tempPreviewBuffer = new Dictionary<long, Texture>();
        private static Dictionary<long, Rect> tempPreviewRects = new Dictionary<long, Rect>();
        private static Dictionary<long, double> tempPreviewTimers = new Dictionary<long, double>();

        public static void DrawComponent(Rect rect, WindowComponent component, int customKey) {

            /*
            var key = UnityEngine.UI.Windows.Utilities.UIWSMath.GetKey(component.GetHashCode(), customKey);

            if (Event.current.type == EventType.MouseUp ||
                Event.current.type == EventType.MouseDown ||
                Event.current.type == EventType.MouseMove) {

                if (WindowLayoutUtilities.tempPreviewTimers.ContainsKey(key) == true) {

                    WindowLayoutUtilities.tempPreviewTimers[key] = 0d;

                }

            }
            
            if (Event.current.type == EventType.Repaint) {

                if (WindowLayoutUtilities.tempPreviewTimers.TryGetValue(key, out var timer) == true) {

                    if (EditorApplication.timeSinceStartup - timer > 100d) {
                        
                        if (WindowLayoutUtilities.tempPreviewRects.TryGetValue(key, out var bufferRect) == true) {

                            if (Mathf.Abs(rect.width - bufferRect.width) > 1f ||
                                Mathf.Abs(rect.height - bufferRect.height) > 1f) {

                                WindowLayoutUtilities.tempPreviewRects.Remove(key);
                                if (WindowLayoutUtilities.tempPreviewBuffer.TryGetValue(key, out var t) == true) {

                                    Object.DestroyImmediate(t);

                                }

                                WindowLayoutUtilities.tempPreviewBuffer.Remove(key);
                                WindowLayoutUtilities.tempPreviewTimers.Remove(key);

                            }

                        }

                    } 
                    
                }
                
            }

            if (WindowLayoutUtilities.tempPreviewBuffer.TryGetValue(key, out var buffer) == true) {
                
                EditorGUI.DrawTextureTransparent(rect, buffer, ScaleMode.ScaleToFit);
                
            } else if (Event.current.type == EventType.Repaint) {

                if (Application.isPlaying == true) return;
                
                var w = (int)rect.width;
                var h = (int)rect.height;
                if (w <= 0 || h <= 0) return;
                
                var camera = new GameObject("Camera", typeof(Camera));
                camera.hideFlags = HideFlags.HideAndDontSave;

                var window = component.GetWindow();
                var cameraInstance = camera.GetComponent<Camera>();
                if (window != null) {
                    
                    cameraInstance.CopyFrom(window.workCamera);
                    
                } else {

                    cameraInstance.cullingMask = 1 << component.gameObject.layer;

                }
                var canvas = new GameObject("Canvas", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler));
                canvas.hideFlags = HideFlags.HideAndDontSave;

                var instance = Object.Instantiate(component, canvas.transform);
                instance.ValidateEditor();
                instance.gameObject.hideFlags = HideFlags.HideAndDontSave;
                instance.SetTransformAs(component.transform as RectTransform);

                var canvasInstance = canvas.GetComponent<Canvas>();
                canvasInstance.renderMode = RenderMode.ScreenSpaceCamera;
                canvasInstance.worldCamera = cameraInstance;
                cameraInstance.clearFlags = CameraClearFlags.Depth;
                var canvases = canvas.GetComponentsInChildren<Canvas>();
                foreach (var c in canvases) {

                    c.enabled = false;

                }

                var canvasScaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
                canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(rect.width, 0f);
                canvasScaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScaler.matchWidthOrHeight = 0f;
                
                var type = canvasScaler.GetType();
                var binds = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                type.GetField("m_Canvas", binds).SetValue(canvasScaler, canvasInstance);
                type.GetMethod("Handle", binds).Invoke(canvasScaler, null);
                Canvas.ForceUpdateCanvases();

                RenderTexture rt = null;
                if (w > 0 && h > 0) {

                    rt = new RenderTexture(w, h, 32, RenderTextureFormat.ARGB32);
                    WindowLayoutUtilities.tempPreviewBuffer.Add(key, rt);
                    
                }

                WindowLayoutUtilities.tempPreviewRects.Remove(key);
                WindowLayoutUtilities.tempPreviewRects.Add(key, rect);
                WindowLayoutUtilities.tempPreviewTimers.Remove(key);
                WindowLayoutUtilities.tempPreviewTimers.Add(key, EditorApplication.timeSinceStartup);

                cameraInstance.Render();
                EditorApplication.delayCall += () => {
                    
                    foreach (var c in canvases) {

                        c.enabled = true;

                    }
                    
                    cameraInstance.targetTexture = rt;
                    cameraInstance.Render();
                    cameraInstance.targetTexture = null;

                    foreach (var c in canvases) {

                        c.enabled = false;

                    }

                    //EditorApplication.delayCall += () => {

                        GameObject.DestroyImmediate(camera);
                        GameObject.DestroyImmediate(canvas);

                    //};

                };

            }
            */

        }
        
        [System.Serializable]
        public struct DeviceInfo {

            [System.Serializable]
            public struct ScreenData {
                public int width;
                public int height;
                public int navigationBarHeight;
                public float dpi;
                public OrientationData[] orientations;
            }
            
            [System.Serializable]
            public struct OrientationData {
                public ScreenOrientation orientation;
                public Rect safeArea;
                public Rect[] cutouts;
            }
            
            public string friendlyName;
            public int version;
            public ScreenData[] Screens;
            public ScreenData[] screens;
            
        }

        private static List<DeviceInfo> loadedDevices = new List<DeviceInfo>();
        
        public static void DrawLayout(int selectedIndexAspect, int selectedIndexInner, int selectedType, System.Action<int, int, int> onSet, ref Vector2 tabsScrollPosition, WindowLayout windowLayout, Rect r, UnityEngine.UI.Windows.WindowTypes.LayoutWindowType drawComponents) {

            var offset = 20f;
            var aspect = 4f / 3f;
            DeviceInfo.OrientationData orienData = default;
            DeviceInfo.ScreenData screenData = default;
            if (Selection.objects.Length == 1) {

                if (WindowLayoutUtilities.loadedDevices.Count == 0) {

                    var devices = new List<DeviceInfo>();
                    var deviceDirectoryPath = System.IO.Path.GetFullPath(System.IO.Path.Combine("Packages", "com.unity.device-simulator", ".DeviceDefinitions"));
                    if (UnityEngine.Windows.Directory.Exists(deviceDirectoryPath) == true) {

                        var deviceDirectory = new System.IO.DirectoryInfo(deviceDirectoryPath);
                        var deviceDefinitions = deviceDirectory.GetFiles("*.device.json");
                        foreach (var deviceDefinition in deviceDefinitions) {

                            string deviceFileText;
                            using (System.IO.StreamReader sr = deviceDefinition.OpenText()) {

                                deviceFileText = sr.ReadToEnd();

                            }

                            var deviceInfo = JsonUtility.FromJson<DeviceInfo>(deviceFileText);
                            devices.Add(deviceInfo);

                        }

                    }

                    WindowLayoutUtilities.loadedDevices = devices;

                }

                GUILayout.BeginHorizontal();
                
                var selectedName = "Default Aspects";
                if (selectedType == 1) {

                    var dInfo = WindowLayoutUtilities.loadedDevices[selectedIndexAspect];
                    selectedName = dInfo.friendlyName;

                }

                if (WindowLayoutUtilities.loadedDevices.Count > 0) {

                    if (GUILayout.Button(selectedName, EditorStyles.toolbarDropDown) == true) {

                        var popup = new Popup(title: "Devices", size: new Vector2(200f, 250f));
                        popup.autoClose = true;
                        popup.autoHeight = false;
                        popup.Item("Default Aspects", () => { onSet.Invoke(0, 0, 0); }, order: -1);

                        for (var i = 0; i < WindowLayoutUtilities.loadedDevices.Count; ++i) {

                            var idx = i;
                            var deviceInfo = WindowLayoutUtilities.loadedDevices[i];
                            var screens = deviceInfo.Screens ?? deviceInfo.screens;
                            if (screens != null) {

                                popup.Item(deviceInfo.friendlyName, () => { onSet.Invoke(1, idx, 0); }, order: idx);

                            }

                        }

                        popup.Show();

                    }

                }

                if (selectedType == 0) {
                    
                    var items = new Item[] {
                        new Item() { name = "4:3", value = 4f / 3f },
                        new Item() { name = "16:9", value = 16f / 9f },
                        new Item() { name = "16:10", value = 16f / 10f },
                        new Item() { name = "5:4", value = 5f / 4f },
                        new Item() { name = "2:1", value = 2f / 1f },

                        new Item() { name = "3:4", value = 3f / 4f },
                        new Item() { name = "9:16", value = 9f / 16f },
                        new Item() { name = "10:16", value = 10f / 16f },
                        new Item() { name = "4:5", value = 4f / 5f },
                        new Item() { name = "1:2", value = 1f / 2f },
                    };

                    var tabs = items.Select(x => new GUITab(x.name, null)).ToArray();
                    selectedIndexAspect = GUILayoutExt.DrawTabs(selectedIndexAspect, ref tabsScrollPosition, tabs);
                    aspect = items[selectedIndexAspect].value;
                    
                } else if (selectedType == 1) {

                    var deviceInfo = WindowLayoutUtilities.loadedDevices[selectedIndexAspect];
                    var screens = deviceInfo.Screens ?? deviceInfo.screens;
                    var items = new Item[4];
                    for (int i = 0; i < screens.Length; ++i) {

                        var oris = screens[i].orientations;
                        for (int j = 0; j < oris.Length; ++j) {

                            if (oris[j].orientation == ScreenOrientation.LandscapeRight) {
                                
                                var hData = screens[i];
                                var w = hData.width;
                                hData.width = hData.height;
                                hData.height = w;

                                items[0] = new Item() {
                                    name = "Landscape Right",
                                    value = hData.width / (float)hData.height,
                                    data = oris[j],
                                    screenData = hData
                                };
                                
                            } else if (oris[j].orientation == ScreenOrientation.LandscapeLeft) {
                                
                                var hData = screens[i];
                                var w = hData.width;
                                hData.width = hData.height;
                                hData.height = w;
                                
                                items[1] = new Item() {
                                    name = "Landscape Left",
                                    value = hData.width / (float)hData.height,
                                    data = oris[j],
                                    screenData = hData
                                };
                                
                            } else if (oris[j].orientation == ScreenOrientation.Portrait) {
                                
                                items[2] = new Item() {
                                    name = "Portrait Up",
                                    value = screens[i].width / (float)screens[i].height,
                                    data = oris[j],
                                    screenData = screens[i]
                                };
                                
                            } else if (oris[j].orientation == ScreenOrientation.PortraitUpsideDown) {
                                
                                items[3] = new Item() {
                                    name = "Portrait Down",
                                    value = screens[i].width / (float)screens[i].height,
                                    data = oris[j],
                                    screenData = screens[i]
                                };
                                
                            }

                        }

                    }
                    
                    var tabs = items.Select(x => new GUITab(x.name, null)).ToArray();
                    selectedIndexInner = GUILayoutExt.DrawTabs(selectedIndexInner, ref tabsScrollPosition, tabs);
                    aspect = items[selectedIndexInner].value;
                    orienData = items[selectedIndexInner].data;
                    screenData = items[selectedIndexInner].screenData;

                }
                
                GUILayout.EndHorizontal();

            } else {

                offset = 0f;

            }

            var used = new HashSet<WindowLayout>();
            WindowLayoutUtilities.DrawLayout(aspect, windowLayout, r, offset, used, screenData, orienData, drawComponents);
            
            onSet.Invoke(selectedType, selectedIndexAspect, selectedIndexInner);

        }
        
        public static bool DrawLayout(float aspect, WindowLayout windowLayout, Rect r, float offset = 20f, HashSet<WindowLayout> used = null, DeviceInfo.ScreenData screenData = default, DeviceInfo.OrientationData orientationData = default, UnityEngine.UI.Windows.WindowTypes.LayoutWindowType drawComponents = null) {
            
            if (used == null) used = new HashSet<WindowLayout>();
            if (used.Contains(windowLayout) == true) return false;
            used.Add(windowLayout);

            var rSource = r;
            
            var rectOffset = r;
            if (offset > 0f) {
            
                rectOffset.x += offset;
                rectOffset.y += offset;
                rectOffset.height -= offset * 2f;
                rectOffset.width -= offset * 2f;

                var tWidth = rectOffset.height * aspect;
                if (tWidth > rectOffset.width) {
            
                    rectOffset.y += rectOffset.height * 0.5f;
                    rectOffset.height = rectOffset.width / aspect;
                    rectOffset.y -= rectOffset.height * 0.5f;

                } else {

                    rectOffset.x += rectOffset.width * 0.5f;
                    rectOffset.width = rectOffset.height * aspect;
                    rectOffset.x -= rectOffset.width * 0.5f;

                }

            } else {
            
                GUILayoutExt.DrawRect(rectOffset, new Color(0f, 0f, 0f, 0.4f));

            }
            
            GUILayoutExt.DrawRect(rectOffset, new Color(0f, 0f, 0f, 0.2f));
            GUILayoutExt.DrawBoxNotFilled(rectOffset, 1f, new Color(0.7f, 0.7f, 0.3f, 0.5f));

            GUI.BeginClip(r);

            var resolution = windowLayout.canvasScaler.referenceResolution;
            /*windowLayout.rectTransform.anchoredPosition = new Vector2(r.x, r.y);
            windowLayout.rectTransform.sizeDelta = new Vector2(r.width, r.height);
            windowLayout.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            windowLayout.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            windowLayout.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            windowLayout.rectTransform.localRotation = Quaternion.identity;
            windowLayout.rectTransform.localScale = Vector3.one;*/

            var prevSize = windowLayout.rectTransform.sizeDelta;
            r = rectOffset;
            
            {

                if (r.width > 0f && r.height > 0f) {
                    
                    Vector2 screenSize = new Vector2(r.width, r.height);

                    var sizeDelta = Vector2.zero;
                    float scaleFactor = 0;
                    switch (windowLayout.canvasScaler.screenMatchMode) {
                        case UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight: {
                            const float kLogBase = 2;
                            // We take the log of the relative width and height before taking the average.
                            // Then we transform it back in the original space.
                            // the reason to transform in and out of logarithmic space is to have better behavior.
                            // If one axis has twice resolution and the other has half, it should even out if widthOrHeight value is at 0.5.
                            // In normal space the average would be (0.5 + 2) / 2 = 1.25
                            // In logarithmic space the average is (-1 + 1) / 2 = 0
                            float logWidth = Mathf.Log(screenSize.x / windowLayout.canvasScaler.referenceResolution.x, kLogBase);
                            float logHeight = Mathf.Log(screenSize.y / windowLayout.canvasScaler.referenceResolution.y, kLogBase);
                            float logWeightedAverage = Mathf.Lerp(logWidth, logHeight, windowLayout.canvasScaler.matchWidthOrHeight);
                            scaleFactor = Mathf.Pow(kLogBase, logWeightedAverage);
                            break;
                        }

                        case UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand: {
                            scaleFactor = Mathf.Min(screenSize.x / windowLayout.canvasScaler.referenceResolution.x, screenSize.y / windowLayout.canvasScaler.referenceResolution.y);
                            break;
                        }

                        case UnityEngine.UI.CanvasScaler.ScreenMatchMode.Shrink: {
                            scaleFactor = Mathf.Max(screenSize.x / windowLayout.canvasScaler.referenceResolution.x, screenSize.y / windowLayout.canvasScaler.referenceResolution.y);
                            break;
                        }
                    }

                    if (scaleFactor > 0f) {

                        sizeDelta = new Vector2(screenSize.x / scaleFactor, screenSize.y / scaleFactor);
                        windowLayout.rectTransform.sizeDelta = sizeDelta;
                        windowLayout.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                        windowLayout.rectTransform.localScale = Vector3.one;
                        resolution = sizeDelta; //windowLayout.rectTransform.sizeDelta;

                    }

                }

            }
            
            var labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.alignment = TextAnchor.LowerLeft;

            var isHighlighted = false;
            var highlightedIndex = -1;
            var highlightedRect = Rect.zero;
            for (int i = 0; i < windowLayout.layoutElements.Length; ++i) {

                var element = windowLayout.layoutElements[i];
                if (element == null) {

                    windowLayout.ValidateEditor();
                    windowLayout.rectTransform.sizeDelta = prevSize;
                    return false;

                }
                
                var rect = WindowLayoutUtilities.GetRect(windowLayout.rectTransform, element.rectTransform, r, resolution, offset > 0f);
                if (rect.Contains(Event.current.mousePosition) == true) {
                    
                    if (highlightedIndex >= 0 && highlightedRect.width * highlightedRect.height < rect.width * rect.height) {

                        continue;
                        
                    }
                    
                    highlightedIndex = i;
                    highlightedRect = rect;
                    isHighlighted = true;

                }

            }

            var hasInnerHighlight = false;
            for (int i = 0; i < windowLayout.layoutElements.Length; ++i) {

                var element = windowLayout.layoutElements[i];
                var rect = WindowLayoutUtilities.GetRect(windowLayout.rectTransform, element.rectTransform, r, resolution, offset > 0f);

                using (new GUILayoutExt.GUIColorUsing(highlightedIndex < 0 || i == highlightedIndex ? Color.white : new Color(1f, 1f, 1f, 0.6f))) {

                    if (drawComponents != null) {

                        drawComponents.layouts.GetActive().GetLayoutComponentItemByTagId(element.tagId, windowLayout, out var componentItem);
                        var comp = componentItem.component.GetEditorRef<WindowComponent>();
                        if (comp != null) WindowLayoutUtilities.DrawComponent(rect, comp, componentItem.localTag);
                        
                        WindowSystemSidePropertyDrawer.DrawLayoutMode(rect, element.rectTransform);

                    } else {

                        WindowSystemSidePropertyDrawer.DrawLayoutMode(rect, element.rectTransform);

                    }

                }

                if (element.innerLayout != null) {

                    hasInnerHighlight = WindowLayoutUtilities.DrawLayout(aspect, element.innerLayout, rect, offset: 0f, used: used, drawComponents: drawComponents);
                    //WindowLayoutUtilities.DrawLayoutElements(highlightedIndex, rect, resolution, element.innerLayout, used);

                }

            }

            if (highlightedIndex >= 0 && hasInnerHighlight == false) {
                
                var element = windowLayout.layoutElements[highlightedIndex];
                var rect = highlightedRect;
                
                var padding = 6f;
                var color = new Color(1f, 1f, 0f, 0.5f);
                var content = new GUIContent(element.name);
                GUI.Label(new Rect(padding, 0f, rSource.width, rSource.height - padding), content, labelStyle);
                var labelWidth = labelStyle.CalcSize(content).x + 10f;
                GUILayoutExt.DrawRect(new Rect(padding, rSource.height - 1f - padding, labelWidth, 1f), color);
                var p1 = new Vector3(labelWidth + padding, rSource.height - 1f - padding);
                var p2 = new Vector3(rect.x, rect.y);
                Handles.color = color;
                Handles.DrawLine(p1, p2);
                    
                GUILayoutExt.DrawBoxNotFilled(rect, 1f, new Color(1f, 1f, 1f, 0.2f));

            }
            
            GUI.EndClip();

            if (offset > 0f) {

                if (orientationData.cutouts != null) {

                    var safeArea = new Rect(orientationData.safeArea);
                    safeArea = WindowLayoutUtilities.GetRectYSwapScaled(safeArea, new Vector2(screenData.width, screenData.height), r, rectOffset);
                    GUILayoutExt.DrawBoxNotFilled(safeArea, 1f, Color.magenta);

                    foreach (var rSafe in orientationData.cutouts) {
                        
                        var rSafeRect = WindowLayoutUtilities.GetRectYSwapScaled(rSafe, new Vector2(screenData.width, screenData.height), r, rectOffset);
                        GUI.BeginClip(rSafeRect);

                        if (rSafeRect.width < rSafeRect.height) {

                            for (float step = -rSafeRect.height; step < rSafeRect.height; step += 5f) {

                                var v1 = new Vector3(0f, step);
                                var v2 = new Vector3(rSafeRect.width, step + rSafeRect.width);
                                Handles.color = Color.yellow;
                                Handles.DrawAAPolyLine(2f, v1, v2);

                            }

                        } else {
                            
                            for (float step = -rSafeRect.width; step < rSafeRect.width; step += 5f) {

                                var v1 = new Vector3(step, 0f);
                                var v2 = new Vector3(step + rSafeRect.height, rSafeRect.height);
                                Handles.color = Color.yellow;
                                Handles.DrawAAPolyLine(2f, v1, v2);

                            }

                        }

                        GUI.EndClip();
                        GUILayoutExt.DrawBoxNotFilled(rSafeRect, 1f, Color.yellow);
                        
                    }

                }
                
            }

            windowLayout.rectTransform.sizeDelta = prevSize;

            return isHighlighted;

        }

        private static Rect GetRectYSwapScaled(Rect source, Vector2 sourceSize, Rect fullRect, Rect contentRect) {
            
            source.x /= sourceSize.x;
            source.y /= sourceSize.y;
            source.width /= sourceSize.x;
            source.height /= sourceSize.y;

            source.x *= contentRect.width;
            source.y *= contentRect.height;
            source.width *= contentRect.width;
            source.height *= contentRect.height;

            source.y = fullRect.height - source.y - source.height;

            source.x += contentRect.x;
            source.y += contentRect.y;

            return source;
            
        }

        private static Rect GetRect(RectTransform root, RectTransform child, Rect r, Vector2 resolution, bool withOffset) {
            
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(root, child);
            var rect = Rect.MinMaxRect(bounds.min.x, resolution.y - bounds.max.y, bounds.max.x, resolution.y - bounds.min.y);
            rect = WindowLayoutUtilities.GetRectScaled(rect, resolution, new Vector2(r.width, r.height));
            rect.x += r.width * 0.5f;
            rect.y -= r.height * 0.5f;
            if (withOffset == true) rect.x += r.x;
            if (withOffset == true) rect.y += r.y - 22f;

            return rect;

        }

        private static Rect GetRectScaled(Rect rect, Vector2 sourceResolution, Vector2 targetResolution) {

            var scaleX = targetResolution.x / sourceResolution.x;
            var scaleY = targetResolution.y / sourceResolution.y;
            
            return new Rect(
                rect.x * scaleX,
                rect.y * scaleY,
                rect.width * scaleX,
                rect.height * scaleY);
            
        }

    }

}

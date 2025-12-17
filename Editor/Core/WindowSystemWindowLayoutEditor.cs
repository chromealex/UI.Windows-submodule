using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.UI.Windows {

    using UnityEngine.UI.Windows;
    
    [CustomEditor(typeof(WindowLayout), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class WindowSystemWindowLayoutEditor : Editor {

        private SerializedProperty createPool;
        
        private SerializedProperty animationParameters;
        private SerializedProperty subObjects;
        private SerializedProperty hideBehaviour;
        private SerializedProperty hideBehaviourOneByOneDelay;
        private SerializedProperty showBehaviour;
        private SerializedProperty showBehaviourOneByOneDelay;

        private SerializedProperty renderBehaviourOnHidden;
        
        private SerializedProperty allowRegisterInRoot;
        private SerializedProperty autoRegisterSubObjects;
        private SerializedProperty hiddenByDefault;
        
        private SerializedProperty useSafeZone;
        private SerializedProperty safeZone;
        private SerializedProperty safeZoneRectTransform;

        private int selectedTab {
            get {
                return EditorPrefs.GetInt("UnityEditor.UI.Windows.WindowLayout.TabIndex");
            }
            set {
                EditorPrefs.SetInt("UnityEditor.UI.Windows.WindowLayout.TabIndex", value);
            }
        }

        private Vector2 tabScrollPosition {
            get {
                return new Vector2(
                    EditorPrefs.GetFloat("UnityEditor.UI.Windows.WindowLayout.TabScrollPosition.X"),
                    EditorPrefs.GetFloat("UnityEditor.UI.Windows.WindowLayout.TabScrollPosition.Y")
                );
            }
            set {
                EditorPrefs.SetFloat("UnityEditor.UI.Windows.WindowLayout.TabScrollPosition.X", value.x);
                EditorPrefs.SetFloat("UnityEditor.UI.Windows.WindowLayout.TabScrollPosition.Y", value.y);
            }
        }

        public void OnEnable() {

            try {

                #pragma warning disable
                var _ = this.serializedObject;
                #pragma warning restore

            } catch (System.Exception) {

                return;

            }

            this.createPool = this.serializedObject.FindProperty("createPool");
            
            this.animationParameters = this.serializedObject.FindProperty("animationParameters");
            this.renderBehaviourOnHidden = this.serializedObject.FindProperty("renderBehaviourOnHidden");

            this.subObjects = this.serializedObject.FindProperty("subObjects");
            this.hideBehaviour = this.serializedObject.FindProperty("hideBehaviour");
            this.showBehaviour = this.serializedObject.FindProperty("showBehaviour");
            this.showBehaviourOneByOneDelay = this.serializedObject.FindProperty("showBehaviourOneByOneDelay");
            this.hideBehaviourOneByOneDelay = this.serializedObject.FindProperty("hideBehaviourOneByOneDelay");

            this.allowRegisterInRoot = this.serializedObject.FindProperty("allowRegisterInRoot");
            this.autoRegisterSubObjects = this.serializedObject.FindProperty("autoRegisterSubObjects");
            this.hiddenByDefault = this.serializedObject.FindProperty("hiddenByDefault");
        
            this.useSafeZone = this.serializedObject.FindProperty("useSafeZone");
            this.safeZone = this.serializedObject.FindProperty("safeZone");
            this.safeZoneRectTransform = this.serializedObject.FindProperty("safeZoneRectTransform");
            
            EditorHelpers.SetFirstSibling(this.targets);

            EditorApplication.update += this.Repaint;
            SceneView.duringSceneGui -= this.OnSceneDraw;
            SceneView.duringSceneGui += this.OnSceneDraw;

            this.LoadPrefs();

        }

        public void OnDisable() {
            
            EditorApplication.update -= this.Repaint;
            SceneView.duringSceneGui -= this.OnSceneDraw;
            
        }

        private void OnSceneDraw(SceneView sceneView) {

            WindowLayout targetLayout = null;
            if (Selection.activeObject != null) {
                var go = Selection.activeObject as GameObject;
                var layout = go?.GetComponentInParent<WindowLayout>(true);
                if (layout == null) return;
                targetLayout = layout;
            } else {
                return;
            }
            
            if (tempLines == null) {
                tempLines = new Vector3[8];
            }

            {

                var rectTransform = targetLayout.rectTransform;
                if (rectTransform == null) return;

                var rect = rectTransform.rect;
                if (rect.width <= 0f || rect.height <= 0f) {
                    return;
                }

                var position = rectTransform.transform.position;
                var scale = rectTransform.localScale.x;
                var parentCanvas = rectTransform.parent?.GetComponentInParent<Canvas>(true);
                if (parentCanvas != null) {
                    scale = parentCanvas.transform.localScale.x;
                }
                if (scale <= 0f) return;

                {
                    var handleSize = HandleUtility.GetHandleSize(position);
                    var alpha = 1f - Mathf.Clamp01(handleSize / 800f);
                    var gridSizeX = rect.width * scale;
                    var gridSizeY = rect.height * scale;
                    var startX = (-rect.width * 0.5f) * scale + position.x;
                    var startY = (-rect.height * 0.5f) * scale + position.y;
                    var startZ = 0f;
                    var subColor = new Color(1f, 1f, 1f, 0.01f * alpha);
                    var mainColor = new Color(1f, 1f, 1f, 0.025f * alpha);
                    try {
                        var smallStep = 10f * scale;
                        var largeStep = 5;

                        for (float x = startX, stepX = 0; x <= gridSizeX + startX; x += smallStep, ++stepX) {
                            if (stepX % largeStep == 0) {
                                Handles.color = mainColor;
                            } else {
                                Handles.color = subColor;
                            }

                            Handles.DrawLine(new Vector3(x, startY, startZ), new Vector3(x, startY + gridSizeY, startZ));
                        }

                        for (float y = startY, stepY = 0; y <= gridSizeY + startY; y += smallStep, ++stepY) {
                            if (stepY % largeStep == 0) {
                                Handles.color = mainColor;
                            } else {
                                Handles.color = subColor;
                            }

                            Handles.DrawLine(new Vector3(startX, y, startZ), new Vector3(startX + gridSizeX, y, startZ));
                        }

                    } catch (System.Exception ex) {
                        Debug.LogException(ex);
                    }

                    { // Draw outline
                        const float offset = 0f;
                        tempLines[0] = new Vector3(startX - offset, startY - offset, startZ);
                        tempLines[1] = new Vector3(startX - offset + gridSizeX + offset * 2f, startY - offset, startZ);
                        tempLines[2] = new Vector3(startX - offset + gridSizeX + offset * 2f, startY - offset, startZ);
                        tempLines[3] = new Vector3(startX - offset + gridSizeX + offset * 2f, startY - offset + gridSizeY + offset * 2f, startZ);
                        tempLines[4] = new Vector3(startX - offset + gridSizeX + offset * 2f, startY - offset + gridSizeY + offset * 2f, startZ);
                        tempLines[5] = new Vector3(startX - offset, startY - offset + gridSizeY + offset * 2f, startZ);
                        tempLines[6] = new Vector3(startX - offset, startY - offset + gridSizeY + offset * 2f, startZ);
                        tempLines[7] = new Vector3(startX - offset, startY - offset, startZ);
                        Handles.color = mainColor;
                        Handles.DrawAAPolyLine(1f, tempLines);
                    }
                    { // Draw outline
                        float offset = 10f * scale;
                        tempLines[0] = new Vector3(startX - offset, startY - offset, startZ);
                        tempLines[1] = new Vector3(startX - offset + gridSizeX + offset * 2f, startY - offset, startZ);
                        tempLines[2] = new Vector3(startX - offset + gridSizeX + offset * 2f, startY - offset, startZ);
                        tempLines[3] = new Vector3(startX - offset + gridSizeX + offset * 2f, startY - offset + gridSizeY + offset * 2f, startZ);
                        tempLines[4] = new Vector3(startX - offset + gridSizeX + offset * 2f, startY - offset + gridSizeY + offset * 2f, startZ);
                        tempLines[5] = new Vector3(startX - offset, startY - offset + gridSizeY + offset * 2f, startZ);
                        tempLines[6] = new Vector3(startX - offset, startY - offset + gridSizeY + offset * 2f, startZ);
                        tempLines[7] = new Vector3(startX - offset, startY - offset, startZ);
                        Handles.color = new Color(1f, 1f, 1f, 0.1f);
                        Handles.DrawAAPolyLine(1f, tempLines);
                    }
                    this.DrawLabel(sceneView, targetLayout.name, 30, scale, new Color(1f, 1f, 1f, 0.2f), new Vector3(startX - 10f * scale, startY + (rect.height) * scale + 10f * scale, startZ), TextAnchor.LowerLeft);
                    
                    foreach (var layoutElement in targetLayout.layoutElements) {
                        if (layoutElement == null || layoutElement.isActiveAndEnabled == false || layoutElement.hideInScreen == true) continue;
                        this.DrawElement(sceneView, layoutElement, rectTransform, scale);
                    }
                }
            }

        }

        private static GUIStyle labelStyle;
        private static Vector3[] tempLines;
        private void DrawElement(SceneView sceneView, WindowLayoutElement layoutElement, Transform root, float scale) {

            var rectTransform = layoutElement.rectTransform;
            var rect = rectTransform.rect;
            var position = rectTransform.position;
            var pivot = rectTransform.pivot;
            var anchorMin = rectTransform.anchorMin;
            var anchorMax = rectTransform.anchorMax;
            
            var startX = (-rect.width * pivot.x) * scale + position.x;
            var startY = (-rect.height * pivot.y) * scale + position.y;
            var startZ = 0f;

            var alpha = layoutElement == ((GameObject)Selection.activeObject).GetComponent<WindowLayoutElement>() ? 1f : 0.5f;
            
            var mainColor = new Color(0.08f, 0.6f, 1f, 1f * alpha);
            var textColor = new Color(0.08f, 0.6f, 1f, 0.75f * alpha);
            var anchorColor = new Color(1f, 0.37f, 0.25f, 1f * alpha);
            var stretchColor = new Color(0.14f, 0.6f, 1f, 1f * alpha);
            var anchorPointColor = new Color(1f, 1f, 0f, 1f * alpha);
            var borderColor = new Color(1f, 1f, 1f, 0.5f * alpha);
            var pivotColor = new Color(0.14f, 0.6f, 1f, 1f * alpha);
            
            var gridSizeX = rect.width * scale;
            var gridSizeY = rect.height * scale;

            GUILayoutExt.HandlesDrawBoxNotFilled(new Rect(startX, startY, gridSizeX, gridSizeY), 2f, borderColor);
            if (anchorMin == anchorMax) {
                GUILayoutExt.HandlesDrawDottedLine(new Vector2(startX + gridSizeX * anchorMin.x, startY), new Vector2(startX + gridSizeX * anchorMin.x, startY + gridSizeY), 2f, anchorColor);
                GUILayoutExt.HandlesDrawDottedLine(new Vector2(startX, startY + gridSizeY * anchorMin.y), new Vector2(startX + gridSizeX, startY + gridSizeY * anchorMin.y), 2f, anchorColor);
            } else {
                GUILayoutExt.HandlesDrawLine(new Vector2(startX + gridSizeX * anchorMin.x, startY), new Vector2(startX + gridSizeX * anchorMin.x, startY + gridSizeY), 2f, stretchColor);
                GUILayoutExt.HandlesDrawLine(new Vector2(startX, startY + gridSizeY * anchorMin.y), new Vector2(startX + gridSizeX, startY + gridSizeY * anchorMin.y), 2f, stretchColor);
            }
            GUILayoutExt.HandlesDrawCircle(new Vector2(startX + gridSizeX * pivot.x, startY + gridSizeY * pivot.y), 4f * scale, pivotColor);
            GUILayoutExt.HandlesDrawBox(new Vector2(startX + gridSizeX * anchorMin.x, startY + gridSizeY * anchorMin.y), 4f * scale, anchorPointColor);
            GUILayoutExt.HandlesDrawBox(new Vector2(startX + gridSizeX * anchorMax.x, startY + gridSizeY * anchorMax.y), 4f * scale, anchorPointColor);

            this.DrawLabel(sceneView, layoutElement.name, 40, scale, textColor, new Vector3(startX, startY + (rect.height) * scale, startZ));

        }

        private void DrawLabel(SceneView sceneView, string text, float fontSize, float scale, Color textColor, Vector3 position, TextAnchor alignment = TextAnchor.UpperLeft) {
            
            Handles.BeginGUI();

            var style = labelStyle ?? new GUIStyle(EditorStyles.label) {
                alignment = TextAnchor.UpperLeft,
                richText = true,
                normal = new GUIStyleState() {
                    textColor = textColor,
                },
                hover = new GUIStyleState() {
                    textColor = textColor,
                },
                font = EditorGUIUtility.Load("Fonts/RobotoMono/RobotoMono-Regular.ttf") as Font,
            };
            labelStyle = style;
            style.normal.textColor = textColor;
            style.hover.textColor = textColor;
            style.fontSize = (int)(fontSize * scale);
            style.alignment = alignment;

            var size = style.CalcSize(new GUIContent(text));
            var r = new Rect(0f, 0f, size.x * 2f, size.y);

            var prev = GUI.matrix;
            var guiPos = HandleUtility.WorldToGUIPoint(position);
            var handleSize = HandleUtility.GetHandleSize(position);
            var prevColor = GUI.color;
            GUI.matrix = Matrix4x4.TRS(guiPos, Quaternion.Inverse(sceneView.camera.transform.rotation), Vector3.one / handleSize * scale * 50f);
            GUI.color = textColor;
            GUI.Label(r, text, style);
            GUI.matrix = prev;
            GUI.color = prevColor;

            Handles.EndGUI();

        }

        public override GUIContent GetPreviewTitle() {
            
            return new GUIContent("Layout");
            
        }

        private int selectedIndexAspect = 0;
        private int selectedIndexInner = 0;
        private int selectedType = 0;
        private Vector2 tabsScrollPosition;
        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background) {

            var windowLayout = this.target as WindowLayout;
            WindowLayoutUtilities.DrawLayout(this.selectedIndexAspect, this.selectedIndexInner, this.selectedType, (type, idx, inner) => {
                    
                this.selectedType = type;
                this.selectedIndexAspect = idx;
                this.selectedIndexInner = inner;
                SavePrefs();

            }, ref this.tabsScrollPosition, windowLayout, r, drawComponents: null);
            
        }

        private void SavePrefs() {
            EditorPrefs.SetInt("UIWS.LayoutPreview.selectedIndexAspect", this.selectedIndexAspect);
            EditorPrefs.SetInt("UIWS.LayoutPreview.selectedIndexInner", this.selectedIndexInner);
            EditorPrefs.SetInt("UIWS.LayoutPreview.selectedType", this.selectedType);
        }

        private void LoadPrefs() {
            this.selectedIndexAspect = EditorPrefs.GetInt("UIWS.LayoutPreview.selectedIndexAspect");
            this.selectedIndexInner = EditorPrefs.GetInt("UIWS.LayoutPreview.selectedIndexInner");
            this.selectedType = EditorPrefs.GetInt("UIWS.LayoutPreview.selectedType");
        }

        public override bool HasPreviewGUI() {
            
            return this.targets.Length == 1;
            
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background) {

        }

        public override void OnInspectorGUI() {

            this.serializedObject.Update();
            
            GUILayoutExt.DrawComponentHeader(this.serializedObject, "L", () => {
                
                GUILayoutExt.DrawComponentHeaderItem("State", ((WindowObject)this.target).GetState().ToString());

            }, new Color(1f, 0.6f, 0f, 0.4f));
            
            GUILayout.Space(5f);
            
            var scroll = this.tabScrollPosition;
            this.selectedTab = GUILayoutExt.DrawTabs(
                this.selectedTab,
                ref scroll,
                new GUITab("Basic", () => {

                    GUILayoutExt.DrawHeader("Main");
                    EditorGUILayout.PropertyField(this.hiddenByDefault);
                    EditorGUILayout.PropertyField(this.subObjects);
                    
                    GUILayoutExt.DrawHeader("Animations");
                    EditorGUILayout.PropertyField(this.animationParameters);

                    GUILayoutExt.DrawHeader("Performance Options");
                    EditorGUILayout.PropertyField(this.createPool);
                    
                }),
                new GUITab("Advanced", () => {
                    
                    GUILayoutExt.DrawHeader("Render Behaviour");
                    EditorGUILayout.PropertyField(this.renderBehaviourOnHidden);

                    GUILayoutExt.DrawHeader("Animations");
                    EditorGUILayout.PropertyField(this.animationParameters);
                    GUILayoutExt.DrawHideBehaviour(this.hideBehaviour, this.hideBehaviourOneByOneDelay);
                    GUILayoutExt.DrawShowBehaviour(this.showBehaviour, this.showBehaviourOneByOneDelay);

                    GUILayoutExt.DrawHeader("Graph");
                    EditorGUILayout.PropertyField(this.allowRegisterInRoot);
                    EditorGUILayout.PropertyField(this.autoRegisterSubObjects);
                    EditorGUILayout.PropertyField(this.hiddenByDefault);
                    EditorGUILayout.PropertyField(this.subObjects);

                    GUILayoutExt.DrawHeader("Performance Options");
                    EditorGUILayout.PropertyField(this.createPool);

                }),
                new GUITab("Tools", () => {

                    GUILayoutExt.Box(4f, 4f, () => {
                        
                        if (GUILayout.Button("Collect Images", GUILayout.Height(30f)) == true) {

                            var images = new List<ImageCollectionItem>();
                            this.lastImagesPreview = EditorHelpers.CollectImages(this.target, images);
                            this.lastImages = images;

                        }

                        GUILayoutExt.DrawImages(this.lastImagesPreview, this.lastImages);
                        
                    });
                    
                })
            );
            this.tabScrollPosition = scroll;
            
            GUILayout.Space(10f);

            if (this.targets.Length == 1) GUILayoutExt.DrawSafeAreaFields(this.target, this.useSafeZone, this.safeZone, this.safeZoneRectTransform);
            
            GUILayout.Space(10f);

            GUILayoutExt.DrawFieldsBeneath(this.serializedObject, typeof(WindowLayout));

            this.serializedObject.ApplyModifiedProperties();

        }
        
        private Texture2D lastImagesPreview;
        private List<ImageCollectionItem> lastImages;

    }

}

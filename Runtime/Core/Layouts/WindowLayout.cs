using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UI.Windows {

    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(UnityEngine.UI.CanvasScaler))]
    public class WindowLayout : WindowObject, IHasPreview, ISerializationCallbackReceiver {

        public Canvas canvas;
        public UnityEngine.UI.CanvasScaler canvasScaler;

        public bool isRootLayout = true;
        public WindowLayoutElement[] layoutElements = System.Array.Empty<WindowLayoutElement>();

        public bool useSafeZone;
        public WindowLayoutSafeZone safeZone;
        public RectTransform safeZoneRectTransform;

        private int order;
        private readonly Dictionary<int, WindowComponent> loadedComponents = new Dictionary<int, WindowComponent>();
        public CanvasRebuilderEvents rebuilder;

        public void SetLoadedComponent(int tag, WindowComponent instance) {
            this.loadedComponents.Add(tag, instance);
        }

        public void RemoveLoadedComponent(int tag) {
            this.loadedComponents.Remove(tag);
        }

        public WindowComponent GetLoadedComponent(int tag) {
            return this.loadedComponents.GetValueOrDefault(tag);
        }

        private void OnEnable() {
            
            if (this.rebuilder != null) {
                UnityEngine.UI.CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this.rebuilder);
            }

        }

        private void OnDisable() {
            
            if (this.rebuilder != null) {
                UnityEngine.UI.CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this.rebuilder);
            }

        }

        public override void OnInit() {
            
            base.OnInit();

            if (this.useSafeZone == true && this.isRootLayout == true) {
                
                this.safeZone.Apply();
                
            }
            
        }

        public override void OnDeInit() {
            
            base.OnDeInit();

            this.loadedComponents.Clear();

        }

        public override void OnShowBegin() {
            
            base.OnShowBegin();
            
            this.SetTransformFullRect();
            
        }

        public void SetCanvasOrder(int order) {

            this.order = order;
            this.canvas.sortingOrder = order;
            this.canvas.sortingLayerName = WindowSystem.GetSettings().canvas.sortingLayerName;

        }
        
        public int GetCanvasOrder() {

            return this.order;

        }

        public Canvas GetCanvas() {

            return this.canvas;

        }

        internal override void Setup(WindowBase source) {

            base.Setup(source);

            this.ApplyRenderMode();

            this.canvas.worldCamera = source.workCamera;

            if (this.canvas.isRootCanvas == false) {

                this.canvasScaler.enabled = false;

            }

            this.SetCanvasOrder(this.order);

        }

        internal void ApplyRenderMode() {

            switch (this.window.preferences.renderMode) {

                case UIWSRenderMode.UseSettings:
                    this.ApplyRenderMode(WindowSystem.GetSettings().canvas.renderMode);
                    break;

                case UIWSRenderMode.WorldSpace:
                    this.ApplyRenderMode(RenderMode.WorldSpace);
                    break;

                case UIWSRenderMode.ScreenSpaceCamera:
                    this.ApplyRenderMode(RenderMode.ScreenSpaceCamera);
                    break;

                case UIWSRenderMode.ScreenSpaceOverlay:
                    this.ApplyRenderMode(RenderMode.ScreenSpaceOverlay);
                    break;

            }

        }

        internal void ApplyRenderMode(RenderMode mode) {

            switch (mode) {

                case RenderMode.WorldSpace:
                    this.canvas.renderMode = RenderMode.WorldSpace;
                    this.window.workCamera.enabled = true;
                    break;

                case RenderMode.ScreenSpaceCamera:
                    this.canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    this.window.workCamera.enabled = true;
                    break;

                case RenderMode.ScreenSpaceOverlay:
                    this.canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    this.window.workCamera.enabled = false;
                    break;

            }

        }

        public override void ValidateEditor() {

            base.ValidateEditor();

            this.canvas = this.GetComponent<Canvas>();
            this.canvasScaler = this.GetComponent<UnityEngine.UI.CanvasScaler>();
            this.canvas.renderMode = WindowSystem.GetSettings().canvas.renderMode;

            if (this.rebuilder == null) this.rebuilder = this.gameObject.AddComponent<CanvasRebuilderEvents>();
            if (this.rebuilder != null) this.rebuilder.windowLayout = this;
            
            if (this.ApplyTagsEditor() == true) {
                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this.gameObject);
                #endif
            }
            
        }

        internal bool ApplyTagsEditor() {
            var prevElements = this.layoutElements;
            this.layoutElements = this.GetComponentsInChildren<WindowLayoutElement>(true);
            return this.ApplyTagsEditor(prevElements);
        }

        private bool ApplyTagsEditor(WindowLayoutElement[] prevElements) {

            var changed = false;
            foreach (var element in this.layoutElements) {
                if (element.tagId != 0) {
                    if (Count(this, element.tagId) > 1 && prevElements.Contains(element) == false) {
                        element.tagId = 0;
                        changed = true;
                    }
                }
            }

            var localTagId = 0;
            foreach (var element in this.layoutElements) {
                element.windowId = this.windowId;
                if (element.tagId == 0) {
                    var reqId = ++localTagId;
                    while (HasAnyTag(this, reqId) == true) {
                        ++reqId;
                    }
                    element.tagId = reqId;
                    changed = true;
                } else {
                    localTagId = element.tagId;
                }
            }

            return changed;

            static bool HasAnyTag(WindowLayout layout, int tagId) {
                foreach (var item in layout.layoutElements) {
                    if (item.tagId == tagId) return true;
                }
                return false;
            }

            static int Count(WindowLayout layout, int tagId) {
                var count = 0;
                foreach (var item in layout.layoutElements) {
                    if (item.tagId == tagId) ++count;
                }
                return count;
            }

        }

        public WindowLayoutElement GetLayoutElementByTagId(int tagId) {
            for (int i = 0; i < this.layoutElements.Length; ++i) {
                if (this.layoutElements[i].tagId == tagId) {
                    return this.layoutElements[i];
                }
            }
            return default;
        }

        public bool HasLayoutElementByTagId(int tagId) {
            return this.GetLayoutElementByTagId(tagId) != null;
        }

        public void OnBeforeSerialize() {
            if (this == null) return;
            this.ApplyTagsEditor();
        }
        
        public void OnAfterDeserialize() {
            
        }

    }

}
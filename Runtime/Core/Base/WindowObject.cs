﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UI.Windows {

    using Modules;
    using Utilities;

    public enum ObjectState {

        NotInitialized,
        Initializing,
        Initialized,
        Showing,
        Shown,
        Hiding,
        Hidden,
        DeInitializing,
        DeInitialized,

    }

    public enum RenderBehaviourSettings {

        UseSettings = 0,
        TurnOffRenderers = 1,
        HideGameObject = 2,
        Nothing = 3,

    }

    public enum RenderBehaviour {

        TurnOffRenderers = 1,
        HideGameObject = 2,
        Nothing = 3,

    }

    public interface ILayoutInstance {

        WindowLayout windowLayoutInstance { get; set; }

    }

    [System.Serializable]
    public struct DebugStateLog {

        [System.Serializable]
        public struct Item {

            public ObjectState state;
            public string stackTrace;

        }
        
        public List<Item> items;
        
        public void Add(ObjectState state) {
            
            if (this.items == null) this.items = new List<Item>();
            var trace = StackTraceUtility.ExtractStackTrace();
            this.items.Add(new Item() {
                state = state,
                stackTrace = trace
            });
            
        }

    }
    
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public abstract class WindowObject : MonoBehaviour, IOnPoolGet, IOnPoolAdd, ISearchComponentByTypeSingleEditor {

        [System.Serializable]
        public struct RenderItem {

            public CanvasRenderer canvasRenderer;
            public Graphic graphics;
            public RectMask2D rectMask;

            public void SetCull(bool state) {

                if (this.canvasRenderer != null) {
                    
                    this.canvasRenderer.SetAlpha(state == true ? 0f : 1f);
                    this.canvasRenderer.cull = state;
                    if (this.graphics != null) UnityEngine.UI.CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this.graphics);

                } else if (this.rectMask != null) {

                    this.rectMask.enabled = !state;

                }

            }

            public void SetCullTransparentMesh(bool state) {

                if (this.canvasRenderer != null) this.canvasRenderer.cullTransparentMesh = state;

            }

        }

        [System.Serializable]
        public struct AnimationParametersContainer {

            [SearchComponentsByTypePopupAttribute(typeof(AnimationParameters), menuName: "Animations", singleOnly: true)]
            public AnimationParameters[] items;

        }
        
        IList ISearchComponentByTypeSingleEditor.GetSearchTypeArray() {

            return this.animationParameters.items;

        }

        [SerializeField]
        internal int windowId;
        [SerializeField]
        internal WindowBase window;

        public RectTransform rectTransform;

        public bool hasObjectCanvas;
        public Canvas objectCanvas;
        public int canvasSortingOrderDelta;
        public RenderItem[] canvasRenderers;
        public bool isActiveSelf;
        //public CanvasGroup canvasGroupRender;

        [Tooltip("Should this object return in pool when window is hidden? Object will returns into pool only if parent object is not mark as `createPool`.")]
        public bool createPool;
        
        [AnimationParameters]
        public AnimationParametersContainer animationParameters;
        
        public ObjectState objectState;
        [Tooltip("Render behaviour when hidden state set or if hiddenByDefault is true.")]
        public RenderBehaviourSettings renderBehaviourOnHidden = RenderBehaviourSettings.UseSettings;
        
        [Tooltip("Allow root to register this object in subObjects array.")]
        public bool allowRegisterInRoot = true;
        [Tooltip("Auto register sub objects in this object.\nIf it turned off you need to set up subObjects array manually in inspector or use API.")]
        public bool autoRegisterSubObjects = true;
        [Tooltip("Make this object is hidden by default.\nWorks only on window showing state, check if this object must be hidden by default it breaks branch graph on this node. After it works current object state will be Initialized.")]
        public bool hiddenByDefault = false;

        public DebugStateLog debugStateLog;
        
        public bool isObjectRoot;
        public WindowObject rootObject;
        public List<WindowObject> subObjects = new List<WindowObject>();

        public ComponentAudio audioEvents;
        
        internal bool internalManualShow;
        internal bool internalManualHide;
        
        public void SetState(ObjectState state) {

            var isDebug = WindowSystem.GetSettings().collectDebugInfo;
            if (isDebug == true) this.debugStateLog.Add(state);
            this.objectState = state;

        }
        
        [System.Serializable]
        public struct EditorParametersRegistry {

            [SerializeField]
            private WindowObject holder;
            [SerializeField]
            private WindowComponentModule moduleHolder;
            
            public bool holdHiddenByDefault;
            public bool holdAllowRegisterInRoot;

            public WindowObject GetHolder() => this.holder;

            public string GetHolderName() {
                if (this.moduleHolder != null) return this.moduleHolder.name;
                return this.holder.name;
            }

            public EditorParametersRegistry(WindowObject holder) {

                this.holder = holder;
                this.moduleHolder = null;
                
                this.holdHiddenByDefault = default;
                this.holdAllowRegisterInRoot = default;

            }

            public EditorParametersRegistry(WindowComponentModule holder) {

                this.holder = holder.windowComponent;
                this.moduleHolder = holder;
                
                this.holdHiddenByDefault = default;
                this.holdAllowRegisterInRoot = default;

            }

            public bool IsEquals(EditorParametersRegistry other) {

                return this.holder == other.holder &&
                       this.holdHiddenByDefault == other.holdHiddenByDefault &&
                       this.holdAllowRegisterInRoot == other.holdAllowRegisterInRoot;

            }
            
        }
        
        public List<EditorParametersRegistry> registry = null;
        
        public void AddEditorParametersRegistry(EditorParametersRegistry param) {
            
            if (this.registry == null) this.registry = new List<EditorParametersRegistry>();

            if (this.registry.Any(x => x.IsEquals(param)) == false) {

                this.registry.Add(param);

            }

        }

        private void ValidateRegistry() {
            
            if (this.registry != null && this.registry.Count > 0) {

                var holders = new List<WindowObject>();
                foreach (var reg in this.registry) {

                    if (reg.GetHolder() != null) {
                        
                        holders.Add(reg.GetHolder());
                        
                    }
                    
                }
                
                this.registry.Clear();
                foreach (var holder in holders) {
                    
                    if (holder != this) holder.ValidateEditor();
                    
                }
                
            }
            
        }

        #if UNITY_EDITOR
        private void OnValidate() {

            if (Application.isPlaying == false) {

                if (WindowSystem.HasInstance() == false) return;
                UnityEditor.EditorApplication.delayCall += () => {
                    
                    if (this != null) this.ValidateEditor();
                    
                };

            }

        }

        private void OnTransformChildrenChanged() {
            
            this.OnValidate();
            
        }

        private void OnTransformParentChanged() {
            
            this.OnValidate();
            
        }
        #endif

        void IOnPoolGet.OnPoolGet() {
            
            this.OnPoolGet();
            
            for (int i = 0; i < this.subObjects.Count; ++i) {
                
                ((IOnPoolGet)this.subObjects[i]).OnPoolGet();
                
            }

        }

        void IOnPoolAdd.OnPoolAdd() {
            
            this.OnPoolAdd();

            for (int i = 0; i < this.subObjects.Count; ++i) {
                
                ((IOnPoolAdd)this.subObjects[i]).OnPoolAdd();
                
            }
            
        }
        
        public virtual void OnPoolGet() {}
        public virtual void OnPoolAdd() {}

        public void SetTransformAs(RectTransform rectTransform) {

            if (rectTransform == null) return;
            
            var rect = this.rectTransform;
            rect.localScale = rectTransform.localScale;
            rect.anchorMin = rectTransform.anchorMin;
            rect.anchorMax = rectTransform.anchorMax;
            rect.sizeDelta = rectTransform.sizeDelta;
            rect.anchoredPosition = rectTransform.anchoredPosition;
            
        }
        
        public void SetTransformFullRect() {
            
            var rect = this.rectTransform;
            rect.localScale = Vector3.one;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            
        }

        public virtual void BreakState() {
            
            WindowObjectAnimation.BreakState(this);
            
        }

        public virtual void BreakStateHierarchy() {
            
            this.BreakState();
            for (int i = 0; i < this.subObjects.Count; ++i) {
                
                this.subObjects[i].BreakState();
                
            }
            
        }

        public bool IsVisible() {

            return this.IsVisibleSelf() == true && (this.rootObject != null ? this.rootObject.IsVisible() == true : true);

        }

        public bool IsVisibleSelf() {

            return this.objectState == ObjectState.Showing || this.objectState == ObjectState.Shown;

        }

        public virtual T FindComponent<T>(System.Func<T, bool> filter = null) where T : WindowComponent {

            if (this is T instance) {

                if (filter == null || filter.Invoke(instance) == true) return instance;

            }

            for (int i = 0; i < this.subObjects.Count; ++i) {

                var obj = this.subObjects[i];
                var comp = obj.FindComponent<T>(filter);
                if (comp != null) return comp;

            }
            
            return null;
            
        }

        [ContextMenu("Validate")]
        public virtual void ValidateEditor() {
            
            this.rectTransform = this.GetComponent<RectTransform>();

            this.ValidateEditor(updateParentObjects: false, updateChildObjects: false);
            
        }

        public void ValidateEditor(bool updateParentObjects, bool updateChildObjects = false) {

            this.ValidateRegistry();
            
            this.isObjectRoot = (this.transform.parent == null);
            this.objectCanvas = this.GetComponent<Canvas>();
            this.hasObjectCanvas = (this.objectCanvas != null);

            { // Collect render items

                /*if (this.canvasGroupRender == null) {

                    var canvasGroup = this.gameObject.GetComponent<CanvasGroup>();
                    if (canvasGroup != null) {

                        this.canvasGroupRender = canvasGroup;

                    } else {
                    
                        this.canvasGroupRender = this.gameObject.AddComponent<CanvasGroup>();
                        this.canvasGroupRender.alpha = 1f;
                        this.canvasGroupRender.interactable = true;
                        this.canvasGroupRender.blocksRaycasts = true;

                    }
                    
                }*/
                
                var canvasRenderers = this.GetComponentsInChildren<CanvasRenderer>(true).Where(x => x.GetComponentsInParent<WindowObject>(true)[0] == this).ToArray();
                for (int i = 0; i < canvasRenderers.Length; ++i) {

                    if (canvasRenderers[i].GetComponent<UnityEngine.UI.Graphic>() == null) {

                        Object.DestroyImmediate(canvasRenderers[i], allowDestroyingAssets: true);
                        canvasRenderers[i] = null;

                    }

                }

                canvasRenderers = canvasRenderers.Where(x => x != null).ToArray();
                var rectMasks = this.GetComponentsInChildren<RectMask2D>().Where(x => x.GetComponentsInParent<WindowObject>(true)[0] == this).ToArray();
                this.canvasRenderers = new RenderItem[canvasRenderers.Length + rectMasks.Length];
                var k = 0;
                for (int i = 0; i < this.canvasRenderers.Length; ++i) {

                    if (i < rectMasks.Length) {

                        this.canvasRenderers[i] = new RenderItem() {
                            rectMask = rectMasks[i]
                        };

                    } else {

                        this.canvasRenderers[i] = new RenderItem() {
                            canvasRenderer = canvasRenderers[k],
                            graphics = canvasRenderers[k].GetComponent<UnityEngine.UI.Graphic>()
                        };

                        ++k;

                    }

                }
                
            }

            var roots = this.GetComponentsInParent<WindowObject>(true);
            if (roots.Length >= 2) this.rootObject = roots[1];
            if (this.autoRegisterSubObjects == true) {

                this.subObjects = this.GetComponentsInChildren<WindowObject>(true).Where(x => {

                    if (x.allowRegisterInRoot == false) return false;
                    
                    var comps = x.GetComponentsInParent<WindowObject>(true);
                    if (comps.Length < 2) return false;

                    var c = comps[1];
                    return x != this && c == this;

                }).ToList();

            }

            if (updateChildObjects == true) {

                var childObjects = this.GetComponentsInChildren<WindowObject>(true);
                foreach (var obj in childObjects) {

                    if (obj != this) obj.ValidateEditor(updateParentObjects: false, updateChildObjects: false);

                }

            }

            if (updateParentObjects == true) {

                var topObjects = this.GetComponentsInParent<WindowObject>(true);
                foreach (var obj in topObjects) {

                    if (obj != this) obj.ValidateEditor(updateParentObjects: false, updateChildObjects: false);

                }

            }

        }

        public void PushToPool() {

            if (this.createPool == false) {

                for (int i = this.subObjects.Count - 1; i >= 0; --i) {

                    this.subObjects[i].PushToPool();

                }

            }

            if (this.isObjectRoot == true) {

                if (this.rootObject != null) this.rootObject.RemoveSubObject(this);
                
                if (this.createPool == false) {

                    this.DoDeInit();

                }

                WindowSystem.GetPools().Despawn(this);

            }

        }

        internal void DoLoadScreenAsync(System.Action onComplete) {
            
            Utilities.Coroutines.CallInSequence(() => {
                
                this.OnLoadScreenAsync(onComplete);

            }, this.subObjects, (x, cb) => x.OnLoadScreenAsync(cb));
            
        }
        
        protected virtual void OnLoadScreenAsync(System.Action onComplete) {
            
            if (onComplete != null) onComplete.Invoke();
            
        }
        
        public ObjectState GetState() {

            return this.objectState;

        }

        /// <summary>
        /// Just turn off all canvases
        /// </summary>
        public virtual void TurnOffRender() {

            if (this.hasObjectCanvas == true) {

                this.objectCanvas.enabled = false;

            }

            for (int i = 0; i < this.subObjects.Count; ++i) {

                this.subObjects[i].TurnOffRender();

            }

        }

        /// <summary>
        /// Just turn on all canvases
        /// </summary>
        public virtual void TurnOnRender() {

            if (this.hasObjectCanvas == true) {

                this.objectCanvas.enabled = true;

            }

            for (int i = 0; i < this.subObjects.Count; ++i) {

                this.subObjects[i].TurnOnRender();

            }

        }

        public bool IsActive() {

            return this.isActiveSelf == true && (this.rootObject == null || this.rootObject.IsActive() == true);

        }
        
        public void SetVisibleHierarchy() {

            this.SetVisible();
            
            for (int i = 0; i < this.subObjects.Count; ++i) {

                this.subObjects[i].SetVisible();

            }

        }

        public void SetVisible() {

            this.isActiveSelf = true;

            if (this.IsActive() == true) {

                var renderBehaviourOnHidden = RenderBehaviour.Nothing; 
                if (this.renderBehaviourOnHidden == RenderBehaviourSettings.UseSettings) {

                    var settings = WindowSystem.GetSettings();
                    renderBehaviourOnHidden = settings.components.renderBehaviourOnHidden;

                } else {
                    
                    renderBehaviourOnHidden = (RenderBehaviour)this.renderBehaviourOnHidden;
                    
                }
                
                switch (renderBehaviourOnHidden) {

                    case RenderBehaviour.TurnOffRenderers:

                        for (int i = 0; i < this.canvasRenderers.Length; ++i) {

                            this.canvasRenderers[i].SetCull(false);

                        }

                        break;

                    case RenderBehaviour.HideGameObject:

                        this.gameObject.SetActive(true);

                        break;

                }

            }

        }

        public void SetInvisibleHierarchy() {

            this.SetInvisible();
            
            for (int i = 0; i < this.subObjects.Count; ++i) {

                this.subObjects[i].SetVisible();

            }

        }

        public void SetInvisible() {

            if (this == null || this.gameObject == null) return;
            
            this.isActiveSelf = false;

            var renderBehaviourOnHidden = RenderBehaviour.Nothing; 
            if (this.renderBehaviourOnHidden == RenderBehaviourSettings.UseSettings) {

                var settings = WindowSystem.GetSettings();
                renderBehaviourOnHidden = settings.components.renderBehaviourOnHidden;

            } else {
                    
                renderBehaviourOnHidden = (RenderBehaviour)this.renderBehaviourOnHidden;
                    
            }

            switch (renderBehaviourOnHidden) {

                case RenderBehaviour.TurnOffRenderers:

                    for (int i = 0; i < this.canvasRenderers.Length; ++i) {

                        this.canvasRenderers[i].SetCull(true);

                    }

                    break;

                case RenderBehaviour.HideGameObject:

                    this.gameObject.SetActive(false);

                    break;

            }

        }

        public void SetSortingOrderDelta(int value) {
            
            if (this.hasObjectCanvas == true) {
                
                var windowCanvas = this.window.GetCanvas();
                if (windowCanvas != null) {
                    
                    this.objectCanvas.overrideSorting = true;
                    this.objectCanvas.sortingOrder = windowCanvas.sortingOrder + value;
                    this.objectCanvas.sortingLayerName = windowCanvas.sortingLayerName;
                    this.objectCanvas.sortingLayerID = windowCanvas.sortingLayerID;
                    
                }

            }
            
        }

        public void Setup(WindowComponent component) {

            this.Setup(component.GetWindow());

        }

        internal virtual void Setup(WindowBase source) {

            if (this.hasObjectCanvas == true) {
                
                var windowCanvas = source.GetCanvas();
                if (windowCanvas != null) {
                    
                    this.objectCanvas.overrideSorting = true;
                    this.objectCanvas.sortingOrder = windowCanvas.sortingOrder + this.canvasSortingOrderDelta;
                    this.objectCanvas.sortingLayerName = windowCanvas.sortingLayerName;
                    this.objectCanvas.sortingLayerID = windowCanvas.sortingLayerID;
                    
                }

            }
            
            for (int i = 0; i < this.subObjects.Count; ++i) {
                
                if (this.subObjects[i] == null) {
                    
                    Debug.LogError($"Null subObject encountered on window [{ source.name}], object [{this.name}], index [{i}] (previous subObject is [{(i > 0 ? this.subObjects[i - 1].name : "")}], next subObject is [{(i < this.subObjects.Count - 1 ? this.subObjects[i + 1].name : "")}])");
                    this.subObjects.RemoveAt(i);
                    --i;
                    continue;
                    
                }
                
                this.subObjects[i].Setup(source);
                
            }
            
            for (int i = 0; i < this.canvasRenderers.Length; ++i) {

                this.canvasRenderers[i].SetCullTransparentMesh(true);

            }

            this.windowId = source.windowId;
            this.window = source;

        }

        public bool RegisterSubObject(WindowObject windowObject) {

            windowObject.Setup(this.window);
            
            if (this.subObjects.Contains(windowObject) == false) {

                this.subObjects.Add(windowObject);
                windowObject.rootObject = this;

                {

                    switch (this.objectState) {
                        
                        case ObjectState.Initializing:
                        case ObjectState.Initialized:
                            if (windowObject.GetState() == ObjectState.NotInitialized) {
							    
                                windowObject.DoInit();
                                
                            }
                            break;
                        
                        case ObjectState.Showing:
                            if (windowObject.GetState() == ObjectState.NotInitialized) {
							    
                                windowObject.DoInit();
                                
                            }

                            if (windowObject.hiddenByDefault == false) {

                                WindowSystem.ShowInstance(windowObject, TransitionParameters.Default.ReplaceImmediately(true), internalCall: true);

                            }
                            break;
                        case ObjectState.Shown:
                            if (windowObject.GetState() == ObjectState.NotInitialized) {
							    
                                windowObject.DoInit();
                                
                            }

                            if (windowObject.hiddenByDefault == false) {

                                WindowSystem.ShowInstance(
                                    windowObject,
                                    TransitionParameters.Default.ReplaceImmediately(true).ReplaceCallback(() => {
                                        
                                        WindowSystem.SetShown(windowObject, TransitionParameters.Default.ReplaceImmediately(true));
                                        
                                    }), internalCall: true);

                            }

                            break;
                        
                        case ObjectState.Hiding:
                            
                            if (windowObject.GetState() == ObjectState.NotInitialized) {

                                windowObject.DoInit();

                            }

                            windowObject.SetState(this.objectState);

                            break;
                        case ObjectState.Hidden:
                            
                            if (windowObject.GetState() == ObjectState.NotInitialized) {

                                windowObject.DoInit();

                            }

                            windowObject.SetState(this.objectState);

                            break;
                        
                        case ObjectState.DeInitializing:
                        case ObjectState.DeInitialized:
                            if (windowObject.GetState() == ObjectState.NotInitialized) {
                                
                                windowObject.DoInit();
                                
                            }
                            
                            windowObject.DoDeInit();
                            break;
                        
                    }
                    
                }
                return true;

            }

            return false;

        }

        public bool RemoveSubObject(WindowObject windowObject) {

            if (this.subObjects.Remove(windowObject) == true) {

                windowObject.rootObject = null;
                return true;

            }

            return false;

        }
        
        public bool UnRegisterSubObject(WindowObject windowObject) {

            if (this.RemoveSubObject(windowObject) == true) {

                switch (windowObject.objectState) {

                    case ObjectState.Initializing:
                    case ObjectState.Initialized:
                        windowObject.DoDeInit();
                        break;
			
                    case ObjectState.Showing:
                    case ObjectState.Shown:

                        // after OnShowEnd
                        windowObject.Hide(TransitionParameters.Default.ReplaceImmediately(true));
                        windowObject.DoDeInit();
                        break;

                    case ObjectState.Hiding:

                        // after OnHideBegin
                        WindowSystem.SetHidden(windowObject, TransitionParameters.Default.ReplaceImmediately(true));
                        windowObject.DoDeInit();
                        break;

                    case ObjectState.Hidden:

                        // after OnHideEnd
                        windowObject.DoDeInit();
                        break;

                }
                return true;

            }

            return false;

        }

        public WindowBase GetWindow() {

            #if UNITY_EDITOR
            if (Application.isPlaying == false && this.window == null) {

                return this.GetComponentInParent<WindowBase>();

            }
            #endif

            return this.window;

        }

        public T GetWindow<T>() where T : WindowBase {

            return (T)this.GetWindow();

        }

        public void SetResetStateHierarchy() {

            this.SetResetState();

            for (int i = 0; i < this.subObjects.Count; ++i) {

                this.subObjects[i].SetResetState();

            }

        }

        public void SetResetState() {

            WindowObjectAnimation.SetResetState(this);

        }

        protected internal virtual void SendEvent<T>(T data) {
            
            this.OnEvent(data);

            for (int i = 0; i < this.subObjects.Count; ++i) {

                this.subObjects[i].SendEvent(data);

            }
            
        }

        public virtual void OnEvent<T>(T data) {
            
        }

        public void DoSendFocusLost() {

            this.OnFocusLost();
            WindowSystem.RaiseEvent(this, WindowEvent.OnFocusLost);

            for (int i = 0; i < this.subObjects.Count; ++i) {

                this.subObjects[i].DoSendFocusLost();

            }

        }

        public void DoSendFocusTook() {

            this.OnFocusTook();
            WindowSystem.RaiseEvent(this, WindowEvent.OnFocusTook);

            for (int i = 0; i < this.subObjects.Count; ++i) {

                this.subObjects[i].DoSendFocusTook();

            }

        }

        public void DoInit() {

            if (this.objectState < ObjectState.Initializing) {

                this.audioEvents.Initialize(this);
                
                this.SetState(ObjectState.Initializing);

                this.OnInitInternal();
                this.OnInit();
                WindowSystem.RaiseEvent(this, WindowEvent.OnInitialize);

                this.SetState(ObjectState.Initialized);

            }

            for (int i = 0; i < this.subObjects.Count; ++i) {

                this.subObjects[i].DoInit();

            }

        }

        public void DoDeInit() {

            if (this.objectState >= ObjectState.DeInitializing) {
                
                Debug.LogWarning("Object is out of state: " + this, this);
                return;
                
            }
            this.SetState(ObjectState.DeInitializing);

            for (int i = 0; i < this.subObjects.Count; ++i) {

                this.subObjects[i].DoDeInit();

            }

            WindowSystem.RaiseEvent(this, WindowEvent.OnDeInitialize);
            this.OnDeInit();
            this.OnDeInitInternal();
            
            this.SetState(ObjectState.DeInitialized);

            this.audioEvents.DeInitialize(this);

            WindowSystem.ClearEvents(this);

        }

        public void ShowHide(bool state, TransitionParameters parameters = default) {

            if (state == true) {
                
                this.Show(parameters);
                
            } else {
                
                this.Hide(parameters);
                
            }
            
        }

        internal void ShowInternal(TransitionParameters parameters = default) {
            
            if (this.hiddenByDefault == true || this.internalManualShow == true) {

                if (this.internalManualShow == false) {

                    this.Hide(TransitionParameters.Default.ReplaceImmediately(true));
                    this.SetInvisible();
                    
                }

                parameters.RaiseCallback();
                return;

            }
            
            if (this.objectState <= ObjectState.Initializing) {
                
                Debug.LogWarning("Object is out of state: " + this, this);
                return;
                
            }

            var cObj = this;
            var cParams = parameters;
            var cbParameters = parameters.ReplaceCallbackWithContext(WindowSystem.SetShown, cObj, cParams);
            WindowSystem.ShowInstance(this, cbParameters, internalCall: true);

        }
        
        public void Show(TransitionParameters parameters = default) {

            if (this.objectState <= ObjectState.Initializing) {

                if (this.objectState == ObjectState.NotInitialized) {
                    
                    this.DoInit();
                    
                } else {

                    Debug.LogWarning("Object is out of state: " + this, this);
                    return;

                }

            }

            this.internalManualShow = true;

            if (this.objectState == ObjectState.Showing || this.objectState == ObjectState.Shown) {
                
                parameters.RaiseCallback();
                return;
                
            }

            var cObj = this;
            var cParams = parameters;
            var cbParameters = parameters.ReplaceCallbackWithContext(WindowSystem.SetShown, cObj, cParams);
            WindowSystem.ShowInstance(this, cbParameters, internalCall: true);

        }

        public virtual void Hide(TransitionParameters parameters = default) {

            if (this.objectState <= ObjectState.Initializing) {
                
                if (this.objectState == ObjectState.NotInitialized) {
                    
                    this.DoInit();
                    
                } else {

                    Debug.LogWarning("Object is out of state: " + this, this);
                    return;

                }
                
            }

            this.internalManualHide = true;

            if (this.objectState == ObjectState.Hiding || this.objectState == ObjectState.Hidden) {
                
                parameters.RaiseCallback();
                return;
                
            }
            
            var cObj = this;
            var cParams = parameters;
            var cbParameters = parameters.ReplaceCallbackWithContext(WindowSystem.SetHidden, cObj, cParams);
            WindowSystem.HideInstance(this, cbParameters);

        }

        public void HideCurrentWindow() {
            
            this.window.Hide();
            
        }
        
        public void LoadAsync<T>(Resource resource, System.Action<T> onComplete = null) where T : WindowObject {
            
            Coroutines.Run(this.LoadAsync_YIELD(resource, onComplete));
            
        }

        private struct LoadAsyncClosure<T> {

            public WindowObject component;
            public System.Action<T> onComplete;

        }

        private IEnumerator LoadAsync_YIELD<T>(Resource resource, System.Action<T> onComplete = null) where T : WindowObject {
            
            var resources = WindowSystem.GetResources();
            var data = new LoadAsyncClosure<T>() {
                component = this,
                onComplete = onComplete,
            };
            yield return resources.LoadAsync<T, LoadAsyncClosure<T>>(this.GetWindow(), data, resource, (asset, closure) => {

                if (asset != null) {

                    var instance = closure.component.Load(asset);
                    if (closure.onComplete != null) closure.onComplete.Invoke(instance);

                } else {
                    
                    if (closure.onComplete != null) closure.onComplete.Invoke(null);
                    
                }

            });

        }

        public T Load<T>(T prefab) where T : WindowObject {
            
            if (prefab.createPool == true) WindowSystem.GetPools().CreatePool(prefab);
            var instance = WindowSystem.GetPools().Spawn(prefab, this.transform);
            instance.Setup(this.GetWindow());
            instance.SetInvisible();
            this.RegisterSubObject(instance);

            return instance;

        }

        internal virtual void OnInitInternal() { }

        internal virtual void OnDeInitInternal() { }

        internal virtual void OnShowBeginInternal() { }

        internal virtual void OnShowEndInternal() { }

        internal virtual void OnHideBeginInternal() { }

        internal virtual void OnHideEndInternal() { }

        #region Public Override Events
        public virtual void OnInit() { }

        public virtual void OnDeInit() { }

        public virtual void OnShowBegin() { }

        public virtual void OnShowEnd() { }

        public virtual void OnHideBegin() { }

        public virtual void OnHideEnd() { }

        public virtual void OnFocusTook() { }

        public virtual void OnFocusLost() { }
        #endregion

    }

}
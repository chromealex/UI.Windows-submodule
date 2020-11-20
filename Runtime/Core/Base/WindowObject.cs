using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    public abstract class WindowObject : MonoBehaviour, ISearchComponentByTypeSingleEditor {

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

        internal bool internalManualShow;
        internal bool internalManualHide;
        
        public void SetState(ObjectState state) {

            var isDebug = WindowSystem.GetSettings().collectDebugInfo;
            if (isDebug == true) this.debugStateLog.Add(state);
            this.objectState = state;

        }
        
        [System.Serializable]
        public struct EditorParametersRegistry {

            public WindowObject holder;
            
            public bool hiddenByDefault;
            public string hiddenByDefaultDescription;

            public bool allowRegisterInRoot;
            public string allowRegisterInRootDescription;

            public bool IsEquals(EditorParametersRegistry other) {

                return this.holder == other.holder &&
                       this.hiddenByDefault == other.hiddenByDefault &&
                       this.allowRegisterInRoot == other.allowRegisterInRoot;

            }
            
        }
        
        public List<EditorParametersRegistry> registry = null;
        
        public void AddEditorParametersRegistry(EditorParametersRegistry param) {
            
            if (this.registry == null) this.registry = new List<EditorParametersRegistry>();

            if (this.registry.Any(x => x.IsEquals(param)) == false) {

                this.registry.Add(param);

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

        public bool IsVisible() {

            return this.IsVisibleSelf() == true && (this.rootObject != null ? this.rootObject.IsVisible() == true : true);

        }

        public bool IsVisibleSelf() {

            return this.objectState == ObjectState.Showing || this.objectState == ObjectState.Shown;

        }

        [ContextMenu("Validate")]
        public virtual void ValidateEditor() {
            
            this.rectTransform = this.GetComponent<RectTransform>();

            this.ValidateEditor(updateParentObjects: true);
            
        }

        public void ValidateEditor(bool updateParentObjects) {

            if (updateParentObjects == true) {

                var topObjects = this.GetComponentsInParent<WindowObject>(true);
                foreach (var obj in topObjects) {

                    if (obj != this) obj.ValidateEditor();

                }

            }

            if (this.registry != null) {

                var holders = new List<WindowObject>();
                foreach (var reg in this.registry) {

                    if (reg.holder != null) {
                        
                        holders.Add(reg.holder);
                        
                    }
                    
                }
                this.registry.Clear();
                foreach (var holder in holders) {
                    
                    if (holder != this) holder.ValidateEditor(updateParentObjects: false);
                    
                }
                
            }
            
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
        public void TurnOffRender() {

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
        public void TurnOnRender() {

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

        internal virtual void Setup(WindowBase source) {

            for (int i = 0; i < this.subObjects.Count; ++i) {
                
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
            
            if (this.hiddenByDefault == true /*&& this.window.GetState() == ObjectState.Showing*/) {

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

            var cbParameters = parameters.ReplaceCallback(() => {
                
                WindowSystem.SetShown(this, parameters);

            });
            WindowSystem.ShowInstance(this, cbParameters, internalCall: true);

        }
        
        public void Show(TransitionParameters parameters = default) {

            if (this.objectState <= ObjectState.Initializing) {
                
                Debug.LogWarning("Object is out of state: " + this, this);
                return;
                
            }

            this.internalManualShow = true;

            var cbParameters = parameters.ReplaceCallback(() => {
                
                WindowSystem.SetShown(this, parameters);

            });
            WindowSystem.ShowInstance(this, cbParameters, internalCall: true);

        }

        public virtual void Hide(TransitionParameters parameters = default) {

            this.internalManualHide = true;

            var cbParameters = parameters.ReplaceCallback(() => {
                
                WindowSystem.SetHidden(this, parameters);

            });
            WindowSystem.HideInstance(this, cbParameters);

        }

        public void HideCurrentWindow() {
            
            this.window.Hide();
            
        }
        
        public void LoadAsync<T>(Resource resource, System.Action<T> onComplete = null) where T : WindowObject {
            
            Coroutines.Run(this.LoadAsync_YIELD(resource, onComplete));
            
        }

        private IEnumerator LoadAsync_YIELD<T>(Resource resource, System.Action<T> onComplete = null) where T : WindowObject {
            
            var resources = WindowSystem.GetResources();
            yield return resources.LoadAsync<T>(this.GetWindow(), resource, (asset) => {

                if (asset != null) {

                    var instance = this.Load(asset);
                    if (onComplete != null) onComplete.Invoke(instance);

                } else {
                    
                    if (onComplete != null) onComplete.Invoke(null);
                    
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnityEngine.UI.Windows {

    using Modules;
    using Utilities;

    public enum ObjectState {

        NotInitialized,
        Initializing,
        Loading,
        Loaded,
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
    public struct EditorRefLocks {

        public string[] directories;
        
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

    public interface IHolder {

        void ValidateEditor();

    }

    public interface ILoadable {

        void Load(System.Action onComplete);

    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public abstract class WindowObject : MonoBehaviour, IOnPoolGet, IOnPoolAdd, ISearchComponentByTypeSingleEditor, IHolder {

        [System.Serializable]
        public struct RenderItem : System.IEquatable<RenderItem> {

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

            public bool Equals(RenderItem other) {
                return Equals(this.canvasRenderer, other.canvasRenderer) && Equals(this.graphics, other.graphics) && Equals(this.rectMask, other.rectMask);
            }

            public override bool Equals(object obj) {
                return obj is RenderItem other && Equals(other);
            }

            public override int GetHashCode() {
                unchecked {
                    var hashCode = (this.canvasRenderer != null ? this.canvasRenderer.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (this.graphics != null ? this.graphics.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (this.rectMask != null ? this.rectMask.GetHashCode() : 0);
                    return hashCode;
                }
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
        public RenderItem[] canvasRenderers = System.Array.Empty<RenderItem>();
        
        //public CanvasGroup canvasGroupRender;

        [Tooltip("Should this object return in pool when window is hidden? Object will returns into pool only if parent object is not mark as `createPool`.")]
        public bool createPool;
        
        [AnimationParameters]
        public AnimationParametersContainer animationParameters;
        
        [Tooltip("Render behaviour when hidden state set or if hiddenByDefault is true.")]
        public RenderBehaviourSettings renderBehaviourOnHidden = RenderBehaviourSettings.UseSettings;
        
        [Tooltip("Allow root to register this object in subObjects array.")]
        public bool allowRegisterInRoot = true;
        [Tooltip("Auto register sub objects in this object.\nIf it turned off you need to set up subObjects array manually in inspector or use API.")]
        public bool autoRegisterSubObjects = true;
        [Tooltip("Make this object is hidden by default.\nWorks only on window showing state, check if this object must be hidden by default it breaks branch graph on this node. After it works current object state will be Initialized.")]
        public bool hiddenByDefault = false;

        public EditorRefLocks editorRefLocks;
        public DebugStateLog debugStateLog;
        
        public bool isObjectRoot;
        public WindowObject rootObject;
        public List<WindowObject> subObjects = new List<WindowObject>();

        public ComponentAudio audioEvents;
        
        internal bool internalManualShow;
        internal bool internalManualHide;
        private bool readyToHide = true;

        private bool isActiveSelf;
        private ObjectState objectState;
        
        public bool IsReadyToHide() {

            return this.readyToHide;

        }

        public void SetReadyToHide(bool state) {
            
            this.readyToHide = state;
            
        }

        public void SetState(ObjectState state) {

            var isDebug = WindowSystem.GetSettings().collectDebugInfo;
            if (isDebug == true) this.debugStateLog.Add(state);
            this.objectState = state;
            
            if (state == ObjectState.Initializing) this.SetResetState();

        }
        
        [System.Serializable]
        public struct EditorParametersRegistry : System.IEquatable<EditorParametersRegistry> {

            [SerializeField]
            private WindowObject holder;
            [SerializeField]
            private WindowComponentModule moduleHolder;
            
            public bool holdHiddenByDefault;
            public bool holdAllowRegisterInRoot;

            public IHolder GetHolder() {

                if (this.holder == null) return this.moduleHolder;
                return this.holder;
                
            }

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

            public bool Equals(EditorParametersRegistry other) {
                return Equals(this.holder, other.holder) && Equals(this.moduleHolder, other.moduleHolder) && this.holdHiddenByDefault == other.holdHiddenByDefault && this.holdAllowRegisterInRoot == other.holdAllowRegisterInRoot;
            }

            public override bool Equals(object obj) {
                return obj is EditorParametersRegistry other && Equals(other);
            }

            public override int GetHashCode() {
                unchecked {
                    var hashCode = (this.holder != null ? this.holder.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (this.moduleHolder != null ? this.moduleHolder.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ this.holdHiddenByDefault.GetHashCode();
                    hashCode = (hashCode * 397) ^ this.holdAllowRegisterInRoot.GetHashCode();
                    return hashCode;
                }
            }

        }
        
        public List<EditorParametersRegistry> registry = null;
        
        public void AddEditorParametersRegistry(EditorParametersRegistry param) {

            if (this.registry == null) this.registry = new List<EditorParametersRegistry>();

            if (this.registry.Any(x => x.IsEquals(param)) == false) {

                this.registry.Add(param);

            }

        }

        private void ValidateRegistry(DirtyHelper dirtyHelper) {
            
            if (this.registry != null && this.registry.Count > 0) {

                var holders = new List<IHolder>();
                foreach (var reg in this.registry) {

                    if (reg.GetHolder() != null) {
                        
                        holders.Add(reg.GetHolder());
                        
                    }
                    
                }
                
                var prevRegistry = this.registry.ToList();
                this.registry.Clear();
                foreach (var holder in holders) {
                    
                    if ((MonoBehaviour)holder != this) holder.ValidateEditor();
                    
                }

                var newRegistry = this.registry;
                this.registry = prevRegistry;
                dirtyHelper.Set(ref this.registry, newRegistry);

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
                
                if (this.CheckSubObject(this.subObjects, ref i) == false) continue;
                ((IOnPoolGet)this.subObjects[i]).OnPoolGet();
                
            }

        }

        void IOnPoolAdd.OnPoolAdd() {
            
            this.OnPoolAdd();
            
            this.internalManualHide = false;
            this.internalManualShow = false;
               
            for (int i = 0; i < this.subObjects.Count; ++i) {
                
                if (this.CheckSubObject(this.subObjects, ref i) == false) continue;
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
                
                if (this.CheckSubObject(this.subObjects, ref i) == false) continue;
                this.subObjects[i].BreakState();
                
            }
            
        }

        public bool IsVisible() {

            return this.IsVisibleSelf() == true && (this.rootObject != null ? this.rootObject.IsVisible() == true : true);

        }

        public bool IsVisibleSelf() {

            return this.objectState == ObjectState.Showing || this.objectState == ObjectState.Shown;

        }

        public virtual T FindComponent<T>(System.Func<T, bool> filter = null) where T : WindowObject {

            return this.FindComponent<T, System.Func<T, bool>>(filter, (fn, c) => {
                if (fn != null) return fn.Invoke(c);
                return c;
            });

        }

        public virtual T FindComponent<T, TState>(TState state, System.Func<TState, T, bool> filter = null) where T : WindowObject {

            if (this is T instance) {

                if (filter == null || filter.Invoke(state, instance) == true) return instance;

            }

            for (int i = 0; i < this.subObjects.Count; ++i) {

                if (this.CheckSubObject(this.subObjects, ref i) == false) continue;
                var obj = this.subObjects[i];
                var comp = obj.FindComponent(state, filter);
                if (comp != null) return comp;

            }
            
            return null;
            
        }

        public virtual T FindComponentParent<T>(System.Func<T, bool> filter = null) where T : WindowObject {

            return this.FindComponentParent<T, System.Func<T, bool>>(filter, (fn, c) => {
                if (fn != null) return fn.Invoke(c);
                return c;
            });

        }

        public virtual T FindComponentParent<T, TState>(TState state, System.Func<TState, T, bool> filter = null) where T : WindowObject {

            if (this is T instance) {

                if (filter == null || filter.Invoke(state, instance) == true) return instance;

            }

            if (this.rootObject != null) {
                
                var comp = this.rootObject.FindComponentParent(state, filter);
                if (comp != null) return comp;
                
            }
            
            return null;
            
        }

        [ContextMenu("Validate")]
        public virtual void ValidateEditor() {

            #if UNITY_EDITOR
            var path = UnityEditor.AssetDatabase.GetAssetPath(this.gameObject);
            if (path.Contains("Packages/") == true) return;
            #endif

            this.rectTransform = this.GetComponent<RectTransform>();

            this.ValidateEditor(updateParentObjects: false, updateChildObjects: false);

        }

        public void ValidateEditor(bool updateParentObjects, bool updateChildObjects = false) {
            
            var helper = new DirtyHelper(this);
            this.ValidateEditor(helper, updateParentObjects: false, updateChildObjects: false);
            helper.Apply();
            
        }

        public void ValidateEditor(DirtyHelper dirtyHelper, bool updateParentObjects, bool updateChildObjects = false) {

            this.ValidateRegistry(dirtyHelper);

            dirtyHelper.SetEnum(ref this.objectState, ObjectState.NotInitialized);
            var up = (this.transform.parent != null ? this.transform.parent.GetComponentsInParent<WindowObject>(true) : null);
            dirtyHelper.Set(ref this.isObjectRoot, up == null || up.Length == 0);
            dirtyHelper.SetObj(ref this.objectCanvas, this.GetComponent<Canvas>());
            dirtyHelper.Set(ref this.hasObjectCanvas, (this.objectCanvas != null));

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
                var newCanvasRenderers = new RenderItem[canvasRenderers.Length + rectMasks.Length];
                var k = 0;
                for (int i = 0; i < newCanvasRenderers.Length; ++i) {

                    if (i < rectMasks.Length) {

                        newCanvasRenderers[i] = new RenderItem() {
                            rectMask = rectMasks[i],
                        };

                    } else {

                        newCanvasRenderers[i] = new RenderItem() {
                            canvasRenderer = canvasRenderers[k],
                            graphics = canvasRenderers[k].GetComponent<UnityEngine.UI.Graphic>(),
                        };

                        ++k;

                    }

                }
                
                for (int i = 0; i < this.canvasRenderers.Length; ++i) {

                    if (this.canvasRenderers[i].canvasRenderer == null) {
      
                        this.canvasRenderers[i].canvasRenderer = null;
      
                    }
                    
                }
                
                dirtyHelper.Set(ref this.canvasRenderers, newCanvasRenderers);

            }

            var roots = this.GetComponentsInParent<WindowObject>(true);
            if (roots.Length >= 2) dirtyHelper.SetObj(ref this.rootObject, roots[1]);
            if (this.autoRegisterSubObjects == true) {

                var newSubObjects = this.GetComponentsInChildren<WindowObject>(true).Where(x => {

                    if (x.allowRegisterInRoot == false) return false;
                    
                    var comps = x.GetComponentsInParent<WindowObject>(true);
                    if (comps.Length < 2) return false;

                    var c = comps[1];
                    return x != this && c == this;

                }).ToList();
                dirtyHelper.SetObj(ref this.subObjects, newSubObjects);

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

            #if UNITY_EDITOR
            if (dirtyHelper.isDirty == true) UnityEditor.EditorUtility.SetDirty(this.gameObject);
            #endif

        }

        public void PushToPool() {

            if (this.createPool == false) {

                for (int i = this.subObjects.Count - 1; i >= 0; --i) {

                    if (this.CheckSubObject(this.subObjects, ref i) == false) continue;
                    this.subObjects[i].PushToPool();

                }

            }

            if (this.isObjectRoot == true) {

                if (this.rootObject != null) this.rootObject.RemoveSubObject(this);
                
                this.window = null;
                this.rootObject = null;
                WindowSystem.GetPools().Despawn(this, (obj) => {
                    
                    obj.DoDeInit();
                    
                });

            } else {
                
                this.window = null;
                this.rootObject = null;
                
            }

        }

        internal void DoLoadScreenAsync(InitialParameters initialParameters, System.Action onComplete) {
            
            Utilities.Coroutines.CallInSequence((closure) => {
                
                this.OnLoadScreenAsync(closure, onComplete);

            }, initialParameters, this.subObjects, (x, cb, closure) => x.OnLoadScreenAsync(closure, cb));
            
        }
        
        protected virtual void OnLoadScreenAsync(InitialParameters initialParameters, System.Action onComplete) {
            
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

                if (this.CheckSubObject(this.subObjects, ref i) == false) continue;
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

                if (this.CheckSubObject(this.subObjects, ref i) == false) continue;
                this.subObjects[i].TurnOnRender();

            }

        }

        public bool IsActive() {

            return this.isActiveSelf == true && (this.rootObject == null || this.rootObject.IsActive() == true);

        }
        
        public void SetVisibleHierarchy() {

            this.SetVisible();
            
            for (int i = 0; i < this.subObjects.Count; ++i) {

                if (this.CheckSubObject(this.subObjects, ref i) == false) continue;
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

                if (this.CheckSubObject(this.subObjects, ref i) == false) continue;
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

            this.windowId = source.windowId;
            this.window = source;

            if (this.hasObjectCanvas == true) {
                
                this.SetSortingOrderDelta(this.canvasSortingOrderDelta);
                
            }
            
            for (int i = 0; i < this.subObjects.Count; ++i) {

                if (this.CheckSubObject(this.subObjects, ref i) == false) continue;
                this.subObjects[i].Setup(source);
                
            }
            
            for (int i = 0; i < this.canvasRenderers.Length; ++i) {

                this.canvasRenderers[i].SetCullTransparentMesh(true);

            }

        }

        private bool CheckSubObject(List<WindowObject> subObjects, ref int index) {
            
            if (subObjects[index] == null) {

                if (this == null) return false;
                Debug.LogError($"Null subObject encountered on window [{(this.window == null ? "Null" : this.window.name)}], object [{this.name}], index [{index}] (previous subObject is [{(index > 0 ? subObjects[index - 1].name : "Empty")}], next subObject is [{(index < subObjects.Count - 1 ? subObjects[index + 1].name : "Empty")}])");
                this.subObjects.RemoveAt(index);
                --index;
                return false;
                    
            }
            
            return true;

        }

        public bool RegisterSubObject(WindowObject windowObject) {

            windowObject.Setup(this.window);
            
            if (this.subObjects.Contains(windowObject) == false) {

                this.subObjects.Add(windowObject);
                windowObject.rootObject = this;
                
                if (windowObject.GetState() == ObjectState.NotInitialized) {
							    
                    windowObject.DoInit(() => this.AdjustObjectState(windowObject));
                                
                } else {
                    
                    this.AdjustObjectState(windowObject);
                    
                }

                return true;

            }

            return false;

        }

        private void AdjustObjectState(WindowObject windowObject) {
            
            switch (this.objectState) {
                        
                case ObjectState.Initializing:
                case ObjectState.Initialized:

                    if (windowObject.GetState() != ObjectState.Initialized) {
                        
                        Debug.LogError($"WindowObject must be initialized before AdjustObjectState");
                        
                    }

                    break;
                        
                case ObjectState.Showing:

                    if (windowObject.hiddenByDefault == false) {

                        WindowSystem.ShowInstance(windowObject, TransitionParameters.Default.ReplaceImmediately(true), internalCall: true);

                    }
                    break;
                case ObjectState.Shown:

                    if (windowObject.hiddenByDefault == false) {

                        WindowSystem.ShowInstance(
                            windowObject,
                            TransitionParameters.Default.ReplaceImmediately(true).ReplaceCallback(() => {
                                        
                                WindowSystem.SetShown(windowObject, TransitionParameters.Default.ReplaceImmediately(true), true);
                                        
                            }), internalCall: true);

                    }

                    break;
                        
                case ObjectState.Hiding:

                    windowObject.SetState(this.objectState);

                    break;
                case ObjectState.Hidden:

                    windowObject.SetState(this.objectState);

                    break;
                        
                case ObjectState.DeInitializing:
                case ObjectState.DeInitialized:

                    windowObject.DoDeInit();
                    break;
                        
            }
            
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
                        WindowSystem.SetHidden(windowObject, TransitionParameters.Default.ReplaceImmediately(true), true);
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

                if (this.CheckSubObject(this.subObjects, ref i) == false) continue;
                this.subObjects[i].SendEvent(data);

            }
            
        }

        public virtual void OnEvent<T>(T data) {
            
        }

        public void DoSendFocusLost() {

            this.OnFocusLost();
            WindowSystem.RaiseEvent(this, WindowEvent.OnFocusLost);

            for (int i = 0; i < this.subObjects.Count; ++i) {

                if (this.CheckSubObject(this.subObjects, ref i) == false) continue;
                this.subObjects[i].DoSendFocusLost();

            }

        }

        public void DoSendFocusTook() {

            this.OnFocusTook();
            WindowSystem.RaiseEvent(this, WindowEvent.OnFocusTook);

            for (int i = 0; i < this.subObjects.Count; ++i) {

                if (this.CheckSubObject(this.subObjects, ref i) == false) continue;
                this.subObjects[i].DoSendFocusTook();

            }

        }
        
        public void DoInit(System.Action callback = null) {

            Coroutines.Run(this.DoInitAsync(callback));

        }

        private IEnumerator DoInitAsync(System.Action callback = null) {
            
            if (this.objectState < ObjectState.Initializing) {

                this.SetState(ObjectState.Initializing);

                if (this is ILoadable loadable) {

                    this.SetState(ObjectState.Loading);

                    var loaded = false;
                    loadable.Load(() => loaded = true);
                    yield return new WaitUntil(() => loaded);

                }

                this.SetState(ObjectState.Loaded);

                this.audioEvents.Initialize(this);

                this.OnInitInternal();
                this.OnInit();
                WindowSystem.RaiseEvent(this, WindowEvent.OnInitialize);

                this.SetState(ObjectState.Initialized);

            }

            var coroutines = PoolList<IEnumerator>.Spawn();

            for (int i = 0; i < this.subObjects.Count; ++i) {

                if (this.CheckSubObject(this.subObjects, ref i) == false) continue;
                coroutines.Add(this.subObjects[i].DoInitAsync());

            }
            
            var moveNext = true;
            while (moveNext == true) {

                moveNext = false;
                foreach (var coroutine in coroutines) {
                    moveNext = moveNext == true || coroutine.MoveNext() == true;
                }
                
            }
            
            PoolList<IEnumerator>.Recycle(coroutines);
            
            callback?.Invoke();

        }

        public void DoDeInit() {

            if (this.objectState >= ObjectState.DeInitializing) {
                
                Debug.LogWarning("Object is out of state: " + this, this);
                return;
                
            }
            this.SetState(ObjectState.DeInitializing);

            for (int i = 0; i < this.subObjects.Count; ++i) {

                if (this.CheckSubObject(this.subObjects, ref i) == false) continue;
                this.subObjects[i].DoDeInit();

            }

            var resources = WindowSystem.GetResources();
            resources.DeleteAll(this);

            WindowSystem.RaiseEvent(this, WindowEvent.OnDeInitialize);
            this.OnDeInit();
            this.OnDeInitInternal();

            this.audioEvents.DeInitialize(this);
            
            this.SetState(ObjectState.DeInitialized);

            WindowSystem.ClearEvents(this);
            
            this.SetState(ObjectState.NotInitialized);

        }
        
        private bool IsInternalManualTouch(TransitionParameters parameters) {

            if (parameters.data.replaceIgnoreTouch == true) {

                if (parameters.data.ignoreTouch == true) return false;

            }
            
            return this.internalManualHide == true || this.internalManualShow == true;

        }

        internal void ShowInternal(TransitionParameters parameters = default) {
            
            if (this.hiddenByDefault == true || this.IsInternalManualTouch(parameters) == true) {

                if (this.internalManualShow == false) {

                    this.HideInternal(TransitionParameters.Default.ReplaceImmediately(true));
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
            var cbParameters = parameters.ReplaceCallbackWithContext(WindowSystem.SetShown, cObj, cParams, true);
            WindowSystem.ShowInstance(this, cbParameters, internalCall: true);

        }

        internal void HideInternal(TransitionParameters parameters = default) {
            
            if (this.IsInternalManualTouch(parameters) == true) {

                parameters.RaiseCallback();
                return;

            }
            
            var cObj = this;
            var cParams = parameters;
            var cbParameters = parameters.ReplaceCallbackWithContext(WindowSystem.SetHidden, cObj, cParams, true);
            WindowSystem.HideInstance(this, cbParameters, internalCall: true);

        }

        public void ShowHide(bool state) {

            this.ShowHide(state, default);
            
        }

        public void ShowHide(bool state, TransitionParameters parameters) {

            if (state == true) {
                
                this.Show(parameters);
                
            } else {
                
                this.Hide(parameters);
                
            }
            
        }

        public void Show() {
            
            this.Show(default);
            
        }

        public virtual void Show(TransitionParameters parameters) {

            if (this.objectState <= ObjectState.Initializing) {

                if (this.objectState == ObjectState.NotInitialized) {

                    var copy = parameters;
                    this.DoInit(() => this.Show(copy));
                    return;

                } else {

                    Debug.LogWarning("Object is out of state: " + this, this);
                    return;

                }

            }

            if (this.objectState == ObjectState.Showing || this.objectState == ObjectState.Shown) {
                
                parameters.RaiseCallback();
                return;
                
            }

            this.internalManualShow = true;

            var cObj = this;
            var cParams = parameters;
            var cbParameters = parameters.ReplaceCallbackWithContext(static (obj, tr, internalCall) => WindowSystem.SetShown(obj, tr, internalCall), cObj, cParams, false);
            WindowSystem.ShowInstance(this, cbParameters, internalCall: true);

        }

        public void Hide() {
            
            this.Hide(default);
            
        }

        public virtual void Hide(TransitionParameters parameters) {

            if (this.objectState <= ObjectState.Initializing) {
                
                if (this.objectState == ObjectState.NotInitialized) {

                    var copy = parameters;
                    this.DoInit(() => this.Hide(copy));
                    return;
                    
                } else {

                    Debug.LogWarning("Object is out of state: " + this, this);
                    return;

                }
                
            }

            if (this.objectState == ObjectState.Hiding || this.objectState == ObjectState.Hidden) {
                
                parameters.RaiseCallback();
                return;
                
            }
            
            this.internalManualHide = true;

            var cObj = this;
            var cParams = parameters;
            var cbParameters = parameters.ReplaceCallbackWithContext(static (obj, tr, internalCall) => WindowSystem.SetHidden(obj, tr, internalCall), cObj, cParams, false);
            WindowSystem.HideInstance(this, cbParameters, internalCall: true);

        }

        public void HideCurrentWindow() {
            
            this.window.Hide();
            
        }
        
        public void LoadAsync<T>(Resource resource, System.Action<T> onComplete = null, bool async = true) where T : WindowObject {
            
            Coroutines.Run(this.LoadAsync_YIELD(resource, onComplete, async));
            
        }

        private struct LoadAsyncClosure<T> {

            public WindowObject component;
            public System.Action<T> onComplete;

        }

        private IEnumerator LoadAsync_YIELD<T>(Resource resource, System.Action<T> onComplete = null, bool async = true) where T : WindowObject {
            
            var resources = WindowSystem.GetResources();
            var data = new LoadAsyncClosure<T>() {
                component = this,
                onComplete = onComplete,
            };
            yield return resources.LoadAsync<T, LoadAsyncClosure<T>>(new WindowSystemResources.LoadParameters() { async = async },this.GetWindow(), data, resource, (asset, closure) => {

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
        
        public void UnloadSubObjects()  {
            
            if (this.subObjects.Count == 0) return;
            
            var pools = WindowSystem.GetPools();

            for (int i = this.subObjects.Count - 1; i >= 0; i--) {

                var subObject = this.subObjects[i];
                
                this.UnRegisterSubObject(subObject);
                pools.Despawn(subObject);
                
            }
            

        }

        public void DoLayoutReady() {

            this.OnLayoutReady();
            WindowSystem.RaiseEvent(this, WindowEvent.OnLayoutReady);

            for (int i = 0; i < this.subObjects.Count; ++i) {

                if (this.CheckSubObject(this.subObjects, ref i) == false) continue;
                this.subObjects[i].DoLayoutReady();

            }

        }

        internal virtual void OnInitInternal() { }

        internal virtual void OnDeInitInternal() { }

        internal virtual void OnShowBeginInternal() { }

        internal virtual void OnShowEndInternal() { }

        internal virtual void OnHideBeginInternal() { }

        internal virtual void OnHideEndInternal() { }

        #region Public Override Events
        public virtual void OnLayoutReady() { }
        
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
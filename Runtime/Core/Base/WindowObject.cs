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

    public interface IHolder {

        void ValidateEditor();

    }

    public interface ILoadable {

        void Load<TState>(TState state, System.Action<TState> onComplete);

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

        public bool hasObjectCanvas => this.objectCanvas != null;
        public Canvas objectCanvas;
        public int canvasSortingOrderDelta;
        
        public RenderItem[] canvasRenderers = System.Array.Empty<RenderItem>();

        public AnimationParametersContainer animationParameters;

        [Tooltip("Simultaneously means that current animation will be played together with the childs.\nWaitForChild means that child animations played first, then played current one.\nOneByOne means that children animation will be played one-by-one.")]
        public HideBehaviour hideBehaviour;
        [Tooltip("Hide behaviour custom delay. If 0 - default animation will be used.")]
        public float hideBehaviourOneByOneDelay;
        [Tooltip("Simultaneously means that current animation will be played together with the childs.\nOneByOne means that children played one-by-one.")]
        public ShowBehaviour showBehaviour;
        [Tooltip("Show behaviour custom delay. If 0 - default animation will be used.")]
        public float showBehaviourOneByOneDelay;
        
        [Tooltip("Render behaviour when hidden state set or if hiddenByDefault is true.")]
        public RenderBehaviourSettings renderBehaviourOnHidden = RenderBehaviourSettings.UseSettings;
        
        [Tooltip("Allow root to register this object in subObjects array.")]
        public bool allowRegisterInRoot = true;
        [Tooltip("Auto register sub objects in this object.\nIf it turned off you need to set up subObjects array manually in inspector or use API.")]
        public bool autoRegisterSubObjects = true;
        [Tooltip("Make this object is hidden by default.\nWorks only on window showing state, check if this object must be hidden by default it breaks branch graph on this node. After it works current object state will be Initialized.")]
        public bool hiddenByDefault = false;

        [Tooltip("Should this object return in pool when window is hidden? Object will returns into pool only if parent object is not mark as `createPool`.")]
        public bool createPool;

        public bool isObjectRoot;
        public WindowObject rootObject;
        public WindowObject prefabSource;
        public List<WindowObject> subObjects = new List<WindowObject>();

        public ComponentAudio audioEvents;
        
        internal bool internalManualShow;
        internal bool internalManualHide;
        private bool readyToHide = true;

        private bool isActiveSelf;
        private ObjectState objectState;
        
        public bool IsReadyToHide() => this.readyToHide;

        public void SetReadyToHide(bool state) => this.readyToHide = state;

        public void SetState(ObjectState state) {

            UnityEngine.UI.Windows.Editor.ComponentsDebugLog.Add(this, state);
            this.objectState = state;
            
            if (state == ObjectState.Initializing) this.SetResetState();

        }
        
        public void AddEditorParametersRegistry(EditorParametersRegistry param) {
            UnityEngine.UI.Windows.Editor.WindowObjectRegistry.Add(this, param);
        }

        private void ValidateRegistry(DirtyHelper dirtyHelper) {
            ref var registry = ref UnityEngine.UI.Windows.Editor.WindowObjectRegistry.GetRegistry(this);
            if (registry != null && registry.Count > 0) {

                var holders = new List<IHolder>();
                foreach (var reg in registry) {

                    if (reg.GetHolder() != null) {
                        
                        holders.Add(reg.GetHolder());
                        
                    }
                    
                }
                
                var prevRegistry = registry.ToList();
                registry.Clear();
                foreach (var holder in holders) {
                    
                    if ((MonoBehaviour)holder != this) holder.ValidateEditor();
                    
                }

                var newRegistry = registry;
                registry = prevRegistry;
                dirtyHelper.Set(ref registry, newRegistry);

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
        
        /// <summary>
        /// Check object state is Showing or Shown in hierarchy
        /// </summary>
        /// <returns>TRUE - if object's state is Showing or Shown in hierarchy, otherwise FALSE</returns>
        public bool IsVisible() {

            var obj = this;
            while (obj != null) {
                if (obj.IsVisibleSelf() == false) return false;
                obj = obj.rootObject;
            }
            return true;

        }

        /// <summary>
        /// Check object state is Showing or Shown
        /// </summary>
        /// <returns>TRUE - if object's state is Showing or Shown, otherwise FALSE</returns>
        public bool IsVisibleSelf() {

            return this.objectState == ObjectState.Showing || this.objectState == ObjectState.Shown;

        }

        public virtual T FindComponent<T>(System.Func<T, bool> filter = null) where T : class {

            return this.FindComponent<T, System.Func<T, bool>>(filter, static (fn, c) => {
                if (fn != null) return fn.Invoke(c);
                return c != null;
            });

        }

        public virtual T FindComponent<T, TState>(TState state, System.Func<TState, T, bool> filter = null) where T : class {

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

        public virtual T FindComponentParent<T>(System.Func<T, bool> filter = null) where T : class {

            return this.FindComponentParent<T, System.Func<T, bool>>(filter, static (fn, c) => {
                if (fn != null) return fn.Invoke(c);
                return c != null;
            });

        }

        public virtual T FindComponentParent<T, TState>(TState state, System.Func<TState, T, bool> filter = null) where T : class {

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
        public void CallValidate() => this.ValidateEditor();
        
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
            this.ValidateEditor(helper, updateParentObjects, updateChildObjects);
            helper.Apply();
            
        }

        public void ValidateEditor(DirtyHelper dirtyHelper, bool updateParentObjects, bool updateChildObjects = false) {

            this.ValidateRegistry(dirtyHelper);

            dirtyHelper.SetEnum(ref this.objectState, ObjectState.NotInitialized);
            var up = (this.transform.parent != null ? this.transform.parent.GetComponentsInParent<WindowObject>(true) : null);
            dirtyHelper.Set(ref this.isObjectRoot, up == null || up.Length == 0);
            dirtyHelper.SetObj(ref this.objectCanvas, this.GetComponent<Canvas>());

            { // Collect render items

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
                WindowSystem.GetPools().Despawn(this, static (obj) => {
                    obj.DoDeInit();
                });

            } else {
                this.window = null;
                this.rootObject = null;
            }

        }

        internal void DoLoadScreenAsync<TState>(TState state, InitialParameters initialParameters, System.Action<TState> onComplete) {

            var closure = PoolClass<DoLoadScreenClosure<TState>>.Spawn();
            closure.component = this;
            closure.initialParameters = initialParameters;
            closure.onComplete = onComplete;
            closure.state = state;
            
            Utilities.Coroutines.CallInSequence(ref closure, static (ref DoLoadScreenClosure<TState> closure) => {
                closure.component.OnLoadScreenAsync(closure.state, closure.initialParameters, closure.onComplete);
                PoolClass<DoLoadScreenClosure<TState>>.Recycle(closure);
            }, this.subObjects, static (WindowObject x, Coroutines.ClosureDelegateCallback<DoLoadScreenClosure<TState>> cb, ref DoLoadScreenClosure<TState> closure) => {
                x.OnLoadScreenAsync(new DoLoadScreenClosureInternal<DoLoadScreenClosure<TState>>() {
                    callback = cb,
                    data = closure,
                }, closure.initialParameters, static cbInt => {
                    cbInt.callback.Invoke(ref cbInt.data);
                });
            });
            
        }

        protected virtual void OnLoadScreenAsync<TState>(TState state, InitialParameters initialParameters, System.Action<TState> onComplete) {
            if (onComplete != null) onComplete.Invoke(state);
        }

        public ObjectState GetState() => this.objectState;

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

        /// <summary>
        /// Similar to IsVisible, but check real object visibility (similar to gameObject.isActive)
        /// </summary>
        /// <returns>TRUE - if all objects are visible in hierarchy, otherwise FALSE</returns>
        public bool IsActive() {

            var obj = this;
            while (obj != null) {
                if (obj.IsActiveSelf() == false) return false;
                obj = obj.rootObject;
            }
            return true;

        }

        /// <summary>
        /// Similar to IsVisibleSelf, but check real object visibility (similar to gameObject.isActiveSelf)
        /// </summary>
        /// <returns>TRUE - if object is visible, otherwise FALSE</returns>
        public bool IsActiveSelf() {
            return this.isActiveSelf;
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

            if (windowObject == null) return false;
            
            windowObject.Setup(this.window);
            
            if (this.subObjects.Contains(windowObject) == false) {

                this.subObjects.Add(windowObject);
                windowObject.rootObject = this;
                
                if (windowObject.GetState() == ObjectState.NotInitialized) {
                    
                    windowObject.DoInit(new DoInitClosure() {
                        component = this,
                        windowObject = windowObject,
                    }, static (c) => c.component.AdjustObjectState(c.windowObject));
                    
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

                    if (windowObject.GetState() < ObjectState.Initialized) {
                        
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

                        var closure = PoolClass<WindowObjectClosure>.Spawn();
                        closure.instance = windowObject;
                        WindowSystem.ShowInstance(
                            windowObject,
                            TransitionParameters.Default.ReplaceImmediately(true).ReplaceCallback(closure, static (obj) => {
                                var closure = (WindowObjectClosure)obj;
                                WindowSystem.SetShown(closure.instance, TransitionParameters.Default.ReplaceImmediately(true), true);
                                PoolClass<WindowObjectClosure>.Recycle(closure);
                            }));

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

        public T TryGetWindow<T>() where T : class {
            if (this.IsWindow<T>() == false) return null;
            return (T)(object)this.GetWindow();
        }

        public T GetWindow<T>() where T : class {
            return (T)(object)this.GetWindow();
        }

        public bool IsWindow<T>() where T : class {
            return (this.GetWindow() as T) != null;
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

        protected internal virtual void SendEvent<T>(T data) where T : class {
            
            this.OnEvent(data);

            for (int i = 0; i < this.subObjects.Count; ++i) {
                if (this.CheckSubObject(this.subObjects, ref i) == false) continue;
                this.subObjects[i].SendEvent(data);
            }
            
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
        
        public void DoInit<TState>(TState state, System.Action<TState> callback = null) {
            this.DoInitAsync(state, callback);
        }

        public void DoInit(System.Action callback = null) {
            this.DoInitAsync(callback, static (cb) => cb?.Invoke());
        }

        private void SetLoaded() {
            
            this.SetState(ObjectState.Loaded);

            this.audioEvents.Initialize(this);

            this.OnInitInternal();
            this.OnInit();
            WindowSystem.RaiseEvent(this, WindowEvent.OnInitialize);

            this.SetState(ObjectState.Initialized);

        }
        
        private void DoInitAsync<TState>(TState state, System.Action<TState> callback = null) {
            
            if (this.objectState < ObjectState.Initializing) {

                this.SetState(ObjectState.Initializing);

                if (this is ILoadable loadable) {

                    this.SetState(ObjectState.Loading);

                    var instance = PoolClass<InitLoader<TState>>.Spawn();
                    instance.instance = this;
                    instance.state = state;
                    instance.callback = callback;
                    loadable.Load(instance, static (c) => c.loaded = true);
                    if (instance.loaded == false) {
                        Coroutines.Wait(instance, static (loader) => loader.loaded, static (loader) => {
                            loader.instance.SetLoaded();
                            loader.instance.DoInitAsync(loader.state, loader.callback);
                            PoolClass<InitLoader<TState>>.Recycle(loader);
                        });
                        return;
                    }

                }

                this.SetLoaded();

            }

            if (this.subObjects.Count > 0) {

                var initialized = PoolClass<WaitForInitialized<TState>>.Spawn();
                initialized.state = state;
                initialized.callback = callback;
                initialized.count = 0;
                for (int i = 0; i < this.subObjects.Count; ++i) {
                    if (this.CheckSubObject(this.subObjects, ref i) == false) continue;
                    ++initialized.count;
                    this.subObjects[i].DoInitAsync(initialized, static (loader) => { --loader.count; });
                }

                Coroutines.Wait(initialized, static (loader) => loader.count == 0, static (loader) => {
                    loader.callback?.Invoke(loader.state);
                    PoolClass<WaitForInitialized<TState>>.Recycle(loader);
                });

            } else {
                
                callback?.Invoke(state);
                
            }

        }
        
        public void DoDeInit() {

            if (this.objectState >= ObjectState.DeInitializing) {
                
                Debug.LogWarning($"Object is out of state: {this}", this);
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
            
            if (parameters.data.replaceShowBehaviour == false) {
                parameters = parameters.ReplaceShowBehaviour(this.showBehaviour);
            }

            if (this.hiddenByDefault == true || this.IsInternalManualTouch(parameters) == true) {
                if (this.internalManualShow == false) {
                    this.HideInternal(TransitionParameters.Default.ReplaceImmediately(true));
                    this.SetInvisible();
                }
                parameters.RaiseCallback();
                return;

            }
            
            if (this.objectState <= ObjectState.Initializing) {
                Debug.LogWarning($"Object is out of state: {this}", this);
                return;
            }

            var cObj = this;
            var cParams = parameters;
            var closure = PoolClass<WindowObjectClosure>.Spawn();
            closure.instance = cObj;
            closure.tr = cParams;
            var cbParameters = parameters.ReplaceCallback(closure, static (closure) => {
                var obj = (WindowObjectClosure)closure;
                WindowSystem.SetShown(obj.instance, obj.tr, true);
                PoolClass<WindowObjectClosure>.Recycle(obj);
            });
            WindowSystem.ShowInstance(this, cbParameters, internalCall: true);

        }

        internal void HideInternal(TransitionParameters parameters = default) {
            
            if (parameters.data.replaceHideBehaviour == false) {
                parameters = parameters.ReplaceHideBehaviour(this.hideBehaviour);
            }

            if (this.IsInternalManualTouch(parameters) == true) {
                parameters.RaiseCallback();
                return;
            }
            
            WindowSystem.HideInstance(this, parameters, internalCall: true);

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

                    this.DoInit(new DoInitClosure() {
                        component = this,
                        parameters = parameters,
                    }, static (c) => {
                        c.component.Show(c.parameters);
                    });
                    
                } else {

                    Debug.LogWarning($"Object is out of state: {this}", this);

                }

                return;

            }

            if (this.objectState == ObjectState.Showing || this.objectState == ObjectState.Shown) {
                
                parameters.RaiseCallback();
                return;
                
            }

            this.internalManualShow = true;

            var cObj = this;
            var cParams = parameters;
            var closure = PoolClass<WindowObjectClosure>.Spawn();
            closure.instance = cObj;
            closure.tr = cParams;
            var cbParameters = parameters.ReplaceCallback(closure, static (closure) => {
                var obj = (WindowObjectClosure)closure;
                WindowSystem.SetShown(obj.instance, obj.tr, false);
                PoolClass<WindowObjectClosure>.Recycle(obj);
            });
            WindowSystem.ShowInstance(this, cbParameters, internalCall: true);

        }

        public void Hide() {
            
            this.Hide(default);
            
        }

        public virtual void Hide(TransitionParameters parameters) {

            if (parameters.data.replaceHideBehaviour == false) {
                parameters = parameters.ReplaceHideBehaviour(this.hideBehaviour);
            }
            
            if (this.objectState <= ObjectState.Initializing) {
                
                if (this.objectState == ObjectState.NotInitialized) {

                    this.DoInit(new DoInitClosure() {
                        component = this,
                        parameters = parameters,
                    }, static (c) => {
                        c.component.Hide(c.parameters);
                    });
                    return;
                    
                } else {

                    Debug.LogWarning($"Object is out of state: {this}", this);
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
            var closure = PoolClass<WindowObjectClosure>.Spawn();
            closure.instance = cObj;
            closure.tr = cParams;
            var cbParameters = parameters.ReplaceCallback(closure, static (closure) => {
                var obj = (WindowObjectClosure)closure;
                WindowSystem.SetHidden(obj.instance, obj.tr, false);
                PoolClass<WindowObjectClosure>.Recycle(obj);
            });
            WindowSystem.HideInstance(this, cbParameters, internalCall: true);

        }

        public void HideCurrentWindow() {
            
            this.window.Hide();
            
        }
        
        public readonly ref struct ClosureAPI<TState> {

            private readonly TState state;
            private readonly WindowObject windowObject;

            public ClosureAPI(TState state, WindowObject windowObject) {
                this.state = state;
                this.windowObject = windowObject;
            }

            public void LoadAsync<T>(Resource resource, System.Action<T, TState> onComplete = null, bool async = true) where T : WindowObject {
                this.windowObject.LoadAsync(this.state, resource, onComplete, async);
            }

            public void LoadAsync<T>(Resource resource, System.Action<TState> onComplete = null, bool async = true) where T : WindowObject {
                this.windowObject.LoadAsync<T, TState>(this.state, resource, onComplete, async);
            }

        }

        public ClosureAPI<TState> Closure<TState>(TState state) {
            return new ClosureAPI<TState>(state, this);
        }

        public void LoadAsync<T, TState>(TState state, Resource resource, System.Action<T, TState> onComplete = null, bool async = true) where T : WindowObject {
            
            this.LoadAsync_YIELD<T, LoadAsyncClosure<T, TState>>(new LoadAsyncClosure<T, TState>() {
                component = this,
                onComplete = onComplete,
                state = state,
            }, resource, static (obj, c) => {
                c.onComplete.Invoke(obj, c.state);
            }, async);
            
        }

        public void LoadAsync<T, TState>(TState state, Resource resource, System.Action<TState> onComplete = null, bool async = true) where T : WindowObject {
            
            this.LoadAsync_YIELD<T, LoadAsyncClosure<T, TState>>(new LoadAsyncClosure<T, TState>() {
                component = this,
                onCompleteState = onComplete,
                state = state,
            }, resource, static (obj, c) => {
                c.onCompleteState.Invoke(c.state);
            }, async);
            
        }

        public void LoadAsync<T>(Resource resource, System.Action<T> onComplete = null, bool async = true) where T : WindowObject {
            
            this.LoadAsync_YIELD<T, LoadAsyncClosure<T, T>>(new LoadAsyncClosure<T, T>() {
                component = this,
                onCompleteNoState = onComplete,
            }, resource, static (obj, c) => {
                c.onCompleteNoState.Invoke(obj);
            }, async);
            
        }

        private void LoadAsync_YIELD<T, TState>(TState state, Resource resource, System.Action<T, TState> onComplete = null, bool async = true) where T : WindowObject {
            
            var resources = WindowSystem.GetResources();
            var data = new LoadAsyncClosure<T, TState>() {
                component = this,
                onComplete = onComplete,
                state = state,
            };
            resources.LoadAsync<T, LoadAsyncClosure<T, TState>>(new WindowSystemResources.LoadParameters() { async = async },this.GetWindow(), data, resource, static (asset, closure) => {

                if (asset != null && closure.component != null) {

                    var instance = closure.component.Load(asset);
                    if (closure.onComplete != null) closure.onComplete.Invoke(instance, closure.state);

                } else {
                    
                    if (closure.onComplete != null) closure.onComplete.Invoke(null, closure.state);
                    
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
        
        public bool UnloadSubObject(WindowObject subObject)  {
            
            if (this.UnRegisterSubObject(subObject) == true) {
                var pools = WindowSystem.GetPools();
                pools.Despawn(subObject);
                return true;
            }

            return false;

        }

        public void UnloadSubObjects()  {
            
            if (this.subObjects.Count == 0) return;
            
            for (int i = this.subObjects.Count - 1; i >= 0; i--) {

                var subObject = this.subObjects[i];
                this.UnloadSubObject(subObject);
                
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
        public virtual void OnEvent<T>(T data) where T : class { }

        public virtual void OnLayoutReady() { }
        
        public virtual void OnInit() { }

        public virtual void OnDeInit() { }

        public virtual void OnShowBegin() { }

        public virtual void OnShowEnd() { }

        public virtual void OnHideBegin() { }

        public virtual void OnHideEnd() { }

        public virtual void OnFocusTook() { }

        public virtual void OnFocusLost() { }
        
        public virtual void OnPoolGet() {}
        
        public virtual void OnPoolAdd() {}
        #endregion

    }

}
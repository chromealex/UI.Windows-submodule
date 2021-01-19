using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEngine.UI.Windows {

    using Modules;
    using Utilities;

    public enum SequenceEvent {

        OnHideBegin,
        OnHideEnd,

    }

    public enum UIWSCameraMode {

        UseSettings,
        Orthographic,
        Perspective,

    }

    public enum UIWSRenderMode {

        UseSettings,
        /// <summary>
        ///   <para>Render at the end of the Scene using a 2D Canvas.</para>
        /// </summary>
        ScreenSpaceOverlay,
        /// <summary>
        ///   <para>Render using the Camera configured on the Canvas.</para>
        /// </summary>
        ScreenSpaceCamera,
        /// <summary>
        ///   <para>Render using any Camera in the Scene that can render the layer.</para>
        /// </summary>
        WorldSpace,

    }

    public interface IHasPreview {}
    
    [System.Serializable]
    public struct WindowPreferences {

        public static WindowPreferences Default => new WindowPreferences() {
            layer = new UIWSLayer() { value = 0 },
            takeFocus = true,
        };

        [Header("Base Parameters")]
        [Tooltip("Window layer to draw on, all windows are sorting by layer first.")]
        public UIWSLayer layer;
        [Tooltip("Check if you need to show this window only once.")]
        public bool singleInstance;

        [Space(10f)]
        [Tooltip("Add in history into current breadcrumb.")]
        public bool addInHistory;
        [Tooltip("Take focus on window open and send unfocused event to all windows behind.")]
        public bool takeFocus;

        [Space(10f)]
        [Tooltip("Override canvas render mode.")]
        public UIWSRenderMode renderMode;
        [Tooltip("Override camera mode. You can change camera mode at runtime.")]
        public UIWSCameraMode cameraMode;

        [Space(10f)]
        [Tooltip("Collect windows into queue and use this queue automatically on event.")]
        public bool showInSequence;
        public SequenceEvent showInSequenceEvent;

        [Header("Performance Options")]
        [Tooltip("If this window has full-rect opaque background you can set this option as true to deactivate render on all windows behind this.")]
        public bool fullCoverage;

    }

    public struct InitialParameters {

        public bool overrideLayer;
        public UIWSLayer layer;

        public bool overrideSingleInstance;
        public bool singleInstance;

    }

    [System.Serializable]
    public struct TransitionParametersData {

        internal bool resetAnimation;
        internal bool immediately;

        internal System.Action callback;

    }

    [System.Serializable]
    public struct TransitionParameters {

        internal TransitionParametersData data;

        public static TransitionParameters Default => new TransitionParameters() {
            data = new TransitionParametersData() { resetAnimation = false },
        };

        public void RaiseCallback() {

            if (this.data.callback != null) this.data.callback.Invoke();

        }

        public TransitionParameters ReplaceImmediately(bool state) {

            var instance = this;
            instance.data.immediately = state;
            return instance;

        }

        public TransitionParameters ReplaceCallback(System.Action callback) {

            var instance = this;
            instance.data.callback = callback;
            return instance;

        }

    }

    public enum WindowEvent {

        None = 0,

        OnInitialize,
        OnDeInitialize,

        OnShowBegin,
        OnShowEnd,

        OnHideBegin,
        OnHideEnd,

        OnFocusTook,
        OnFocusLost,

    }

    public class WindowSystem : MonoBehaviour {

        [System.Serializable]
        public struct WindowItem {

            public WindowBase prefab;
            public WindowBase instance;

        }

        public static System.Action onPointerUp;
        
        [Tooltip("Automatically show `Root Screen` on Start.")]
        public bool showRootOnStart;
        public WindowBase rootScreen;
        
        public List<WindowBase> registeredPrefabs = new List<WindowBase>();

        [Tooltip("Platform emulation for target filters.")]
        public bool emulatePlatform;
        public RuntimePlatform emulateRuntimePlatform;

        public WindowSystemBreadcrumbs breadcrumbs;
        public WindowSystemEvents events;
        public WindowSystemSettings settings;
        public WindowSystemResources resources;
        public WindowSystemPools pools;
        public Tweener tweener;

        private List<WindowItem> currentWindows = new List<WindowItem>();
        private Dictionary<int, int> windowsCountByLayer = new Dictionary<int, int>();
        private Dictionary<int, WindowBase> topWindowsByLayer = new Dictionary<int, WindowBase>();
        private Dictionary<int, WindowBase> hashToPrefabs = new Dictionary<int, WindowBase>();

        private int nextWindowId;
        
        private static WindowSystem _instance;

        private static WindowSystem instance {
            get {

                #if UNITY_EDITOR
                if (Application.isPlaying == false && WindowSystem._instance == null) {

                    WindowSystem._instance = Object.FindObjectOfType<WindowSystem>();

                }
                #endif

                return WindowSystem._instance;
            }
        }

        public static bool HasInstance() {

            return WindowSystem.instance != null;

        }
        
        public void Awake() {

            WindowSystem._instance = this;
            GameObject.DontDestroyOnLoad(this.gameObject);

        }

        public void Start() {

            this.events.Initialize();
            this.breadcrumbs.Initialize();

            foreach (var item in this.registeredPrefabs) {

                var key = item.GetType().GetHashCode();
                if (this.hashToPrefabs.ContainsKey(key) == true) {
                    
                    Debug.LogWarning($"Window with hash `{key}` already exists in windows hash map!");
                    continue;
                    
                }
                this.hashToPrefabs.Add(key, item);

            }

            if (this.showRootOnStart == true) WindowSystem.ShowRoot();

        }

        private Vector2 pointerScreenPosition;

        public static Vector2 GetPointerPosition() {

            return WindowSystem.instance.pointerScreenPosition;

        }
        
        public void Update() {

            #if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Mouse.current.leftButton.wasReleasedThisFrame == true ||
                UnityEngine.InputSystem.Mouse.current.rightButton.wasReleasedThisFrame == true ||
                UnityEngine.InputSystem.Mouse.current.middleButton.wasReleasedThisFrame == true) {
                
                this.pointerScreenPosition = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
                if (WindowSystem.onPointerUp != null) WindowSystem.onPointerUp.Invoke();
                
            }

            var touches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
            if (touches.Count > 0) {

                for (int i = 0; i < touches.Count; ++i) {

                    var touch = touches[i];
                    if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended || touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled) {

                        this.pointerScreenPosition = touch.screenPosition;
                        if (WindowSystem.onPointerUp != null) WindowSystem.onPointerUp.Invoke();

                    }

                }
                
            }
            #elif ENABLE_LEGACY_INPUT_MANAGER
            if (UnityEngine.Input.GetMouseButtonUp(0) == true ||
                UnityEngine.Input.GetMouseButtonUp(1) == true ||
                UnityEngine.Input.GetMouseButtonUp(2) == true) {
                
                this.pointerScreenPosition = Input.mousePosition;
                if (WindowSystem.onPointerUp != null) WindowSystem.onPointerUp.Invoke();
                
            }

            if (UnityEngine.Input.touchCount > 0) {

                for (int i = 0; i < UnityEngine.Input.touches.Length; ++i) {

                    var touch = UnityEngine.Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) {
                        
                        this.pointerScreenPosition = touch.position;
                        if (WindowSystem.onPointerUp != null) WindowSystem.onPointerUp.Invoke();

                    }

                }
                
            }
            #endif
            
        }

        public void OnValidate() {

            foreach (var item in this.registeredPrefabs) {

                item.windowId = item.GetType().GetHashCode();

            }

        }

        public static RuntimePlatform GetCurrentRuntimePlatform() {

            if (WindowSystem.instance.emulatePlatform == true) {

                return WindowSystem.instance.emulateRuntimePlatform;

            }

            return Application.platform;

        }

        public static TargetData GetTargetData() {

            return new TargetData() {
                platform = WindowSystem.GetCurrentRuntimePlatform(),
                screenSize = new Vector2(Screen.width, Screen.height)
            };

        }

        internal static void SendFullCoverageOnShowEnd(WindowBase window) {

            if (window.preferences.fullCoverage == true) WindowSystem.instance.TurnRenderBeneath(window, false);

        }

        internal static void SendFullCoverageOnHideBegin(WindowBase window) {

            if (window.preferences.fullCoverage == true) WindowSystem.instance.TurnRenderBeneath(window, true);

        }

        internal static void SendFocusOnShowBegin(WindowBase window) {

            WindowSystem.instance.SendFocusOnShowBegin_INTERNAL(window);

        }

        internal static void SendFocusOnHideBegin(WindowBase window) {

            WindowSystem.instance.SendFocusOnHideBegin_INTERNAL(window);

        }

        private void TurnRenderBeneath(WindowBase window, bool state) {

            var ordered = this.currentWindows.OrderByDescending(x => x.instance.GetDepth());
            foreach (var item in ordered) {

                var instance = item.instance;

                var depth = instance.GetDepth();
                if (depth < window.GetDepth()) {

                    if (state == false) {

                        instance.TurnOffRender();

                    } else {

                        instance.TurnOnRender();

                    }

                    // If beneath window has fullCoverage - break enumeration because beneath graph has its own coverage handler
                    if (instance.preferences.fullCoverage == true) break;

                }

            }

        }

        private WindowBase GetTopInstanceForFocus(WindowBase ignore = null) {

            var maxDepth = float.MinValue;
            WindowBase topInstance = null;
            for (int i = 0; i < this.currentWindows.Count; ++i) {

                var instance = this.currentWindows[i].instance;
                if (instance.preferences.takeFocus == false) continue;
                if (instance.GetState() >= ObjectState.Hiding) continue;

                var depth = instance.GetDepth();
                if (depth > maxDepth && instance != ignore) {

                    maxDepth = depth;
                    topInstance = instance;

                }

            }

            return topInstance;

        }

        internal void SendFocusOnShowBegin_INTERNAL(WindowBase window) {

            if (window.preferences.takeFocus == false) return;

            var topInstance = this.GetTopInstanceForFocus();
            if (topInstance != null) {

                var topInstancePrev = this.GetTopInstanceForFocus(window);
                if (topInstancePrev != null) topInstancePrev.DoFocusLostInternal();

                topInstance.DoFocusTookInternal();
                if (topInstance != window) window.DoFocusLostInternal();

            } else {

                window.DoFocusTookInternal();

            }

        }

        internal void SendFocusOnHideBegin_INTERNAL(WindowBase window) {

            if (window.preferences.takeFocus == false) return;

            var topInstance = this.GetTopInstanceForFocus(window);
            window.DoFocusLostInternal();
            if (topInstance != null) topInstance.DoFocusTookInternal();

        }

        public static int GetCountByLayer(UIWSLayer layer) {

            if (WindowSystem.instance.windowsCountByLayer.TryGetValue(layer.value, out int count) == true) {

                return count;

            }

            return 0;

        }

        public static float GetNextDepth(UIWSLayer layer) {

            var settings = WindowSystem.GetSettings();
            var layerInfo = settings.GetLayerInfo(layer.value);
            var step = (layerInfo.maxDepth - layerInfo.minDepth) / settings.windowsPerLayer;

            if (WindowSystem.instance.topWindowsByLayer.TryGetValue(layer.value, out var instance) == true) {

                return instance.GetDepth() + step;

            }

            return layerInfo.minDepth;

        }

        public static float GetNextZDepth(UIWSLayer layer) {

            var settings = WindowSystem.GetSettings();
            var layerInfo = settings.GetLayerInfo(layer.value);
            var step = (layerInfo.maxZDepth - layerInfo.minZDepth) / settings.windowsPerLayer;

            if (WindowSystem.instance.topWindowsByLayer.TryGetValue(layer.value, out var instance) == true) {

                return instance.GetZDepth() + step;

            }

            return layerInfo.minZDepth;

        }

        public static void RaiseEvent(WindowObject instance, WindowEvent windowEvent) {

            var events = WindowSystem.GetEvents();
            events.Raise(instance, windowEvent);

        }

        public static void ClearEvents(WindowObject instance) {

            var events = WindowSystem.GetEvents();
            events.Clear(instance);

        }

        public static void RegisterActionOnce(WindowObject instance, WindowEvent windowEvent, System.Action callback) {

            var events = WindowSystem.GetEvents();
            events.RegisterOnce(instance, windowEvent, callback);

        }

        public static void RegisterAction(WindowObject instance, WindowEvent windowEvent, System.Action callback) {

            var events = WindowSystem.GetEvents();
            events.Register(instance, windowEvent, callback);

        }

        public static void UnRegisterAction(WindowObject instance, WindowEvent windowEvent, System.Action callback) {

            var events = WindowSystem.GetEvents();
            events.UnRegister(instance, windowEvent, callback);

        }

        public static WindowSystemBreadcrumbs GetBreadcrumbs() {

            return WindowSystem.instance.breadcrumbs;

        }

        public static WindowSystemPools GetPools() {

            return WindowSystem.instance.pools;

        }

        public static WindowSystemEvents GetEvents() {

            return WindowSystem.instance.events;

        }

        public static WindowSystemSettings GetSettings() {

            return WindowSystem.instance.settings;

        }

        public static WindowSystemResources GetResources() {

            return WindowSystem.instance.resources;

        }

        public static Tweener GetTweener() {

            return WindowSystem.instance.tweener;

        }

        public class ShowHideClosureParametersClass {

            public WindowObject instance;
            public TransitionParameters parameters;
            public bool internalCall;
            public bool animationComplete;
            public bool hierarchyComplete;

            public void Dispose() {

                this.instance = null;
                this.parameters = default;
                PoolClass<ShowHideClosureParametersClass>.Recycle(this);
                
            }

        }

        public static void ShowInstance(WindowObject instance, TransitionParameters parameters, bool internalCall = false) {

            if (instance.objectState == ObjectState.Showing || instance.objectState == ObjectState.Shown) {
                
                parameters.RaiseCallback();
                return;
                
            }
            instance.SetState(ObjectState.Showing);

            instance.OnShowBeginInternal();
            instance.OnShowBegin();
            WindowSystem.RaiseEvent(instance, WindowEvent.OnShowBegin);

            {

                if (instance.gameObject.activeSelf == false) instance.gameObject.SetActive(true);
                instance.SetVisible();
                instance.SetResetState();
                
                var closure = PoolClass<ShowHideClosureParametersClass>.Spawn();
                closure.animationComplete = false;
                closure.hierarchyComplete = false;
                closure.instance = instance;
                closure.parameters = parameters;
                closure.internalCall = internalCall;

                Coroutines.CallInSequence((p) => {

                    p.hierarchyComplete = true;
                    if (p.animationComplete == true) {

                        var pars = p.parameters;
                        p.Dispose();
                        pars.RaiseCallback();

                    }

                }, closure, instance.subObjects, (obj, cb, p) => {

                    if (p.internalCall == true) {
                       
                        obj.ShowInternal(p.parameters.ReplaceCallback(cb));
 
                    } else {

                        obj.Show(p.parameters.ReplaceCallback(cb));

                    }

                });
                
                WindowObjectAnimation.Show(closure, instance, parameters, (cParams) => {
                    
                    cParams.animationComplete = true;
                    if (cParams.hierarchyComplete == true) {
                        
                        var pars = cParams.parameters;
                        cParams.Dispose();
                        pars.RaiseCallback();
                        
                    }
                    
                });

            }

        }

        internal static void SetShown(WindowObject instance, TransitionParameters parameters) {

            if (instance.objectState != ObjectState.Showing) {
                
                parameters.RaiseCallback();
                return;
                
            }
            
            var innerParameters = parameters.ReplaceCallback(null);
            for (int i = 0; i < instance.subObjects.Count; ++i) {
                
                WindowSystem.SetShown(instance.subObjects[i], innerParameters);
                
            }
            
            instance.OnShowEndInternal();
            instance.OnShowEnd();
            WindowSystem.RaiseEvent(instance, WindowEvent.OnShowEnd);

            instance.SetState(ObjectState.Shown);

            parameters.RaiseCallback();

        }

        internal static void SetHidden(WindowObject instance, TransitionParameters parameters) {

            if (instance.objectState != ObjectState.Hiding) {
                
                parameters.RaiseCallback();
                return;
                
            }

            var innerParameters = parameters.ReplaceCallback(null);
            for (int i = 0; i < instance.subObjects.Count; ++i) {
                
                WindowSystem.SetHidden(instance.subObjects[i], innerParameters);
                
            }

            instance.OnHideEndInternal();
            instance.OnHideEnd();
            WindowSystem.RaiseEvent(instance, WindowEvent.OnHideEnd);

            instance.SetState(ObjectState.Hidden);
            instance.SetInvisible();

            parameters.RaiseCallback();

        }
        
        public static void HideInstance(WindowObject instance, TransitionParameters parameters) {

            if (instance.objectState <= ObjectState.Initializing) {
                
                Debug.LogWarning("Object is out of state: " + instance, instance);
                return;
                
            }

            if (instance.objectState == ObjectState.Hiding || instance.objectState == ObjectState.Hidden) {
                
                parameters.RaiseCallback();
                return;
                
            }
            instance.SetState(ObjectState.Hiding);

            instance.OnHideBeginInternal();
            instance.OnHideBegin();
            WindowSystem.RaiseEvent(instance, WindowEvent.OnHideBegin);

            {

                var closure = PoolClass<ShowHideClosureParametersClass>.Spawn();
                closure.animationComplete = false;
                closure.hierarchyComplete = false;
                closure.instance = instance;
                closure.parameters = parameters;
                
                Coroutines.CallInSequence((p) => {

                    p.hierarchyComplete = true;
                    
                    if (p.animationComplete == true) {

                        var pars = p.parameters;
                        p.Dispose();
                        pars.RaiseCallback();

                    }

                }, closure, instance.subObjects, (obj, cb, p) => {
                    
                    obj.Hide(p.parameters.ReplaceCallback(cb));
                    
                });
                
                WindowObjectAnimation.Hide(closure, instance, parameters, (cParams) => {
                    
                    cParams.animationComplete = true;
                    if (cParams.hierarchyComplete == true) {
                        
                        var pars = cParams.parameters;
                        cParams.Dispose();
                        pars.RaiseCallback();
                        
                    }
                    
                });

            }

        }

        /// <summary>
        /// Clean up window instance.
        /// This instance would be removed from pools, and all resources will be free.
        /// </summary>
        /// <param name="instance"></param>
        public static void Clean(WindowBase instance) {

            instance.DoDeInit();

            var pools = WindowSystem.GetPools();
            pools.RemoveInstance(instance);
            
        }

        public static void ShowRoot() {
            
            WindowSystem.Show(WindowSystem.instance.rootScreen);
            
        }
        
        public static void HideAll<T>(TransitionParameters parameters = default) where T : WindowBase {

            WindowSystem.HideAll((x) => x is T, parameters);

        }

        public static void HideAll(TransitionParameters parameters = default) {

            WindowSystem.HideAll(null, parameters);

        }
        
        public static void HideAll(System.Predicate<WindowBase> predicate, TransitionParameters parameters = default) {

            var currentList = WindowSystem.instance.currentWindows;
            var count = currentList.Count;
            var filteredCount = 0;
            for (int i = 0; i < count; ++i) {
                
                var instance = currentList[i].instance;
                if (predicate == null || predicate.Invoke(instance) == true) ++filteredCount;

            }

            if (filteredCount == 0) {

                parameters.RaiseCallback();
                return;

            }
            
            var ptr = 0;
            var instanceParameters = parameters.ReplaceCallback(() => {
                
                ++ptr;
                if (ptr == filteredCount) {
                    
                    parameters.RaiseCallback();
                    
                }
                
            });
            for (int i = 0; i < count; ++i) {

                var instance = currentList[i].instance;
                if (predicate == null || predicate.Invoke(instance) == true) instance.Hide(instanceParameters);
                
            }

        }
        
        public static void Show<T>(System.Action<T> onInitialized = null) where T : WindowBase {

            WindowSystem.instance.Show_INTERNAL(new InitialParameters(), onInitialized);

        }

        public static void Show(WindowBase source, System.Action<WindowBase> onInitialized = null) {

            WindowSystem.instance.Show_INTERNAL(source, new InitialParameters(), onInitialized);

        }

        public static void Show<T>(WindowBase source, System.Action<T> onInitialized = null) where T : WindowBase {

            WindowSystem.instance.Show_INTERNAL(source, new InitialParameters(), (x) => {

                if (onInitialized != null) onInitialized.Invoke((T)x);

            });

        }

        public static void Show<T>(InitialParameters initialParameters, System.Action<T> onInitialized = null) where T : WindowBase {

            WindowSystem.instance.Show_INTERNAL(initialParameters, onInitialized);

        }

        public static void Show(WindowBase source, InitialParameters initialParameters, System.Action<WindowBase> onInitialized = null) {

            WindowSystem.instance.Show_INTERNAL(source, initialParameters, onInitialized);

        }

        public static void Show<T>(WindowBase source, InitialParameters initialParameters, System.Action<T> onInitialized = null) where T : WindowBase {

            WindowSystem.instance.Show_INTERNAL(source, initialParameters, (x) => {

                if (onInitialized != null) onInitialized.Invoke((T)x);

            });

        }

        public static void RemoveWindow(WindowBase instance) {

            var uiws = WindowSystem.instance;
            if (uiws.windowsCountByLayer.TryGetValue(instance.preferences.layer.value, out var count) == true) {

                uiws.windowsCountByLayer[instance.preferences.layer.value] = count - 1;

            }

            uiws.RemoveWindow_INTERNAL(instance);

        }

        private void RemoveWindow_INTERNAL(WindowBase instance) {

            var layer = instance.preferences.layer;
            for (int i = 0; i < this.currentWindows.Count; ++i) {

                if (this.currentWindows[i].instance == instance) {

                    this.currentWindows.RemoveAt(i);
                    if (instance.preferences.addInHistory == true) this.breadcrumbs.OnWindowRemoved(instance);
                    break;

                }

            }

            if (this.topWindowsByLayer.TryGetValue(layer.value, out var topInstance) == true) {

                if (topInstance == instance) {

                    this.topWindowsByLayer.Remove(layer.value);
                    for (int i = 0; i < this.currentWindows.Count; ++i) {

                        this.TryAddTopWindow(layer, this.currentWindows[i].instance);

                    }

                }

            }

        }

        private T GetSource<T>() where T : WindowBase {

            var hash = typeof(T).GetHashCode();
            if (this.hashToPrefabs.TryGetValue(hash, out var prefab) == true) {

                return (T)prefab;

            }

            return default;

        }

        public static bool IsOpenedBySource(WindowBase source) {

            return WindowSystem.instance.IsOpenedBySource_INTERNAL(source);

        }

        private bool IsOpenedBySource_INTERNAL(WindowBase source) {

            return this.IsOpenedBySource_INTERNAL(source, out _);

        }

        private bool IsOpenedBySource_INTERNAL(WindowBase source, out WindowBase result, ObjectState minimumState = ObjectState.NotInitialized, ObjectState maximumState = ObjectState.DeInitialized, bool last = false) {

            if (last == true) {

                for (int i = this.currentWindows.Count - 1; i >= 0; --i) {

                    var win = this.currentWindows[i];
                    if (win.prefab == source && win.instance.GetState() >= minimumState && win.instance.GetState() <= maximumState) {

                        result = win.instance;
                        return true;

                    }

                }

            } else {

                for (int i = 0; i < this.currentWindows.Count; ++i) {

                    var win = this.currentWindows[i];
                    if (win.prefab == source && win.instance.GetState() >= minimumState && win.instance.GetState() <= maximumState) {

                        result = win.instance;
                        return true;

                    }

                }

            }

            result = default;
            return false;

        }

        private void TryAddTopWindow(UIWSLayer layer, WindowBase instance) {

            if (this.topWindowsByLayer.TryGetValue(layer.value, out var topInstance) == true) {

                if (instance.GetDepth() > topInstance.GetDepth()) {

                    this.topWindowsByLayer[layer.value] = instance;

                }

            } else {

                this.topWindowsByLayer.Add(layer.value, instance);

            }

        }

        public static WindowItem GetPreviousWindow(WindowBase instance) {

            var breadcrumbs = WindowSystem.GetBreadcrumbs();
            return breadcrumbs.GetPrevious(instance, true);

        }
        
        private void Show_INTERNAL<T>(InitialParameters initialParameters, System.Action<T> onInitialized = null) where T : WindowBase {

            var source = this.GetSource<T>();
            this.Show_INTERNAL(source, initialParameters, (x) => {

                if (onInitialized != null) onInitialized.Invoke((T)x);

            });

        }

        private void Show_INTERNAL(WindowBase source, InitialParameters initialParameters, System.Action<WindowBase> onInitialized) {

            if (source == null) {

                throw new System.Exception("Window Source is null, did you forget to collect your screens?");

            }
            
            WindowBase instance;
            var singleInstance = source.preferences.singleInstance;
            if (initialParameters.overrideSingleInstance == true) {

                singleInstance = initialParameters.singleInstance;

            }

            if (singleInstance == true) {

                if (this.IsOpenedBySource_INTERNAL(source, out instance, maximumState: ObjectState.Shown) == true) {

                    onInitialized.Invoke(instance);
                    return;

                }

            }

            if (source.createPool == true) this.pools.CreatePool(source);
            instance = this.pools.Spawn(source, null, out var fromPool);
            instance.identifier = ++this.nextWindowId;
            #if UNITY_EDITOR
            instance.name = "[" + instance.identifier.ToString("00") + "] " + source.name;
            #endif
            instance.SetInitialParameters(initialParameters);
            GameObject.DontDestroyOnLoad(instance.gameObject);

            if (instance.preferences.showInSequence == true) {

                if (this.IsOpenedBySource_INTERNAL(source, out var existInstance, minimumState: ObjectState.Hiding, last: true) == true) {

                    WindowEvent state = WindowEvent.None;
                    switch (instance.preferences.showInSequenceEvent) {

                        case SequenceEvent.OnHideBegin:
                            state = WindowEvent.OnHideBegin;
                            break;

                        case SequenceEvent.OnHideEnd:
                            state = WindowEvent.OnHideEnd;
                            break;

                    }

                    WindowSystem.RegisterActionOnce(existInstance, state, () => { this.Show_INTERNAL(existInstance, initialParameters, onInitialized); });

                    return;

                }

            }

            var item = new WindowItem() {
                prefab = source,
                instance = instance
            };

            if (instance.preferences.addInHistory == true) {
                
                this.breadcrumbs.Add(item);
                instance.breadcrumb = this.breadcrumbs.GetMain();

            }
            this.currentWindows.Add(item);

            GameObject.DontDestroyOnLoad(instance.gameObject);

            instance.LoadAsync(() => {
            
                { // Setup for each instance

                    instance.Setup(instance);
                    this.TryAddTopWindow(instance.preferences.layer, instance);
                    if (this.windowsCountByLayer.TryGetValue(instance.preferences.layer.value, out var count) == true) {

                        this.windowsCountByLayer[instance.preferences.layer.value] = count + 1;

                    } else {

                        this.windowsCountByLayer.Add(instance.preferences.layer.value, 1);

                    }

                }

                instance.DoInit();

                if (onInitialized != null) onInitialized.Invoke(instance);

                instance.ShowInternal();

            });

        }

    }

}
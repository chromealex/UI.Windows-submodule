using System.Collections.Generic;
using System.Linq;

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
        UseCameraSettings,

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

    public enum DontDestroy {

        Default = 0x0,
        Ever = -1,
        
        OnHideAll = 0x1,

    }
    
    [System.Serializable]
    public struct WindowPreferences {

        public static WindowPreferences Default => new WindowPreferences() {
            layer = new UIWSLayer() { value = 0 },
            takeFocus = true,
            forceSyncLoad = true,
        };

        [Header("Base Parameters")]
        [Tooltip("Window layer to draw on, all windows are sorting by layer first.")]
        public UIWSLayer layer;
        [Tooltip("Check if you need to show this window only once.")]
        public bool singleInstance;
        [Tooltip("Force sync screen load even loading set up via async WindowSystem.Show API")]
        public bool forceSyncLoad;

        [Space(10f)]
        [Tooltip("Add in history into current breadcrumb.")]
        public bool addInHistory;
        [Tooltip("Take focus on window open and send unfocused event to all windows behind.")]
        public bool takeFocus;

        [Space(10f)]
        public DontDestroy dontDestroy;
        
        [Space(10f)]
        [Tooltip("Override canvas render mode.")]
        public UIWSRenderMode renderMode;
        [Tooltip("Override camera mode. You can change camera mode at runtime.")]
        public UIWSCameraMode cameraMode;

        [Space(10f)]
        [Tooltip("Collect screens into queue and use this queue automatically on event.")]
        public bool showInSequence;
        public SequenceEvent showInSequenceEvent;

        [Header("Performance Options")]
        [Tooltip("If this screen has full-rect opaque background you can set this option as true to deactivate render on all screens behind this.")]
        public bool fullCoverage;

    }

    public struct InitialParameters {

        public bool overrideLayer;
        public UIWSLayer layer;

        public bool overrideSingleInstance;
        public bool singleInstance;

        public bool showSync;

    }

    [System.Serializable]
    public struct TransitionParametersData {

        internal bool resetAnimation;
        internal bool immediately;
        
        internal bool replaceDelay;
        internal float delay;

        internal bool replaceAffectChilds;
        internal bool affectChilds;

        internal bool replaceIgnoreTouch;
        internal bool ignoreTouch;

        internal System.Action callback;

    }
    
    internal struct TransitionInternalData {

        public WindowObject context;
        public TransitionParametersData data;
        public bool internalCall;

    }

    [System.Serializable]
    public struct TransitionParameters {

        internal TransitionParametersData data;

        private System.Action<WindowObject, TransitionParameters, bool> contextCallback;
        private TransitionInternalData internalData;

        public static TransitionParameters Default => new TransitionParameters() {
            data = new TransitionParametersData() { resetAnimation = false },
        };

        public void RaiseCallback() {

            if (this.contextCallback != null) this.contextCallback.Invoke(this.internalData.context, new TransitionParameters() { data = this.internalData.data }, this.internalData.internalCall);
            if (this.data.callback != null) this.data.callback.Invoke();

        }

        public TransitionParameters ReplaceIgnoreTouch(bool state) {

            var instance = this;
            instance.data.replaceIgnoreTouch = true;
            instance.data.ignoreTouch = state;
            return instance;

        }

        public TransitionParameters ReplaceAffectChilds(bool state) {

            var instance = this;
            instance.data.replaceAffectChilds = true;
            instance.data.affectChilds = state;
            return instance;

        }

        public TransitionParameters ReplaceResetAnimation(bool state) {

            var instance = this;
            instance.data.resetAnimation = state;
            return instance;

        }

        public TransitionParameters ReplaceImmediately(bool state) {

            var instance = this;
            instance.data.immediately = state;
            return instance;

        }

        public TransitionParameters ReplaceDelay(float value) {

            var instance = this;
            instance.data.delay = value;
            instance.data.replaceDelay = value > 0f;
            return instance;

        }

        public TransitionParameters ReplaceCallback(System.Action callback) {

            var instance = this;
            instance.data.callback = callback;
            instance.contextCallback = null;
            return instance;

        }

        public TransitionParameters ReplaceCallbackWithContext(System.Action<WindowObject, TransitionParameters, bool> callback, WindowObject context, TransitionParameters other, bool internalCall) {

            var instance = this;
            instance.data.callback = null;
            instance.contextCallback = callback;
            instance.internalData = new TransitionInternalData() { context = context, data = other.data, internalCall = internalCall };
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
        
        OnLayoutReady,

    }

    [DefaultExecutionOrder(-1000)]
    public class WindowSystem : MonoBehaviour {

        private const float CLICK_THRESHOLD = 0.125f;
        [System.Serializable]
        public struct WindowItem {

            public WindowBase prefab;
            public WindowBase instance;

        }

        public static System.Action onPointerUp;
        public static System.Action onPointerDown;

        [Tooltip("Automatically show `Root Screen` on Start.")]
        public bool showRootOnStart;
        public WindowBase rootScreen;
        
        public WindowBase loaderScreen;
        
        public List<WindowBase> registeredPrefabs = new List<WindowBase>();

        [Tooltip("Platform emulation for target filters.")]
        public bool emulatePlatform;
        public RuntimePlatform emulateRuntimePlatform;

        [RequiredReference]
        public WindowSystemBreadcrumbs breadcrumbs;
        new public WindowSystemAudio audio;
        [RequiredReference]
        public WindowSystemEvents events;
        [RequiredReference]
        public WindowSystemSettings settings;
        [RequiredReference]
        public WindowSystemResources resources;
        [RequiredReference]
        public WindowSystemPools pools;
        [RequiredReference]
        public Tweener tweener;

        [SearchAssetsByTypePopupAttribute(typeof(WindowSystemModule), menuName: "Modules")]
        public List<WindowSystemModule> modules = new List<WindowSystemModule>();

        private List<WindowItem> currentWindows = new List<WindowItem>();
        private Dictionary<int, int> windowsCountByLayer = new Dictionary<int, int>();
        private Dictionary<int, WindowBase> topWindowsByLayer = new Dictionary<int, WindowBase>();
        private Dictionary<System.Type, WindowBase> hashToPrefabs = new Dictionary<System.Type, WindowBase>();

        private bool loaderShowBegin;
        private WindowBase loaderInstance;
        private int nextWindowId;

        private System.Action waitInteractableOnComplete;
        private UnityEngine.UI.Windows.Components.IInteractable waitInteractable;
        private UnityEngine.UI.Windows.Components.IInteractable[] waitInteractables;
        private bool lockInteractables;
        private System.Action<UnityEngine.UI.Windows.Components.IInteractable> callbackOnAnyInteractable;
        private List<WindowObject> interactablesIgnoreContainers = new List<WindowObject>();

        internal static WindowSystem _instance;

        internal static WindowSystem instance {
            get {

                #if UNITY_EDITOR
                if (WindowSystem._instance == null && Application.isPlaying == false) {

                    WindowSystem._instance = Object.FindObjectOfType<WindowSystem>();

                }
                #endif

                return WindowSystem._instance;
            }
        }

        #if WITHOUT_DOMAIN_RELOAD
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void CleanupInstanceOnRun() {

            WindowSystem._instance = null;

        }
        #endif

        public static T GetWindowSystemModule<T>() where T : WindowSystemModule {

            if (WindowSystem.instance.modules != null) {

                for (int i = 0; i < WindowSystem.instance.modules.Count; ++i) {

                    var module = WindowSystem.instance.modules[i];
                    if (module != null && module is T moduleT) {
                        return moduleT;
                    }

                }

            }

            return null;

        }

        public static void AddModule<T>(T module) where T : WindowSystemModule {

            if (WindowSystem.instance.modules == null) WindowSystem.instance.modules = new List<WindowSystemModule>();
            WindowSystem.instance.modules.Add(module);
            
        }

        public static T FindOpened<T>() {

            foreach (var item in WindowSystem.instance.currentWindows) {

                if (item.instance is T win) return win;

            }

            return default;

        }

        public static T GetFocused<T>() where T : WindowBase {
            
            foreach (var item in WindowSystem.instance.currentWindows) {

                if (item.instance is T win && win.GetFocusState() == FocusState.Focused) return win;

            }

            return default;

        }
        
        public static bool HasInstance() {

            return WindowSystem.instance != null;

        }
        
        public void Awake() {

            #if ENABLE_INPUT_SYSTEM
            UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
            #endif

            this.Run();

        }

        public void OnEnable() {
            
            this.Run();
            
        }

        private void Run() {
        
            if (WindowSystem._instance != null) return;
            WindowSystem._instance = this;
            GameObject.DontDestroyOnLoad(this.gameObject);

            this.events.Initialize();
            this.breadcrumbs.Initialize();

            foreach (var item in this.registeredPrefabs) {

                var key = item.GetType();
                if (this.hashToPrefabs.TryAdd(key, item) != true) {
                    
                    Debug.LogWarning($"Window with hash `{key}` already exists in windows hash map!");
                    
                }

            }

        }

        public virtual void Start() {

            if (this.modules != null) {

                for (int i = 0; i < this.modules.Count; ++i) {

                    this.modules[i]?.OnStart();

                }

            }

            if (this.showRootOnStart == true) WindowSystem.ShowRoot();

        }

        public void OnDestroy() {
            
            if (this.modules != null) {

                for (int i = this.modules.Count - 1; i >= 0; --i) {

                    this.modules[i]?.OnDestroy();

                }

            }

            WindowSystem._instance = null;

            WindowSystem.onPointerUp = null;
            WindowSystem.onPointerDown = null;

        }

        private Vector2 pointerScreenPosition;
        private bool hasPointerUpThisFrame;
        private bool hasPointerDownThisFrame;
        private float lastPointerActionTime;

        public static T FindComponent<T>(System.Func<T, bool> filter = null) where T : WindowComponent {

            foreach (var window in WindowSystem.instance.currentWindows) {

                if (window.instance == null) continue;
                
                var component = window.instance.FindComponent(filter);
                if (component != null) return component;

            }

            return null;

        }

        public static void LockAllInteractables() {

            WindowSystem.instance.lockInteractables = true;

        }
        
        public static void UnlockAllInteractables() {

            WindowSystem.instance.lockInteractables = false;

        }

        public static void SetCallbackOnAnyInteractable(System.Action<UnityEngine.UI.Windows.Components.IInteractable> callback) {

            WindowSystem.instance.callbackOnAnyInteractable = callback;

        }

        public static void AddWaitInteractable(System.Action onComplete, UnityEngine.UI.Windows.Components.IInteractable interactable) {
            
            WindowSystem.instance.waitInteractableOnComplete = onComplete;
            
            ref var arr = ref WindowSystem.instance.waitInteractables;
            if (arr == null) {
                arr = new UnityEngine.UI.Windows.Components.IInteractable[1] {
                    interactable,
                };
            } else {
                var list = arr.ToList();
                list.Add(interactable);
                arr = list.ToArray();
            }
            
        }

        public static bool HasWaitInteractable() {
            return WindowSystem.instance.waitInteractables == null && WindowSystem.instance.waitInteractable == null;
        }

        public static void WaitInteractable(System.Action onComplete, UnityEngine.UI.Windows.Components.IInteractable interactable) {

            WindowSystem.instance.waitInteractableOnComplete = onComplete;
            WindowSystem.instance.waitInteractable = interactable;
            WindowSystem.instance.waitInteractables = null;
            WindowSystem.instance.lockInteractables = false;

        }

        public static void WaitInteractable(System.Action onComplete, params UnityEngine.UI.Windows.Components.IInteractable[] interactables) {

            WindowSystem.instance.waitInteractableOnComplete = onComplete;
            WindowSystem.instance.waitInteractable = null;
            WindowSystem.instance.waitInteractables = interactables;
            WindowSystem.instance.lockInteractables = false;

        }

        public static void CancelWaitInteractables() {

            WindowSystem.instance.waitInteractable = null;
            WindowSystem.instance.waitInteractables = null;
            WindowSystem.instance.waitInteractableOnComplete = null;
            WindowSystem.instance.lockInteractables = false;

        }

        public static void RaiseAndCancelWaitInteractables() {

            if (WindowSystem.instance.waitInteractable != null && WindowSystem.InteractWith(WindowSystem.instance.waitInteractable) == true) {
                
            }
            
            if (WindowSystem.instance.waitInteractables != null) {

                foreach (var item in WindowSystem.instance.waitInteractables) {

                    var comp = (item as WindowComponent);
                    if (comp != null && comp.GetState() == ObjectState.Shown) WindowSystem.InteractWith(item);

                }
                
            }
            
            WindowSystem.CancelWaitInteractables();

        }

        public static void AddInteractablesIgnoreContainer(WindowObject container) {
            
            WindowSystem.instance.interactablesIgnoreContainers.Add(container);
            
        }

        public static void RemoveInteractablesIgnoreContainer(WindowObject container) {
            
            WindowSystem.instance.interactablesIgnoreContainers.Remove(container);
            
        }

        public static bool CanInteractWith(UnityEngine.UI.Windows.Components.IInteractable interactable) {

            if (WindowSystem.instance.lockInteractables == true) return false;

            for (int i = 0; i < WindowSystem.instance.interactablesIgnoreContainers.Count; ++i) {

                var container = WindowSystem.instance.interactablesIgnoreContainers[i];
                if (container != null) {
                    
                    if (interactable is WindowObject interactableObj) {

                        var parent = interactableObj.FindComponentParent<WindowObject, WindowObject>(container, (obj, x) => {
                            return x == obj;
                        });
                        if (parent != null) return true;

                    }
                    
                }

            }
            
            if (WindowSystem.instance.waitInteractables == null) {

                if (WindowSystem.instance.waitInteractable == null) return true;

                return WindowSystem.instance.waitInteractable == interactable;

            } else {

                for (int i = 0; i < WindowSystem.instance.waitInteractables.Length; ++i) {

                    var interactableItem = WindowSystem.instance.waitInteractables[i];
                    if (interactableItem == interactable) return true;

                }
                
                return false;
                
            }
            
        }

        public static bool InteractWith(UnityEngine.UI.Windows.Components.IInteractable interactable) {
            
            WindowSystem.instance.callbackOnAnyInteractable?.Invoke(interactable);
            
            if (WindowSystem.instance.lockInteractables == true) return false;

            if (WindowSystem.instance.waitInteractables == null) {

                if (WindowSystem.instance.waitInteractable == null ||
                    WindowSystem.instance.waitInteractable == interactable) {
                    
                    WindowSystem.instance.waitInteractableOnComplete?.Invoke();
                    return true;

                }

            } else {

                for (int i = 0; i < WindowSystem.instance.waitInteractables.Length; ++i) {

                    var interactableItem = WindowSystem.instance.waitInteractables[i];
                    if (interactableItem == interactable) {

                        WindowSystem.instance.waitInteractableOnComplete?.Invoke();
                        return true;

                    }

                }
                
            }
            
            return false;
                
        }

        public static Vector2 GetPointerPosition() {

            return WindowSystem.instance.pointerScreenPosition;

        }

        public static bool HasPointerUpThisFrame() {

            return WindowSystem.instance.hasPointerUpThisFrame;

        }
        
        public static bool HasPointerDownThisFrame() {

            return WindowSystem.instance.hasPointerDownThisFrame;

        }

        
        public void Update() {

            if (this.modules != null) {

                for (int i = 0; i < this.modules.Count; ++i) {

                    this.modules[i]?.OnUpdate();

                }

            }

            this.hasPointerUpThisFrame = false;
            this.hasPointerDownThisFrame = false;
            WindowSystemInput.hasPointerClickThisFrame.Data = false;
            
            #if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Mouse.current != null &&
                (UnityEngine.InputSystem.Mouse.current.leftButton.wasReleasedThisFrame == true ||
                UnityEngine.InputSystem.Mouse.current.rightButton.wasReleasedThisFrame == true ||
                UnityEngine.InputSystem.Mouse.current.middleButton.wasReleasedThisFrame == true)) {
                
                this.pointerScreenPosition = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
                this.hasPointerUpThisFrame = true;
                this.lastPointerActionTime = Time.realtimeSinceStartup;
                if (WindowSystem.onPointerUp != null) WindowSystem.onPointerUp.Invoke();
                
            }

            var touches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
            if (touches.Count > 0) {

                for (int i = 0; i < touches.Count; ++i) {

                    var touch = touches[i];
                    if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended || touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled) {

                        this.pointerScreenPosition = touch.screenPosition;
                        this.hasPointerUpThisFrame = true;
                        WindowSystemInput.hasPointerClickThisFrame.Data = (Time.realtimeSinceStartup - this.lastPointerActionTime) <= CLICK_THRESHOLD;
                        this.lastPointerActionTime = Time.realtimeSinceStartup;
                        if (WindowSystem.onPointerUp != null) WindowSystem.onPointerUp.Invoke();

                    }

                }
                
            }
            #elif ENABLE_LEGACY_INPUT_MANAGER
            if (UnityEngine.Input.GetMouseButtonDown(0) == true ||
                UnityEngine.Input.GetMouseButtonDown(1) == true ||
                UnityEngine.Input.GetMouseButtonDown(2) == true) {
                
                this.pointerScreenPosition = Input.mousePosition;
                this.hasPointerDownThisFrame = true;
                this.lastPointerActionTime = Time.realtimeSinceStartup;
                if (WindowSystem.onPointerDown != null) WindowSystem.onPointerDown.Invoke();
                
            }
            
            if (UnityEngine.Input.GetMouseButtonUp(0) == true ||
                UnityEngine.Input.GetMouseButtonUp(1) == true ||
                UnityEngine.Input.GetMouseButtonUp(2) == true) {
                
                this.pointerScreenPosition = Input.mousePosition;
                this.hasPointerUpThisFrame = true;
                WindowSystemInput.hasPointerClickThisFrame.Data = (Time.realtimeSinceStartup - this.lastPointerActionTime) <= CLICK_THRESHOLD;
                this.lastPointerActionTime = Time.realtimeSinceStartup;
                if (WindowSystem.onPointerUp != null) WindowSystem.onPointerUp.Invoke();
                
            }

            if (UnityEngine.Input.touchCount > 0) {

                for (int i = 0; i < UnityEngine.Input.touches.Length; ++i) {

                    var touch = UnityEngine.Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) {
                        
                        this.pointerScreenPosition = touch.position;
                        this.hasPointerUpThisFrame = true;
                        if (WindowSystem.onPointerUp != null) WindowSystem.onPointerUp.Invoke();

                    }

                }
                
            }
            #endif
            
        }

        public static List<WindowItem> GetCurrentOpened() {

            return WindowSystem.instance.currentWindows;

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

        public static void SendEvent<T>(T data) {

            foreach (var item in WindowSystem.instance.currentWindows) {
                
                item.instance.SendEvent<T>(data);
                
            }
            
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

            var ordered = this.currentWindows.OrderByDescending(x => x.instance.GetDepth()).ToList();
            foreach (var item in ordered) {

                var instance = item.instance;
                if (instance.IsVisible() == false) continue;

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

            if (WindowSystem.instance.topWindowsByLayer.TryGetValue(layer.value, out var instance) == true) {

                var step = (layerInfo.maxDepth - layerInfo.minDepth) / settings.windowsPerLayer;
                return instance.GetDepth() + step;

            }

            return layerInfo.minDepth;

        }

        public static int GetNextCanvasDepth(UIWSLayer layer) {

            var settings = WindowSystem.GetSettings();
            var layerInfo = settings.GetLayerInfo(layer.value);

            if (WindowSystem.instance.topWindowsByLayer.TryGetValue(layer.value, out var instance) == true) {

                var step = settings.windowsPerLayer;
                return instance.GetCanvasDepth() + step;

            }

            return layer.value * settings.windowsPerLayer;

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

        public static void RegisterActionOnce(WindowObject instance, WindowEvent windowEvent, System.Action<WindowObject> callback) {

            var events = WindowSystem.GetEvents();
            events.RegisterOnce(instance, windowEvent, callback);

        }

        public static void RegisterAction(WindowObject instance, WindowEvent windowEvent, System.Action<WindowObject> callback) {

            var events = WindowSystem.GetEvents();
            events.Register(instance, windowEvent, callback);

        }

        public static void UnRegisterAction(WindowObject instance, WindowEvent windowEvent, System.Action<WindowObject> callback) {

            var events = WindowSystem.GetEvents();
            events.UnRegister(instance, windowEvent, callback);

        }

        public static WindowSystemBreadcrumbs GetBreadcrumbs() {

            return WindowSystem.instance?.breadcrumbs;

        }

        public static WindowSystemAudio GetAudio() {

            return WindowSystem.instance?.audio;

        }

        public static WindowSystemPools GetPools() {

            return WindowSystem.instance?.pools;

        }

        public static WindowSystemEvents GetEvents() {

            return WindowSystem.instance?.events;

        }

        public static WindowSystemSettings GetSettings() {

            return WindowSystem.instance?.settings;

        }

        public static WindowSystemResources GetResources() {

            return WindowSystem.instance?.resources;

        }

        public static Tweener GetTweener() {

            return WindowSystem.instance?.tweener;

        }

        public class ShowHideClosureParametersClass {

            public WindowObject instance;
            public TransitionParameters parameters;
            public bool internalCall;
            public bool animationComplete;
            public bool hierarchyComplete;
            public bool baseComplete;

            public void Dispose() {

                this.internalCall = default;
                this.animationComplete = default;
                this.hierarchyComplete = default;
                this.baseComplete = default;
                this.instance = null;
                this.parameters = default;
                PoolClass<ShowHideClosureParametersClass>.Recycle(this);
                
            }

        }

        public static void ShowInstance(WindowObject instance, TransitionParameters parameters, bool internalCall = false) {
            
            if (instance.GetState() == ObjectState.Showing || instance.GetState() == ObjectState.Shown) {
                
                parameters.RaiseCallback();
                return;
                
            }

            instance.SetState(ObjectState.Showing);

            {

                instance.OnShowBeginInternal();
                instance.OnShowBegin();
                WindowSystem.RaiseEvent(instance, WindowEvent.OnShowBegin);

            }

            var closure = PoolClass<ShowHideClosureParametersClass>.Spawn();
            {

                if (instance.gameObject.activeSelf == false) instance.gameObject.SetActive(true);
                instance.SetVisible();
                instance.SetResetState();

                closure.baseComplete = false;
                closure.animationComplete = false;
                closure.hierarchyComplete = false;
                closure.instance = instance;
                closure.parameters = parameters;
                closure.internalCall = internalCall;

                if (closure.parameters.data.replaceAffectChilds == false ||
                    closure.parameters.data.affectChilds == true) {

                    instance.BreakStateHierarchy();
                    
                    Coroutines.CallInSequence((p) => {

                        p.hierarchyComplete = true;
                        if (p.animationComplete == true && p.baseComplete == true) {

                            var pars = p.parameters;
                            p.Dispose();
                            pars.RaiseCallback();

                        }

                    }, closure, instance.subObjects, (obj, cb, p) => {

                        if (p.parameters.data.replaceDelay == true) {

                            if (p.internalCall == true) {

                                obj.ShowInternal(p.parameters.ReplaceCallback(cb).ReplaceDelay(0f));

                            } else {

                                obj.Show(p.parameters.ReplaceCallback(cb).ReplaceDelay(0f));

                            }

                        } else {
                            
                            if (p.internalCall == true) {

                                obj.ShowInternal(p.parameters.ReplaceCallback(cb));

                            } else {

                                obj.Show(p.parameters.ReplaceCallback(cb));

                            }

                        }

                    });

                } else {
                    
                    instance.BreakState();
                    
                    closure.hierarchyComplete = true;
                    
                }

                WindowObjectAnimation.Show(closure, instance, parameters, (cParams) => {
                    
                    cParams.animationComplete = true;
                    if (cParams.hierarchyComplete == true && cParams.baseComplete == true) {
                        
                        var pars = cParams.parameters;
                        cParams.Dispose();
                        pars.RaiseCallback();
                        
                    }
                    
                });

            }

            closure.baseComplete = true;
            if (closure.animationComplete == true && closure.hierarchyComplete == true) {
                
                closure.Dispose();
                parameters.RaiseCallback();
                
            }

        }

        internal static void SetShown(WindowObject instance, TransitionParameters parameters, bool internalCall) {

            if (instance.GetState() != ObjectState.Showing) {
                
                parameters.RaiseCallback();
                return;
                
            }

            if (internalCall == true) {
                
                if (instance.hiddenByDefault == true) {
                    
                    parameters.RaiseCallback();
                    return;
                    
                }
                
            }

            WindowObjectAnimation.SetState(instance, AnimationState.Show);
            
            var innerParameters = parameters.ReplaceCallback(null);
            for (int i = 0; i < instance.subObjects.Count; ++i) {
                
                WindowSystem.SetShown(instance.subObjects[i], innerParameters, internalCall);
                
            }
            
            instance.SetState(ObjectState.Shown);

            instance.OnShowEndInternal();
            instance.OnShowEnd();
            WindowSystem.RaiseEvent(instance, WindowEvent.OnShowEnd);

            parameters.RaiseCallback();

        }

        internal static void SetHidden(WindowObject instance, TransitionParameters parameters, bool internalCall) {

            if (instance.GetState() != ObjectState.Hiding) {
                
                parameters.RaiseCallback();
                return;
                
            }
            
            WindowObjectAnimation.SetState(instance, AnimationState.Hide);

            var innerParameters = parameters.ReplaceCallback(null);
            for (int i = 0; i < instance.subObjects.Count; ++i) {
                
                WindowSystem.SetHidden(instance.subObjects[i], innerParameters, internalCall);
                
            }

            instance.SetState(ObjectState.Hidden);            
            instance.SetInvisible();

            instance.OnHideEndInternal();
            instance.OnHideEnd();
            WindowSystem.RaiseEvent(instance, WindowEvent.OnHideEnd);

            parameters.RaiseCallback();

        }

        private struct HideInstanceClosure {

            public WindowObject instance;
            public TransitionParameters parameters;
            public bool internalCall;

        }
        
        public static void HideInstance(WindowObject instance, TransitionParameters parameters, bool internalCall = false) {

            if (instance.GetState() <= ObjectState.Initializing) {
                
                Debug.LogWarning("Object is out of state: " + instance, instance);
                return;
                
            }

            if (instance.GetState() == ObjectState.Hiding || instance.GetState() == ObjectState.Hidden) {
                
                parameters.RaiseCallback();
                return;
                
            }
            instance.SetState(ObjectState.Hiding);

            instance.OnHideBeginInternal();
            instance.OnHideBegin();
            WindowSystem.RaiseEvent(instance, WindowEvent.OnHideBegin);

            var closureInstance = new HideInstanceClosure() {
                instance = instance,
                parameters = parameters,
                internalCall = internalCall,
            };
            Coroutines.Wait(closureInstance, (inst) => inst.instance.IsReadyToHide(), (inst) => {
                
                {

                    var closure = PoolClass<ShowHideClosureParametersClass>.Spawn();
                    closure.animationComplete = false;
                    closure.hierarchyComplete = false;
                    closure.instance = inst.instance;
                    closure.parameters = inst.parameters;
                    closure.internalCall = inst.internalCall;

                    if (inst.parameters.data.replaceAffectChilds == false ||
                        inst.parameters.data.affectChilds == true) {

                        inst.instance.BreakStateHierarchy();
                        
                    } else {
                        
                        inst.instance.BreakState();

                    }

                    WindowObjectAnimation.Hide(closure, inst.instance, inst.parameters, (cParams) => {
                        
                        if (cParams.parameters.data.replaceAffectChilds == false ||
                            cParams.parameters.data.affectChilds == true) {

                            Coroutines.CallInSequence((p) => {

                                p.hierarchyComplete = true;
                            
                                if (p.animationComplete == true) {

                                    var pars = p.parameters;
                                    p.Dispose();
                                    pars.RaiseCallback();

                                }

                            }, cParams, cParams.instance.subObjects, (obj, cb, p) => {

                                if (p.parameters.data.replaceDelay == true) {

                                    if (p.internalCall == true) {

                                        obj.HideInternal(p.parameters.ReplaceCallback(cb).ReplaceDelay(0f));

                                    } else {

                                        obj.Hide(p.parameters.ReplaceCallback(cb).ReplaceDelay(0f));

                                    }

                                } else {
                            
                                    if (p.internalCall == true) {

                                        obj.HideInternal(p.parameters.ReplaceCallback(cb));

                                    } else {

                                        obj.Hide(p.parameters.ReplaceCallback(cb));

                                    }

                                }

                            });
                            
                        } else {
                            
                            cParams.hierarchyComplete = true;
                            
                        }

                        cParams.animationComplete = true;
                        if (cParams.hierarchyComplete == true) {
                            
                            var pars = cParams.parameters;
                            cParams.Dispose();
                            pars.RaiseCallback();
                            
                        }
                        
                    });

                }

            });
            
        }

        /// <summary>
        /// Clean up window instance.
        /// This instance would be removed from pools, and all resources will be free.
        /// </summary>
        /// <param name="instance"></param>
        public static void Clean(WindowBase instance) {

            if (instance.GetState() != ObjectState.Hidden) {

                throw new System.Exception($"WindowSystem.Clean failed because of instance state: {instance.GetState()} (required state: Hidden)");
                
            }

            instance.DoDeInit();

            var pools = WindowSystem.GetPools();
            pools.RemoveInstance(instance);
            
        }

        public static void ShowLoader(TransitionParameters transitionParameters = default, bool showSync = false) {

            if (WindowSystem.instance.loaderScreen != null && WindowSystem.instance.loaderInstance == null && WindowSystem.instance.loaderShowBegin == false) {

                WindowSystem.instance.loaderShowBegin = true;
                var w = WindowSystem.ShowSync(WindowSystem.instance.loaderScreen, new InitialParameters() { showSync = showSync },
                                  onInitialized: static (w) => {
                                      WindowSystem.instance.loaderInstance = w;
                                      WindowSystem.instance.loaderShowBegin = false;
                                  }, transitionParameters);
                WindowSystem.GetEvents().RegisterOnce(w, WindowEvent.OnHideBegin, static (_) => {
                    WindowSystem.instance.loaderInstance = null;
                    WindowSystem.instance.loaderShowBegin = false;
                });

            }

        }

        public static void HideLoader() {

            if (WindowSystem.instance.loaderShowBegin == true && WindowSystem.instance.loaderInstance == null) {
                Coroutines.Wait(static () => WindowSystem.instance.loaderInstance != null, WindowSystem.HideLoader);
            } else {
                if (WindowSystem.instance.loaderInstance != null) {
                    WindowSystem.instance.loaderInstance.Hide();
                    WindowSystem.instance.loaderInstance = null;
                    WindowSystem.instance.loaderShowBegin = false;
                }
            }
            
        }

        public static void ShowRoot(TransitionParameters transitionParameters = default) {
            
            WindowSystem.Show(WindowSystem.instance.rootScreen, transitionParameters: transitionParameters);
            
        }
        
        public static void HideAll<T>(TransitionParameters parameters = default) where T : WindowBase {

            WindowSystem.HideAll_INTERNAL((x) => x is T, parameters);

        }

        public static void HideAll(TransitionParameters parameters = default) {

            WindowSystem.HideAll_INTERNAL(null, parameters);

        }

        public static void HideAll(System.Predicate<WindowBase> predicate, TransitionParameters parameters = default) {

            WindowSystem.HideAll_INTERNAL(predicate, parameters);

        }

        public static void HideAllAndClean<T>(TransitionParameters parameters = default) where T : WindowBase {
            
            WindowSystem.HideAllAndClean((w) => w is T, parameters);
            
        }

        public static void HideAllAndClean(TransitionParameters parameters = default) {
            
            WindowSystem.HideAllAndClean(null, parameters);
            
        }

        public static void HideAllAndClean(System.Predicate<WindowBase> predicate, TransitionParameters parameters = default) {

            var list = PoolList<WindowBase>.Spawn();
            var cb = parameters.ReplaceCallback(() => {

                foreach (var item in list) {
                    WindowSystem.Clean(item);
                }
                parameters.RaiseCallback();
                PoolList<WindowBase>.Recycle(ref list);
                
            });
            WindowSystem.HideAll_INTERNAL(predicate, cb, (w) => {

                list.Add(w);

            });

        }

        private static bool CanBeDestroy(DontDestroy state, DontDestroy windowInstanceFlag) {

            return windowInstanceFlag == DontDestroy.Default || (state & windowInstanceFlag) == 0;

        }
        
        private static void HideAll_INTERNAL(System.Predicate<WindowBase> predicate, TransitionParameters parameters, System.Action<WindowBase> onWindow = null) {

            var currentList = WindowSystem.instance.currentWindows;
            var count = currentList.Count;
            var filteredCount = 0;
            for (int i = 0; i < count; ++i) {
                
                var instance = currentList[i].instance;
                if ((predicate == null || predicate.Invoke(instance) == true) && WindowSystem.CanBeDestroy(DontDestroy.OnHideAll, instance.preferences.dontDestroy) == true) ++filteredCount;

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
            for (int i = count - 1; i >= 0; --i) {

                var instance = currentList[i].instance;
                if ((predicate == null || predicate.Invoke(instance) == true) && WindowSystem.CanBeDestroy(DontDestroy.OnHideAll, instance.preferences.dontDestroy) == true) {

                    if (instance.GetState() == ObjectState.Hiding ||
                        instance.GetState() == ObjectState.Hidden) {
                        
                        instanceParameters.RaiseCallback();
                        continue;
                        
                    }
                    
                    instance.BreakStateHierarchy();
                    onWindow?.Invoke(instance);
                    instance.Hide(instanceParameters);
                    
                }
                
            }

        }
        
        public static T ShowSync<T>(T source, InitialParameters initialParameters, System.Action<T> onInitialized = null, TransitionParameters transitionParameters = default) where T : WindowBase {

            T instance = default;
            initialParameters.showSync = true;
            WindowSystem.instance.Show_INTERNAL<T>(source, initialParameters, (w) => {
                
                instance = w;
                onInitialized?.Invoke(w);
                
            }, transitionParameters);
            return instance;

        }
        
        /// <summary>
        /// Initializing window in sync mode.
        /// Just returns instance immediately, but still stay in async mode for layout because of Addressable assets.
        /// </summary>
        /// <param name="initialParameters"></param>
        /// <param name="onInitialized"></param>
        /// <param name="transitionParameters"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ShowSync<T>(InitialParameters initialParameters, System.Action<T> onInitialized = null, TransitionParameters transitionParameters = default) where T : WindowBase {

            return WindowSystem.ShowSync(WindowSystem.instance.GetSource<T>(), initialParameters, onInitialized, transitionParameters);

        }

        public static T ShowSync<T>(System.Action<T> onInitialized = null, TransitionParameters transitionParameters = default) where T : WindowBase {

            return WindowSystem.ShowSync(default, onInitialized, transitionParameters);

        }

        public static void Show<T>(System.Action<T> onInitialized = null, TransitionParameters transitionParameters = default) where T : WindowBase {

            WindowSystem.instance.Show_INTERNAL(new InitialParameters(), onInitialized, transitionParameters);

        }

        public static void Show(WindowBase source, System.Action<WindowBase> onInitialized = null, TransitionParameters transitionParameters = default) {

            WindowSystem.instance.Show_INTERNAL(source, new InitialParameters(), onInitialized, transitionParameters);

        }

        public static void Show<T>(WindowBase source, System.Action<T> onInitialized = null, TransitionParameters transitionParameters = default) where T : WindowBase {

            WindowSystem.instance.Show_INTERNAL(source, new InitialParameters(), onInitialized, transitionParameters);

        }

        public static void Show<T>(InitialParameters initialParameters, System.Action<T> onInitialized = null, TransitionParameters transitionParameters = default) where T : WindowBase {

            WindowSystem.instance.Show_INTERNAL(initialParameters, onInitialized, transitionParameters);

        }

        public static void Show(WindowBase source, InitialParameters initialParameters, System.Action<WindowBase> onInitialized = null, TransitionParameters transitionParameters = default) {

            WindowSystem.instance.Show_INTERNAL(source, initialParameters, onInitialized, transitionParameters);

        }

        public static void Show<T>(WindowBase source, InitialParameters initialParameters, System.Action<T> onInitialized = null, TransitionParameters transitionParameters = default) where T : WindowBase {

            WindowSystem.instance.Show_INTERNAL(source, initialParameters, onInitialized, transitionParameters);

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

                        this.TryAddTopWindow(this.currentWindows[i].instance.preferences.layer, this.currentWindows[i].instance);

                    }

                }

            }

        }

        public static WindowBase GetSource(System.Type type) {

            return WindowSystem.instance.hashToPrefabs.GetValueOrDefault(type);

        }

        private T GetSource<T>() where T : WindowBase {

            var hash = typeof(T);
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
        
        private void Show_INTERNAL<T>(InitialParameters initialParameters, System.Action<T> onInitialized = null, TransitionParameters transitionParameters = default) where T : WindowBase {

            var source = this.GetSource<T>();
            this.Show_INTERNAL(source, initialParameters, onInitialized, transitionParameters);

        }

        private void Show_INTERNAL<T>(WindowBase source, InitialParameters initialParameters, System.Action<T> onInitialized, TransitionParameters transitionParameters) where T : WindowBase {

            if (source == null) {

                throw new System.Exception("Window Source is null, did you forget to collect your screens?");

            }

            if (source.preferences.forceSyncLoad == true && initialParameters.showSync == false) {

                initialParameters.showSync = true;

            }
            
            WindowBase instance;
            var singleInstance = source.preferences.singleInstance;
            if (initialParameters.overrideSingleInstance == true) {

                singleInstance = initialParameters.singleInstance;

            }

            if (singleInstance == true) {

                if (this.IsOpenedBySource_INTERNAL(source, out instance, maximumState: ObjectState.Shown) == true) {

                    if (onInitialized != null) {
                        
                        onInitialized.Invoke((T)instance);
                        
                    } else {
                        
                        instance.OnEmptyPass();
                        
                    }
                    return;

                }

            }

            if (source.createPool == true) this.pools.CreatePool(source);
            instance = this.pools.Spawn(source, null, out var fromPool);
            instance.identifier = ++this.nextWindowId;
            instance.windowSourceId = source.GetType().GetHashCode();
            instance.windowId = instance.identifier;
            #if UNITY_EDITOR
            instance.name = $"[{instance.identifier:00}] {source.name}";
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

                    WindowSystem.RegisterActionOnce(existInstance, state, (obj) => { this.Show_INTERNAL(existInstance, initialParameters, onInitialized, transitionParameters); });

                    return;

                }

            }

            var item = new WindowItem() {
                prefab = source,
                instance = instance,
            };

            if (instance.preferences.addInHistory == true) {
                
                this.breadcrumbs.Add(item);
                
            }
            this.currentWindows.Add(item);

            GameObject.DontDestroyOnLoad(instance.gameObject);

            { // Setup for each instance

                instance.Setup(instance);
                this.TryAddTopWindow(instance.preferences.layer, instance);
                if (this.windowsCountByLayer.TryGetValue(instance.preferences.layer.value, out var count) == true) {

                    this.windowsCountByLayer[instance.preferences.layer.value] = count + 1;

                } else {

                    this.windowsCountByLayer.Add(instance.preferences.layer.value, 1);

                }

            }

            if (initialParameters.showSync == true) {

                if (onInitialized != null) {
                    
                    onInitialized.Invoke((T)instance);
                    
                } else {
                    
                    instance.OnEmptyPass();
                    
                }

            }

            instance.LoadAsync(initialParameters, () => {
            
                if (initialParameters.showSync == false) {

                    if (onInitialized != null) {

                        onInitialized.Invoke((T)instance);

                    } else {

                        instance.OnEmptyPass();

                    }

                }

                var tr = transitionParameters.ReplaceCallback(() => {

                    Coroutines.Run(WindowSystem.WaitForLayoutBuildComplete(instance));
                    transitionParameters.RaiseCallback();
                    
                });
                
                instance.DoInit(() => instance.ShowInternal(tr));

            });

        }

        private static System.Collections.IEnumerator WaitForLayoutBuildComplete(WindowBase instance) {
            
            yield return new WaitForEndOfFrame();
            instance.DoLayoutReady();
            
        }

    }

}
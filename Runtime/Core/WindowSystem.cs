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

    public enum Algorithm {

        /// <summary>
        /// Returns next component derived from type T or of type T
        /// </summary>
        GetNextTypeAny,
        /// <summary>
        /// Returns next component strongly of type T
        /// </summary>
        GetNextTypeStrong,
        /// <summary>
        /// Returns first component derived from type T or of type T
        /// </summary>
        GetFirstTypeAny,
        /// <summary>
        /// Returns first component strongly of type T
        /// </summary>
        GetFirstTypeStrong,

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

    public enum WindowEvent {

        None = 0,

        OnInitializing,
        OnInitialized,
        OnDeInitializing,
        OnDeInitialized,

        OnShowBegin,
        OnShowEnd,

        OnHideBegin,
        OnHideEnd,

        OnFocusTook,
        OnFocusLost,
        
        OnLayoutReady,

    }

    public class RTLModeChangedEvent {
        public bool state;
    }
    
    [DefaultExecutionOrder(-1000)]
    public partial class WindowSystem : MonoBehaviour {

        private const float CLICK_THRESHOLD = 0.125f;
        [System.Serializable]
        public struct WindowItem {

            public WindowBase prefab;
            public WindowBase instance;

        }

        public static event System.Action onPointerUp;
        public static event System.Action onPointerDown;

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

        private readonly List<WindowItem> currentWindows = new List<WindowItem>();
        private readonly Dictionary<int, int> windowsCountByLayer = new Dictionary<int, int>();
        private readonly Dictionary<int, WindowBase> topWindowsByLayer = new Dictionary<int, WindowBase>();
        private readonly Dictionary<System.Type, WindowBase> hashToPrefabs = new Dictionary<System.Type, WindowBase>();

        private bool loaderShowBegin;
        private WindowBase loaderInstance;
        private int nextWindowId;
        private bool useRTL;

        internal static WindowSystem _instance;

        internal static WindowSystem instance {
            get {

                #if UNITY_EDITOR
                if (WindowSystem._instance == null && Application.isPlaying == false) {

                    WindowSystem._instance = Object.FindFirstObjectByType<WindowSystem>();

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

        public static RuntimePlatform GetCurrentRuntimePlatform() {

            if (WindowSystem.instance.emulatePlatform == true) {

                return WindowSystem.instance.emulateRuntimePlatform;

            }

            return Application.platform;

        }

        internal static TargetData GetTargetData() {

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

            var temp = PoolList<WindowItem>.Spawn();
            temp.AddRange(this.currentWindows);
            temp.Sort(static (item1, item2) => {
                var d1 = item1.instance.GetDepth();
                var d2 = item2.instance.GetDepth();
                return d1.CompareTo(d2);
            });
            foreach (var item in temp) {

                var instance = item.instance;
                if (instance.IsVisible() == false) continue;
                if (instance.preferences.takeFocus == false) continue;

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
            PoolList<WindowItem>.Recycle(temp);

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

            return WindowSystem.instance.windowsCountByLayer.GetValueOrDefault(layer.value, 0);

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

        internal static void ShowInstance(WindowObject instance, TransitionParameters parameters, bool internalCall = false) {
            
            if (instance.GetState() == ObjectState.Showing || instance.GetState() == ObjectState.Shown) {
                
                parameters.RaiseCallback();
                return;
                
            }

            instance.SetState(ObjectState.Showing);

            {

                WindowSystem.TryAddUpdateListener(instance);
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

                    if (instance.showBehaviour == ShowBehaviour.OneByOne && instance.showBehaviourOneByOneDelay > 0f) {
                        Coroutines.CallInSequence(
                            ref closure,
                            static (ref ShowHideClosureParametersClass p) => {
                                p.hierarchyComplete = true;
                                if (p.animationComplete == true && p.baseComplete == true) {
                                    var pars = p.parameters;
                                    p.Dispose();
                                    pars.RaiseCallback();
                                }
                            },
                            instance.subObjects,
                            static (WindowObject obj, Coroutines.ClosureDelegateCallback<ShowHideClosureParametersClass> cb, ref ShowHideClosureParametersClass p) => {

                                var closure = PoolClass<ShowHideInstanceInternalClosure>.Spawn();
                                closure.cb = cb;
                                closure.data = p;

                                var parameters = p.parameters.ReplaceCallback(
                                    closure,
                                    static (obj) => {
                                        var d = (ShowHideInstanceInternalClosure)obj;
                                        d.cb.Invoke(ref d.data);
                                        PoolClass<ShowHideInstanceInternalClosure>.Recycle(d);
                                    });
                                if (p.parameters.data.replaceDelay == true) {
                                    parameters = parameters.ReplaceDelay(p.parameters.data.delay);
                                } else {
                                    parameters = parameters.ReplaceDelay(p.instance.showBehaviourOneByOneDelay * p.index);
                                }

                                if (p.internalCall == true) {
                                    obj.ShowInternal(parameters);
                                } else {
                                    obj.Show(parameters);
                                }

                            }, waitPrevious: false);
                    } else {
                        Coroutines.CallInSequence(ref closure,
                                                  static (ref ShowHideClosureParametersClass p) => {
                                                      p.hierarchyComplete = true;
                                                      if (p.animationComplete == true && p.baseComplete == true) {
                                                          var pars = p.parameters;
                                                          p.Dispose();
                                                          pars.RaiseCallback();
                                                      }
                                                  }, instance.subObjects, static (WindowObject obj, Coroutines.ClosureDelegateCallback<ShowHideClosureParametersClass> cb, ref ShowHideClosureParametersClass p) => {

                                                      var closure = PoolClass<ShowHideInstanceInternalClosure>.Spawn();
                                                      closure.cb = cb;
                                                      closure.data = p;

                                                      var parameters = p.parameters.ReplaceCallback(
                                                          closure,
                                                          static (obj) => {
                                                              var d = (ShowHideInstanceInternalClosure)obj;
                                                              d.cb.Invoke(ref d.data);
                                                              PoolClass<ShowHideInstanceInternalClosure>.Recycle(d);
                                                          });
                                                      if (p.parameters.data.replaceDelay == true) {
                                                          parameters = parameters.ReplaceDelay(p.parameters.data.delay);
                                                      }

                                                      if (p.internalCall == true) {
                                                          obj.ShowInternal(parameters);
                                                      } else {
                                                          obj.Show(parameters);
                                                      }

                                                  }, waitPrevious: instance.showBehaviour == ShowBehaviour.OneByOne);
                    }

                } else {
                    
                    instance.BreakState();
                    closure.hierarchyComplete = true;
                    
                }
                
                WindowObjectAnimation.Show(closure, instance, parameters, static (cParams) => {
                    
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

        public static void HideInstance(WindowObject instance, TransitionParameters parameters, bool internalCall = false) {

            if (instance.GetState() <= ObjectState.Initializing) {
                
                Debug.LogWarning($"Object is out of state: {instance}", instance);
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
            
            Coroutines.Wait(closureInstance, static (inst) => inst.instance.IsReadyToHide(), static (inst) => {

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

                    if (inst.parameters.data.replaceAffectChilds == false ||
                        inst.parameters.data.affectChilds == true) {

                        var waitForNext = ((inst.instance.hideBehaviour & HideBehaviour.OneByOne) != 0 && inst.instance.hideBehaviourOneByOneDelay <= 0f);
                        
                        if ((inst.parameters.data.hideBehaviour & HideBehaviour.WaitForChild) != 0) {
                            
                            // hide sub objects first
                            Coroutines.CallInSequence(ref closure, static (ref ShowHideClosureParametersClass p) => {

                                p.hierarchyComplete = true;

                                WindowObjectAnimation.Hide(p, p.instance, p.parameters, static (cParams) => {
                                
                                    var pars = cParams.parameters;
                                    cParams.Dispose();
                                    pars.RaiseCallback();

                                });

                            }, inst.instance.subObjects, static (WindowObject obj, Coroutines.ClosureDelegateCallback<ShowHideClosureParametersClass> cb,
                                                                 ref ShowHideClosureParametersClass p) => {

                                var closure = PoolClass<ShowHideInstanceInternalClosure>.Spawn();
                                closure.cb = cb;
                                closure.data = p;

                                var parameters = p.parameters.ReplaceCallback(closure, static (obj) => {
                                    var d = (ShowHideInstanceInternalClosure)obj;
                                    d.cb.Invoke(ref d.data);
                                    PoolClass<ShowHideInstanceInternalClosure>.Recycle(d);
                                });
                                if (p.parameters.data.replaceDelay == true) {
                                    parameters = parameters.ReplaceDelay(p.parameters.data.delay);
                                } else if ((p.instance.hideBehaviour & HideBehaviour.OneByOne) != 0 && p.instance.hideBehaviourOneByOneDelay > 0f) { 
                                    parameters = parameters.ReplaceDelay(p.instance.hideBehaviourOneByOneDelay * p.index);
                                }

                                if (p.internalCall == true) {
                                    obj.HideInternal(parameters);
                                } else {
                                    obj.Hide(parameters);
                                }

                            }, waitPrevious: waitForNext);

                        } else {
                            
                            // do not wait current object - start hiding all subobjects
                            Coroutines.CallInSequence(ref closure, static (ref ShowHideClosureParametersClass p) => {

                                p.hierarchyComplete = true;
                                if (p.animationComplete == true) {

                                    var pars = p.parameters;
                                    p.Dispose();
                                    pars.RaiseCallback();

                                }
                                
                            }, inst.instance.subObjects, static (WindowObject obj, Coroutines.ClosureDelegateCallback<ShowHideClosureParametersClass> cb,
                                                                 ref ShowHideClosureParametersClass p) => {

                                var closure = PoolClass<ShowHideInstanceInternalClosure>.Spawn();
                                closure.cb = cb;
                                closure.data = p;

                                var parameters = p.parameters.ReplaceCallback(closure, static (obj) => {
                                    var d = (ShowHideInstanceInternalClosure)obj;
                                    d.cb.Invoke(ref d.data);
                                    PoolClass<ShowHideInstanceInternalClosure>.Recycle(d);
                                });
                                if (p.parameters.data.replaceDelay == true) {
                                    parameters = parameters.ReplaceDelay(p.parameters.data.delay);
                                } else if ((p.instance.hideBehaviour & HideBehaviour.OneByOne) != 0 && p.instance.hideBehaviourOneByOneDelay > 0f) { 
                                    parameters = parameters.ReplaceDelay(p.instance.hideBehaviourOneByOneDelay * p.index);
                                }

                                if (p.internalCall == true) {
                                    obj.HideInternal(parameters);
                                } else {
                                    obj.Hide(parameters);
                                }

                            }, waitPrevious: waitForNext);
                            
                            WindowObjectAnimation.Hide(closure, closure.instance, closure.parameters, static (cParams) => {
                                
                                cParams.animationComplete = true;
                                if (cParams.hierarchyComplete == true) {

                                    var pars = cParams.parameters;
                                    cParams.Dispose();
                                    pars.RaiseCallback();

                                }
                                
                            });
                            
                        }
                        
                    } else {

                        // hide current object only
                        WindowObjectAnimation.Hide(closure, inst.instance, inst.parameters, static (cParams) => {
                            
                            cParams.hierarchyComplete = true;
                            {

                                var pars = cParams.parameters;
                                cParams.Dispose();
                                pars.RaiseCallback();

                            }
                            
                        });
                        
                    }

                }

            });

        }

        private static bool CanBeDestroy(DontDestroy state, DontDestroy windowInstanceFlag) {

            return windowInstanceFlag == DontDestroy.Default || (state & windowInstanceFlag) == 0;

        }

        private static void HideAll_INTERNAL<TState>(TState state, System.Func<WindowBase, TState, bool> predicate, TransitionParameters parameters, System.Action<WindowBase, TState> onWindow = null) {

            var currentList = WindowSystem.instance.currentWindows;
            var count = currentList.Count;
            var filteredCount = 0;
            for (int i = 0; i < count; ++i) {
                
                var instance = currentList[i].instance;
                if ((predicate == null || predicate.Invoke(instance, state) == true) && WindowSystem.CanBeDestroy(DontDestroy.OnHideAll, instance.preferences.dontDestroy) == true) ++filteredCount;

            }

            if (filteredCount == 0) {

                parameters.RaiseCallback();
                return;

            }
            
            var closure = PoolClass<HideAllClosure>.Spawn();
            closure.parameters = parameters;
            closure.ptr = 0;
            closure.filteredCount = filteredCount;
            var instanceParameters = parameters.ReplaceCallback(closure, static (closure) => {
                
                var data = (HideAllClosure)closure;
                ++data.ptr;
                if (data.ptr != data.filteredCount) return;

                data.parameters.RaiseCallback();
                PoolClass<HideAllClosure>.Recycle(data);


            });
            for (int i = count - 1; i >= 0; --i) {

                var instance = currentList[i].instance;
                if ((predicate == null || predicate.Invoke(instance, state) == true) && WindowSystem.CanBeDestroy(DontDestroy.OnHideAll, instance.preferences.dontDestroy) == true) {

                    if (instance.GetState() == ObjectState.Hiding ||
                        instance.GetState() == ObjectState.Hidden) {
                        
                        instanceParameters.RaiseCallback();
                        continue;
                        
                    }
                    
                    instance.BreakStateHierarchy();
                    onWindow?.Invoke(instance, state);
                    instance.Hide(instanceParameters);
                    
                }
                
            }

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

        private T GetSource<T>() where T : WindowBase {

            var hash = typeof(T);
            if (this.hashToPrefabs.TryGetValue(hash, out var prefab) == true) {

                return (T)prefab;

            }

            return default;

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

        private WindowHandler<T> Show_INTERNAL<T>(InitialParameters initialParameters, System.Action<T> onInitialized = null, TransitionParameters transitionParameters = default) where T : WindowBase {
            var source = this.GetSource<T>();
            return this.Show_INTERNAL<T, System.Action<T>>(onInitialized, source, initialParameters, static (instance, state) => {
                state?.Invoke(instance);
            }, transitionParameters);
        }

        private WindowHandler<T> Show_INTERNAL<T, TState>(TState state, InitialParameters initialParameters, System.Action<T, TState> onInitialized = null, TransitionParameters transitionParameters = default) where T : WindowBase {
            var source = this.GetSource<T>();
            return this.Show_INTERNAL(state, source, initialParameters, onInitialized, transitionParameters);
        }

        private WindowHandler<T> Show_INTERNAL<T, TState>(TState closure, WindowBase source, InitialParameters initialParameters, System.Action<T, TState> onInitialized, TransitionParameters transitionParameters) where T : WindowBase {

            if (source == null) {
                throw new System.Exception("Window Source is null, did you forget to collect your screens?");
            }

            #if UNITY_WEBGL
            if (initialParameters.showSync == true) {
                Debug.LogWarning("[ UIWS ] ShowSync ignored because WebGL doesn't support synchronous loading.");
                initialParameters.showSync = false;
            }
            #else
            if (source.preferences.forceSyncLoad == true && initialParameters.showSync == false) {
                initialParameters.showSync = true;
            }
            #endif
            
            WindowBase instance;
            var singleInstance = source.preferences.singleInstance;
            if (initialParameters.overrideSingleInstance == true) {
                singleInstance = initialParameters.singleInstance;
            }

            if (singleInstance == true) {
                if (this.IsOpenedBySource_INTERNAL(source, out instance, maximumState: ObjectState.Shown) == true) {
                    if (onInitialized != null) {
                        onInitialized.Invoke((T)instance, closure);
                    } else {
                        instance.OnEmptyPass();
                    }
                    return WindowHandler<T>.Create((T)instance);
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

                    WindowSystem.RegisterActionOnce(new ShowInstanceClosure<T, TState>() {
                        instance = this,
                        existInstance = existInstance,
                        initialParameters = initialParameters,
                        onInitialized = onInitialized,
                        state = closure,
                        transitionParameters = transitionParameters,
                    }, existInstance, state, static (obj, state) => {
                        state.instance.Show_INTERNAL(state.state, state.existInstance, state.initialParameters, state.onInitialized, state.transitionParameters);
                    });

                    return WindowHandler<T>.Create((T)instance);

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
                    onInitialized.Invoke((T)instance, closure);
                } else {
                    instance.OnEmptyPass();
                }
            }

            instance.LoadAsync(new ShowLoadAsyncClosure<TState>() {
                instance = instance,
                onInitialized = onInitialized,
                closure = closure,
                transitionParameters = transitionParameters,
                initialParameters = initialParameters,
            }, initialParameters, static (state) => {
            
                if (state.initialParameters.showSync == false) {
                    if (state.onInitialized != null) {
                        ((System.Action<T, TState>)state.onInitialized).Invoke((T)state.instance, state.closure);
                    } else {
                        state.instance.OnEmptyPass();
                    }
                }

                var closure = PoolClass<WindowObjectClosure>.Spawn();
                closure.instance = state.instance;
                closure.tr = state.transitionParameters;
                var tr = state.transitionParameters.ReplaceCallback(closure, static (data) => {

                    var closure = (WindowObjectClosure)data;
                    var obj = closure.instance;
                    Coroutines.WaitEndOfFrame(obj, static (obj) => {
                        obj.DoLayoutReady();
                    });
                    closure.tr.RaiseCallback();
                    closure.Dispose();
                    PoolClass<WindowObjectClosure>.Recycle(closure);
                    
                });
                
                state.instance.DoInit(new DoInitClosure() {
                    component = state.instance,
                    parameters = tr,
                }, static (data) => data.component.ShowInternal(data.parameters));

            });

            return WindowHandler<T>.Create((T)instance);

        }

    }
    
}

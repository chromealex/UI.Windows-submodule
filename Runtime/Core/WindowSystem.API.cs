using System.Collections.Generic;

namespace UnityEngine.UI.Windows {

    public struct WindowHandler {

        private WindowBase instance;

        public WindowBase screen => this.instance;

        public T GetWindow<T>() where T : WindowBase {
            return this.instance as T;
        }

        public void Hide() {
            this.instance.Hide();
        }

        public void Hide(TransitionParameters transitionParameters) {
            this.instance.Hide(transitionParameters);
        }

        public ObjectState GetState() => this.instance.GetState();
        public FocusState GetFocusState() => this.instance.GetFocusState();

        public static implicit operator WindowHandler(WindowBase window) {
            return new WindowHandler() { instance = window };
        }

    }

    public struct WindowHandler<T> where T : WindowBase {

        private T instance;

        internal static WindowHandler<T> Create(T instance) {
            return new WindowHandler<T>() {
                instance = instance,
            };
        }
        
        public T screen => this.instance;

        public void Hide() {
            this.instance.Hide();
        }

        public void Hide(TransitionParameters transitionParameters) {
            this.instance.Hide(transitionParameters);
        }

        public ObjectState GetState() => this.instance.GetState();
        public FocusState GetFocusState() => this.instance.GetFocusState();

        public static implicit operator WindowBase(WindowHandler<T> handler) {
            return handler.instance;
        }

    }
    
    public partial class WindowSystem {

        public struct ClosureAPI<TClosure> {

            public TClosure data;

            public class TaskCompletionShowScreen<T> {

                public System.Action<T, TClosure> onInitialized;
                public TClosure data;
                public T instance;

            }

            public T ShowSync<T>(System.Action<T, TClosure> onInitialized = null, TransitionParameters transitionParameters = default) where T : WindowBase {
                return this.ShowSync(new InitialParameters(), onInitialized, transitionParameters);
            }

            public T ShowSync<T>(InitialParameters initialParameters, System.Action<T, TClosure> onInitialized = null, TransitionParameters transitionParameters = default) where T : WindowBase {

                initialParameters.showSync = true;
                var taskCompletionData = PoolClass<TaskCompletionShowScreen<T>>.Spawn();
                taskCompletionData.onInitialized = onInitialized;
                taskCompletionData.data = this.data;
                WindowSystem.Show<T, TaskCompletionShowScreen<T>>(taskCompletionData, initialParameters, static (instance, state) => {
                    state.instance = instance;
                    state.onInitialized?.Invoke(instance, state.data);
                }, transitionParameters);

                var result = taskCompletionData.instance;
                PoolClass<TaskCompletionShowScreen<T>>.Recycle(taskCompletionData);
                return result;
                
            }

            public WindowHandler<T> Show<T>(System.Action<T, TClosure> onInitialized = null, TransitionParameters transitionParameters = default) where T : WindowBase {
                return WindowSystem.instance.Show_INTERNAL(this.data, new InitialParameters(), onInitialized, transitionParameters);
            }

            public WindowHandler<T> Show<T>(InitialParameters initialParameters, System.Action<T, TClosure> onInitialized = null, TransitionParameters transitionParameters = default) where T : WindowBase {
                return WindowSystem.instance.Show_INTERNAL(this.data, initialParameters, onInitialized, transitionParameters);
            }
            
            public WindowHandler<WindowBase> Show(WindowBase source, System.Action<WindowBase, TClosure> onInitialized = null, TransitionParameters transitionParameters = default) {
                return WindowSystem.instance.Show_INTERNAL(this.data, source, default, onInitialized, transitionParameters);
            }
            
            private WindowHandler<T> Show_INTERNAL<T>(InitialParameters initialParameters, System.Action<T, TClosure> onInitialized = null, TransitionParameters transitionParameters = default) where T : WindowBase {
                return WindowSystem.instance.Show_INTERNAL(this.data, initialParameters, onInitialized, transitionParameters);
            }

        }
        
        public static ClosureAPI<T> Closure<T>(T closure) {
            return new ClosureAPI<T>() {
                data = closure,
            };
        }

        public static T ShowSync<T>(T source, InitialParameters initialParameters, System.Action<T> onInitialized = null, TransitionParameters transitionParameters = default) where T : WindowBase {

            initialParameters.showSync = true;
            var taskCompletionData = PoolClass<TaskCompletionShowScreen<T>>.Spawn();
            taskCompletionData.onInitialized = onInitialized;
            WindowSystem.instance.Show_INTERNAL<T, TaskCompletionShowScreen<T>>(taskCompletionData, source, initialParameters, static (instance, state) => {
                state.instance = instance;
                state.onInitialized?.Invoke(instance);
            }, transitionParameters);
            
            var result = taskCompletionData.instance;
            PoolClass<TaskCompletionShowScreen<T>>.Recycle(taskCompletionData);
            return result;

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

        public static WindowHandler<T> Show<T, TState>(TState state, System.Action<T, TState> onInitialized = null, TransitionParameters transitionParameters = default) where T : WindowBase {
            return WindowSystem.instance.Show_INTERNAL(state, new InitialParameters(), onInitialized, transitionParameters);
        }

        public static WindowHandler<T> Show<T, TState>(TState state, InitialParameters initialParameters, System.Action<T, TState> onInitialized = null, TransitionParameters transitionParameters = default) where T : WindowBase {
            return WindowSystem.instance.Show_INTERNAL(state, initialParameters, onInitialized, transitionParameters);
        }

        public static WindowHandler<T> Show<T>(System.Action<T> onInitialized = null, TransitionParameters transitionParameters = default) where T : WindowBase {
            return WindowSystem.instance.Show_INTERNAL(new InitialParameters(), onInitialized, transitionParameters);
        }

        public static WindowHandler<WindowBase> Show(WindowBase source, System.Action<WindowBase> onInitialized = null, TransitionParameters transitionParameters = default) {
            return WindowSystem.instance.Show_INTERNAL<WindowBase, System.Action<WindowBase>>(onInitialized, source, default, static (instance, state) => {
                state?.Invoke(instance);
            }, transitionParameters);
        }

        public static WindowHandler<T> Show<T>(WindowBase source, System.Action<T> onInitialized = null, TransitionParameters transitionParameters = default) where T : WindowBase {
            return WindowSystem.instance.Show_INTERNAL<T, System.Action<T>>(onInitialized, source, default, static (instance, state) => {
                state?.Invoke(instance);
            }, transitionParameters);
        }

        public static WindowHandler<T> Show<T>(InitialParameters initialParameters, System.Action<T> onInitialized = null, TransitionParameters transitionParameters = default) where T : WindowBase {
            return WindowSystem.instance.Show_INTERNAL(initialParameters, onInitialized, transitionParameters);
        }

        public static WindowHandler<WindowBase> Show(WindowBase source, InitialParameters initialParameters, System.Action<WindowBase> onInitialized = null, TransitionParameters transitionParameters = default) {
            return WindowSystem.instance.Show_INTERNAL<WindowBase, System.Action<WindowBase>>(onInitialized, source, initialParameters, static (instance, state) => {
                state?.Invoke(instance);
            }, transitionParameters);
        }

        public static WindowHandler<T> Show<T>(WindowBase source, InitialParameters initialParameters, System.Action<T> onInitialized = null, TransitionParameters transitionParameters = default) where T : WindowBase {
            return WindowSystem.instance.Show_INTERNAL<T, System.Action<T>>(onInitialized, source, initialParameters, static (instance, state) => {
                state?.Invoke(instance);
            }, transitionParameters);
        }

        public static void RemoveWindow(WindowBase instance) {

            var uiws = WindowSystem.instance;
            if (uiws.windowsCountByLayer.TryGetValue(instance.preferences.layer.value, out var count) == true) {

                uiws.windowsCountByLayer[instance.preferences.layer.value] = count - 1;

            }

            uiws.RemoveWindow_INTERNAL(instance);

        }

        public static WindowBase GetSource(System.Type type) {

            return WindowSystem.instance.hashToPrefabs.GetValueOrDefault(type);

        }

        public static bool IsOpenedBySource(WindowBase source) {

            return WindowSystem.instance.IsOpenedBySource_INTERNAL(source);

        }

        public static WindowItem GetPreviousWindow(WindowBase instance) {

            var breadcrumbs = WindowSystem.GetBreadcrumbs();
            return breadcrumbs.GetPrevious(instance, true);

        }

        public static void ShowRoot(TransitionParameters transitionParameters = default) {
            
            WindowSystem.Show(WindowSystem.instance.rootScreen, transitionParameters: transitionParameters);
            
        }
        
        public static void HideAll<T>(TransitionParameters parameters = default) where T : WindowBase {

            WindowSystem.HideAll_INTERNAL(false, static (x, _) => x is T, parameters);

        }

        public static void HideAll(TransitionParameters parameters = default) {

            WindowSystem.HideAll_INTERNAL(false, null, parameters);

        }

        public static void HideAll(System.Predicate<WindowBase> predicate, TransitionParameters parameters = default) {

            WindowSystem.HideAll_INTERNAL(predicate, static (w, s) => s.Invoke(w), parameters);

        }

        public static void HideAll<TState>(TState state, System.Func<WindowBase, TState, bool> predicate, TransitionParameters parameters = default) {

            WindowSystem.HideAll_INTERNAL((state, predicate), static (w, s) => s.predicate.Invoke(w, s.state), parameters);

        }

        public static void HideAll<T, TState>(TState state, System.Func<T, TState, bool> predicate, TransitionParameters parameters = default) where T : WindowBase {

            WindowSystem.HideAll_INTERNAL((state, predicate), static (w, s) => w is T type && s.predicate.Invoke(type, s.state), parameters);

        }

        public static void HideAllAndClean<T>(TransitionParameters parameters = default) where T : WindowBase {
            
            WindowSystem.HideAllAndClean(static (w) => w is T, parameters);
            
        }

        public static void HideAllAndClean(TransitionParameters parameters = default) {
            
            WindowSystem.HideAllAndClean(null, parameters);
            
        }

        public static void HideAllAndClean(System.Predicate<WindowBase> predicate, TransitionParameters parameters = default) {
            
            HideAllAndClean(predicate, static (w, s) => s.Invoke(w), parameters);
            
        }

        public static void HideAllAndClean<TState>(TState state, System.Func<WindowBase, TState, bool> predicate, TransitionParameters parameters = default) {

            var list = PoolList<WindowBase>.Spawn();
            var closure = PoolClass<HideAllAndCleanClosure<TState>>.Spawn();
            closure.list = list;
            closure.transitionParameters = parameters;
            closure.predicate = predicate;
            closure.state = state;
            var cb = parameters.ReplaceCallback(closure, static (data) => {

                var closure = (HideAllAndCleanClosure<TState>)data;
                
                foreach (var item in closure.list) {
                    WindowSystem.Clean(item);
                }
                closure.transitionParameters.RaiseCallback();
                PoolList<WindowBase>.Recycle(ref closure.list);
                PoolClass<HideAllAndCleanClosure<TState>>.Recycle(closure);
                
            });
            WindowSystem.HideAll_INTERNAL(closure, static (w, closure) => closure.predicate.Invoke(w, closure.state), cb, static (w, closure) => {

                closure.list.Add(w);

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

        public static List<WindowItem> GetCurrentOpened() {

            return WindowSystem.instance.currentWindows;

        }

        public static T FindComponent<T>(System.Func<T, bool> filter = null) where T : WindowComponent {

            foreach (var window in WindowSystem.instance.currentWindows) {

                if (window.instance == null) continue;
                
                var component = window.instance.FindComponent(filter);
                if (component != null) return component;

            }

            return null;

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

    }

}
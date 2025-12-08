namespace UnityEngine.UI.Windows {

    using Modules;
    using UnityEngine.UI.Windows.Utilities;
    
    public class InitLoader<TState> {

        public bool loaded;
        public WindowObject instance;
        public TState state;
        public System.Action<TState> callback;

    }

    public class WaitForInitialized<TState> {

        public int count;
        public TState state;
        public System.Action<TState> callback;

    }

    public struct LayoutLoadingClosure {

        public int index;
        public WindowLayoutElement element;
        public WindowLayout windowLayoutInstance;
        public UnityEngine.UI.Windows.WindowTypes.LayoutItem.LayoutComponentItem[] layoutComponentItems;
        public UnityEngine.UI.Windows.WindowTypes.LayoutItem instance;
        public InitialParameters initialParameters;

    }

    public struct LoadAsyncLayoutClosure<TState> {

        public System.Collections.Generic.HashSet<WindowLayout> used;
        public TState state;
        public System.Action<TState> onComplete;

    }

    public struct LoadingClosure {

        public WindowBase window;
        public WindowModule.Parameters parameters;
        public WindowModules windowModules;
        public InitialParameters initialParameters;
        public int index;

    }

    public struct HideInstanceClosure {

        public WindowObject instance;
        public TransitionParameters parameters;
        public bool internalCall;

    }

    public struct ShowInstanceClosure<T> {

        public WindowSystem instance;
        public TransitionParameters parameters;
        public WindowBase existInstance;
        public InitialParameters initialParameters;
        public TransitionParameters transitionParameters;
        public System.Action<T> onInitialized;
            
    }

    public struct ShowInstanceClosure<T, TState> : System.IEquatable<ShowInstanceClosure<T, TState>> {

        public WindowSystem instance;
        public TransitionParameters parameters;
        public TState state;
        public WindowBase existInstance;
        public InitialParameters initialParameters;
        public TransitionParameters transitionParameters;
        public System.Action<T, TState> onInitialized;

        public bool Equals(ShowInstanceClosure<T, TState> other) {
            return Equals(this.instance, other.instance) && this.parameters.Equals(other.parameters) && System.Collections.Generic.EqualityComparer<TState>.Default.Equals(this.state, other.state) && Equals(this.existInstance, other.existInstance) && this.initialParameters.Equals(other.initialParameters) && this.transitionParameters.Equals(other.transitionParameters) && Equals(this.onInitialized, other.onInitialized);
        }

        public override bool Equals(object obj) {
            return obj is ShowInstanceClosure<T, TState> other && this.Equals(other);
        }

        public override int GetHashCode() {
            return System.HashCode.Combine(this.instance, this.parameters, this.state, this.existInstance, this.initialParameters, this.transitionParameters, this.onInitialized);
        }

    }

    public struct DoInitClosure {

        public WindowObject component;
        public WindowObject windowObject;
        public TransitionParameters parameters;

    }
    
    public class TaskCompletionShowScreen<T> {

        public T instance;
        public System.Action<T> onInitialized;

    }

    public class HideAllAndCleanClosure<TState> {

        public System.Collections.Generic.List<WindowBase> list;
        public TransitionParameters transitionParameters;
        public System.Func<WindowBase, TState, bool> predicate;
        public TState state;

    }

    public struct ShowLoadAsyncClosure<TState, TStateClosure> {

        public UnityEngine.UI.Windows.WindowTypes.LayoutWindowType component;
        public System.Action<TState> onComplete;
        public TState state;
        public TStateClosure closure;
        public System.Action<TState, TStateClosure> onInitialized;
        public InitialParameters initialParameters;
        public WindowBase instance;
        public TransitionParameters transitionParameters;

    }

    public class ShowHideInstanceInternalClosure {

        public Coroutines.ClosureDelegateCallback<ShowHideClosureParametersClass> cb;
        public ShowHideClosureParametersClass data;

    }

    public class ShowHideClosureParametersClass : Coroutines.ICallInSequenceClosure<WindowObject, ShowHideClosureParametersClass> {

        public int index { get; set; }

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

        int Coroutines.ICallInSequenceClosure<WindowObject, ShowHideClosureParametersClass>.counter { get; set; }
        bool Coroutines.ICallInSequenceClosure<WindowObject, ShowHideClosureParametersClass>.completed { get; set; }
        Coroutines.ClosureDelegateCallback<ShowHideClosureParametersClass> Coroutines.ICallInSequenceClosure<WindowObject, ShowHideClosureParametersClass>.callback { get; set; }
        Coroutines.ClosureDelegateEachCallback<WindowObject, ShowHideClosureParametersClass> Coroutines.ICallInSequenceClosure<WindowObject, ShowHideClosureParametersClass>.each { get; set; }
        Coroutines.ClosureDelegateCallback<ShowHideClosureParametersClass> Coroutines.ICallInSequenceClosure<WindowObject, ShowHideClosureParametersClass>.doNext { get; set; }
        Coroutines.ClosureDelegateCallback<ShowHideClosureParametersClass> Coroutines.ICallInSequenceClosure<WindowObject, ShowHideClosureParametersClass>.callbackItem { get; set; }
        System.Collections.Generic.List<WindowObject> Coroutines.ICallInSequenceClosure<WindowObject, ShowHideClosureParametersClass>.collection { get; set; }
        WindowObject[] Coroutines.ICallInSequenceClosure<WindowObject, ShowHideClosureParametersClass>.collectionArr { get; set; }
        System.Collections.Generic.List<WindowObject>.Enumerator Coroutines.ICallInSequenceClosure<WindowObject, ShowHideClosureParametersClass>.ie { get; set; }
        Coroutines.SZArrayEnumerator<WindowObject> Coroutines.ICallInSequenceClosure<WindowObject, ShowHideClosureParametersClass>.ieArr { get; set; }

    }

    public class DoLoadScreenClosure<TState> : Coroutines.ICallInSequenceClosure<WindowObject, DoLoadScreenClosure<TState>> {

        public int index { get; set; }

        public WindowObject component;
        public InitialParameters initialParameters;
        public System.Action<TState> onComplete;
        public TState state;

        int Coroutines.ICallInSequenceClosure<WindowObject, DoLoadScreenClosure<TState>>.counter { get; set; }
        bool Coroutines.ICallInSequenceClosure<WindowObject, DoLoadScreenClosure<TState>>.completed { get; set; }
        Coroutines.ClosureDelegateCallback<DoLoadScreenClosure<TState>> Coroutines.ICallInSequenceClosure<WindowObject, DoLoadScreenClosure<TState>>.callback { get; set; }
        Coroutines.ClosureDelegateEachCallback<WindowObject, DoLoadScreenClosure<TState>> Coroutines.ICallInSequenceClosure<WindowObject, DoLoadScreenClosure<TState>>.each { get; set; }
        Coroutines.ClosureDelegateCallback<DoLoadScreenClosure<TState>> Coroutines.ICallInSequenceClosure<WindowObject, DoLoadScreenClosure<TState>>.doNext { get; set; }
        Coroutines.ClosureDelegateCallback<DoLoadScreenClosure<TState>> Coroutines.ICallInSequenceClosure<WindowObject, DoLoadScreenClosure<TState>>.callbackItem { get; set; }
        System.Collections.Generic.List<WindowObject> Coroutines.ICallInSequenceClosure<WindowObject, DoLoadScreenClosure<TState>>.collection { get; set; }
        WindowObject[] Coroutines.ICallInSequenceClosure<WindowObject, DoLoadScreenClosure<TState>>.collectionArr { get; set; }
        System.Collections.Generic.List<WindowObject>.Enumerator Coroutines.ICallInSequenceClosure<WindowObject, DoLoadScreenClosure<TState>>.ie { get; set; }
        Coroutines.SZArrayEnumerator<WindowObject> Coroutines.ICallInSequenceClosure<WindowObject, DoLoadScreenClosure<TState>>.ieArr { get; set; }

    }

    public struct DoLoadScreenClosureInternal<T> {

        public Coroutines.ClosureDelegateCallback<T> callback;
        public T data;

    }

    public struct DoLoadScreenClosureStruct<TState> {

        public WindowObject component;
        public InitialParameters initialParameters;
        public System.Action<TState> onComplete;
        public TState state;

    }

    public struct LoadAsyncClosure<T, TState> {

        public WindowObject component;
        public System.Action<T, TState> onComplete;
        public System.Action<T> onCompleteNoState;
        public System.Action<TState> onCompleteState;
        public InitialParameters initialParameters;
        public TState state;
            
    }

}
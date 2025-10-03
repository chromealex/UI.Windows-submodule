namespace UnityEngine.UI.Windows {

    using Modules;
    
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

    public struct DoInitClosure {

        public WindowObject component;
        public WindowObject windowObject;
        public TransitionParameters parameters;

    }

    public struct LoadAsyncClosure<TState> {

        public UnityEngine.UI.Windows.WindowTypes.LayoutWindowType component;
        public System.Action<TState> onComplete;
        public TState state;
        public System.Action<TState> onInitialized;
        public InitialParameters initialParameters;
        public WindowBase instance;
        public TransitionParameters transitionParameters;

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

    public struct DoLoadScreenClosure<TState> {

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
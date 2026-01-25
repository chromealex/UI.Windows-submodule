namespace UnityEngine.UI.Windows.Components {

    using Modules;
    
    public class LinkerComponent : WindowComponent {

        [ResourceType(typeof(WindowObject))]
        public Resource prefab;
        public bool autoLoad;

        private WindowObject loadedAsset;

        public override void PushToPool() {
            if (this.loadedAsset != null && this.loadedAsset.IsForPool() == true) {
                this.loadedAsset = null;
            }
            base.PushToPool();
        }

        protected override void OnLoadScreenAsync<TState>(TState state, InitialParameters initialParameters, System.Action<TState> onComplete) {

            if (this.autoLoad == true) {
                
                this.LoadAsync<WindowObject, LoadAsyncClosure<WindowObject, TState>>(new LoadAsyncClosure<WindowObject, TState> {
                    component = this,
                    onCompleteState = onComplete,
                    state = state,
                }, this.prefab, static (instance, state) => {

                    if (instance != null) {
                        
                        instance.DoLoadScreenAsync(new DoLoadScreenClosureStruct<TState>() {
                            component = state.component,
                            initialParameters = state.initialParameters,
                            onComplete = state.onCompleteState,
                            state = state.state,
                        }, state.initialParameters, static (c) => {
                            
                            c.onComplete?.Invoke(c.state);

                        });

                        ((LinkerComponent)state.component).loadedAsset = instance;

                    } else {

                        state.onCompleteState?.Invoke(state.state);

                    }

                }, !initialParameters.showSync);

                return;

            }
            
            base.OnLoadScreenAsync(state, initialParameters, onComplete);
            
        }

        public readonly ref struct LinkerClosureAPI<TState> {

            private readonly TState state;
            private readonly LinkerComponent linkerComponent;
            private readonly ClosureAPI<TState> baseClosure; 

            public LinkerClosureAPI(TState state, LinkerComponent linkerComponent) {
                this.state = state;
                this.linkerComponent = linkerComponent;
                this.baseClosure = new ClosureAPI<TState>(state, linkerComponent);
            }

            public void LoadAsync<T>(System.Action<T, TState> onComplete = null) where T : WindowObject {
                this.linkerComponent.LoadAsync(this.state, onComplete);
            }

            public void ReloadAsync<T>(System.Action<T, TState> onComplete) where T : WindowObject {
                this.linkerComponent.ReloadAsync(this.state, onComplete);
            }

            public void ReloadAsync<T>(Resource prefab, System.Action<T, TState> onComplete) where T : WindowObject {
                this.linkerComponent.ReloadAsync(this.state, prefab, onComplete);
            }

            public void LoadAsync<T>(Resource resource, System.Action<T, TState> onComplete = null, bool async = true) where T : WindowObject {
                this.baseClosure.LoadAsync(resource, onComplete, async);
            }

            public void LoadAsync<T>(Resource resource, System.Action<TState> onComplete = null, bool async = true) where T : WindowObject {
                this.baseClosure.LoadAsync<T>(resource, onComplete, async);
            }

        }

        new public LinkerClosureAPI<TState> Closure<TState>(TState state) {
            return new LinkerClosureAPI<TState>(state, this);
        }
        
        private struct ClosureData<T> {

            public System.Action<T> onComplete;
            public LinkerComponent component;

        }

        public void LoadAsync<T>(System.Action<T> onComplete = null) where T : WindowObject {
            
            this.LoadAsync<T, ClosureData<T>>(new ClosureData<T>() {
                onComplete = onComplete,
                component = this,
            }, this.prefab, static (asset, state) => {

                state.component.loadedAsset = asset;
                state.onComplete?.Invoke(asset);

            });
            
        }

        public void LoadAsync<T, TState>(TState state, System.Action<T, TState> onComplete = null) where T : WindowObject {
            
            this.LoadAsync<T, System.ValueTuple<TState, System.Action<T, TState>, LinkerComponent>>((state, onComplete, this), this.prefab, static (asset, state) => {

                state.Item3.loadedAsset = asset;
                state.Item2?.Invoke(asset, state.Item1);

            });
            
        }

        public T LoadSync<T>() where T : WindowObject {
            
            this.LoadAsync<T, ClosureData<T>>(new ClosureData<T>() {
                component = this,
            }, this.prefab, static (asset, state) => {

                state.component.loadedAsset = asset;
                
            }, async: false);
            return this.loadedAsset as T;

        }

        public void ReloadAsync<T>(Resource prefab, System.Action<T> onComplete) where T : WindowObject {
            this.prefab = prefab;
            this.ReloadAsync(onComplete);
        }

        public void ReloadAsync<T>(System.Action<T> onComplete) where T : WindowObject {
            if (this.loadedAsset != null) {
                this.Unload();
            }
            this.LoadAsync(onComplete);
        }

        public void ReloadAsync<T, TState>(TState state, Resource prefab, System.Action<T, TState> onComplete) where T : WindowObject {
            this.prefab = prefab;
            this.ReloadAsync(state, onComplete);
        }

        public void ReloadAsync<T, TState>(TState state, System.Action<T, TState> onComplete) where T : WindowObject {
            if (this.loadedAsset != null) {
                this.Unload();
            }
            this.LoadAsync(state, onComplete);
        }

        public T ReloadSync<T>() where T : WindowObject {
            if (this.loadedAsset != null) {
                this.Unload();
            }
            return this.LoadSync<T>();
        }

        public bool Unload() {
            if (this.loadedAsset != null) {
                return this.UnloadSubObject(this.loadedAsset);
            }
            return false;
        }

        public T Get<T>() where T : WindowObject {

            return this.loadedAsset as T;

        }
        
    }

}
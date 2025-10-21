namespace UnityEngine.UI.Windows.Components {

    using Modules;
    
    public class LinkerComponent : WindowComponent {

        [ResourceType(typeof(WindowObject))]
        public Resource prefab;
        public bool autoLoad;

        private WindowObject loadedAsset;

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

        private struct Closure<T> {

            public System.Action<T> onComplete;
            public LinkerComponent component;

        }

        public void LoadAsync<T>(System.Action<T> onComplete = null) where T : WindowObject {
            
            this.LoadAsync<T, Closure<T>>(new Closure<T>() {
                onComplete = onComplete,
                component = this,
            }, this.prefab, static (asset, state) => {

                state.component.loadedAsset = asset;
                state.onComplete?.Invoke(asset);

            });
            
        }

        public T LoadSync<T>() where T : WindowObject {
            
            this.LoadAsync<T, Closure<T>>(new Closure<T>() {
                component = this,
            }, this.prefab, static (asset, state) => {

                state.component.loadedAsset = asset;
                
            }, async: false);
            return this.loadedAsset as T;

        }

        public void ReloadAsync<T>(System.Action<T> onComplete) where T : WindowObject {
            if (this.loadedAsset != null) {
                this.Unload();
            }
            this.LoadAsync(onComplete);
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
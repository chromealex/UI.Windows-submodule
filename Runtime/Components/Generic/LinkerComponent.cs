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

        public void LoadAsync<T>(System.Action<T> onComplete = null) where T : WindowObject {
            
            this.LoadAsync<T>(this.prefab, (asset) => {

                this.loadedAsset = asset;
                onComplete?.Invoke(asset);

            });
            
        }

        public T LoadSync<T>() where T : WindowObject {
            
            this.LoadAsync<T>(this.prefab, (asset) => {

                this.loadedAsset = asset;
                
            }, async: false);
            return this.loadedAsset as T;

        }

        public T Get<T>() where T : WindowObject {

            return this.loadedAsset as T;

        }
        
    }

}
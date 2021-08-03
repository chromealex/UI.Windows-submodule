namespace UnityEngine.UI.Windows.Components {

    using Modules;
    
    public class LinkerComponent : WindowComponent {

        [ResourceType(typeof(WindowObject))]
        public Resource prefab;
        public bool autoLoad;

        private WindowObject loadedAsset;

        protected override void OnLoadScreenAsync(InitialParameters initialParameters, System.Action onComplete) {

            if (this.autoLoad == true) {
                
                this.LoadAsync<WindowObject>(this.prefab, (instance) => {

                    if (instance != null) {
                        
                        instance.DoLoadScreenAsync(initialParameters, () => {
                            
                            base.OnLoadScreenAsync(initialParameters, onComplete);

                        });

                        this.loadedAsset = instance;

                    } else {

                        base.OnLoadScreenAsync(initialParameters, onComplete);

                    }

                }, !initialParameters.showSync);

                return;

            }
            
            base.OnLoadScreenAsync(initialParameters, onComplete);
            
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
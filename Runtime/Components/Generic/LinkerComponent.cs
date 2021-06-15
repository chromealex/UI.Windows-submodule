namespace UnityEngine.UI.Windows.Components {

    using Modules;
    
    public class LinkerComponent : WindowComponent {

        [ResourceType(typeof(WindowObject))]
        public Resource prefab;
        public bool autoLoad;

        private WindowObject loadedAsset;

        protected override void OnLoadScreenAsync(System.Action onComplete) {

            if (this.autoLoad == true) {
                
                this.LoadAsync<WindowObject>(this.prefab, (instance) => {

                    if (instance != null) {
                        
                        instance.DoLoadScreenAsync(() => {
                            
                            base.OnLoadScreenAsync(onComplete);

                        });

                        this.loadedAsset = instance;

                    } else {

                        base.OnLoadScreenAsync(onComplete);

                    }

                });

                return;

            }
            
            base.OnLoadScreenAsync(onComplete);
            
        }

        public void LoadAsync<T>() where T : WindowObject {
            
            this.LoadAsync<T>(this.prefab);
            
        }

        public T Get<T>() where T : WindowObject {

            return this.loadedAsset as T;

        }
        
    }

}
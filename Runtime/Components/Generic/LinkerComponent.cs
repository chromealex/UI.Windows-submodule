using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Components {

    using Modules;
    
    public class LinkerComponent : WindowComponent {

        [ResourceType(typeof(WindowObject))]
        public Resource prefab;
        public bool autoLoad;

        protected override void OnLoadScreenAsync(System.Action onComplete) {

            if (this.autoLoad == true) {
                
                this.LoadAsync<WindowObject>(this.prefab, (instance) => {

                    if (instance != null) {
                        
                        instance.DoLoadScreenAsync(() => {
                            
                            base.OnLoadScreenAsync(onComplete);

                        });
                        
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
        
    }

}
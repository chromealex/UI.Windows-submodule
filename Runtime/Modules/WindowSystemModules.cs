﻿using System.Collections;

namespace UnityEngine.UI.Windows.Modules {

    using Utilities;

    public abstract class WindowModule : WindowLayout {

        [System.Serializable]
        public class Parameters {

            public int defaultOrder;

            public virtual void Apply(WindowModule instance) {

                instance.SetCanvasOrder(instance.GetWindow().GetCanvasOrder() + this.defaultOrder);

            }

        }

        [SerializeReference]
        public Parameters parameters;

    }

    [System.Serializable]
    public class WindowModules {

        [System.Serializable]
        public struct WindowModuleInfo {

            public WindowSystemTargets targets;
            [ResourceType(typeof(WindowModule))]
            public Resource module;
            [SerializeReference]
            public WindowModule.Parameters parameters;

            [System.NonSerialized]
            internal WindowModule moduleInstance;

        }

        public WindowModuleInfo[] modules;
        
        public T FindComponent<T>(System.Func<T, bool> filter = null) where T : WindowComponent {

            for (int i = 0; i < this.modules.Length; ++i) {

                var comp = this.modules[i].moduleInstance.FindComponent(filter);
                if (comp != null) return comp;

            }

            return null;

        }
        
        internal void SendEvent<T>(T data) {
            
            for (int i = 0; i < this.modules.Length; ++i) {

                this.modules[i].moduleInstance.SendEvent(data);

            }
            
        }
        
        public void LoadAsync(WindowBase window, System.Action onComplete) {

            Coroutines.Run(this.InitModules(window, onComplete));

        }

        public T Get<T>() where T : WindowModule {

            for (int i = 0; i < this.modules.Length; ++i) {

                if (this.modules[i].moduleInstance is T module) return module;

            }

            return null;

        }
        
        private struct LoadingClosure {

            public WindowBase window;
            public WindowModule.Parameters parameters;
            public WindowModules windowModules;
            public int index;

        }
        
        private IEnumerator InitModules(WindowBase window, System.Action onComplete) {

            var resources = WindowSystem.GetResources();
            var targetData = WindowSystem.GetTargetData();

            for (int i = 0; i < this.modules.Length; ++i) {

                var moduleInfo = this.modules[i];
                if (moduleInfo.targets.IsValid(targetData) == false) continue;
                if (moduleInfo.moduleInstance != null) continue;

                var parameters = moduleInfo.parameters;
                var data = new LoadingClosure() {
                    windowModules = this,
                    index = i,
                    parameters = parameters,
                    window = window,
                };
                yield return resources.LoadAsync<WindowModule, LoadingClosure>(window, data, moduleInfo.module, (asset, closure) => {

                    var instance = WindowSystem.GetPools().Spawn(asset, closure.window.transform);
                    instance.Setup(closure.window);
                    if (closure.parameters != null) closure.parameters.Apply(instance);
                    
                    var layoutPreferences = closure.window.GetCurrentLayoutPreferences();
                    if (layoutPreferences != null && instance.canvasScaler != null) layoutPreferences.Apply(instance.canvasScaler);

                    closure.window.RegisterSubObject(instance);

                    closure.windowModules.modules[closure.index].moduleInstance = instance;

                });

            }

            onComplete.Invoke();

        }

    }

}
using System.Collections;

namespace UnityEngine.UI.Windows.Modules {

    using Utilities;

    public abstract class WindowModule : WindowLayout {

        [System.Serializable]
        public class Parameters {

            public int defaultOrder = 0;
            public bool applyCanvasScaler = true;

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

        public void Unload() {
            
            for (int i = 0; i < this.modules.Length; ++i) {

                this.modules[i].moduleInstance = null;

            }
            
        }
        
        public void LoadAsync(InitialParameters initialParameters, WindowBase window, System.Action onComplete) {

            Coroutines.Run(this.InitModules(initialParameters, window, onComplete));

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
            public InitialParameters initialParameters;
            public int index;

        }
        
        private int loadingCount;
        private IEnumerator InitModules(InitialParameters initialParameters, WindowBase window, System.Action onComplete) {

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
                    initialParameters = initialParameters,
                };
                ++this.loadingCount;
                Coroutines.Run(resources.LoadAsync<WindowModule, LoadingClosure>(new WindowSystemResources.LoadParameters() { async = !initialParameters.showSync }, window, data, moduleInfo.module, (asset, closure) => {

                    if (asset.createPool == true) WindowSystem.GetPools().CreatePool(asset);
                    var instance = WindowSystem.GetPools().Spawn(asset, closure.window.transform);
                    instance.Setup(closure.window);
                    if (closure.parameters != null) closure.parameters.Apply(instance);
                    instance.SetResetState();

                    if (closure.parameters != null && closure.parameters.applyCanvasScaler == true) {

                        var layoutPreferences = closure.window.GetCurrentLayoutPreferences();
                        if (layoutPreferences != null && instance.canvasScaler != null) layoutPreferences.Apply(instance.canvasScaler);

                    }
                    
                    closure.window.RegisterSubObject(instance);

                    closure.windowModules.modules[closure.index].moduleInstance = instance;
                    
                    instance.DoLoadScreenAsync(closure.initialParameters, () => { --closure.windowModules.loadingCount; });
                    
                }));

            }

            while (this.loadingCount > 0) yield return null;

            onComplete.Invoke();

        }

    }

}
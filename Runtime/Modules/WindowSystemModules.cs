using System.Collections;

namespace UnityEngine.UI.Windows.Modules {

    using Utilities;
    using System.Threading.Tasks;

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
        
        public async void LoadAsync<TState>(TState state, InitialParameters initialParameters, WindowBase window, System.Action<TState> onComplete) {

            await this.InitModules(state, initialParameters, window, onComplete);

        }

        public T Get<T>() where T : WindowModule {

            for (int i = 0; i < this.modules.Length; ++i) {

                if (this.modules[i].moduleInstance is T module) return module;

            }

            return null;

        }
        
        private int loadingCount;
        private async Task InitModules<TState>(TState state, InitialParameters initialParameters, WindowBase window, System.Action<TState> onComplete) {

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
                await resources.LoadAsync<WindowModule, LoadingClosure>(new WindowSystemResources.LoadParameters() { async = !initialParameters.showSync }, window, data, moduleInfo.module, static (asset, closure) => {

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
                    
                    instance.DoLoadScreenAsync(closure, closure.initialParameters, static (c) => { --c.windowModules.loadingCount; });
                    
                });

            }

            while (this.loadingCount > 0) await Task.Yield();

            onComplete.Invoke(state);

        }

    }

}
using System.Collections;

namespace UnityEngine.UI.Windows.Modules {

    using Utilities;

    public abstract class WindowModule : WindowLayout {

        public int defaultOrder;

    }

    [System.Serializable]
    public class WindowModules {

        [System.Serializable]
        public struct WindowModuleInfo {

            public WindowSystemTargets targets;
            [ResourceType(typeof(WindowModule))]
            public Resource module;
            public int order;

            [System.NonSerialized]
            internal WindowModule moduleInstance;

        }

        public WindowModuleInfo[] modules;

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
            public int order;
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

                var order = moduleInfo.order;
                var data = new LoadingClosure() {
                    windowModules = this,
                    index = i,
                    order = order,
                    window = window,
                };
                yield return resources.LoadAsync<WindowModule, LoadingClosure>(window, data, moduleInfo.module, (asset, closure) => {

                    var instance = WindowSystem.GetPools().Spawn(asset, closure.window.transform);
                    instance.Setup(closure.window);
                    instance.SetCanvasOrder(closure.window.GetCanvasOrder() + closure.order);

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
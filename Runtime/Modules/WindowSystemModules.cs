using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        private IEnumerator InitModules(WindowBase window, System.Action onComplete) {

            var resources = WindowSystem.GetResources();
            var targetData = WindowSystem.GetTargetData();

            for (int i = 0; i < this.modules.Length; ++i) {

                var moduleInfo = this.modules[i];
                if (moduleInfo.targets.IsValid(targetData) == false) continue;
                if (moduleInfo.moduleInstance != null) continue;

                var order = moduleInfo.order;
                WindowModule instance = null;
                yield return resources.LoadAsync<WindowModule>(window, moduleInfo.module, (asset) => {

                    instance = WindowSystem.GetPools().Spawn(asset, window.transform);
                    instance.Setup(window);
                    instance.SetCanvasOrder(window.GetCanvasOrder() + order);
                    window.RegisterSubObject(instance);

                });

                moduleInfo.moduleInstance = instance;
                this.modules[i] = moduleInfo;

            }

            onComplete.Invoke();

        }

    }

}
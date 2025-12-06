using UnityEngine.UI.Windows.Utilities;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows {
    
    using Modules;

    public partial class WindowSystem {

        public static WindowSystemBreadcrumbs GetBreadcrumbs() {

            return WindowSystem.instance?.breadcrumbs;

        }

        public static WindowSystemPools GetPools() {

            return WindowSystem.instance?.pools;

        }

        public static WindowSystemEvents GetEvents() {

            return WindowSystem.instance?.events;

        }

        public static WindowSystemSettings GetSettings() {

            return WindowSystem.instance?.settings;

        }

        public static WindowSystemResources GetResources() {

            return WindowSystem.instance?.resources;

        }

        public static Tweener GetTweener() {

            return WindowSystem.instance?.tweener;

        }

        public static T GetWindowSystemModule<T>() where T : WindowSystemModule {

            if (WindowSystem.instance.modules != null) {

                for (int i = 0; i < WindowSystem.instance.modules.Count; ++i) {

                    var module = WindowSystem.instance.modules[i];
                    if (module != null && module is T moduleT) {
                        return moduleT;
                    }

                }

            }

            return null;

        }

        public static void AddModule<T>(T module) where T : WindowSystemModule {

            if (WindowSystem.instance.modules == null) WindowSystem.instance.modules = new List<WindowSystemModule>();
            WindowSystem.instance.modules.Add(module);
            
        }

    }

}
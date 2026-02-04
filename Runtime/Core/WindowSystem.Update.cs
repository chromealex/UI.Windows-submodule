using System.Collections.Generic;

namespace UnityEngine.UI.Windows {

    public interface IUpdate {
        void OnUpdate(float dt);
        bool IsVisible();
    }

    public interface ILateUpdate {
        void OnLateUpdate(float dt);
        bool IsVisible();
    }

    public partial class WindowSystem {
        
        private readonly List<IUpdate> updates = new List<IUpdate>();
        private readonly List<ILateUpdate> lateUpdates = new List<ILateUpdate>();
        private readonly List<Object> toRemoveTemp = new List<Object>();

        public static bool TryAddUpdateListener(WindowComponentModule module) {
            var result = false;
            var instance = WindowSystem.instance;
            if (module is IUpdate update) {
                instance.updates.Add(update);
                result = true;
            }
            if (module is ILateUpdate lateUpdate) {
                instance.lateUpdates.Add(lateUpdate);
                result = true;
            }

            if (result == true) {
                WindowSystem.GetEvents().RegisterOnce((module, _: 0), module.windowComponent, WindowEvent.OnHideEnd, static (obj, module) => TryRemoveUpdateListener(module.module));
            }
            return result;
        }
        
        public static bool TryAddUpdateListener(WindowObject component) {
            var result = false;
            var instance = WindowSystem.instance;
            if (component is IUpdate update) {
                instance.updates.Add(update);
                result = true;
            }
            if (component is ILateUpdate lateUpdate) {
                instance.lateUpdates.Add(lateUpdate);
                result = true;
            }

            if (result == true) {
                WindowSystem.GetEvents().RegisterOnce(component, WindowEvent.OnHideEnd, static (obj) => TryRemoveUpdateListener(obj));
            }
            return result;
        }

        public static void TryRemoveUpdateListener(WindowComponentModule module) {
            var instance = WindowSystem.instance;
            instance.toRemoveTemp.Add(module);
        }

        public static void TryRemoveUpdateListener(WindowObject component) {
            var instance = WindowSystem.instance;
            instance.toRemoveTemp.Add(component);
        }

        private void ApplyRemoved() {

            foreach (var component in this.toRemoveTemp) {
                if (component is IUpdate update) {
                    instance.updates.Remove(update);
                }
                if (component is ILateUpdate lateUpdate) {
                    instance.lateUpdates.Remove(lateUpdate);
                }
            }
            this.toRemoveTemp.Clear();

        }

        private void DoUpdateComponents(float dt) {

            for (var index = this.updates.Count - 1; index >= 0; --index) {
                var component = this.updates[index];
                if (component == null) {
                    this.updates.RemoveAt(index);
                    continue;
                }
                if (component.IsVisible() == false) continue;
                component.OnUpdate(dt);
            }

            this.ApplyRemoved();

        }

        private void DoLateUpdateComponents(float dt) {

            for (var index = this.lateUpdates.Count - 1; index >= 0; --index) {
                var component = this.lateUpdates[index];
                if (component == null) {
                    this.lateUpdates.RemoveAt(index);
                    continue;
                }
                if (component.IsVisible() == false) continue;
                component.OnLateUpdate(dt);
            }
            
            this.ApplyRemoved();
            
        }

        public void Update() {

            if (this.modules != null) {
                for (int i = 0; i < this.modules.Count; ++i) {
                    this.modules[i]?.OnUpdate();
                }
            }
            this.DoUpdateInput();
            this.DoUpdateComponents(Time.deltaTime);

        }

        public void LateUpdate() {
            
            this.DoLateUpdateComponents(Time.deltaTime);

        }
        
    }

}
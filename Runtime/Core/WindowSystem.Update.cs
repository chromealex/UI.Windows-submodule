using System.Collections.Generic;

namespace UnityEngine.UI.Windows {

    public interface IUpdate {
        void OnUpdate(float dt);
    }

    public interface ILateUpdate {
        void OnLateUpdate(float dt);
    }

    public partial class WindowSystem {
        
        private readonly List<WindowObject> updates = new List<WindowObject>();
        private readonly List<WindowObject> lateUpdates = new List<WindowObject>();
        private readonly List<WindowObject> toRemoveTemp = new List<WindowObject>();

        public static bool TryAddUpdateListener(WindowObject component) {
            var result = false;
            var instance = WindowSystem.instance;
            if (component is IUpdate) {
                instance.updates.Add(component);
                result = true;
            }
            if (component is ILateUpdate) {
                instance.lateUpdates.Add(component);
                result = true;
            }

            if (result == true) {
                WindowSystem.GetEvents().RegisterOnce(component, WindowEvent.OnHideEnd, static (obj) => TryRemoveUpdateListener(obj));
            }
            return result;
        }

        public static void TryRemoveUpdateListener(WindowObject component) {
            var instance = WindowSystem.instance;
            instance.toRemoveTemp.Add(component);
        }

        private void ApplyRemoved() {

            foreach (var component in this.toRemoveTemp) {
                if (component is IUpdate) {
                    instance.updates.Remove(component);
                }
                if (component is ILateUpdate) {
                    instance.lateUpdates.Remove(component);
                }
            }
            this.toRemoveTemp.Clear();

        }

        private void DoUpdateComponents(float dt) {

            for (var index = this.updates.Count - 1; index >= 0; --index) {
                var component = this.updates[index];
                if (component.IsVisible() == false) continue;
                ((IUpdate)component).OnUpdate(dt);
            }

            this.ApplyRemoved();

        }

        private void DoLateUpdateComponents(float dt) {

            for (var index = this.lateUpdates.Count - 1; index >= 0; --index) {
                var component = this.lateUpdates[index];
                if (component.IsVisible() == false) continue;
                ((ILateUpdate)component).OnLateUpdate(dt);
            }
            
            this.ApplyRemoved();
            
        }

        public void Update() {

            this.DoUpdateInput();
            this.DoUpdateComponents(Time.deltaTime);

        }

        public void LateUpdate() {
            
            this.DoLateUpdateComponents(Time.deltaTime);

        }
        
    }

}
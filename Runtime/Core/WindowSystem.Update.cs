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
                WindowSystem.GetEvents().RegisterOnce(component, WindowEvent.OnHideEnd, static (obj) => {
                    TryRemoveUpdateListener(obj);
                });
            }
            return result;
        }

        public static bool TryRemoveUpdateListener(WindowObject component) {
            var result = false;
            var instance = WindowSystem.instance;
            if (component is IUpdate) {
                result |= instance.updates.Remove(component);
            }
            if (component is ILateUpdate) {
                result |= instance.lateUpdates.Remove(component);
            }
            return result;
        }

        private void DoUpdateComponents(float dt) {

            foreach (var component in this.updates) {
                ((IUpdate)component).OnUpdate(dt);
            }
            
        }

        private void DoLateUpdateComponents(float dt) {

            foreach (var component in this.lateUpdates) {
                ((ILateUpdate)component).OnLateUpdate(dt);
            }
            
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
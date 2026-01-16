using UnityEngine.UI.Windows.Utilities;

namespace UnityEngine.UI.Windows {

    public partial class WindowSystem {

        public static void SendEvent<T>(T data) where T : class {

            foreach (var item in WindowSystem.instance.currentWindows) {
                
                item.instance.SendEvent(data);
                
            }
            
        }

        public static void RaiseEvent(WindowObject instance, WindowEvent windowEvent) {

            var events = WindowSystem.GetEvents();
            events.Raise(instance, windowEvent);

        }

        public static void ClearEvents(WindowObject instance) {

            var events = WindowSystem.GetEvents();
            events.Clear(instance);

        }

        public static void RegisterActionOnce(WindowObject instance, WindowEvent windowEvent, System.Action<WindowObject> callback) {

            var events = WindowSystem.GetEvents();
            events.RegisterOnce(instance, windowEvent, callback);

        }

        public static void RegisterAction(WindowObject instance, WindowEvent windowEvent, System.Action<WindowObject> callback) {

            var events = WindowSystem.GetEvents();
            events.Register(instance, windowEvent, callback);

        }

        public static void UnRegisterAction(WindowObject instance, WindowEvent windowEvent, System.Action<WindowObject> callback) {

            var events = WindowSystem.GetEvents();
            events.UnRegister(instance, windowEvent, callback);

        }

        public static void RaiseEvent<TState>(WindowObject instance, WindowEvent windowEvent) where TState : System.IEquatable<TState> {

            var events = WindowSystem.GetEvents();
            events.Raise<TState>(instance, windowEvent);

        }

        public static void RegisterActionOnce<TState>(TState state, WindowObject instance, WindowEvent windowEvent, System.Action<WindowObject, TState> callback) where TState : System.IEquatable<TState> {

            var events = WindowSystem.GetEvents();
            events.RegisterOnce(state, instance, windowEvent, callback);

        }

        public static void RegisterAction<TState>(TState state, WindowObject instance, WindowEvent windowEvent, System.Action<WindowObject, TState> callback) where TState : System.IEquatable<TState> {

            var events = WindowSystem.GetEvents();
            events.Register(state, instance, windowEvent, callback);

        }

        public static void UnRegisterAction<TState>(TState state, WindowObject instance, WindowEvent windowEvent, System.Action<WindowObject, TState> callback) where TState : System.IEquatable<TState> {

            var events = WindowSystem.GetEvents();
            events.UnRegister(state, instance, windowEvent, callback);

        }

    }

}
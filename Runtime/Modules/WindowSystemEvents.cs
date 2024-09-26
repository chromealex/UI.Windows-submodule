using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Modules {

    using Utilities;

    public class WindowSystemEvents : MonoBehaviour {

        public struct Info {

            public string name;
            public WindowObject instance;
            
        }

        internal Dictionary<int, System.Action<WindowObject>> cacheUnknown = new Dictionary<int, System.Action<WindowObject>>();
        internal Dictionary<long, System.Action<WindowObject>> cache = new Dictionary<long, System.Action<WindowObject>>();
        internal Dictionary<long, System.Action<WindowObject>> cacheOnce = new Dictionary<long, System.Action<WindowObject>>();
        internal Dictionary<long, Info> objects = new Dictionary<long, Info>();
        internal int eventsCount;

        public void Initialize() {

            this.eventsCount = System.Enum.GetValues(typeof(WindowEvent)).Length;

        }

        public void Raise(WindowObject instance, WindowEvent windowEvent) {

            if (windowEvent == WindowEvent.None || instance == null) return;

            var key = UIWSMath.GetKey(instance.GetHashCode(), (int)windowEvent);
            {
                if (this.cache.TryGetValue(key, out var actions) == true) {

                    actions?.Invoke(instance);

                }
            }

            {
                if (this.cacheOnce.TryGetValue(key, out var actions) == true) {

                    actions?.Invoke(instance);
                    this.cacheOnce.Remove(key);

                }
            }

            {
                
                if (this.cacheUnknown.TryGetValue((int)windowEvent, out var actions) == true) {

                    actions?.Invoke(instance);

                }
                
            }

        }

        public void Register(WindowEvent windowEvent, System.Action<WindowObject> callback) {

            if (windowEvent == WindowEvent.None) return;

            var key = (int)windowEvent;
            if (this.cacheUnknown.TryGetValue(key, out var actions) == true) {

                actions += callback;
                this.cacheUnknown[key] = actions;

            } else {

                this.cacheUnknown.Add(key, callback);

            }
            
        }

        public void UnRegister(WindowEvent windowEvent, System.Action<WindowObject> callback) {

            if (windowEvent == WindowEvent.None) return;

            var key = (int)windowEvent;
            if (this.cacheUnknown.TryGetValue(key, out var actions) == true) {

                actions -= callback;
                this.cacheUnknown[key] = actions;

            }
            
        }

        public void Clear() {

            this.cache.Clear();

        }

        public void Clear(WindowObject instance) {

            for (int i = 0; i <= this.eventsCount; ++i) {

                var key = UIWSMath.GetKey(instance.GetHashCode(), i);
                {
                    if (this.cache.ContainsKey(key) == true) {

                        this.cache[key] = null;

                    }
                }
                {
                    if (this.cacheOnce.ContainsKey(key) == true) {

                        this.cacheOnce[key] = null;

                    }
                }
                {
                    this.objects.Remove(key);
                }

            }

        }

        public void RegisterOnce(WindowObject instance, WindowEvent windowEvent, System.Action<WindowObject> callback) {

            if (windowEvent == WindowEvent.None) return;

            var key = UIWSMath.GetKey(instance.GetHashCode(), (int)windowEvent);
            if (this.cacheOnce.TryGetValue(key, out var actions) == true) {

                actions += callback;
                this.cacheOnce[key] = actions;

            } else {

                this.cacheOnce.Add(key, callback);

            }

            if (this.objects.ContainsKey(key) == false) this.objects.Add(key, new Info() { instance = instance, name = instance.name });

        }

        public void Register(WindowObject instance, WindowEvent windowEvent, System.Action<WindowObject> callback) {

            if (windowEvent == WindowEvent.None) return;

            var key = UIWSMath.GetKey(instance.GetHashCode(), (int)windowEvent);
            if (this.cache.TryGetValue(key, out var actions) == true) {

                actions += callback;
                this.cache[key] = actions;

            } else {

                this.cache.Add(key, callback);

            }
            
            if (this.objects.ContainsKey(key) == false) this.objects.Add(key, new Info() { instance = instance, name = instance.name });

        }

        public void UnRegister(WindowObject instance, WindowEvent windowEvent, System.Action<WindowObject> callback) {

            if (windowEvent == WindowEvent.None) return;

            var key = UIWSMath.GetKey(instance.GetHashCode(), (int)windowEvent);
            if (this.cache.TryGetValue(key, out var actions) == true) {

                actions -= callback;
                this.cache[key] = actions;

            }

        }

        public void UnRegister(WindowObject instance, WindowEvent windowEvent) {

            if (windowEvent == WindowEvent.None) return;

            var key = UIWSMath.GetKey(instance.GetHashCode(), (int)windowEvent);
            if (this.cache.ContainsKey(key) == true) {

                this.cache[key] = null;

            }

        }

    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Modules {

    using Utilities;

    public class WindowSystemEvents : MonoBehaviour {

        private Dictionary<long, System.Action> cache = new Dictionary<long, System.Action>();
        private Dictionary<long, System.Action> cacheOnce = new Dictionary<long, System.Action>();
        private int eventsCount;

        public void Initialize() {

            this.eventsCount = System.Enum.GetValues(typeof(WindowEvent)).Length;

        }

        public void Raise(WindowObject instance, WindowEvent windowEvent) {

            var key = UIWSMath.GetKey(instance.GetHashCode(), (int)windowEvent);
            {
                if (this.cache.TryGetValue(key, out var actions) == true) {

                    actions.Invoke();

                }
            }

            {
                if (this.cacheOnce.TryGetValue(key, out var actions) == true) {

                    actions.Invoke();
                    this.cacheOnce.Remove(key);

                }
            }

        }

        public void Clear() {

            this.cache.Clear();

        }

        public void Clear(WindowObject instance) {

            for (int i = 0; i < this.eventsCount; ++i) {

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

            }

        }

        public void RegisterOnce(WindowObject instance, WindowEvent windowEvent, System.Action callback) {

            var key = UIWSMath.GetKey(instance.GetHashCode(), (int)windowEvent);
            if (this.cacheOnce.TryGetValue(key, out var actions) == true) {

                actions += callback;
                this.cacheOnce[key] = actions;

            } else {

                this.cacheOnce.Add(key, callback);

            }

        }

        public void Register(WindowObject instance, WindowEvent windowEvent, System.Action callback) {

            var key = UIWSMath.GetKey(instance.GetHashCode(), (int)windowEvent);
            if (this.cache.TryGetValue(key, out var actions) == true) {

                actions += callback;
                this.cache[key] = actions;

            } else {

                this.cache.Add(key, callback);

            }

        }

        public void UnRegister(WindowObject instance, WindowEvent windowEvent, System.Action callback) {

            var key = UIWSMath.GetKey(instance.GetHashCode(), (int)windowEvent);
            if (this.cache.TryGetValue(key, out var actions) == true) {

                actions -= callback;
                this.cache[key] = actions;

            }

        }

        public void UnRegister(WindowObject instance, WindowEvent windowEvent) {

            var key = UIWSMath.GetKey(instance.GetHashCode(), (int)windowEvent);
            if (this.cache.ContainsKey(key) == true) {

                this.cache[key] = null;

            }

        }

    }

}
﻿using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UI.Windows.Modules {

    using Utilities;

    public class WindowSystemEvents : MonoBehaviour {

        public abstract class RegistryBase {

            public abstract void Clear();
            public abstract void Clear(WindowObject instance, int eventsCount);

            public abstract Dictionary<long, Info> GetObjects();

            public abstract bool ContainsCache(long key);

        }
        
        public class Registry : RegistryBase {

            internal readonly Dictionary<int, System.Action<WindowObject>> cacheUnknown = new Dictionary<int, System.Action<WindowObject>>();
            internal readonly Dictionary<long, System.Action<WindowObject>> cache = new Dictionary<long, System.Action<WindowObject>>();
            internal readonly Dictionary<long, System.Action<WindowObject>> cacheOnce = new Dictionary<long, System.Action<WindowObject>>();
            internal readonly Dictionary<long, Info> objects = new Dictionary<long, Info>();

            public override void Clear() {
                this.cacheUnknown.Clear();
                this.cache.Clear();
                this.cacheOnce.Clear();
                this.objects.Clear();
            }

            public override Dictionary<long, Info> GetObjects() {
                return this.objects;
            }

            public override bool ContainsCache(long key) {
                return this.cache.ContainsKey(key);
            }

            public override void Clear(WindowObject instance, int eventsCount) {
                
                for (int i = 0; i <= eventsCount; ++i) {

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

        }

        public class Registry<T> : RegistryBase {

            public static bool isCreated;
            public static readonly Registry<T> instance = new Registry<T>();

            internal readonly Dictionary<int, System.Action<WindowObject, T>> cacheUnknown = new Dictionary<int, System.Action<WindowObject, T>>();
            internal readonly Dictionary<long, System.Action<WindowObject, T>> cache = new Dictionary<long, System.Action<WindowObject, T>>();
            internal readonly Dictionary<long, System.Action<WindowObject, T>> cacheOnce = new Dictionary<long, System.Action<WindowObject, T>>();
            internal readonly Dictionary<long, Info> objects = new Dictionary<long, Info>();
            
            public override void Clear() {
                this.cacheUnknown.Clear();
                this.cache.Clear();
                this.cacheOnce.Clear();
                this.objects.Clear();
            }

            public override Dictionary<long, Info> GetObjects() {
                return this.objects;
            }

            public override bool ContainsCache(long key) {
                return this.cache.ContainsKey(key);
            }

            public override void Clear(WindowObject instance, int eventsCount) {
                
                for (int i = 0; i <= eventsCount; ++i) {

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

        }
        
        public struct Info {

            public string name;
            public WindowObject instance;
            
        }

        internal readonly Registry registry = new Registry();
        internal readonly List<RegistryBase> registriesGeneric = new List<RegistryBase>();
        internal int eventsCount;
        
        public void Initialize() {

            this.eventsCount = System.Enum.GetValues(typeof(WindowEvent)).Length;

        }

        private void ClearRegistry(WindowObject instance, RegistryBase registryBase) {
            
            registryBase.Clear(instance, this.eventsCount);
            
        }

        public void Raise(WindowObject instance, WindowEvent windowEvent) {

            if (windowEvent == WindowEvent.None || instance == null) return;
            
            var registry = this.registry;
            var key = UIWSMath.GetKey(instance.GetHashCode(), (int)windowEvent);
            {
                if (registry.cache.TryGetValue(key, out var actions) == true) {

                    actions?.Invoke(instance);

                }
            }

            {
                if (registry.cacheOnce.TryGetValue(key, out var actions) == true) {

                    actions?.Invoke(instance);
                    registry.cacheOnce.Remove(key);

                }
            }

            {
                
                if (registry.cacheUnknown.TryGetValue((int)windowEvent, out var actions) == true) {

                    actions?.Invoke(instance);

                }
                
            }

        }

        public void Raise<TState>(TState state, WindowObject instance, WindowEvent windowEvent) {

            if (windowEvent == WindowEvent.None || instance == null) return;
            
            var registry = this.GetInstance<TState>();
            var key = UIWSMath.GetKey(instance.GetHashCode(), (int)windowEvent);
            {
                if (registry.cache.TryGetValue(key, out var actions) == true) {

                    actions?.Invoke(instance, state);

                }
            }

            {
                if (registry.cacheOnce.TryGetValue(key, out var actions) == true) {

                    actions?.Invoke(instance, state);
                    registry.cacheOnce.Remove(key);

                }
            }

            {
                
                if (registry.cacheUnknown.TryGetValue((int)windowEvent, out var actions) == true) {

                    actions?.Invoke(instance, state);

                }
                
            }

        }

        public void Register(WindowEvent windowEvent, System.Action<WindowObject> callback) {

            if (windowEvent == WindowEvent.None) return;

            var registry = this.registry;
            var key = (int)windowEvent;
            if (registry.cacheUnknown.TryGetValue(key, out var actions) == true) {

                actions += callback;
                registry.cacheUnknown[key] = actions;

            } else {

                registry.cacheUnknown.Add(key, callback);

            }
            
        }

        public void Register<TState>(TState state, WindowEvent windowEvent, System.Action<WindowObject, TState> callback) {

            if (windowEvent == WindowEvent.None) return;

            var registryInstance = this.GetInstance<TState>();
            var key = (int)windowEvent;
            if (registryInstance.cacheUnknown.TryGetValue(key, out var actions) == true) {

                actions += callback;
                registryInstance.cacheUnknown[key] = actions;

            } else {

                registryInstance.cacheUnknown.Add(key, callback);

            }
            
        }

        public void UnRegister(WindowEvent windowEvent, System.Action<WindowObject> callback) {

            if (windowEvent == WindowEvent.None) return;

            var registry = this.registry;
            var key = (int)windowEvent;
            if (registry.cacheUnknown.TryGetValue(key, out var actions) == true) {

                actions -= callback;
                registry.cacheUnknown[key] = actions;

            }
            
        }

        public void UnRegister<TState>(TState state, WindowEvent windowEvent, System.Action<WindowObject, TState> callback) {

            if (windowEvent == WindowEvent.None) return;

            var registryInstance = this.GetInstance<TState>();
            var key = (int)windowEvent;
            if (registryInstance.cacheUnknown.TryGetValue(key, out var actions) == true) {

                actions -= callback;
                registryInstance.cacheUnknown[key] = actions;

            }
            
        }

        public void Clear(WindowObject instance) {

            this.ClearRegistry(instance, this.registry);
            foreach (var item in this.registriesGeneric) this.ClearRegistry(instance, item);
            

        }

        public void RegisterOnce(WindowObject instance, WindowEvent windowEvent, System.Action<WindowObject> callback) {

            if (windowEvent == WindowEvent.None) return;

            var registryInstance = this.registry;
            var key = UIWSMath.GetKey(instance.GetHashCode(), (int)windowEvent);
            if (registryInstance.cacheOnce.TryGetValue(key, out var actions) == true) {

                actions += callback;
                registryInstance.cacheOnce[key] = actions;

            } else {

                registryInstance.cacheOnce.Add(key, callback);

            }

            if (registryInstance.objects.ContainsKey(key) == false) registryInstance.objects.Add(key, new Info() { instance = instance, name = instance.name });

        }

        public void RegisterOnce<TState>(TState state, WindowObject instance, WindowEvent windowEvent, System.Action<WindowObject, TState> callback) {

            if (windowEvent == WindowEvent.None) return;

            var registryInstance = this.GetInstance<TState>();
            var key = UIWSMath.GetKey(instance.GetHashCode(), (int)windowEvent);
            if (registryInstance.cacheOnce.TryGetValue(key, out var actions) == true) {

                actions += callback;
                registryInstance.cacheOnce[key] = actions;

            } else {

                registryInstance.cacheOnce.Add(key, callback);

            }

            if (registryInstance.objects.ContainsKey(key) == false) registryInstance.objects.Add(key, new Info() { instance = instance, name = instance.name });

        }

        private Registry<TState> GetInstance<TState>() {
            if (Registry<TState>.isCreated == false) {
                Registry<TState>.isCreated = true;
                this.registriesGeneric.Add(Registry<TState>.instance);
            }
            return Registry<TState>.instance;
        }

        public void Register(WindowObject instance, WindowEvent windowEvent, System.Action<WindowObject> callback) {

            if (windowEvent == WindowEvent.None) return;

            var registryInstance = this.registry;
            var key = UIWSMath.GetKey(instance.GetHashCode(), (int)windowEvent);
            if (registryInstance.cache.TryGetValue(key, out var actions) == true) {

                actions += callback;
                registryInstance.cache[key] = actions;

            } else {

                registryInstance.cache.Add(key, callback);

            }
            
            if (registryInstance.objects.ContainsKey(key) == false) registryInstance.objects.Add(key, new Info() { instance = instance, name = instance.name });

        }

        public void Register<TState>(TState state, WindowObject instance, WindowEvent windowEvent, System.Action<WindowObject, TState> callback) {

            if (windowEvent == WindowEvent.None) return;

            var registryInstance = this.GetInstance<TState>();
            var key = UIWSMath.GetKey(instance.GetHashCode(), (int)windowEvent);
            if (registryInstance.cache.TryGetValue(key, out var actions) == true) {

                actions += callback;
                registryInstance.cache[key] = actions;

            } else {

                registryInstance.cache.Add(key, callback);

            }
            
            if (registryInstance.objects.ContainsKey(key) == false) registryInstance.objects.Add(key, new Info() { instance = instance, name = instance.name });

        }

        public void UnRegister(WindowObject instance, WindowEvent windowEvent, System.Action<WindowObject> callback) {

            if (windowEvent == WindowEvent.None) return;

            var registryInstance = this.registry;
            var key = UIWSMath.GetKey(instance.GetHashCode(), (int)windowEvent);
            if (registryInstance.cache.TryGetValue(key, out var actions) == true) {

                actions -= callback;
                registryInstance.cache[key] = actions;

            }

        }

        public void UnRegister<TState>(TState state, WindowObject instance, WindowEvent windowEvent, System.Action<WindowObject, TState> callback) {

            if (windowEvent == WindowEvent.None) return;

            var registryInstance = this.GetInstance<TState>();
            var key = UIWSMath.GetKey(instance.GetHashCode(), (int)windowEvent);
            if (registryInstance.cache.TryGetValue(key, out var actions) == true) {

                actions -= callback;
                registryInstance.cache[key] = actions;

            }

        }

        public void UnRegister(WindowObject instance, WindowEvent windowEvent) {

            if (windowEvent == WindowEvent.None) return;

            var registryInstance = this.registry;
            var key = UIWSMath.GetKey(instance.GetHashCode(), (int)windowEvent);
            if (registryInstance.cache.ContainsKey(key) == true) {

                registryInstance.cache[key] = null;

            }

        }

        public void UnRegister<TState>(TState state, WindowObject instance, WindowEvent windowEvent) {

            if (windowEvent == WindowEvent.None) return;

            var registryInstance = this.GetInstance<TState>();
            var key = UIWSMath.GetKey(instance.GetHashCode(), (int)windowEvent);
            if (registryInstance.cache.ContainsKey(key) == true) {

                registryInstance.cache[key] = null;

            }

        }

    }

}
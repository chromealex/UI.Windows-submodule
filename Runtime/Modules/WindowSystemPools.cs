using System.Collections.Generic;

[assembly: System.Runtime.CompilerServices.InternalsVisibleToAttribute("UI.Windows.Editor")]

namespace UnityEngine.UI.Windows.Modules {

    using Utilities;

    public interface IOnPoolGet {
        void OnPoolGet();
    }

    public interface IOnPoolAdd {
        void OnPoolAdd();
    }
    
    public class WindowSystemPools : MonoBehaviour {

        internal struct PrefabInstance : System.IEquatable<PrefabInstance> {

            private int hashCode;
            private int instanceId;

            public PrefabInstance(Object obj) {
                this.hashCode = obj.GetHashCode();
                this.instanceId = obj.GetInstanceID();
            }
            
            public static bool operator ==(PrefabInstance a, PrefabInstance b) {
                return a.hashCode == b.hashCode && a.instanceId == b.instanceId;
            }

            public static bool operator !=(PrefabInstance a, PrefabInstance b) {
                return !(a == b);
            }

            public bool Equals(PrefabInstance other) {
                return this.hashCode == other.hashCode && this.instanceId == other.instanceId;
            }

            public override bool Equals(object obj) {
                return obj is PrefabInstance other && this.Equals(other);
            }

            public override int GetHashCode() {
                return System.HashCode.Combine(this.hashCode, this.instanceId);
            }

            public override string ToString() {
                #if UNITY_EDITOR
                return $"#{this.instanceId} {UnityEditor.EditorUtility.InstanceIDToObject(this.instanceId)}";
                #else
                return $"#{this.instanceId} hash {this.hashCode}";
                #endif
            }

        }
        
        internal readonly HashSet<PrefabInstance> registeredPrefabs = new HashSet<PrefabInstance>();
        internal readonly Dictionary<PrefabInstance, Stack<Component>> prefabToPooledInstances = new Dictionary<PrefabInstance, Stack<Component>>();
        internal readonly Dictionary<Component, PrefabInstance> instanceOnSceneToPrefab = new Dictionary<Component, PrefabInstance>();

        public void Clean() {
            
            this.CleanUpNullInstances();
            
            this.registeredPrefabs.Clear();
            foreach (var kv in this.instanceOnSceneToPrefab) {
                this.registeredPrefabs.Add(kv.Value);
            }

            foreach (var kv in this.prefabToPooledInstances) {
                
                foreach (var component in kv.Value) {

                    if (component != null) {
                        
                        if (component is WindowObject wo) {
                            wo.DoDeInit();
                        }
                        GameObject.DestroyImmediate(component.gameObject);
                        
                    }
                    
                }
                
            }
            this.prefabToPooledInstances.Clear();
            
        }

        public void CreatePool<T>(T prefab) where T : Component {
            this.registeredPrefabs.Add(new PrefabInstance(prefab));
        }

        public bool RemoveInstance<T>(T instance) where T : Component {

            foreach (var kv in this.prefabToPooledInstances) {

                foreach (var item in kv.Value) {

                    if (item == instance) {

                        var list = UnityEngine.Pool.ListPool<Component>.Get();
                        foreach (var stackItem in kv.Value) {
                            if (item != instance) list.Add(stackItem);
                        }

                        if (list.Count > 0) {
                            kv.Value.Clear();
                            for (int i = 0; i < list.Count; ++i) {
                                kv.Value.Push(list[i]);
                            }
                        } else {
                            kv.Value.Clear();
                            this.prefabToPooledInstances.Remove(kv.Key);
                        }
                        
                        UnityEngine.Pool.ListPool<Component>.Release(list);

                        Object.DestroyImmediate(instance.gameObject);
                        this.CleanUpNullInstances();
                        
                        return true;

                    }
                    
                }
                
            }

            return false;

        }

        private void CleanUpNullInstances() {

            var list = UnityEngine.Pool.DictionaryPool<Component, PrefabInstance>.Get();
            foreach (var kv in this.instanceOnSceneToPrefab) {
                var comp = kv.Key;
                if (comp != null) {
                    list.Add(comp, kv.Value);
                }
            }
            
            this.instanceOnSceneToPrefab.Clear();
            foreach (var kv in list) {
                this.instanceOnSceneToPrefab.Add(kv.Key, kv.Value);
            }
            
            UnityEngine.Pool.DictionaryPool<Component, PrefabInstance>.Release(list);

        }

        public void RemovePool<T>(T prefab) where T : Component {
            if (this.registeredPrefabs.Remove(new PrefabInstance(prefab)) == true) {
                this.Clear(prefab);
            }
        }

        public void Clear<T>(T prefab) where T : Component {

            var list = UnityEngine.Pool.ListPool<Component>.Get();
            foreach (var instance in this.instanceOnSceneToPrefab) {
                if (instance.Value == new PrefabInstance(prefab)) {
                    list.Add(instance.Key);
                }
            }

            foreach (var item in list) {
                this.instanceOnSceneToPrefab.Remove(item);
            }
            
            UnityEngine.Pool.ListPool<Component>.Release(list);

        }

        public T Spawn<T>(T prefab, Transform root) where T : Component {
            return this.Spawn(prefab, root, out _);
        }

        public T Spawn<T>(T prefab, Transform root, out bool fromPool) where T : Component {

            T result = null;

            var prefabInstance = new PrefabInstance(prefab);
            if (this.registeredPrefabs.Contains(prefabInstance) == false) {

                fromPool = false;
                result = Object.Instantiate(prefab, root);

            } else {

                if (this.prefabToPooledInstances.TryGetValue(prefabInstance, out var stack) == true && stack.Count > 0) {

                    var instance = stack.Pop();
                    if (instance == null) {

                        return this.Spawn(prefab, root, out fromPool);

                    } else {

                        UIWSUtils.SetParent(instance.transform, root);
                        if (instance.gameObject.activeSelf == false) instance.gameObject.SetActive(true);
                        this.instanceOnSceneToPrefab.Add(instance, prefabInstance);
                        fromPool = true;

                        result = (T)instance;

                    }

                } else {

                    var instance = Object.Instantiate(prefab, root);
                    if (instance.gameObject.activeSelf == false) instance.gameObject.SetActive(true);
                    this.instanceOnSceneToPrefab.Add(instance, prefabInstance);
                    fromPool = false;

                    result = instance;

                }

            }

            if (fromPool == true && result is IOnPoolGet onPoolPull) {
                onPoolPull.OnPoolGet();
            }

            if (result is WindowObject windowObject) {
                windowObject.prefabSource = prefab as WindowObject;
            }

            return result;

        }

        public void Despawn<T>(T instance, System.Action<T> onDestroy = null) where T : Component {

            if (instance is WindowObject windowObject) {
                windowObject.prefabSource = null;
            }

            if (instance is IOnPoolAdd onPoolPush) {
                onPoolPush.OnPoolAdd();
            }

            if (this.instanceOnSceneToPrefab.Remove(instance, out var prefab) == true) {

                if (this.prefabToPooledInstances.TryGetValue(prefab, out var stack) == true) {
                    stack.Push(instance);
                } else {
                    stack = new Stack<Component>();
                    stack.Push(instance);
                    this.prefabToPooledInstances.Add(prefab, stack);
                }

                instance.gameObject.SetActive(false);
                UIWSUtils.SetParent(instance.transform, this.transform);

            } else {

                if (onDestroy != null) onDestroy.Invoke(instance);
                Object.Destroy(instance.gameObject);

            }

        }

    }

}
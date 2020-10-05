using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[assembly: System.Runtime.CompilerServices.InternalsVisibleToAttribute("UI.Windows.Editor")]

namespace UnityEngine.UI.Windows.Modules {

    using Utilities;

    public class WindowSystemPools : MonoBehaviour {

        internal HashSet<int> registeredPrefabs = new HashSet<int>();
        internal Dictionary<int, Stack<Component>> prefabToPooledInstances = new Dictionary<int, Stack<Component>>();
        internal Dictionary<Component, int> instanceOnSceneToPrefab = new Dictionary<Component, int>();

        public void CreatePool<T>(T prefab) where T : Component {

            var key = prefab.GetHashCode();
            if (this.registeredPrefabs.Contains(key) == false) {

                this.registeredPrefabs.Add(key);

            }

        }

        public bool RemoveInstance<T>(T instance) where T : Component {

            foreach (var kv in this.prefabToPooledInstances) {

                foreach (var item in kv.Value) {

                    if (item == instance) {

                        var list = new List<Component>();
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

                        Object.DestroyImmediate(instance.gameObject);
                        this.CleanUpNullInstances();
                        
                        return true;

                    }
                    
                }
                
            }

            return false;

        }

        private void CleanUpNullInstances() {

            var list = new Dictionary<Component, int>();
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

        }

        public void RemovePool<T>(T prefab) where T : Component {

            var key = prefab.GetHashCode();
            if (this.registeredPrefabs.Contains(key) == true) {

                this.Clear(prefab);

                this.registeredPrefabs.Remove(key);

            }

        }

        public void Clear<T>(T prefab) where T : Component {

            var key = prefab.GetHashCode();

            var list = new List<Component>();
            foreach (var instance in this.instanceOnSceneToPrefab) {

                if (instance.Value == key) {

                    list.Add(instance.Key);

                }

            }

            foreach (var item in list) {

                this.instanceOnSceneToPrefab.Remove(item);

            }

        }

        public T Spawn<T>(T prefab, Transform root) where T : Component {

            return this.Spawn(prefab, root, out _);

        }

        public T Spawn<T>(T prefab, Transform root, out bool fromPool) where T : Component {

            var key = prefab.GetHashCode();
            if (this.registeredPrefabs.Contains(key) == false) {

                fromPool = false;
                return Object.Instantiate(prefab, root);

            }

            if (this.prefabToPooledInstances.TryGetValue(key, out var stack) == true && stack.Count > 0) {

                var instance = stack.Pop();
                UIWSUtils.SetParent(instance.transform, root);
                if (instance.gameObject.activeSelf == false) instance.gameObject.SetActive(true);
                this.instanceOnSceneToPrefab.Add(instance, key);
                fromPool = true;
                return (T)instance;

            } else {

                var instance = Object.Instantiate(prefab, root);
                if (instance.gameObject.activeSelf == false) instance.gameObject.SetActive(true);
                this.instanceOnSceneToPrefab.Add(instance, key);
                fromPool = false;
                return instance;

            }

        }

        public void Despawn<T>(T instance, System.Action<T> onDestroy = null) where T : Component {

            if (this.instanceOnSceneToPrefab.TryGetValue(instance, out var prefabKey) == true) {

                this.instanceOnSceneToPrefab.Remove(instance);

                if (this.prefabToPooledInstances.TryGetValue(prefabKey, out var stack) == true) {

                    stack.Push(instance);

                } else {

                    stack = new Stack<Component>();
                    stack.Push(instance);
                    this.prefabToPooledInstances.Add(prefabKey, stack);

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
namespace UnityEngine.UI.Windows {

    public struct CallbackRegistries {

        public abstract class RegistryBase {

            public abstract void Invoke();
            public abstract void Recycle();

        }

        public class RegistryNoState : RegistryBase {

            public System.Action callback;

            public override void Invoke() {

                this.callback?.Invoke();

            }

            public override void Recycle() {
                
                this.callback = null;
                PoolClass<RegistryNoState>.Recycle(this);
                
            }

            public bool Remove(System.Action callback) {

                if (this.callback == callback) {
                    
                    return true;
                    
                }

                return false;

            }

        }

        public class Registry<T> : RegistryBase where T : System.IEquatable<T> {

            public System.Action<T> callback;
            public T data;

            public override void Invoke() {

                this.callback?.Invoke(this.data);

            }

            public override void Recycle() {

                this.callback = null;
                this.data = default;
                PoolClass<Registry<T>>.Recycle(this);
                
            }

            public bool Remove(T state, System.Action<T> callback) {

                if (callback == null && this.data.Equals(state) == true) return false;
                
                if (this.callback == callback) {
                    
                    return true;
                    
                }

                return false;

            }

        }

        private System.Collections.Generic.List<RegistryBase> list;
        private int count;
        
        public int Count {
            get {
                return this.count;
            }
        }

        public void Initialize() {

            if (this.list == null) {
                this.list = PoolClass<System.Collections.Generic.List<RegistryBase>>.Spawn();
                this.list.Clear();
            }

        }

        public void DeInitialize() {

            if (this.list != null) {
                this.Clear();
                PoolClass<System.Collections.Generic.List<RegistryBase>>.Recycle(ref this.list);
            }

        }

        public void Remove(System.Action callback) {

            if (this.list == null) return;

            for (var i = 0; i < this.list.Count; ++i) {

                var item = this.list[i];
                if (item is RegistryNoState reg) {

                    if (reg.Remove(callback) == true) {
                        
                        this.list.RemoveAt(i);
                        --this.count;
                        --i;
                        
                    }

                }
                
            }

        }

        public void Remove<TState>(TState state, System.Action<TState> callback) where TState : System.IEquatable<TState> {

            if (this.list == null) return;

            for (var i = 0; i < this.list.Count; ++i) {

                var item = this.list[i];
                if (item is Registry<TState> reg) {

                    if (reg.Remove(state, callback) == true) {

                        this.list.RemoveAt(i);
                        --this.count;
                        --i;
                        
                    }

                }
                
            }
            
        }

        public void Add(System.Action callback) {
            
            this.Initialize();
            
            var item = PoolClass<RegistryNoState>.Spawn();
            item.callback = callback;
            this.list.Add(item);
            ++this.count;

        }

        public void Add<TState>(TState state, System.Action<TState> callback) where TState : System.IEquatable<TState> {

            this.Initialize();

            var item = PoolClass<Registry<TState>>.Spawn();
            item.data = state;
            item.callback = callback;
            this.list.Add(item);
            ++this.count;

        }

        public void Clear() {

            if (this.list == null) return;

            for (var i = 0; i < this.list.Count; ++i) {

                var item = this.list[i];
                item.Recycle();

            }
            this.list.Clear();
            this.count = 0;
            
        }

        public void Invoke() {

            if (this.list == null) return;

            var copy = PoolClass<System.Collections.Generic.List<RegistryBase>>.Spawn();
            copy.Clear();
            for (var i = 0; i < this.list.Count; ++i) copy.Add(this.list[i]);
            
            for (var i = 0; i < copy.Count; ++i) {

                var item = copy[i];
                item.Invoke();
                
            }

            PoolClass<System.Collections.Generic.List<RegistryBase>>.Recycle(ref copy);

        }

    }
    
}
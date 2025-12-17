namespace UnityEngine.UI.Windows {

    public struct Callback : System.IEquatable<Callback> {

        private abstract class CallbackData {

            public object callbackObj;
            public System.Action<Callback> callback;

            public virtual void Dispose() {
                this.callbackObj = null;
            }

            public override int GetHashCode() {
                return System.HashCode.Combine(this.callbackObj, this.callback);
            }

        }
        
        private class CallbackData<T> : CallbackData {
            
            public T obj;
            
            public override void Dispose() {
                base.Dispose();
                PoolClass<CallbackData<T>>.Recycle(this);
            }
            
            public override int GetHashCode() {
                return System.HashCode.Combine(this.callbackObj, this.callback, this.obj);
            }

        }
        
        private CallbackData data;
        
        public bool IsCreated => this.data != null;

        public void Set<T>(WindowObject windowObject, T data, System.Action<T> callback) {
            var inst = PoolClass<CallbackData<T>>.Spawn();
            inst.obj = data;
            this.data = inst;
            this.data.callbackObj = callback;
            this.data.callback = static (x) => {
                ((System.Action<T>)x.data.callbackObj)?.Invoke(((CallbackData<T>)x.data).obj);
            };
            WindowSystem.GetEvents().RegisterOnce(this, windowObject, WindowEvent.OnHideEnd, static (x, obj) => {
                obj.Dispose();
            });
        }

        private void Dispose() {
            if (this.IsCreated == false) return;
            this.data.Dispose();
            this = default;
        }
        
        public void Invoke() {
            this.data?.callback?.Invoke(this);
        }

        public bool Equals(Callback other) {
            return Equals(this.data, other.data);
        }

        public override bool Equals(object obj) {
            return obj is Callback other && this.Equals(other);
        }

        public override int GetHashCode() {
            return (this.data != null ? this.data.GetHashCode() : 0);
        }

    }

    public struct CallbackRegistries : System.IEquatable<CallbackRegistries> {

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

            public void Invoke(T obj) {

                this.callback?.Invoke(obj);

            }

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
        
        public int Count => this.list.Count;

        public void InitializeAuto(WindowObject windowObject) {
            this.Initialize();
            WindowSystem.GetEvents().RegisterOnce(this, windowObject, WindowEvent.OnDeInitialize, static (obj, state) => {
                state.DeInitialize();
            });
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

        }

        public void Add<TState>(TState state, System.Action<TState> callback) where TState : System.IEquatable<TState> {

            this.Initialize();

            var item = PoolClass<Registry<TState>>.Spawn();
            item.data = state;
            item.callback = callback;
            this.list.Add(item);

        }

        public void Add<T>(System.Action<T> callback) where T : System.IEquatable<T> {
            
            this.Initialize();
            
            var item = PoolClass<Registry<T>>.Spawn();
            item.callback = callback;
            this.list.Add(item);

        }

        public void Clear() {

            if (this.list == null) return;

            for (var i = 0; i < this.list.Count; ++i) {

                var item = this.list[i];
                item.Recycle();

            }
            this.list.Clear();
            
        }

        public void Invoke() {

            if (this.list == null) return;

            var copy = PoolClass<System.Collections.Generic.List<RegistryBase>>.Spawn();
            copy.Clear();
            copy.AddRange(this.list);
            
            for (var i = 0; i < copy.Count; ++i) {

                var item = copy[i];
                item.Invoke();
                
            }

            PoolClass<System.Collections.Generic.List<RegistryBase>>.Recycle(ref copy);

        }

        public void Invoke<T>(T obj) where T : System.IEquatable<T> {

            if (this.list == null) return;

            var copy = PoolClass<System.Collections.Generic.List<Registry<T>>>.Spawn();
            copy.Clear();
            foreach (var item in this.list) {
                if (item is Registry<T> reg) {
                    copy.Add(reg);
                }
            }
            
            for (var i = 0; i < copy.Count; ++i) {

                var item = copy[i];
                item.Invoke(obj);
                
            }

            PoolClass<System.Collections.Generic.List<Registry<T>>>.Recycle(ref copy);

        }

        public bool Equals(CallbackRegistries other) {
            return Equals(this.list, other.list);
        }

        public override bool Equals(object obj) {
            return obj is CallbackRegistries other && this.Equals(other);
        }

        public override int GetHashCode() {
            return System.HashCode.Combine(this.list);
        }

    }
    
}
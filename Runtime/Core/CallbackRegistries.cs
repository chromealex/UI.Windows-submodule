namespace UnityEngine.UI.Windows {

    public struct Callback : System.IEquatable<Callback> {

        public abstract class CallbackData {

            public object callbackObj;
            public System.Action<Callback> callback;

            public virtual void Dispose() {
                this.callbackObj = null;
            }

            public override int GetHashCode() {
                return System.HashCode.Combine(this.callbackObj, this.callback);
            }

        }
        
        public class CallbackData<T> : CallbackData {
            
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

        public CallbackData<T> Set<T>(T data, System.Action<T, CallbackData<T>> callback) {
            var inst = PoolClass<CallbackData<T>>.Spawn();
            inst.obj = data;
            this.data = inst;
            this.data.callbackObj = callback;
            this.data.callback = static (x) => {
                var cbk = (CallbackData<T>)x.data;
                ((System.Action<T, CallbackData<T>>)x.data.callbackObj)?.Invoke(cbk.obj, cbk);
            };
            return inst;
        }

        public CallbackData<T> Set<T>(T data, System.Action<T> callback) {
            var inst = PoolClass<CallbackData<T>>.Spawn();
            inst.obj = data;
            this.data = inst;
            this.data.callbackObj = callback;
            this.data.callback = static (x) => {
                ((System.Action<T>)x.data.callbackObj)?.Invoke(((CallbackData<T>)x.data).obj);
            };
            return inst;
        }

        public CallbackData<T> Set<T>(WindowObject windowObject, T data, System.Action<T> callback) {
            var inst = this.Set(data, callback);
            WindowSystem.GetEvents().RegisterOnce(this, windowObject, WindowEvent.OnHideEnd, static (x, obj) => {
                obj.Dispose();
            });
            return inst;
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

    public struct CallbackHandler {

        public uint id;
        public uint index;

    }

    public struct CallbackRegistries : System.IEquatable<CallbackRegistries> {

        public abstract class RegistryBase {

            public uint id;
            public abstract void Invoke();
            public abstract void Recycle();

        }

        public abstract class RegistryBase<T> : RegistryBase {

            public abstract void Invoke(T obj);

        }

        public class RegistryNoState : RegistryBase {

            public System.Action callback;

            public override void Invoke() {

                this.callback?.Invoke();

            }

            public override void Recycle() {
                
                this.id = 0u;
                this.callback = null;
                PoolClass<RegistryNoState>.Recycle(this);
                
            }

        }

        public class Registry<T> : RegistryBase<T> {

            public System.Action<T> callback;
            public T data;

            public override void Invoke(T obj) {

                this.callback?.Invoke(obj);

            }

            public override void Invoke() {

                this.callback?.Invoke(this.data);

            }

            public override void Recycle() {

                this.id = 0u;
                this.callback = null;
                this.data = default;
                PoolClass<Registry<T>>.Recycle(this);
                
            }

        }

        public class RegistryHandler<T> : RegistryBase<T> {

            public System.Action<T, CallbackHandler> callback;
            public T data;
            public CallbackHandler handler;

            public override void Invoke(T obj) {

                this.callback?.Invoke(obj, this.handler);

            }

            public override void Invoke() {

                this.callback?.Invoke(this.data, this.handler);

            }

            public override void Recycle() {

                this.id = 0u;
                this.callback = null;
                this.data = default;
                PoolClass<RegistryHandler<T>>.Recycle(this);
                
            }

        }

        private System.Collections.Generic.List<RegistryBase> list;
        private static uint nextId;

        public int Count => this.list?.Count ?? 0;

        public void InitializeAuto(WindowObject windowObject) {
            this.Initialize();
            WindowSystem.GetEvents().RegisterOnce(this, windowObject, WindowEvent.OnDeInitialized, static (obj, state) => {
                state.DeInitialize();
            });
        }

        public void Initialize() {

            if (nextId == uint.MaxValue) nextId = 0u;
            
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

        public bool Remove(CallbackHandler handler) {

            if (this.list == null) return false;

            for (var i = handler.index; i < this.list.Count; ++i) {
                var item = this.list[(int)i];
                if (item.id == handler.id) {
                    this.list.RemoveAt((int)i);
                    return true;
                }
            }
            
            for (var i = 0u; i < handler.index; ++i) {
                var item = this.list[(int)i];
                if (item.id == handler.id) {
                    this.list.RemoveAt((int)i);
                    return true;
                }
            }

            return false;

        }

        public CallbackHandler Add(System.Action callback) {
            
            this.Initialize();
            
            var item = PoolClass<RegistryNoState>.Spawn();
            item.callback = callback;
            item.id = ++nextId;
            this.list.Add(item);

            return new CallbackHandler() {
                id = item.id,
                index = (uint)this.list.Count - 1u,
            };

        }

        public CallbackHandler Add<TState>(TState state, System.Action<TState> callback) {

            this.Initialize();

            var item = PoolClass<Registry<TState>>.Spawn();
            item.data = state;
            item.callback = callback;
            item.id = ++nextId;
            this.list.Add(item);

            return new CallbackHandler() {
                id = item.id,
                index = (uint)this.list.Count - 1u,
            };

        }

        public CallbackHandler Add<TState>(TState state, System.Action<TState, CallbackHandler> callback) {

            this.Initialize();

            var item = PoolClass<RegistryHandler<TState>>.Spawn();
            item.data = state;
            item.callback = callback;
            item.id = ++nextId;
            this.list.Add(item);

            return new CallbackHandler() {
                id = item.id,
                index = (uint)this.list.Count - 1u,
            };

        }

        public CallbackHandler Add<T>(System.Action<T> callback) {
            
            this.Initialize();
            
            var item = PoolClass<Registry<T>>.Spawn();
            item.callback = callback;
            item.id = ++nextId;
            this.list.Add(item);

            return new CallbackHandler() {
                id = item.id,
                index = (uint)this.list.Count - 1u,
            };

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

            var copy = PoolClass<System.Collections.Generic.List<RegistryBase<T>>>.Spawn();
            copy.Clear();
            foreach (var item in this.list) {
                if (item is RegistryBase<T> reg) {
                    copy.Add(reg);
                }
            }
            
            for (var i = 0; i < copy.Count; ++i) {
                var item = copy[i];
                item.Invoke(obj);
            }

            PoolClass<System.Collections.Generic.List<RegistryBase<T>>>.Recycle(ref copy);

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
    
    public struct CallbackRegistries<TInvokeData> : System.IEquatable<CallbackRegistries<TInvokeData>> {

        public abstract class RegistryBase {

            public uint id;
            public abstract void Invoke(TInvokeData data);
            public abstract void Recycle();

        }

        public abstract class RegistryBase<T> : RegistryBase {

            public abstract void Invoke(T obj, TInvokeData data);

        }

        public class RegistryNoState : RegistryBase {

            public System.Action<TInvokeData> callback;

            public override void Invoke(TInvokeData data) {

                this.callback?.Invoke(data);

            }

            public override void Recycle() {

                this.id = 0u;
                this.callback = null;
                PoolClass<RegistryNoState>.Recycle(this);
                
            }

        }

        public class Registry<T> : RegistryBase<T> {

            public System.Action<T, TInvokeData> callback;
            public T data;

            public override void Invoke(T obj, TInvokeData data) {

                this.callback?.Invoke(obj, data);

            }

            public override void Invoke(TInvokeData data) {

                this.callback?.Invoke(this.data, data);

            }

            public override void Recycle() {

                this.id = 0u;
                this.callback = null;
                this.data = default;
                PoolClass<Registry<T>>.Recycle(this);
                
            }

        }

        public class RegistryHandler<T> : RegistryBase<T> {

            public System.Action<T, TInvokeData, CallbackHandler> callback;
            public T data;
            public CallbackHandler handler;

            public override void Invoke(T obj, TInvokeData data) {

                this.callback?.Invoke(obj, data, this.handler);

            }

            public override void Invoke(TInvokeData data) {

                this.callback?.Invoke(this.data, data, this.handler);

            }

            public override void Recycle() {

                this.id = 0u;
                this.callback = null;
                this.data = default;
                PoolClass<RegistryHandler<T>>.Recycle(this);
                
            }

        }

        private System.Collections.Generic.List<RegistryBase> list;
        private static uint nextId;
        
        public int Count => this.list?.Count ?? 0;

        public void InitializeAuto(WindowObject windowObject) {
            this.Initialize();
            WindowSystem.GetEvents().RegisterOnce(this, windowObject, WindowEvent.OnDeInitialized, static (obj, state) => {
                state.DeInitialize();
            });
        }

        public void Initialize() {

            if (nextId == uint.MaxValue) nextId = 0u;
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

        public bool Remove(CallbackHandler handler) {

            if (this.list == null) return false;

            for (var i = handler.index; i < this.list.Count; ++i) {
                var item = this.list[(int)i];
                if (item.id == handler.id) {
                    this.list.RemoveAt((int)i);
                    return true;
                }
            }
            
            for (var i = 0u; i < handler.index; ++i) {
                var item = this.list[(int)i];
                if (item.id == handler.id) {
                    this.list.RemoveAt((int)i);
                    return true;
                }
            }

            return false;

        }

        public CallbackHandler Add(System.Action<TInvokeData> callback) {
            
            this.Initialize();
            
            var item = PoolClass<RegistryNoState>.Spawn();
            item.callback = callback;
            item.id = ++nextId;
            this.list.Add(item);

            return new CallbackHandler() {
                id = item.id,
                index = (uint)this.list.Count - 1u,
            };

        }

        public CallbackHandler Add<TState>(TState state, System.Action<TState, TInvokeData> callback) where TState : System.IEquatable<TState> {

            this.Initialize();

            var item = PoolClass<Registry<TState>>.Spawn();
            item.data = state;
            item.callback = callback;
            item.id = ++nextId;
            this.list.Add(item);

            return new CallbackHandler() {
                id = item.id,
                index = (uint)this.list.Count - 1u,
            };

        }

        public CallbackHandler Add<T>(System.Action<T, TInvokeData> callback) where T : System.IEquatable<T> {
            
            this.Initialize();
            
            var item = PoolClass<Registry<T>>.Spawn();
            item.callback = callback;
            item.id = ++nextId;
            this.list.Add(item);

            return new CallbackHandler() {
                id = item.id,
                index = (uint)this.list.Count - 1u,
            };

        }

        public void Clear() {

            if (this.list == null) return;

            for (var i = 0; i < this.list.Count; ++i) {
                var item = this.list[i];
                item.Recycle();
            }
            this.list.Clear();
            
        }

        public void Invoke(TInvokeData data) {

            if (this.list == null) return;

            var copy = PoolClass<System.Collections.Generic.List<RegistryBase>>.Spawn();
            copy.Clear();
            copy.AddRange(this.list);
            
            for (var i = 0; i < copy.Count; ++i) {
                var item = copy[i];
                item.Invoke(data);
            }

            PoolClass<System.Collections.Generic.List<RegistryBase>>.Recycle(ref copy);

        }

        public void Invoke<T>(T obj, TInvokeData data) where T : System.IEquatable<T> {

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
                item.Invoke(obj, data);
            }

            PoolClass<System.Collections.Generic.List<Registry<T>>>.Recycle(ref copy);

        }

        public bool Equals(CallbackRegistries<TInvokeData> other) {
            return Equals(this.list, other.list);
        }

        public override bool Equals(object obj) {
            return obj is CallbackRegistries<TInvokeData> other && this.Equals(other);
        }

        public override int GetHashCode() {
            return System.HashCode.Combine(this.list);
        }

    }
    
}
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.UI.Windows {
    
    using UnityEngine.UI.Windows.Utilities;

    public enum UnloadResourceEventType {

        Manually,
        OnHideBegin,
        OnHideEnd,
        OnDeInit,

    }

    public interface IResourceProvider {

        ref Resource GetResource();

    }

    public interface ICustomLoader {

        #if UNITY_6000_0_OR_NEWER
        public Awaitable<T> Load<T>(UnityEngine.UI.Windows.Modules.WindowSystemResources.LoadParameters loadParameters, Resource resource);
        #else
        public Task<T> Load<T>(UnityEngine.UI.Windows.Modules.WindowSystemResources.LoadParameters loadParameters, Resource resource);
        #endif
        void Unload(object obj);

    }

    [System.Serializable]
    public class Resource<T> : IResourceProvider where T : UnityEngine.Object {
        
        private struct AsyncClosure {

            public TaskCompletionSource<T> tcs;
            public Resource<T> resource;
            public System.Action<T> callback;

        }

        [SerializeField]
        private Resource data;
        private T loaded;
        
        internal Resource() { }
        
        ref Resource IResourceProvider.GetResource() => ref this.data;

        #if UNITY_EDITOR
        public void ValidateSource(T resource) {
            var res = Resource<T>.Validate(resource);
            this.data = res.data;
            this.loaded = res.loaded;
        }
        
        public static Resource<T> Validate(T resource) {
            var res = new Resource<T>();
            res.data.directRef = resource;
            res.data.objectType = Resource.ObjectType.Unknown;
            res.data.guid = UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(resource));
            res.data.type = Resource.Type.Direct;
            res.data.validationRequired = true;
            return res;
        }
        #endif

        public T Load(object handler) {
            if (this.loaded != null) return this.loaded;
            this.loaded = WindowSystem.GetResources().Load<T>(handler, this.data);
            return this.loaded;
        }
        
        public void LoadAsync(object handler, System.Action<T> callback) {

            if (this.loaded != null) {
                callback.Invoke(this.loaded);
            } else {
                var closure = new AsyncClosure {
                    resource = this,
                    callback = callback,
                };
                
                var preloadResult = WindowSystem.GetResources().Load_INTERNAL<T, AsyncClosure>(new UnityEngine.UI.Windows.Modules.WindowSystemResources.LoadParameters() { async = true }, handler, closure, this.data, static (asset, p) => {
                    p.resource.loaded = asset;
                    p.callback.Invoke(p.resource.loaded);
                });
                if (preloadResult.result == false) {
                    Coroutines.Run(preloadResult.op);
                }
            }

        }

        public AssetReferenceT<T> AsAssetReference() {
            if (this.data.type == Resource.Type.Addressables) {
                return new AssetReferenceT<T>(this.data.guid);
            }
            return null;
        }

        public T Get() {
            return this.loaded;
        }

        public void Unload(object handler) {
            WindowSystem.GetResources()?.Delete(handler, ref this.loaded);
            this.loaded = null;
        }
        
        public static implicit operator T(Resource<T> item) {
            return item.loaded;
        }

    }
    
    [System.Serializable]
    public struct Resource : System.IEquatable<Resource> {

        public enum ObjectType {
            Unknown = 0,
            GameObject,
            Component,
            ScriptableObject,
            Sprite,
            Texture,
        }
        
        public enum Type {
            Manual = 0,
            Direct,
            Addressables,
            CustomLoader,
        }

        public Type type;
        public ObjectType objectType;
        public string address;
        public string guid;
        public string subObjectName;
        public Object directRef;
        public bool validationRequired;

        public static Resource Create<TObject>(string address) {
            return new Resource() {
                address = address,
                objectType = UnityEngine.UI.Windows.Modules.WindowSystemResources.GetObjectType<TObject>(),
                type = Type.CustomLoader,
            };
        }

        public bool Equals(Resource other) {
            return this.IsEquals(in other);
        }

        public string GetAddress() {
            var result = this.address;
            if (string.IsNullOrEmpty(this.guid) == false && string.IsNullOrEmpty(this.guid.Trim()) == false) result = this.guid;
            if (string.IsNullOrEmpty(this.subObjectName) == false) return $"{result}[{this.subObjectName}]";
            return result;
        }

        public override string ToString() {
            return $"[Resource] Type: {this.type}, Object Type: {this.objectType}, GUID: {this.guid} ({this.address}, {this.subObjectName}), Direct Reference: {this.directRef}";
        }

        public override int GetHashCode() {
            return (int)this.type ^ (int)this.objectType ^ (this.guid != null ? this.guid.GetHashCode() : 0);
        }

        public bool IsEquals(in Resource other) {
            return this.type == other.type &&
                   this.objectType == other.objectType &&
                   this.address == other.address &&
                   this.guid == other.guid &&
                   this.subObjectName == other.subObjectName &&
                   this.directRef == other.directRef;
        }
        
        public bool IsEmpty() {
            return this.directRef == null && string.IsNullOrEmpty(this.guid) == true;
        }

        public T GetEditorRef<T>() where T : Object {
            return Resource.GetEditorRef<T>(this);
        }

        public static T GetEditorRef<T>(Resource resource) where T : Object {
            return Resource.GetEditorRef<T>(resource.guid, resource.subObjectName, resource.objectType, resource.directRef);
        }

        public static T GetEditorRef<T>(string guid, string subObjectName, ObjectType objectType, Object directRef) where T : Object {
            return Resource.GetEditorRef(guid, subObjectName, typeof(T), objectType, directRef) as T;
        }

        public static Object GetEditorRef(string guid, string subObjectName, System.Type type, ObjectType objectType, Object directRef) {

            #if UNITY_EDITOR
            if (directRef != null) return directRef;
            
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (objectType == ObjectType.Component) {
                
                var go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go == null) return default;
                
                if (type == typeof(Object)) type = typeof(Component);
                return go.GetComponent(type);

            } else if (objectType == ObjectType.Sprite) {
                
                var objs = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var obj in objs) {

                    if (obj.name == subObjectName && obj.GetType().IsSubclassOf(type) == true) {

                        return obj;

                    }
                    
                }
                
            }
            return UnityEditor.AssetDatabase.LoadAssetAtPath(path, type);
            #else
            return default;
            #endif

        }

    }

}

namespace UnityEngine.UI.Windows.Modules {

    using Utilities;
    
    public interface IResourceConstructorContainer {

        void Dispose();

    }

    public interface IResourceConstructor {

    }

    public abstract class ConstructorContainer : IResourceConstructorContainer {

        public abstract void Dispose();
        

    }

    public class ConstructorContainer<T> : ConstructorContainer where T : IResourceConstructor {

        public T constructor;

        public override void Dispose() {
            this.constructor = default;
            PoolClass<ConstructorContainer<T>>.Recycle(this);
        }

    }
    
    public interface IResourceConstructor<T> : IResourceConstructor where T : class {

        T Construct();
        void Deconstruct(ref T obj);

    }

    public struct DefaultConstructor<T> : IResourceConstructor<T> where T : class, new() {

        public T Construct() {

            return new T();

        }

        public void Deconstruct(ref T obj) {

            obj = default;

        }

    }

    public class ResourceTypeAttribute : PropertyAttribute {

        public System.Type type;
        public RequiredType required;

        public ResourceTypeAttribute(System.Type type, RequiredType required = RequiredType.None) {

            this.type = type;
            this.required = required;

        }

    }

    public class WindowSystemResources : MonoBehaviour {

        public readonly struct InternalTask : System.IEquatable<InternalTask> {

            public readonly int resourceId;
            public readonly Resource resourceSource;
            
            public InternalTask(Resource resourceSource) {

                this.resourceId = resourceSource.GetHashCode();
                this.resourceSource = resourceSource;

            }

            public bool Equals(InternalTask other) {

                return other.resourceId == this.resourceId;

            }

            public override int GetHashCode() {

                return this.resourceId;

            }

        }

        public struct DefaultClosureData { }

        public class ClosureResult<T> {

            public T result;
            
        }

        public struct LoadParameters {

            public bool async;

        }

        public class IntResource {

            public object loaded;
            public Resource resource;
            public List<object> references;
            public HashSet<object> handlers;
            public System.Action<WindowSystemResources, object, IResourceConstructorContainer> deconstruct;
            public IResourceConstructorContainer resourceConstructor;

            public int referencesCount => this.references.Count;

            public void Reset() {
                
                this.loaded = null;
                this.resource = default;
                this.references = null;
                this.handlers = null;
                this.deconstruct = null;
                this.resourceConstructor = null;

            }

        }

        public class ResourceComparer : IEqualityComparer<Resource> {
            
            public bool Equals(Resource x, Resource y) {

                return x.Equals(y);

            }

            public int GetHashCode(Resource obj) {

                return obj.GetHashCode();

            }

        }

        public struct CallbackList {

            private List<ObjBase> callbackRegistries;

            private abstract class ObjBase {

                public abstract void Invoke(object obj);
                public abstract void Dispose();

            }

            private class Obj<T> : ObjBase {

                public T obj;
                public System.Action<T, object> action;

                public override void Invoke(object obj) {
                    this.action.Invoke(this.obj, obj);
                }

                public override void Dispose() {
                    this.obj = default;
                    PoolClass<Obj<T>>.Recycle(this);
                }

            }

            public int Count => this.callbackRegistries.Count;

            public void Initialize() {
                this.callbackRegistries = PoolList<ObjBase>.Spawn();
            }

            public void Dispose() {
                foreach (var item in this.callbackRegistries) {
                    item.Dispose();
                }
                PoolList<ObjBase>.Recycle(this.callbackRegistries);
            }
            
            public void Add<T>(T closure, System.Action<T, object> action) {
                var instance = PoolClass<Obj<T>>.Spawn();
                instance.obj = closure;
                instance.action = action;
                this.callbackRegistries.Add(instance);
            }

            public void Invoke(object result) {
                foreach (var item in this.callbackRegistries) {
                    try {
                        item.Invoke(result);
                    } catch (System.Exception ex) {
                        Debug.LogException(ex);
                    }
                }
            }

        }

        private struct RequestLoadItem<T, TClosure> {

            public TClosure data;
            public System.Action<T, TClosure> onComplete;

        }

        private readonly Dictionary<InternalTask, CallbackList> tasks = new Dictionary<InternalTask, CallbackList>();
        private readonly Dictionary<int, HashSet<System.Action>> handlerToTasks = new Dictionary<int, HashSet<System.Action>>();
        private readonly Dictionary<Resource, IntResource> loaded = new Dictionary<Resource, IntResource>(new ResourceComparer());
        private readonly Dictionary<object, IntResource> loadedObjCache = new Dictionary<object, IntResource>();
        private readonly List<object> internalDeleteAllCache = new List<object>();
        public bool showLogs;

        private ICustomLoader customLoader;
        public void SetCustomLoader(ICustomLoader loader) {
            this.customLoader = loader;
        }

        public Dictionary<Resource, IntResource> GetAllObjects() => this.loaded;

        public int GetAllocatedCount() => this.loaded.Count;

        public Dictionary<InternalTask, CallbackList> GetTasks() => this.tasks;

        private bool RequestLoad<T, TClosure>(object handler, TClosure closure, Resource resource, System.Action<T, TClosure> onComplete) where T : class {
            
            var item = new InternalTask(resource);
            if (this.tasks.TryGetValue(item, out var onCompleteActions) == true) {

                onCompleteActions.Add(new RequestLoadItem<T, TClosure>() {
                    data = closure,
                    onComplete = onComplete,
                }, static (closure, obj) => {
                    closure.onComplete.Invoke((T)obj, closure.data);
                });
                this.tasks[item] = onCompleteActions;
                return true;

            } else {
                
                onCompleteActions.Initialize();
                onCompleteActions.Add(new RequestLoadItem<T, TClosure>() {
                    data = closure,
                    onComplete = onComplete,
                }, static (closure, obj) => {
                    closure.onComplete.Invoke((T)obj, closure.data);
                });
                this.tasks.Add(item, onCompleteActions);
                
            }

            return false;

        }

        private void CompleteTask(object handler, Resource resource, object result) {

            var item = new InternalTask(resource);
            if (this.tasks.TryGetValue(item, out var onCompleteActions) == true) {

                onCompleteActions.Invoke(result);
                onCompleteActions.Dispose();
                this.tasks.Remove(item);

            }

        }

        public void LoadAsync<T, TClosure>(LoadParameters loadParameters, object handler, TClosure closure, Resource resource, System.Action<T, TClosure> onComplete) where T : class {

            var preloadResult = this.Load_INTERNAL(loadParameters, handler, closure, resource, onComplete);
            if (preloadResult.result == false) {
                Coroutines.Run(preloadResult.op);
            }
            
        }

        public void LoadAsync<T>(object handler, Resource resource, System.Action<T, DefaultClosureData> onComplete) where T : class {

            this.LoadAsync(handler, new DefaultClosureData(), resource, onComplete);

        }

        public void LoadAsync<T, TClosure>(object handler, TClosure closure, Resource resource, System.Action<T, TClosure> onComplete) where T : class {
            
            var preloadResult = this.Load_INTERNAL(new LoadParameters() { async = true }, handler, closure, resource, onComplete);
            if (preloadResult.result == false) {
                Coroutines.Run(preloadResult.op);
            }
            
        }

        public (bool result, IEnumerator op) LoadAsyncWait<T, TClosure>(LoadParameters loadParameters, object handler, TClosure closure, Resource resource, System.Action<T, TClosure> onComplete) where T : class {
            
            return this.Load_INTERNAL(loadParameters, handler, closure, resource, onComplete);
            
        }

        public (bool result, IEnumerator op) LoadAsyncWait<T, TClosure>(object handler, TClosure closure, Resource resource, System.Action<T, TClosure> onComplete) where T : class {
            
            return this.Load_INTERNAL(new LoadParameters() { async = true }, handler, closure, resource, onComplete);
            
        }
        
        #if UNITY_6000_0_OR_NEWER
        public async Awaitable<T> LoadAsync<T>(object handler, Resource resource) where T : class {
            var tcs = PoolClass<TaskCompletionSource<T>>.Spawn();
            var preloadResult = this.Load_INTERNAL<T, TaskCompletionSource<T>>(new LoadParameters() { async = true }, handler, tcs, resource, static (asset, s) => s.SetResult(asset));
            if (preloadResult.result == false) {
                while (preloadResult.op.MoveNext() == true) {
                    await Awaitable.NextFrameAsync();
                }
            }
            var result = tcs.Task.Result;
            PoolClass<TaskCompletionSource<T>>.Recycle(tcs);
            return result;
        }
        #else
        public async ValueTask<T> LoadAsync<T>(object handler, Resource resource) where T : class {
            var tcs = PoolClass<TaskCompletionSource<T>>.Spawn();
            var preloadResult = this.Load_INTERNAL<T, TaskCompletionSource<T>>(new LoadParameters() { async = true }, handler, tcs, resource, static (asset, s) => s.SetResult(asset));
            if (preloadResult.result == false) {
                while (preloadResult.op.MoveNext() == true) {
                    await Task.Yield();
                }
            }

            var result = tcs.Task.Result;
            PoolClass<TaskCompletionSource<T>>.Recycle(tcs);
            return result;
        }
        #endif

        public T Load<T>(object handler, Resource resource) where T : class {
            
            var closure = PoolClass<ClosureResult<T>>.Spawn();
            var preloadResult = this.Load_INTERNAL<T, ClosureResult<T>>(new LoadParameters() { async = false }, handler, closure, resource, static (asset, c) => {
                c.result = asset;
            });
            if (preloadResult.result == false) {
                while (preloadResult.op.MoveNext()) {}
            }
            
            var result = closure.result;
            closure.result = null;
            PoolClass<ClosureResult<T>>.Recycle(ref closure);
            return result;

        }

        private bool IsLoaded(object handler, Resource resource) {

            if (this.loaded.TryGetValue(resource, out var internalResource) == true) {

                this.AddObject(handler, internalResource.loaded, null, resource, internalResource.deconstruct);
                this.CompleteTask(handler, resource, internalResource.loaded);
                return true;
                
            }

            return false;

        }

        internal (bool result, IEnumerator op) Load_INTERNAL<T, TClosure>(LoadParameters loadParameters, object handler, TClosure closure, Resource resource, System.Action<T, TClosure> onComplete) where T : class {
            
            if (typeof(Component).IsAssignableFrom(typeof(T)) == true) {
                resource.objectType = Resource.ObjectType.Component;
            }

            if (resource.type != Resource.Type.Direct) {
                
                if (this.RequestLoad(handler, closure, resource, onComplete) == true) {
                    // We have already subscribed - just quit
                    return (true, null);
                }

                if (this.IsLoaded(handler, resource) == true) {
                    return (true, null);
                }
                
            }

            switch (resource.type) {
                
                case Resource.Type.Manual: {
                    this.CompleteTask(handler, resource, default);
                    break;
                }

                case Resource.Type.Direct: {
                    if (resource.directRef is GameObject go && resource.objectType == Resource.ObjectType.Component) {
                        var direct = go.GetComponent<T>();
                        onComplete.Invoke(direct, closure);
                        return (true, null);
                    } else if (WindowSystem.IsStrictMode() == false && typeof(T).IsAssignableFrom(resource.directRef.GetType()) == true) {
                        onComplete.Invoke((T)(object)resource.directRef, closure);
                        return (true, null);
                    } else if (WindowSystem.IsStrictMode() == true && resource.directRef is T obj) {
                        onComplete.Invoke(obj, closure);
                        return (true, null);
                    }
                    break;
                }
                
                default:
                    var op = this.Load_INTERNAL_YIELD(loadParameters, handler, closure, resource, onComplete);
                    return (false, op);

            }

            return (true, null);

        }

        internal IEnumerator Load_INTERNAL_YIELD<T, TClosure>(LoadParameters loadParameters, object handler, TClosure closure, Resource resource, System.Action<T, TClosure> onComplete) where T : class {

            switch (resource.type) {

                case Resource.Type.Addressables: {
                    if (resource.objectType == Resource.ObjectType.Component) {
                        yield return this.LoadAddressable_INTERNAL<GameObject, T>(loadParameters, handler, resource, static go => {
                            var result = go.GetComponent<T>();
                            if (result == null && WindowSystem.GetResources().showLogs == true) {
                                Debug.LogError($"[ UIWR ] Failed to load resource, gameObject loaded {go}, but component was not found with type {typeof(T)}");
                            }

                            return result;
                        });
                    } else {
                        yield return this.LoadAddressable_INTERNAL<T, T>(loadParameters, handler, resource, r => r);
                    }
                    break;
                }

                case Resource.Type.CustomLoader: {
                    yield return this.LoadCustomLoader_INTERNAL<T>(loadParameters, handler, resource);
                    break;
                }
                
            }

        }

        private IEnumerator LoadCustomLoader_INTERNAL<TResource>(LoadParameters loadParameters, object handler, Resource resource) {

            var task = this.customLoader.Load<TResource>(loadParameters, resource);
            yield return task;
            
            if (task.GetAwaiter().IsCompleted == true) {

                var asset = task.GetAwaiter().GetResult();
                if (asset == null) {

                    this.CompleteTask(handler, resource, default);

                } else {

                    var result = asset;
                    this.AddObject(handler, result, null, resource, static (res, obj, _) => res.customLoader.Unload(obj));
                    this.CompleteTask(handler, resource, result);

                }

            } else {
                
                if (this.showLogs == true) Debug.LogError($"[ UIWR ] Resource failed while loading: {resource}");
                this.CompleteTask(handler, resource, null);
                
            }
            
        }

        private IEnumerator LoadAddressable_INTERNAL<TResource, TResult>(LoadParameters loadParameters, object handler, Resource resource, System.Func<TResource, TResult> converter) where TResult : class {

            var op = Addressables.LoadAssetAsync<TResource>(resource.GetAddress());
            yield return null;
            System.Action cancellationTask = () => { if (op.IsValid() == true) Addressables.Release(op); };
            this.LoadBegin(handler, cancellationTask);            
            #if UNITY_WEBGL
            if (loadParameters.async == false) Debug.LogWarning($"[ UIWR ] LoadAddressable started as async because WebGL doesn't support synchronous loading.\n{resource}");
            #else
            if (loadParameters.async == false) op.WaitForCompletion();
            #endif
            while (op.IsDone == false) yield return null;

            if (op.IsValid() == true && op.Status == AsyncOperationStatus.Succeeded) {

                var asset = op.Result;
                if (asset == null) {

                    this.CompleteTask(handler, resource, default);

                } else {

                    var result = converter.Invoke(asset);
                    this.AddObject(handler, result, null, resource, static (_, obj, _) => {
                        if (obj is Component component) {
                            UnityEngine.AddressableAssets.Addressables.Release(component.gameObject);
                            return;
                        }
                        UnityEngine.AddressableAssets.Addressables.Release(obj);
                    });
                    this.CompleteTask(handler, resource, result);

                }

            } else {
                
                if (this.showLogs == true) Debug.LogError($"[ UIWR ] Resource failed while loading: {resource}\nValid: {op.IsValid()}");
                this.CompleteTask(handler, resource, null);
                            
            }
                        
            this.LoadEnd(handler, cancellationTask);
            
        }
        
        private void LoadBegin(object handler, System.Action cancellationTask) {

            var key = handler.GetHashCode();
            if (this.handlerToTasks.TryGetValue(key, out var list) == true) {
                
                list.Add(cancellationTask);
                
            } else {
                
                list = new HashSet<System.Action>();
                list.Add(cancellationTask);
                this.handlerToTasks.Add(key, list);
                
            }

        }

        private void LoadEnd(object handler, System.Action task) {

            var key = handler.GetHashCode();
            var list = this.handlerToTasks[key];
            list.Remove(task);
            if (list.Count == 0) this.handlerToTasks.Remove(key);

        }

        public void StopLoadAll(object handler) {

            var key = handler.GetHashCode();
            if (this.handlerToTasks.TryGetValue(key, out var list) == true) {

                foreach (var item in list) {

                    item.Invoke();

                }
                
            }

        }

        public T New<T>() where T : class, new() {

            return this.New<T, DefaultConstructor<T>>(null, new DefaultConstructor<T>());

        }

        public T New<T, TConstruct>(TConstruct resourceConstructor) where T : class, new() where TConstruct : IResourceConstructor<T> {

            return this.New<T, TConstruct>(null, resourceConstructor);

        }

        public T New<T>(object handler) where T : class, new() {

            return this.New<T, DefaultConstructor<T>>(handler, new DefaultConstructor<T>());

        }

        public T New<T, TConstruct>(object handler, TConstruct resourceConstructor) where T : class where TConstruct : IResourceConstructor<T> {

            if (handler == null) handler = this;

            var obj = resourceConstructor.Construct();
            var constructor = PoolClass<ConstructorContainer<TConstruct>>.Spawn();
            constructor.constructor = resourceConstructor;
            this.AddObject(handler, obj, constructor, new Resource() { type = Resource.Type.Manual }, static (_, o, resourceConstructor) => {
                var obj = (T)o;
                ((ConstructorContainer<TConstruct>)resourceConstructor).constructor.Deconstruct(ref obj);
            });
            return obj;

        }

        public void Delete<T>(T obj) where T : class {

            this.Delete(null, ref obj);

        }

        public void Delete<T>(ref T obj) where T : class {

            this.Delete(null, ref obj);

        }

        public void Delete<T>(object handler, T obj) where T : class {

            this.Delete(handler, ref obj);

        }

        public void Delete<T>(object handler, ref T obj) where T : class {

            if (Object.ReferenceEquals(obj, null) == true) return;
            if (handler == null) handler = this;

            //Debug.Log("Delete obj: " + handler + " :: " + obj);
            this.RemoveObject(handler, obj);
            obj = null;

        }

        public void DeleteAll(object handler) {

            if (handler == null) handler = this;

            this.internalDeleteAllCache.Clear();
            foreach (var kv in this.loaded) {
                if (kv.Value.handlers.Contains(handler) == true) {
                    foreach (var item in kv.Value.references) {
                        this.internalDeleteAllCache.Add(kv.Value.loaded);
                    }
                }
            }

            foreach (var obj in this.internalDeleteAllCache) {
                this.RemoveObject(handler, obj);
            }
            this.internalDeleteAllCache.Clear();
            
        }
        
        private bool RemoveObject<T>(object handler, T obj) where T : class {

            if (this.loadedObjCache.TryGetValue(obj, out var intResource) == true) {

                if (intResource.handlers.Contains(handler) == true) {

                    var hidx = 0;
                    var count = 0;
                    for (int i = 0; i < intResource.references.Count; ++i) {
                        if (intResource.references[i] == handler) {
                            hidx = i;
                            ++count;
                        }
                    }
                    intResource.references.RemoveAt(hidx);
                    if (count == 1) intResource.handlers.Remove(handler);
                    
                    if (intResource.referencesCount == 0) {

                        if (this.loaded.Remove(intResource.resource) == false) {
                            if (this.showLogs == true) Debug.LogError($"[ UIWR ] Remove resource failed while refCount = 0 and resource is {intResource.resource}");
                        }
                        this.loadedObjCache.Remove(obj);
                        
                        intResource.handlers.Remove(handler);
                        intResource.deconstruct?.Invoke(this, intResource.loaded, intResource.resourceConstructor);
                        intResource.resourceConstructor?.Dispose();
                        PoolHashSet<object>.Recycle(ref intResource.handlers);
                        PoolList<object>.Recycle(ref intResource.references);
                        intResource.Reset();
                        PoolClass<IntResource>.Recycle(intResource);
                        return true;
                        
                    }

                }
                
            }

            return false;

        }

        private void AddObject(object handler, object obj, IResourceConstructorContainer resourceConstructor, Resource resource, System.Action<WindowSystemResources, object, IResourceConstructorContainer> deconstruct) {

            if (this.loaded.TryGetValue(resource, out var intResource) == false) {

                intResource = PoolClass<IntResource>.Spawn();
                intResource.handlers = PoolHashSet<object>.Spawn();
                intResource.references = PoolList<object>.Spawn();
                intResource.loaded = obj;
                intResource.resourceConstructor = resourceConstructor;
                intResource.resource = resource;
                intResource.deconstruct = deconstruct;
                this.loaded.Add(resource, intResource);
                this.loadedObjCache.Add(obj, intResource);

            }
            
            intResource.handlers.Add(handler);
            intResource.references.Add(handler);

        }

        public static Resource.ObjectType GetObjectType(Object obj) {
            return GetObjectType(obj.GetType());
        }

        public static Resource.ObjectType GetObjectType<T>() {
            return GetObjectType(typeof(T));
        }

        public static Resource.ObjectType GetObjectType(System.Type type) {
            var val = Resource.ObjectType.Unknown;
            if (type == typeof(GameObject)) {
                val = Resource.ObjectType.GameObject;
            } else if (type == typeof(Component)) {
                val = Resource.ObjectType.Component;
            } else if (type == typeof(ScriptableObject)) {
                val = Resource.ObjectType.ScriptableObject;
            } else if (type == typeof(Sprite)) {
                val = Resource.ObjectType.Sprite;
            } else if (typeof(Texture).IsAssignableFrom(type) == true) {
                val = Resource.ObjectType.Texture;
            }

            return val;
        }

    }

}
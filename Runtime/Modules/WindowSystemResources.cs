﻿using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows {

    public enum UnloadResourceEventType {

        Manually,
        OnHideBegin,
        OnHideEnd,
        OnDeInit,

    }
    
    [System.Serializable]
    public struct Resource {

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

        }

        [HideInInspector]
        public Type type;
        [HideInInspector]
        public ObjectType objectType;
        [HideInInspector]
        public string guid;
        [HideInInspector]
        public string subObjectName;
        [HideInInspector]
        public Object directRef;

        public override string ToString() {
            
            return "Resource: " + this.type + "/" + this.objectType + "/" + this.guid + ". Ref: " + this.directRef;
            
        }

        public override int GetHashCode() {
            
            return (int)this.type ^ (int)this.objectType ^ this.guid.GetHashCode() ^ (this.directRef != null ? this.directRef.GetHashCode() : 0);
            
        }

        public bool IsEquals(in Resource other) {

            return this.type == other.type &&
                   this.objectType == other.objectType &&
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
    
    public interface IResourceConstructor<T> where T : class {

        T Construct();

    }

    public struct DefaultConstructor<T> : IResourceConstructor<T> where T : class, new() {

        public T Construct() {

            return new T();

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

        public readonly struct InternalResourceItem : System.IEquatable<InternalResourceItem> {

            public readonly object handler;
            public readonly object resource;
            public readonly int resourceId;
            public readonly Resource resourceSource;

            public InternalResourceItem(object handler, object resource, Resource resourceSource) {

                this.handler = handler;
                this.resource = resource;
                this.resourceId = resource.GetHashCode();
                this.resourceSource = resourceSource;

            }

            public InternalResourceItem(object handler, int resourceId) {

                this.handler = handler;
                this.resource = null;
                this.resourceId = resourceId;
                this.resourceSource = default;

            }

            public bool Equals(InternalResourceItem other) {

                return other.resourceId == this.resourceId;

            }

            public override int GetHashCode() {

                return this.resourceId;

            }

        }

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

        private Dictionary<InternalTask, System.Action<object>> tasks = new Dictionary<InternalTask, System.Action<object>>();
        private Dictionary<int, HashSet<InternalResourceItem>> handlerToObjects = new Dictionary<int, HashSet<InternalResourceItem>>();
        private Dictionary<int, HashSet<System.Action>> handlerToTasks = new Dictionary<int, HashSet<System.Action>>();

        private bool RequestLoad<T, TClosure>(object handler, TClosure closure, Resource resource, System.Action<T, TClosure> onComplete) where T : class {
            
            var item = new InternalTask(resource);
            if (this.tasks.TryGetValue(item, out var onCompleteActions) == true) {

                onCompleteActions += (obj) => onComplete.Invoke((T)obj, closure);
                this.tasks[item] = onCompleteActions;
                return true;

            } else {
                
                onCompleteActions += (obj) => onComplete.Invoke((T)obj, closure);
                this.tasks.Add(item, onCompleteActions);
                
            }

            return false;

        }

        private void CompleteTask(object handler, Resource resource, object result) {

            var item = new InternalTask(resource);
            if (this.tasks.TryGetValue(item, out var onCompleteActions) == true) {
                
                onCompleteActions.Invoke(result);
                this.tasks.Remove(item);

            }

        }

        public struct DefaultClosureData { }

        public IEnumerator LoadAsync<T>(object handler, Resource resource, System.Action<T, DefaultClosureData> onComplete) where T : class {

            yield return this.LoadAsync<T, DefaultClosureData>(handler, new DefaultClosureData(), resource, onComplete);

        }

        public IEnumerator LoadAsync<T, TClosure>(object handler, TClosure closure, Resource resource, System.Action<T, TClosure> onComplete) where T : class {

            if (this.RequestLoad(handler, closure, resource, onComplete) == true) {
                
                // Waiting for loading then break
                var item = new InternalTask(resource);
                while (this.tasks.ContainsKey(item) == true) yield return null;
                yield break;
                
            }
            
            switch (resource.type) {
                
                case Resource.Type.Manual: {

                    this.CompleteTask(handler, resource, default);

                    break;

                }

                case Resource.Type.Direct: {

                    if (resource.directRef is T direct) {

                        this.AddObject(handler, direct, resource);

                        this.CompleteTask(handler, resource, direct);
                        yield break;

                    }

                    this.CompleteTask(handler, resource, default);

                    break;

                }

                case Resource.Type.Addressables: {

                    if (resource.objectType == Resource.ObjectType.Component) {

                        //Debug.Log("Loading: " + resource.guid);
                        var op = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(resource.guid);
                        System.Action cancellationTask = () => { if (op.IsValid() == true) UnityEngine.AddressableAssets.Addressables.Release(op); };
                        this.LoadBegin(handler, cancellationTask);
                        while (op.IsDone == false) yield return null;

                        if (op.IsValid() == false) {
                            
                            //this.CompleteTask(handler, resource, default);
                            
                        } else {

                            if (op.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded) {

                                var asset = op.Result;
                                if (asset == null) {

                                    this.CompleteTask(handler, resource, default);

                                } else {

                                    this.AddObject(handler, asset, resource);

                                    this.CompleteTask(handler, resource, asset.GetComponent<T>());

                                }

                            }

                        }

                        this.LoadEnd(handler, cancellationTask);

                    } else {

                        UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<T> op;
                        if (string.IsNullOrEmpty(resource.subObjectName) == false) {
                            
                            op = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(resource.guid + "[" + resource.subObjectName + "]");
                            
                        } else {
                            
                            op = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(resource.guid);
                            
                        }
                        
                        System.Action cancellationTask = () => { if (op.IsValid() == true) UnityEngine.AddressableAssets.Addressables.Release(op); };
                        this.LoadBegin(handler, cancellationTask);
                        while (op.IsDone == false) yield return null;

                        if (op.IsValid() == false) {
                            
                            //this.CompleteTask(handler, resource, default);
                            
                        } else {
                            
                            if (op.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded) {

                                var asset = op.Result;
                                if (asset == null) {

                                    this.CompleteTask(handler, resource, default);

                                } else {

                                    this.AddObject(handler, asset, resource);

                                    this.CompleteTask(handler, resource, asset);

                                }

                            }

                        }
                        
                        this.LoadEnd(handler, cancellationTask);

                    }

                    break;

                }

            }

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

            this.handlerToTasks[handler.GetHashCode()].Remove(task);

        }

        public void StopLoadAll(object handler) {

            var key = handler.GetHashCode();
            if (this.handlerToTasks.TryGetValue(key, out var list) == true) {

                foreach (var item in list) {

                    item.Invoke();

                }
                
            }

        }

        public Dictionary<int, HashSet<InternalResourceItem>> GetAllObjects() {

            return this.handlerToObjects;

        }

        public int GetAllocatedCount() {

            var count = 0;
            foreach (var item in this.handlerToObjects) {

                foreach (var resItem in item.Value) {

                    ++count;

                }

            }

            return count;

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
            this.AddObject(handler, obj, new Resource() { type = Resource.Type.Manual });
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

            if (obj == null) return;
            if (handler == null) handler = this;

            var objId = obj.GetHashCode();
            this.UnloadObject(handler, obj);
            this.RemoveObject(handler, objId);
            obj = null;

        }

        public void DeleteAll(object handler) {

            if (handler == null) handler = this;

            this.UnloadAllObjects(handler);
            this.RemoveAllObjects(handler);

        }

        private void UnloadAllObjects(object handler) {

            var key = handler.GetHashCode();
            if (this.handlerToObjects.TryGetValue(key, out var list) == true) {

                foreach (var resItem in list) {

                    this.UnloadObject(resItem.handler, resItem.resource);

                }

            }

        }

        private void UnloadObject(object handler, object obj) {

            var key = handler.GetHashCode();
            if (this.handlerToObjects.TryGetValue(key, out var list) == true) {

                var objId = obj.GetHashCode();
                var resItem = new InternalResourceItem(handler, objId);
                foreach (var item in list) {

                    if (item.GetHashCode() == resItem.GetHashCode()) {

                        //Debug.Log("Attempt to unload object " + obj + " (handler: " + handler + "), type: " + item.resourceSource.type);
                        switch (item.resourceSource.type) {

                            case Resource.Type.Manual: {

                                // TODO: Put into pool if object could been pooled

                                if (obj is Object o) {

                                    Object.DestroyImmediate(o);

                                }

                            }
                                break;

                            case Resource.Type.Direct: {

                                // Direct asset skipped because there are always in memory

                            }
                                break;

                            case Resource.Type.Addressables: {

                                UnityEngine.AddressableAssets.Addressables.Release(obj);

                            }
                                break;

                        }

                        break;

                    }

                }

            }

        }

        private void RemoveObject(object handler, int objId) {

            var key = handler.GetHashCode();
            if (this.handlerToObjects.TryGetValue(key, out var list) == true) {

                var resItem = new InternalResourceItem(handler, objId);
                if (list.Remove(resItem) == true) {

                    if (list.Count == 0) {

                        this.handlerToObjects.Remove(key);

                    }

                }

            }

        }

        private void RemoveAllObjects(object handler) {

            var key = handler.GetHashCode();
            if (this.handlerToObjects.TryGetValue(key, out var list) == true) {

                list.Clear();

            }

            this.handlerToObjects.Remove(key);

        }

        private void AddObject(object handler, object obj, Resource resource) {

            var key = handler.GetHashCode();
            if (this.handlerToObjects.TryGetValue(key, out var list) == false) {

                list = new HashSet<InternalResourceItem>();
                this.handlerToObjects.Add(key, list);

            }

            {

                var resItem = new InternalResourceItem(handler, obj, resource);
                if (list.Contains(resItem) == false) {

                    list.Add(resItem);

                }

            }

        }

    }

}
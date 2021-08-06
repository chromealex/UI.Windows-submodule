using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Components {

    using Utilities;
    
    public interface IListClosureParameters {

        int index { get; set; }

    }

    public abstract class ListBaseComponent : GenericComponent, ILayoutSelfController, UnityEngine.EventSystems.IBeginDragHandler, UnityEngine.EventSystems.IDragHandler, UnityEngine.EventSystems.IEndDragHandler {

        [UnityEngine.UI.Windows.Modules.ResourceTypeAttribute(typeof(WindowComponent), RequiredType.Warning)]
        public Resource source;
        public Transform customRoot;

        public List<WindowComponent> items = new List<WindowComponent>();
        private HashSet<Object> loadedAssets = new HashSet<Object>();
        private System.Action onElementsChangedCallback;
        private System.Action onLayoutChangedCallback;
        private bool layoutHasChanged;

        [SerializeField]
        internal ListRectTransformChangedInternal listRectTransformChangedInternal;

        private void ValidateEditorRectTransformInternal() {

            if (this.listRectTransformChangedInternal != null && this.listRectTransformChangedInternal.listBaseComponent == null) {

                this.listRectTransformChangedInternal.listBaseComponent = this;
                this.listRectTransformChangedInternal.hideFlags = HideFlags.HideInInspector;

            }
            
            if (this.listRectTransformChangedInternal == null) {

                var tr = this.customRoot;
                if (tr == null) tr = this.transform;
                this.listRectTransformChangedInternal = tr.gameObject.AddComponent<ListRectTransformChangedInternal>();
                this.listRectTransformChangedInternal.listBaseComponent = this;
                this.listRectTransformChangedInternal.hideFlags = HideFlags.HideInInspector;

            } else {

                var tr = this.customRoot;
                if (tr == null) tr = this.transform;
                if (this.listRectTransformChangedInternal.transform != tr) {
                    
                    Object.DestroyImmediate(this.listRectTransformChangedInternal);
                    this.listRectTransformChangedInternal = null;
                    this.ValidateEditorRectTransformInternal();

                }
                
            }

        }
        
        #if UNITY_EDITOR
        public override void ValidateEditor() {
            
            base.ValidateEditor();

            this.ValidateEditorRectTransformInternal();
            
            var editorObj = this.source.GetEditorRef<WindowComponent>();
            if (editorObj != null) {
            
                if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(editorObj) == false) {

                    editorObj.allowRegisterInRoot = false;
                    editorObj.AddEditorParametersRegistry(new EditorParametersRegistry(this) {
                        holdAllowRegisterInRoot = true,
                    });

                    editorObj.gameObject.SetActive(false);

                }

            }
            
        }
        #endif

        void ILayoutController.SetLayoutHorizontal() {

            this.OnLayoutChanged();

        }

        void ILayoutController.SetLayoutVertical() {
            
            this.OnLayoutChanged();
            
        }

        internal void ForceLayoutChange() {
            
            this.OnLayoutChanged();
            
        }
        
        public int Count {
            get {
                return this.items.Count;
            }
        }

        protected virtual void OnLayoutChanged() {

            this.layoutHasChanged = true;
            if (this.onLayoutChangedCallback != null) this.onLayoutChangedCallback.Invoke();

        }

        public void SetOnLayoutChangedCallback(System.Action callback) {

            this.onLayoutChangedCallback = callback;

        }
        
        public void SetOnElementsCallback(System.Action callback) {

            this.onElementsChangedCallback = callback;

        }
        
        public override void OnInit() {
            
            base.OnInit();

            WindowSystem.onPointerUp += this.OnPointerUp;

        }

        public override void OnDeInit() {
            
            base.OnDeInit();

            this.ResetInstance();

        }
        
        public virtual void LateUpdate() {

            if (this.layoutHasChanged == true) {

                this.layoutHasChanged = false;
                this.componentModules.OnLayoutChanged();

            }

        }

        private void ResetInstance() {
            
            this.onElementsChangedCallback = null;
            this.onLayoutChangedCallback = null;
            
            WindowSystem.onPointerUp -= this.OnPointerUp;
            
            var resources = WindowSystem.GetResources();
            foreach (var asset in this.loadedAssets) {
            
                resources.Delete(this, asset);

            }
            this.loadedAssets.Clear();

        }

        private void OnPointerUp() {
            
            var eventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
            for (int i = 0; i < this.componentModules.modules.Length; ++i) {

                var module = this.componentModules.modules[i] as ListComponentModule;
                if (module == null) continue;
                
                module.OnDragEnd(eventData);

            }

        }
        
        void UnityEngine.EventSystems.IBeginDragHandler.OnBeginDrag(UnityEngine.EventSystems.PointerEventData eventData) {
            
            for (int i = 0; i < this.componentModules.modules.Length; ++i) {

                var module = this.componentModules.modules[i] as ListComponentModule;
                if (module == null) continue;
                
                module.OnDragBegin(eventData);

            }
            
        }

        void UnityEngine.EventSystems.IDragHandler.OnDrag(UnityEngine.EventSystems.PointerEventData eventData) {
            
            for (int i = 0; i < this.componentModules.modules.Length; ++i) {

                var module = this.componentModules.modules[i] as ListComponentModule;
                if (module == null) continue;
                
                module.OnDragMove(eventData);

            }

        }

        void UnityEngine.EventSystems.IEndDragHandler.OnEndDrag(UnityEngine.EventSystems.PointerEventData eventData) {
            
            for (int i = 0; i < this.componentModules.modules.Length; ++i) {

                var module = this.componentModules.modules[i] as ListComponentModule;
                if (module == null) continue;
                
                module.OnDragEnd(eventData);

            }

        }

        public Transform GetRoot() {

            if (this.customRoot != null) return this.customRoot;
            
            return this.transform;

        }

        public virtual void Clear() {

            this.RemoveRange(0, this.items.Count);
            
        }
        
        public void RemoveRange(int from, int to) {
            
            var pools = WindowSystem.GetPools();
            for (int i = to - 1; i >= from; --i) {

                this.UnRegisterSubObject(this.items[i]);
                pools.Despawn(this.items[i]);
                this.NotifyModulesComponentRemoved(this.items[i]);
                
            }
            this.items.RemoveRange(from, to - from);
            this.OnElementsChanged();

        }
        
        public virtual void OnElementsChanged() {

            for (int i = 0; i < this.componentModules.modules.Length; ++i) {

                var module = this.componentModules.modules[i] as ListComponentModule;
                if (module == null) continue;
                
                module.OnComponentsChanged();

            }
            
            if (this.onElementsChangedCallback != null) this.onElementsChangedCallback.Invoke();
            
        }
        
        public virtual T GetItem<T>(int index) where T : WindowComponent {

            return this.items[index] as T;

        }
        
        public virtual void AddItem(System.Action<WindowComponent, DefaultParameters> onComplete = null) {
            
            this.AddItem(this.source, new DefaultParameters(), onComplete);
            
        }

        public virtual void AddItem<T>(Resource source, System.Action<T, DefaultParameters> onComplete = null) where T : WindowComponent {
            
            this.AddItem(source, new DefaultParameters(), onComplete);
            
        }

        public virtual void AddItem<T>(System.Action<T, DefaultParameters> onComplete = null) where T : WindowComponent {
            
            this.AddItem(this.source, new DefaultParameters(), onComplete);
            
        }

        public virtual void AddItem<T, TClosure>(Resource source, TClosure closure, System.Action<T, TClosure> onComplete) where T : WindowComponent where TClosure : UnityEngine.UI.Windows.Components.IListClosureParameters {

            this.AddItemInternal(source, closure, onComplete);

        }

        private struct AddItemClosure<T, TClosure> {

            public TClosure data;
            public System.Action<T, TClosure> onComplete;
            public ListBaseComponent component;

        }
        internal void AddItemInternal<T, TClosure>(Resource source, TClosure closure, System.Action<T, TClosure> onComplete) where T : WindowComponent where TClosure : UnityEngine.UI.Windows.Components.IListClosureParameters {
            
            var resources = WindowSystem.GetResources();
            var data = new AddItemClosure<T, TClosure>() {
                data = closure,
                onComplete = onComplete,
                component = this,
            };
            Coroutines.Run(resources.LoadAsync<T, AddItemClosure<T, TClosure>>(this, data, source, (asset, innerClosure) => {

                if (innerClosure.component.loadedAssets.Contains(asset) == false) {
                    
                    if (asset.createPool == true) WindowSystem.GetPools().CreatePool(asset);
                    innerClosure.component.loadedAssets.Add(asset);
                    
                }
                
                var pools = WindowSystem.GetPools();
                var instance = pools.Spawn(asset, innerClosure.component.GetRoot());
                innerClosure.component.RegisterSubObject(instance);
                innerClosure.component.items.Add(instance);
                innerClosure.component.NotifyModulesComponentAdded(instance);
                innerClosure.component.OnElementsChanged();
                if (innerClosure.onComplete != null) innerClosure.onComplete.Invoke(instance, innerClosure.data);

            }));

        }

        public virtual void RemoveAt(int index) {

            if (index < this.items.Count) {

                var pools = WindowSystem.GetPools();
                this.UnRegisterSubObject(this.items[index]);
                pools.Despawn(this.items[index]);
                this.NotifyModulesComponentRemoved(this.items[index]);
                this.items.RemoveAt(index);
                this.OnElementsChanged();

            }

        }

        public virtual int IndexOf<T>(T component) where T : WindowComponent {

            for (int i = 0; i < this.items.Count; ++i) {

                if (this.items[i] == component) return i;

            }

            return -1;

        }

        public struct DefaultParameters : IListClosureParameters {

            public int index { get; set; }

        }

        public virtual void ForEach<T>(System.Action<T, DefaultParameters> onItem) where T : WindowComponent {
            
            this.ForEach(onItem, new DefaultParameters());

        }

        public virtual void ForEach<T, TClosure>(System.Action<T, TClosure> onItem, TClosure closure) where T : WindowComponent where TClosure : IListClosureParameters {
            
            for (int i = 0; i < this.Count; ++i) {

                closure.index = i;
                onItem.Invoke((T)this.items[i], closure);
                    
            }
            
        }

        public virtual void SetItems<T>(int count, System.Action<T, DefaultParameters> onItem, System.Action onComplete = null) where T : WindowComponent {
            
            this.SetItems(count, this.source, onItem, new DefaultParameters(), onComplete);
            
        }

        public virtual void SetItems<T, TClosure>(int count, System.Action<T, TClosure> onItem, TClosure closure, System.Action onComplete = null) where T : WindowComponent where TClosure : IListClosureParameters {
            
            this.SetItems(count, this.source, onItem, closure, onComplete);
            
        }

        private bool isLoadingRequest = false;
        public virtual void SetItems<T, TClosure>(int count, Resource source, System.Action<T, TClosure> onItem, TClosure closure, System.Action onComplete) where T : WindowComponent where TClosure : IListClosureParameters {

            if (this.isLoadingRequest == true) {

                return;

            }
            
            if (count == this.Count) {

                for (int i = 0; i < this.Count; ++i) {

                    closure.index = i;
                    onItem.Invoke((T)this.items[i], closure);
                    
                }

                if (onComplete != null) onComplete.Invoke();

            } else {

                var delta = count - this.Count;
                if (delta > 0) {

                    for (int i = 0; i < this.Count; ++i) {

                        closure.index = i;
                        onItem.Invoke((T)this.items[i], closure);
                    
                    }
                    this.Emit(delta, source, onItem, closure, onComplete);

                } else {
                    
                    this.RemoveRange(this.Count + delta, this.Count);
                    for (int i = 0; i < this.Count; ++i) {
                    
                        closure.index = i;
                        onItem.Invoke((T)this.items[i], closure);
                    
                    }
                    if (onComplete != null) onComplete.Invoke();
                    
                }

            }

        }

        private struct EmitClosure<T, TClosure> : IListClosureParameters where T : WindowComponent where TClosure : IListClosureParameters {

            public int index { get; set; }
            public ListBaseComponent list;
            public int requiredCount;
            public System.Action<T, TClosure> onItem;
            public System.Action onComplete;
            public TClosure data;

        }
        private void Emit<T, TClosure>(int count, Resource source, System.Action<T, TClosure> onItem, TClosure closure, System.Action onComplete = null) where T : WindowComponent where TClosure : IListClosureParameters {

            if (count == 0) {
                
                if (onComplete != null) onComplete.Invoke();
                this.isLoadingRequest = false;
                return;

            }
            
            var closureInner = new EmitClosure<T, TClosure>();
            closureInner.data = closure;
            closureInner.onItem = onItem;
            closureInner.onComplete = onComplete;
            closureInner.list = this;
            closureInner.requiredCount = count;
            
            this.isLoadingRequest = true;
            var offset = this.Count;
            var loaded = 0;
            for (int i = 0; i < count; ++i) {

                var index = i + offset;
                closureInner.index = index;
                
                this.AddItemInternal<T, EmitClosure<T, TClosure>>(source, closureInner, (item, c) => {

                    c.data.index = c.index;
                    c.onItem.Invoke(item, c.data);
                    
                    ++loaded;
                    if (loaded == c.requiredCount) {
                        
                        if (c.onComplete != null) c.onComplete.Invoke();
                        c.list.isLoadingRequest = false;
                        
                    }
                    
                });
                
            }
            
        }
        
        private void NotifyModulesComponentAdded(WindowComponent component) {
            
            foreach (var module in this.componentModules.modules) {
            
                (module as ListComponentModule)?.OnComponentAdded(component);
                
            }
            
        }
        
        private void NotifyModulesComponentRemoved(WindowComponent component) {
            
            foreach (var module in this.componentModules.modules) {
                
                (module as ListComponentModule)?.OnComponentRemoved(component);
                
            }
            
        }

    }

}
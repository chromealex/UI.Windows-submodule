using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Components {

    using Utilities;
    
    public abstract class ListBaseComponent : WindowComponent, ILayoutSelfController, UnityEngine.EventSystems.IBeginDragHandler, UnityEngine.EventSystems.IDragHandler, UnityEngine.EventSystems.IEndDragHandler {

        [UnityEngine.UI.Windows.Modules.ResourceTypeAttribute(typeof(WindowComponent), RequiredType.Warning)]
        public Resource source;
        public Transform customRoot;

        public List<WindowComponent> items = new List<WindowComponent>();
        private HashSet<Object> loadedAssets = new HashSet<Object>();
        private System.Action onElementsChangedCallback;
        private System.Action onLayoutChangedCallback;

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
                    editorObj.AddEditorParametersRegistry(new EditorParametersRegistry() {
                        holder = this,
                        allowRegisterInRoot = true,
                        allowRegisterInRootDescription = "Hold by ListComponent"
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

            this.componentModules.OnLayoutChanged();
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

            var pools = WindowSystem.GetPools();
            for (int i = this.items.Count - 1; i >= 0; --i) {

                this.UnRegisterSubObject(this.items[i]);
                pools.Despawn(this.items[i]);
                
            }
            this.items.Clear();
            this.OnElementsChanged();
            
        }

        public virtual void OnElementsChanged() {

            for (int i = 0; i < this.componentModules.modules.Length; ++i) {

                var module = this.componentModules.modules[i] as ListComponentModule;
                if (module == null) continue;
                
                module.OnElementsChanged();

            }
            
            if (this.onElementsChangedCallback != null) this.onElementsChangedCallback.Invoke();
            
        }

        public virtual T GetItem<T>(int index) where T : WindowComponent {

            return this.items[index] as T;

        }
        
        public virtual void AddItem(System.Action<WindowComponent> onComplete = null) {
            
            this.AddItem(this.source, onComplete);
            
        }

        public virtual void AddItem<T>(System.Action<T> onComplete = null) where T : WindowComponent {
            
            this.AddItem(this.source, onComplete);
            
        }

        public virtual void AddItem<T>(Resource source, System.Action<T> onComplete = null) where T : WindowComponent {

            var resources = WindowSystem.GetResources();
            var pools = WindowSystem.GetPools();
            Coroutines.Run(resources.LoadAsync<T>(this, source, (asset) => {

                if (this.loadedAssets.Contains(asset) == false) this.loadedAssets.Add(asset);
                
                var instance = pools.Spawn(asset, this.GetRoot());
                this.RegisterSubObject(instance);
                this.items.Add(instance);
                this.OnElementsChanged();
                if (onComplete != null) onComplete.Invoke(instance);

            }));
            
        }

        public virtual void RemoveItem(int index) {

            if (index < this.items.Count) {

                var pools = WindowSystem.GetPools();
                this.UnRegisterSubObject(this.items[index]);
                pools.Despawn(this.items[index]);
                this.items.RemoveAt(index);
                this.OnElementsChanged();

            }

        }

        public virtual void SetItems<T>(int count, System.Action<T, int> onItem, System.Action onComplete = null) where T : WindowComponent {
            
            this.SetItems(count, this.source, onItem, onComplete);
            
        }

        private bool isLoadingRequest = false;
        public virtual void SetItems<T>(int count, Resource source, System.Action<T, int> onItem, System.Action onComplete = null) where T : WindowComponent {

            if (this.isLoadingRequest == true) {

                return;

            }
            
            if (count == this.Count) {

                for (int i = 0; i < this.Count; ++i) {
                    
                    onItem.Invoke((T)this.items[i], i);
                    
                }

                if (onComplete != null) onComplete.Invoke();
                return;

            }

            this.isLoadingRequest = true;
            this.Clear();
            var loaded = 0;
            for (int i = 0; i < count; ++i) {

                var index = i;
                this.AddItem<T>(source, (item) => {

                    onItem.Invoke(item, index);
                    
                    ++loaded;
                    if (loaded == count) {
                        
                        if (onComplete != null) onComplete.Invoke();
                        this.isLoadingRequest = false;
                        
                    }
                    
                });
                
            }

        }

    }

}
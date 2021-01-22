using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows {
    
    using Utilities;

    public interface IEndlessElement {

        void GetHeight();

    }
    
    [ComponentModuleDisplayName("Endless List")]
    public class ListEndlessComponentModule : ListComponentModule {

        public abstract class RegistryBase {

            public ListEndlessComponentModule module;
            
            public abstract void Clear();

            public abstract void UpdateContent();

        }

        public class Registry<T, TClosure> : RegistryBase {

            private List<Item<T, TClosure>> items = new List<Item<T, TClosure>>();

            public override void Clear() {
                
                this.items.Clear();
                
            }
            
            public void Add(Item<T, TClosure> data) {
            
                this.items.Add(data);
            
            }

            public override void UpdateContent() {

                var allCount = this.module.allCount;
                var currentVisibleCount = this.module.currentVisibleCount;
                var requiredVisibleCount = this.module.requiredVisibleCount;
                //this.module.listComponent.

            }

        }
        
        public struct Item<T, TClosure> {

            public Resource source;
            public System.Action<T, TClosure> onItem;
            public TClosure closure;

        }
        
        [Space(10f)]
        [RequiredReference]
        public ScrollRect scrollRect;

        [Space(10f)]
        public LayoutElement top;
        public LayoutElement bottom;

        public LayoutGroup layoutGroup;
        public List<RegistryBase> registries = new List<RegistryBase>();
        private int allCount;
        private int currentVisibleCount;
        private int requiredVisibleCount;
        
        public override void ValidateEditor() {
            
            base.ValidateEditor();

            if (this.scrollRect == null) {

                this.scrollRect = this.GetComponentInChildren<ScrollRect>(true);

            }

            if (this.scrollRect != null && this.scrollRect.content != null && this.layoutGroup == null) {

                this.layoutGroup = this.scrollRect.content.GetComponent<LayoutGroup>();

            }
            
            if (this.top != null && this.scrollRect != null && this.scrollRect.content != null) {

                var go = new GameObject("--Top--", typeof(LayoutElement));
                go.transform.SetParent(this.scrollRect.content);
                this.top = go.GetComponent<LayoutElement>();

            }
            
            if (this.bottom != null && this.scrollRect != null && this.scrollRect.content != null) {

                var go = new GameObject("--Bottom--", typeof(LayoutElement));
                go.transform.SetParent(this.scrollRect.content);
                this.bottom = go.GetComponent<LayoutElement>();

            }
            
        }

        public override bool HasCustomAdd() {

            return true;

        }

        public override void OnInit() {
            
            base.OnInit();

            if (this.scrollRect != null) {
                
                this.scrollRect.onValueChanged.AddListener(this.OnScrollValueChanged);
                this.OnScrollValueChanged(this.scrollRect.normalizedPosition);
                
            }
            
        }

        public override void OnDeInit() {
            
            if (this.scrollRect != null) this.scrollRect.onValueChanged.RemoveListener(this.OnScrollValueChanged);

            base.OnDeInit();
            
        }

        public override void OnHideBegin() {
            
            base.OnHideBegin();
            
            if (this.scrollRect != null) this.OnScrollValueChanged(this.scrollRect.normalizedPosition);

        }

        public override void OnShowBegin() {
            
            base.OnShowBegin();
            
            if (this.scrollRect != null) this.OnScrollValueChanged(this.scrollRect.normalizedPosition);

        }

        public override void OnLayoutChanged() {
            
            base.OnLayoutChanged();
            
            if (this.scrollRect != null) this.OnScrollValueChanged(this.scrollRect.normalizedPosition);

        }

        public override void OnComponentsChanged() {
            
            base.OnComponentsChanged();
            
            if (this.scrollRect != null) this.OnScrollValueChanged(this.scrollRect.normalizedPosition);

        }

        public override void AddItem<T, TClosure>(Resource source, TClosure closure, System.Action<T, TClosure> onComplete) {
            
        }

        private Registry<T, TClosure> GetRegistry<T, TClosure>() {

            foreach (var regBase in this.registries) {

                if (regBase is Registry<T, TClosure> reg) return reg;

            }

            var registry = PoolClassCustom<RegistryBase>.Spawn<Registry<T, TClosure>>();
            return registry;

        }
        
        public override void SetItems<T, TClosure>(int count, Resource source, System.Action<T, TClosure> onItem, TClosure closure, System.Action onComplete) {

            foreach (var reg in this.registries) {

                PoolClassCustom<RegistryBase>.Recycle(reg);
                reg.Clear();

            }
            this.registries.Clear();

            this.allCount = count;
            var registry = this.GetRegistry<T, TClosure>();
            registry.module = this;
            for (int i = 0; i < count; ++i) {

                var item = new Item<T, TClosure>() {
                    source = source,
                    onItem = onItem,
                    closure = closure,
                };

                registry.Add(item);

            }
            this.registries.Add(registry);

            if (onComplete != null) onComplete.Invoke();
            
        }

        public void UpdateContentItems() {
            
            foreach (var reg in this.registries) {

                reg.UpdateContent();

            }
            
        }
        
        private void OnScrollValueChanged(Vector2 position) {

            #if UNITY_EDITOR
            if (Application.isPlaying == false) return;
            #endif
            
            var contentRect = this.scrollRect.content.rect;
            var borderRect = this.windowComponent.rectTransform.rect;

            {

                this.UpdateContentItems();

            }

        }

    }

}

using System.Collections.Generic;

namespace UnityEngine.UI.Windows {
    
    using Utilities;

    public interface IEndlessElement {

        void GetHeight();

    }

    public interface IDataSource {

        float GetSize(int index);
        
    }
    
    [ComponentModuleDisplayName("Endless List")]
    public class ListEndlessComponentModule : ListComponentModule {

        public abstract class RegistryBase {

            public ListEndlessComponentModule module;
            
            public abstract void Clear();

            public abstract void UpdateContent(bool forceRebuild = false);

        }

        public class Registry<T, TClosure> : RegistryBase where T : WindowComponent where TClosure : UnityEngine.UI.Windows.Components.IListClosureParameters {

            private List<Item<T, TClosure>> items = new List<Item<T, TClosure>>();
            private int loadingCount;
            private bool isDirty;
            private bool forceRebuild;

            public override void Clear() {
                
                this.items.Clear();
                
            }
            
            public void Add(Item<T, TClosure> data) {
            
                this.items.Add(data);
            
            }

            private struct InnerClosure : UnityEngine.UI.Windows.Components.IListClosureParameters {

                public int index { get; set; }

                public TClosure closure;
                public Registry<T, TClosure> registry;
                public Item<T, TClosure> item;

            }
            
            public override void UpdateContent(bool forceRebuild = false) {

                ref var currentVisibleCount = ref this.module.currentVisibleCount;
                ref var requiredVisibleCount = ref this.module.requiredVisibleCount;
                var fromIndex = this.module.fromIndex;
                var toIndex = this.module.toIndex;
                var contentSize = this.module.contentSize;

                var delta = requiredVisibleCount - currentVisibleCount;
                if (delta > 0) {

                    if (this.loadingCount > 0) return;
                    
                    for (int i = 0; i < delta; ++i) {

                        var k = i + fromIndex;
                        var item = this.items[k];
                        ++currentVisibleCount;
                        ++this.loadingCount;
                        this.module.listComponent.AddItemInternal<T, InnerClosure>(item.source, new InnerClosure() { closure = item.closure, registry = this, item = item }, (obj, closure) => {
                            --closure.registry.loadingCount;
                            closure.item.onItem.Invoke(obj, closure.closure);
                            closure.registry.isDirty = true;
                            closure.registry.forceRebuild = true;
                        });
                        this.isDirty = true;

                    }

                } else if (delta < 0) {

                    if (this.loadingCount > 0) return;
                    
                    //Debug.Log("REMOVE ITEMS: " + delta);
                    currentVisibleCount += delta;
                    this.module.listComponent.RemoveRange(this.module.listComponent.items.Count + delta, this.module.listComponent.items.Count);
                    this.isDirty = true;
                    
                }

                if (this.isDirty == true || forceRebuild == true) {
                
                    // Update
                    var k = 0;
                    for (int i = fromIndex; i <= toIndex; ++i) {

                        if (k >= this.module.listComponent.items.Count) break;

                        var instance = this.module.listComponent.items[k];
                        var item = this.items[i];
                        ref var data = ref this.module.items[i];
                        if (forceRebuild == true) data.size = this.module.dataSource.GetSize(i);
                        
                        var isLocalDirty = false;
                        var pos = instance.rectTransform.anchoredPosition;
                        var posOffset = contentSize - data.accumulatedSize - data.size;

                        if (this.module.direction == Direction.Vertical) {

                            if (UnityEngine.Mathf.Abs(pos.y - posOffset) >= Mathf.Epsilon) {

                                pos.y = posOffset;
                                pos.x = 0f;
                                instance.rectTransform.anchoredPosition = pos;
                                isLocalDirty = true;

                            }

                            var newSize = new Vector2(0f, data.size);
                            if (instance.rectTransform.sizeDelta != newSize) {

                                instance.rectTransform.sizeDelta = newSize;
                                isLocalDirty = true;

                            }

                        } else if (this.module.direction == Direction.Horizontal) {
                            
                            if (UnityEngine.Mathf.Abs(pos.x - posOffset) >= Mathf.Epsilon) {

                                pos.x = posOffset;
                                pos.y = 0f;
                                instance.rectTransform.anchoredPosition = pos;
                                isLocalDirty = true;

                            }

                            var newSize = new Vector2(data.size, 0f);
                            if (instance.rectTransform.sizeDelta != newSize) {

                                instance.rectTransform.sizeDelta = newSize;
                                isLocalDirty = true;

                            }

                        }

                        if (isLocalDirty == true || this.forceRebuild == true || forceRebuild == true) LayoutRebuilder.ForceRebuildLayoutImmediate(instance.rectTransform);

                        item.closure.index = i;
                        item.onItem.Invoke((T)instance, item.closure);
                        ++k;

                    }
                    
                    this.isDirty = false;
                    
                }

            }

        }
        
        public struct Item<T, TClosure> where T : WindowComponent where TClosure : UnityEngine.UI.Windows.Components.IListClosureParameters {

            public Resource source;
            public System.Action<T, TClosure> onItem;
            public TClosure closure;

        }

        [System.Serializable]
        public struct Item {

            public float size;
            public float accumulatedSize;
            
        }

        public enum Direction {

            Vertical,
            Horizontal,

        }
        
        [Space(10f)]
        [RequiredReference]
        public ScrollRect scrollRect;
        public Direction direction;

        [Space(10f)]
        public LayoutGroup layoutGroup;
        public List<RegistryBase> registries = new List<RegistryBase>();
        public float createOffset = 50f;

        private int allCount;
        private int currentVisibleCount;
        private int requiredVisibleCount;
        private int fromIndex;
        private int toIndex;
        private float contentSize;
        private IDataSource dataSource;
        private Item[] items;
        private bool forceRebuild;
        
        public override void ValidateEditor() {
            
            base.ValidateEditor();

            if (this.scrollRect == null) {

                this.scrollRect = this.GetComponentInChildren<ScrollRect>(true);

            }

            if (this.scrollRect != null) {

                this.scrollRect.vertical = (this.direction == Direction.Vertical);
                this.scrollRect.horizontal = (this.direction == Direction.Horizontal);

            }

            if (this.scrollRect != null && this.scrollRect.content != null && this.layoutGroup == null) {

                this.layoutGroup = this.scrollRect.content.GetComponent<LayoutGroup>();

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
            this.UpdateContentItems(true);

        }

        public override void OnShowEnd() {
            
            base.OnShowEnd();
            
            if (this.scrollRect != null) this.OnScrollValueChanged(this.scrollRect.normalizedPosition);
            this.UpdateContentItems(true);

        }

        public override void OnLayoutChanged() {
            
            base.OnLayoutChanged();
            
            if (this.scrollRect != null) this.OnScrollValueChanged(this.scrollRect.normalizedPosition);
            this.UpdateContentItems(true);

        }

        public override void OnComponentsChanged() {
            
            base.OnComponentsChanged();
            
            if (this.scrollRect != null) this.OnScrollValueChanged(this.scrollRect.normalizedPosition);

        }

        public override void AddItem<T, TClosure>(Resource source, TClosure closure, System.Action<T, TClosure> onComplete) {
            
        }

        private Registry<T, TClosure> GetRegistry<T, TClosure>() where T : WindowComponent where TClosure : UnityEngine.UI.Windows.Components.IListClosureParameters {

            foreach (var regBase in this.registries) {

                if (regBase is Registry<T, TClosure> reg) return reg;

            }

            var registry = PoolClassCustom<RegistryBase>.Spawn<Registry<T, TClosure>>();
            return registry;

        }

        public override void SetDataSource(IDataSource dataSource) {

            this.dataSource = dataSource;

        }
        
        public override void SetItems<T, TClosure>(int count, Resource source, System.Action<T, TClosure> onItem, TClosure closure, System.Action<TClosure> onComplete) {

            foreach (var reg in this.registries) {

                PoolClassCustom<RegistryBase>.Recycle(reg);
                reg.Clear();

            }
            this.registries.Clear();

            this.forceRebuild = (this.allCount != count);
            this.allCount = count;
            System.Array.Resize(ref this.items, count);
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
            //this.forceRebuild = true;

            if (onComplete != null) onComplete.Invoke(closure);
            
        }

        public void UpdateContentItems(bool forceRebuild) {
            
            foreach (var reg in this.registries) {

                reg.UpdateContent(forceRebuild);

            }
            
        }

        public int GetIndexByOffset(float pos) {
            
            for (int i = 0; i < this.allCount; ++i) {
                
                ref var item = ref this.items[i];
                if (item.accumulatedSize >= pos) return i;

            }

            return -1;

        }

        public int GetCount() {
            
            return this.items.Length;

        }

        public Item GetItemByIndex(int index) {
            
            return this.items[index];
            
        }

        public void CalculateBounds() {

            var accumulatedSize = 0f;
            for (int i = 0; i < this.allCount; ++i) {
                
                ref var item = ref this.items[i];
                if (item.size <= 0f) item.size = this.dataSource.GetSize(i);
                item.accumulatedSize = accumulatedSize;
                accumulatedSize += item.size;
                
            }

            var contentSize = accumulatedSize;
            var scrollRect = this.scrollRect.transform as RectTransform;
            var contentRect = this.scrollRect.content;
            var viewSize = (this.direction == Direction.Vertical ? scrollRect.rect.height + this.createOffset : scrollRect.rect.width + this.createOffset);
            this.contentSize = contentSize;
            
            contentRect.sizeDelta = (this.direction == Direction.Vertical ? new Vector2(0f, accumulatedSize) : new Vector2(accumulatedSize, 0f));
            
            var posOffset = (this.direction == Direction.Vertical ? this.scrollRect.normalizedPosition.y : 1f - this.scrollRect.normalizedPosition.x);
            var offInv = 1f - posOffset;
            var offset = offInv * contentSize - offInv * viewSize;
            
            if (contentSize <= viewSize) {
                
                offset = 0f;
                
            }

            var visibleContentHeight = Mathf.Min(viewSize, contentSize);

            var fromIndex = this.GetIndexByOffset(offset);
            if (fromIndex == -1) {
                
                fromIndex = 0;
                
            } else {

                --fromIndex;

            }
            
            var toIndex = this.GetIndexByOffset(offset + visibleContentHeight);
            if (toIndex == -1) {
                
                toIndex = this.allCount;
                
            } else {

                ++toIndex;

            }

            fromIndex = Mathf.Clamp(fromIndex, 0, this.allCount);
            toIndex = Mathf.Clamp(toIndex, 0, this.allCount);
            
            this.requiredVisibleCount = toIndex - fromIndex;
            this.fromIndex = fromIndex;
            this.toIndex = toIndex - 1;

        }
        
        private void OnScrollValueChanged(Vector2 position) {

            #if UNITY_EDITOR
            if (Application.isPlaying == false) return;
            #endif

            if (this.scrollRect == null || this.scrollRect.content == null) return;
            
            {

                this.CalculateBounds();
                this.UpdateContentItems(this.forceRebuild);
                this.forceRebuild = false;

            }

        }

    }

}

using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UI.Windows {

    using Utilities;

    public interface IEndlessElement {

        // void GetHeight();
        public int GetIndex();

    }

    public interface IDataSource {

        float GetSize(int index);
        
    }
    
    public struct ItemInstanceToRedraw {

        public WindowComponent instance;
        public int prevIndex;
        public int reqIndex;
        public bool shouldToRebuild;

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
            private int fromIndex = -1;
            private int toIndex = -1;
            private int prevFromIndex;
            private int prevToIndex;

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
            
            private List<Item<T, TClosure>> itemsToRedraw = new List<Item<T, TClosure>>();
            private List<ItemInstanceToRedraw> instancesToRedraw = new List<ItemInstanceToRedraw>();
            private Dictionary<int, int> instancePrevIndexToListIndex = new Dictionary<int, int>();
            
            public override void UpdateContent(bool forceRebuild = false) {

                ref var requiredVisibleCount = ref this.module.requiredVisibleCount;
                var fromIndex = this.module.fromIndex;
                var toIndex = this.module.toIndex;
                var contentSize = this.module.contentSize;
                if (this.fromIndex != fromIndex ||
                    this.toIndex != toIndex) {

                    this.prevFromIndex = this.fromIndex;
                    this.prevToIndex = this.toIndex;
                    this.fromIndex = fromIndex;
                    this.toIndex = toIndex;
                    this.isDirty = true;

                }

                var delta = requiredVisibleCount - this.module.listComponent.items.Count;
                if (delta > 0) {

                    if (this.loadingCount == 0) {

                        for (int i = 0; i < delta; ++i) {

                            var k = i + fromIndex;
                            var item = this.items[k];
                            ++this.loadingCount;
                            this.module.listComponent.AddItemInternal<T, InnerClosure>(item.source, new InnerClosure() { closure = item.closure, registry = this, item = item, },
                                                                                       (obj, closure) => {
                                                                                           --closure.registry.loadingCount;
                                                                                           //closure.item.onItem.Invoke(obj, closure.closure);
                                                                                           closure.registry.isDirty = true;
                                                                                           closure.registry.forceRebuild = true;
                                                                                       });
                            this.isDirty = true;

                        }

                    }

                } else if (delta < 0) {

                    if (this.loadingCount == 0) {

                        //Debug.Log("REMOVE ITEMS: " + delta);
                        this.module.listComponent.RemoveRange(this.module.listComponent.items.Count + delta, this.module.listComponent.items.Count);
                        this.isDirty = true;

                    }

                }

                if (this.isDirty == true || forceRebuild == true) {
                    
                    this.instancesToRedraw.Clear();
                    this.itemsToRedraw.Clear();
                    this.instancePrevIndexToListIndex.Clear();
                
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

                        var axis = new Vector2(0f, 0f);
                        switch (this.module.direction) {
                            
                            case Direction.Horizontal: {
                                axis = new Vector2(1f, 0f);
                                var posOffset = data.accumulatedSize;
                                if (UnityEngine.Mathf.Abs(pos.x - posOffset) >= Mathf.Epsilon) {

                                    pos.x = posOffset;
                                    pos.y = 0f;
                                    instance.rectTransform.anchoredPosition = pos;
                                    isLocalDirty = true;

                                }
                            }
                                break;

                            case Direction.HorizontalUpside: {
                                axis = new Vector2(1f, 0f);
                                var posOffset = contentSize - data.accumulatedSize - data.size;
                                if (UnityEngine.Mathf.Abs(pos.x - posOffset) >= Mathf.Epsilon) {

                                    pos.x = -posOffset;
                                    pos.y = 0f;
                                    instance.rectTransform.anchoredPosition = pos;
                                    isLocalDirty = true;

                                }
                            }
                                break;

                            case Direction.Vertical: {
                                axis = new Vector2(0f, 1f);
                                var posOffset = data.accumulatedSize;
                                if (UnityEngine.Mathf.Abs(pos.y - posOffset) >= Mathf.Epsilon) {

                                    pos.y = -posOffset;
                                    pos.x = 0f;
                                    instance.rectTransform.anchoredPosition = pos;
                                    isLocalDirty = true;

                                }
                            }
                                break;

                            case Direction.VerticalUpside: {
                                axis = new Vector2(0f, 1f);
                                var posOffset = contentSize - data.accumulatedSize - data.size;
                                if (UnityEngine.Mathf.Abs(pos.y - posOffset) >= Mathf.Epsilon) {

                                    pos.y = posOffset;
                                    pos.x = 0f;
                                    instance.rectTransform.anchoredPosition = pos;
                                    isLocalDirty = true;

                                }
                            }
                                break;

                        }
                        
                        var newSize = new Vector2(data.size * axis.x, data.size * axis.y);
                        if (instance.rectTransform.sizeDelta != newSize) {

                            instance.rectTransform.sizeDelta = newSize;
                            isLocalDirty = true;

                        }

                        {

                            item.closure.index = i;
                            var endlessElement = instance as IEndlessElement;
                            var prevIndex = -1;
                            if (endlessElement != null) {
                                prevIndex = endlessElement.GetIndex();
                            }
                            this.instancesToRedraw.Add(new ItemInstanceToRedraw() {
                                instance = instance,
                                prevIndex = prevIndex,
                                reqIndex = i,
                                shouldToRebuild = isLocalDirty == true || this.forceRebuild == true || forceRebuild == true,
                            });
                            this.itemsToRedraw.Add(item);

                            this.instancePrevIndexToListIndex.TryAdd(prevIndex, this.instancesToRedraw.Count - 1);

                        }

                        ++k;

                    }

                    for (var i = 0; i < this.itemsToRedraw.Count; ++i) {

                        var item = this.itemsToRedraw[i];
                        var itemVisibleIndex = item.closure.index;

                        var actualInstance = this.instancesToRedraw[i];

                        if (this.instancePrevIndexToListIndex.TryGetValue(itemVisibleIndex, out var indexFitInstance) ==
                            false) continue;

                        var fitInstance = this.instancesToRedraw[indexFitInstance];

                        if (fitInstance.instance == actualInstance.instance) continue;

                        // swap instances
                        (fitInstance.instance.rectTransform.anchoredPosition,
                            actualInstance.instance.rectTransform.anchoredPosition) = (
                            actualInstance.instance.rectTransform.anchoredPosition,
                            fitInstance.instance.rectTransform.anchoredPosition);
                        (fitInstance.prevIndex, actualInstance.prevIndex) =
                            (actualInstance.prevIndex, fitInstance.prevIndex);
                        (fitInstance.instance, actualInstance.instance) =
                            (actualInstance.instance, fitInstance.instance);

                        actualInstance.shouldToRebuild = true;
                        fitInstance.shouldToRebuild = true;

                        this.instancesToRedraw[indexFitInstance] = fitInstance;
                        this.instancesToRedraw[i] = actualInstance;

                        this.instancePrevIndexToListIndex[fitInstance.prevIndex] = indexFitInstance;
                        this.instancePrevIndexToListIndex[actualInstance.prevIndex] = i;

                    }

                    for (var i = 0; i < this.itemsToRedraw.Count; ++i) {

                        var item = this.itemsToRedraw[i];
                        var instanceToRedraw = this.instancesToRedraw[i];

                        if (instanceToRedraw.shouldToRebuild == true) {
                            item.onItem.Invoke((T) instanceToRedraw.instance, item.closure);
                            LayoutRebuilder.ForceRebuildLayoutImmediate(instanceToRedraw.instance.rectTransform);
                        }

                    }

                    this.isDirty = false;
                    
                }

            }

        }
        
        public struct Item<T, TClosure> where T : WindowComponent where TClosure : UnityEngine.UI.Windows.Components.IListClosureParameters {

            public Resource source;
            public System.Action<T, TClosure> onItem;
            public TClosure closure;
            public bool initialized;
            
        }

        [System.Serializable]
        public struct Item {

            public float size;
            public float accumulatedSize;
            
        }

        public enum Direction {

            Vertical,
            Horizontal,
            VerticalUpside,
            HorizontalUpside,

        }
        
        [Space(10f)]
        [RequiredReference]
        public ScrollRect scrollRect;
        public Direction direction;

        [Space(10f)]
        public HorizontalOrVerticalLayoutGroup layoutGroup;
        public List<RegistryBase> registries = new List<RegistryBase>();
        public float createOffset = 50f;
        
        private int allCount;
        private int requiredVisibleCount;
        private int fromIndex;
        private int toIndex;
        private float contentSize;
        private IDataSource dataSource;
        private Item[] items;
        private bool forceRebuild;
        private float contentRectExtend = 0f;

        public override void ValidateEditor() {
            
            base.ValidateEditor();

            if (this.scrollRect == null) {

                this.scrollRect = this.GetComponentInChildren<ScrollRect>(true);

            }

            if (this.scrollRect != null) {

                this.scrollRect.vertical = (this.direction == Direction.Vertical || this.direction == Direction.VerticalUpside);
                this.scrollRect.horizontal = (this.direction == Direction.Horizontal || this.direction == Direction.HorizontalUpside);

            }

            if (this.scrollRect != null && this.scrollRect.content != null && this.layoutGroup == null) {

                this.layoutGroup = this.scrollRect.content.GetComponent<HorizontalOrVerticalLayoutGroup>();

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

                var contentLayoutGroup = this.layoutGroup;
                if (contentLayoutGroup != null) {
                    if (this.direction == Direction.Horizontal || this.direction == Direction.HorizontalUpside) {
                        this.contentRectExtend = contentLayoutGroup.padding.horizontal;
                    } else {
                        this.contentRectExtend = contentLayoutGroup.padding.vertical;
                    }
                }
                
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
                    initialized = false
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

        private Vector2 listSize;
        private int prevCount;
        private float invOffset;
        private float offset;
        private float visibleContentHeight;
        public void CalculateBounds() {

            var forceRebuild = false;
            var size = this.listComponent.rectTransform.rect.size;
            if (this.listSize != size) {

                this.listSize = size;
                this.prevCount = 0;
                forceRebuild = true;

            }

            if (this.prevCount != this.allCount) {

                forceRebuild = true;

            }
            
            var padding = this.layoutGroup.padding;

            float accumulatedSize = 0f;
            if (forceRebuild == true) {

                switch (this.direction) {
                    case Direction.Horizontal:
                        accumulatedSize = padding.left;
                        break;
                    case Direction.Vertical:
                        accumulatedSize = padding.top;
                        break;
                    case Direction.HorizontalUpside:
                        accumulatedSize = padding.right;
                        break;
                    case Direction.VerticalUpside:
                        accumulatedSize = padding.bottom;
                        break;
                }
                if (this.prevCount > 0 && this.prevCount - 1 < this.items.Length) {
                    
                    accumulatedSize = this.items[this.prevCount - 1].accumulatedSize + this.items[this.prevCount - 1].size;
                    
                }
                for (int i = this.prevCount; i < this.allCount; ++i) {

                    ref var item = ref this.items[i];
                    item.size = this.dataSource.GetSize(i);
                    accumulatedSize += i == 0 ? 0 : this.layoutGroup.spacing;
                    item.accumulatedSize = accumulatedSize;
                    accumulatedSize += item.size;

                }
                
                this.prevCount = this.allCount;

            } else {

                if (this.allCount > 0) {
                    
                    accumulatedSize = this.items[this.allCount - 1].accumulatedSize + this.items[this.allCount - 1].size;
                    
                }

            }

            var scrollRect = (RectTransform)this.scrollRect.transform;
            var contentRect = this.scrollRect.content;
            var viewSize = 0f;
            var contentSize = accumulatedSize;
            var axis = new Vector2(0f, 0f);
            var posOffset = 0f;
            switch (this.direction) {
                case Direction.Horizontal:
                    contentSize += padding.right;
                    posOffset = 1f - this.scrollRect.normalizedPosition.x;
                    axis.x = 1f;
                    viewSize = scrollRect.rect.width;
                    break;
                case Direction.Vertical:
                    contentSize += padding.bottom;
                    posOffset = this.scrollRect.normalizedPosition.y;
                    axis.y = 1f;
                    viewSize = scrollRect.rect.height;
                    break;
                case Direction.HorizontalUpside:
                    contentSize += padding.left;
                    posOffset = 1f - this.scrollRect.normalizedPosition.x;
                    axis.x = 1f;
                    viewSize = scrollRect.rect.width;
                    break;
                case Direction.VerticalUpside:
                    contentSize += padding.top;
                    posOffset = this.scrollRect.normalizedPosition.y;
                    axis.y = 1f;
                    viewSize = scrollRect.rect.height;
                    break;
            }
            
            this.contentSize = contentSize;
            contentRect.sizeDelta = new Vector2((accumulatedSize + this.contentRectExtend) * axis.x, (accumulatedSize + this.contentRectExtend) * axis.y);
            
            var offInv = 1f - posOffset;
            this.invOffset = offInv;
            var offset = offInv * (contentSize - viewSize);
            if (contentSize <= viewSize) {
                
                offset = 0f;
                
            }

            var visibleContentHeight = Mathf.Min(viewSize, contentSize);
            this.offset = offset;
            this.visibleContentHeight = visibleContentHeight;
            var fromIndex = this.GetIndexByOffset(offset - this.createOffset);
            if (fromIndex == -1) {
                
                fromIndex = 0;
                
            } else {

                --fromIndex;

            }
            
            var toIndex = this.GetIndexByOffset(offset + visibleContentHeight + this.createOffset);
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

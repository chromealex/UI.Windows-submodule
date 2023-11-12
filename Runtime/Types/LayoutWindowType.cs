using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UI.Windows.WindowTypes {

    using Modules;
    using Utilities;
    
    public class LayoutSelectorAttribute : PropertyAttribute {}
    
    [System.Serializable]
    public class LayoutItem {

        [System.Serializable]
        public struct LayoutComponentItem : System.IEquatable<LayoutComponentItem> {

            public WindowLayout windowLayout;
            public int tag;
            public int localTag;
            [ResourceType(typeof(WindowComponent))]
            public Resource component;
            
            [System.NonSerialized]
            internal WindowComponent componentInstance;

            public bool Equals(LayoutComponentItem other) {
                return Equals(this.windowLayout, other.windowLayout) && this.tag == other.tag && this.localTag == other.localTag && this.component.Equals(other.component) && Equals(this.componentInstance, other.componentInstance);
            }

            public override bool Equals(object obj) {
                return obj is LayoutComponentItem other && Equals(other);
            }

            public override int GetHashCode() {
                unchecked {
                    var hashCode = (this.windowLayout != null ? this.windowLayout.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ this.tag;
                    hashCode = (hashCode * 397) ^ this.localTag;
                    hashCode = (hashCode * 397) ^ this.component.GetHashCode();
                    hashCode = (hashCode * 397) ^ (this.componentInstance != null ? this.componentInstance.GetHashCode() : 0);
                    return hashCode;
                }
            }

        }

        [Tooltip("Current target filter for this layout. If no layout filtered - will be used layout with 0 index.")]
        public WindowSystemTargets targets;
        [LayoutSelector]
        public WindowLayout windowLayout;
        [SearchAssetsByTypePopup(typeof(WindowLayoutPreferences), menuName: "Layout Preferences")]
        public WindowLayoutPreferences layoutPreferences;
        public LayoutComponentItem[] components;

        internal WindowLayout windowLayoutInstance;
        private int localTag;

        public void Unload() {

            this.windowLayoutInstance = null;
            for (int i = 0; i < this.components.Length; ++i) {

                this.components[i].componentInstance = null;

            }

        }
        
        public T FindComponent<T>(System.Func<T, bool> filter = null) where T : WindowComponent {

            return this.windowLayoutInstance.FindComponent<T>(filter);
            
        }

        public int GetCanvasOrder() {

            if (this.windowLayoutInstance == null) return 0;
            return this.windowLayoutInstance.GetCanvasOrder();

        }

        public Canvas GetCanvas() {

            #if UNITY_EDITOR
            if (Application.isPlaying == false) {
                
                return this.windowLayout.GetCanvas();
                
            }
            #endif
            
            if (this.windowLayoutInstance == null) return null;
            return this.windowLayoutInstance.GetCanvas();

        }

        public void Validate(DirtyHelper helper) {

            helper.Set(ref this.localTag, 0);

        }

        public bool HasLocalTagId(int localTagId) {

            for (int i = 0; i < this.components.Length; ++i) {

                if (this.components[i].localTag == localTagId) {

                    return true;

                }

            }

            return false;

        }

        public WindowLayoutElement GetLayoutElement(int localTagId) {

            int globalTag = -1;
            for (int i = 0; i < this.components.Length; ++i) {

                var comp = this.components[i];
                if (comp.localTag == localTagId) {

                    globalTag = comp.tag;
                    break;

                }

            }

            for (int i = 0; i < this.windowLayoutInstance.layoutElements.Length; ++i) {

                if (this.windowLayoutInstance.layoutElements[i].tagId == globalTag) {

                    return this.windowLayoutInstance.layoutElements[i];

                }
                
            }

            return null;

        }

        public bool GetLayoutComponent<T>(out T component, int localTagId) where T : WindowComponent {

            for (int i = 0; i < this.components.Length; ++i) {

                var comp = this.components[i];
                if (comp.localTag == localTagId) {

                    component = comp.componentInstance as T;
                    return true;

                }

            }

            component = default;
            return false;

        }

        public bool GetLayoutComponent<T>(out T component, ref int lastIndex, Algorithm algorithm) where T : WindowComponent {

            if (algorithm == Algorithm.GetFirstTypeAny || algorithm == Algorithm.GetNextTypeAny) {

                for (int i = 0; i < this.components.Length; ++i) {

                    var comp = this.components[i].componentInstance;
                    if (comp != null && comp is T c && lastIndex < i) {

                        lastIndex = i;
                        component = c;
                        return true;

                    }

                }

            } else if (algorithm == Algorithm.GetFirstTypeStrong || algorithm == Algorithm.GetNextTypeStrong) {

                var typeOf = typeof(T);
                for (int i = 0; i < this.components.Length; ++i) {

                    var comp = this.components[i].componentInstance;
                    if (comp != null && comp.GetType() == typeOf && lastIndex < i) {

                        lastIndex = i;
                        component = (T)comp;
                        return true;

                    }

                }

            }
            
            component = default;
            return false;

        }

        public void Unload(LayoutWindowType windowInstance) {

            var resources = WindowSystem.GetResources();
            resources.DeleteAll(windowInstance);

        }

        public void LoadAsync(InitialParameters initialParameters, LayoutWindowType windowInstance, System.Action onComplete) {

            windowInstance.Setup(windowInstance);
            
            var used = new HashSet<WindowLayout>();
            var layoutItem = this;
            Coroutines.Run(layoutItem.InitLayoutInstance(initialParameters, windowInstance, windowInstance, layoutItem.windowLayout, used, onComplete));

        }

        public void ApplyLayoutPreferences(WindowLayoutPreferences layoutPreferences) {

            if (layoutPreferences != null) layoutPreferences.Apply(this.windowLayoutInstance.canvasScaler);

        }

        private struct LoadingClosure {

            public int index;
            public WindowLayoutElement element;
            public WindowLayout windowLayoutInstance;
            public LayoutComponentItem[] layoutComponentItems;
            public LayoutItem instance;
            public InitialParameters initialParameters;

        }

        private int loadingCount;
        private IEnumerator InitLayoutInstance(InitialParameters initialParameters, LayoutWindowType windowInstance, WindowObject root, WindowLayout windowLayout, HashSet<WindowLayout> used, System.Action onComplete, bool isInner = false) {

            if (((ILayoutInstance)root).windowLayoutInstance != null || windowLayout == null) {
                
                if (onComplete != null) onComplete.Invoke();
                yield break;
                
            }
            
            if (windowLayout.createPool == true) WindowSystem.GetPools().CreatePool(windowLayout);
            var windowLayoutInstance = WindowSystem.GetPools().Spawn(windowLayout, root.transform);
            windowLayoutInstance.isRootLayout = (isInner == false);
            
            if (isInner == true) {

                windowLayoutInstance.canvasScaler.enabled = false;

            }

            windowLayoutInstance.Setup(windowInstance);
            windowLayoutInstance.SetCanvasOrder(windowInstance.GetCanvasDepth());
            root.RegisterSubObject(windowLayoutInstance);
            ((ILayoutInstance)root).windowLayoutInstance = windowLayoutInstance;
            this.ApplyLayoutPreferences(this.layoutPreferences);

            windowLayoutInstance.SetTransformFullRect();
            
            used.Add(this.windowLayout);

            this.loadingCount = 0;
            var arr = this.components;
            for (int i = 0; i < arr.Length; ++i) {

                var layoutComponent = arr[i];
                if (layoutComponent.windowLayout != windowLayout) continue;

                var layoutElement = windowLayoutInstance.GetLayoutElementByTagId(layoutComponent.tag);
                layoutComponent.componentInstance = windowLayoutInstance.GetLoadedComponent(layoutComponent.tag);
                layoutElement.Setup(windowInstance);
                arr[i] = layoutComponent;

                if (layoutComponent.componentInstance == null) {

                    if (layoutComponent.component.IsEmpty() == false) {

                        var resources = WindowSystem.GetResources();
                        var data = new LoadingClosure() {
                            index = i,
                            element = layoutElement,
                            windowLayoutInstance = windowLayoutInstance,
                            layoutComponentItems = arr,
                            instance = this,
                            initialParameters = initialParameters,
                        };
                        ++this.loadingCount;
                        Coroutines.Run(resources.LoadAsync<WindowComponent, LoadingClosure>(new WindowSystemResources.LoadParameters() { async = !initialParameters.showSync }, layoutElement, data, layoutComponent.component, (asset, closure) => {

                            if (asset == null) {

                                Debug.LogWarning("Component is null while component resource is not empty. Skipped.");
                                --closure.instance.loadingCount;
                                return;

                            }

                            ref var item = ref closure.layoutComponentItems[closure.index];

                            var instance = closure.element.Load(asset);
                            instance.SetInvisible();
                            closure.windowLayoutInstance.SetLoadedComponent(item.tag, instance);
                            item.componentInstance = instance;

                            instance.DoLoadScreenAsync(closure.initialParameters, () => { --closure.instance.loadingCount; });
                            
                        }));

                    }

                }

            }

            while (this.loadingCount > 0) yield return null;
            
            for (int i = 0; i < arr.Length; ++i) {

                var layoutComponent = arr[i];
                if (layoutComponent.windowLayout != windowLayout) continue;

                var layoutElement = windowLayoutInstance.GetLayoutElementByTagId(layoutComponent.tag);
                if (layoutElement.innerLayout != null) {

                    if (used.Contains(layoutElement.innerLayout) == false) {

                        yield return this.InitLayoutInstance(initialParameters, windowInstance, layoutElement, layoutElement.innerLayout, used, null, isInner: true);

                    } else {

                        Debug.LogWarning("Ignoring inner layout because of a cycle");

                    }

                }

            }

            if (onComplete != null) onComplete.Invoke();

        }

        public void Add(DirtyHelper helper, int tag, WindowLayout windowLayout) {

            for (int i = 0; i < this.components.Length; ++i) {

                if (this.components[i].tag == tag && this.components[i].windowLayout == windowLayout) {

                    return;

                }

            }

            var list = this.components.ToList();
            list.Add(new LayoutComponentItem() {
                component = new Resource(),
                componentInstance = null,
                tag = tag,
                localTag = ++this.localTag,
                windowLayout = windowLayout,
            });
            helper.Set(ref this.components, list.ToArray());

        }

        public void Remove(DirtyHelper helper, int tag, WindowLayout windowLayout) {

            for (int i = 0; i < this.components.Length; ++i) {

                if (this.components[i].tag == tag && this.components[i].windowLayout == windowLayout) {

                    var list = this.components.ToList();
                    list.RemoveAt(i);
                    helper.Set(ref this.components, list.ToArray());

                }

            }

        }

        public bool GetLayoutComponentItemByTagId(int tagId, WindowLayout windowLayout, out LayoutComponentItem layoutComponentItem) {

            for (int i = 0; i < this.components.Length; ++i) {

                if (this.components[i].tag == tagId && this.components[i].windowLayout == windowLayout) {

                    layoutComponentItem = this.components[i];
                    return true;

                }

            }

            layoutComponentItem = default;
            return false;

        }

    }

    [System.Serializable]
    public struct Layouts {

        public LayoutItem[] items;

        private int activeIndex;
        private bool forcedIndex;

        public void Unload() {
            
            for (int i = this.items.Length - 1; i >= 0; --i) {
                
                this.items[i].Unload();
                
            }
            
        }

        public void SetActive(int index) {

            this.activeIndex = index;
            this.forcedIndex = true;

        }

        public void SetActive() {

            if (this.forcedIndex == true) return;

            var targetData = WindowSystem.GetTargetData();
            for (int i = this.items.Length - 1; i >= 0; --i) {

                if (this.items[i].targets.IsValid(targetData) == true) {

                    this.activeIndex = i;
                    return;

                }

            }

            this.activeIndex = 0;

        }

        public LayoutItem GetActive() {

            return this.items[this.activeIndex];

        }

    }

    public enum Algorithm {

        /// <summary>
        /// Returns next component derived from type T or of type T
        /// </summary>
        GetNextTypeAny,
        /// <summary>
        /// Returns next component strongly of type T
        /// </summary>
        GetNextTypeStrong,
        /// <summary>
        /// Returns first component derived from type T or of type T
        /// </summary>
        GetFirstTypeAny,
        /// <summary>
        /// Returns first component strongly of type T
        /// </summary>
        GetFirstTypeStrong,

    }
    
    public abstract class LayoutWindowType : WindowBase, ILayoutInstance {

        public Layouts layouts = new Layouts() {
            items = new LayoutItem[1],
        };

        private Dictionary<int, int> requestedIndexes = new Dictionary<int, int>();

        internal override void OnDeInitInternal() {
            
            var currentItem = this.layouts.GetActive();
            currentItem.Unload(this);
            
            this.layouts.Unload();
            this.requestedIndexes.Clear();
            
            base.OnDeInitInternal();
            
        }

        WindowLayout ILayoutInstance.windowLayoutInstance {
            get {
                return this.layouts.GetActive().windowLayoutInstance; }
            set {
                this.layouts.GetActive().windowLayoutInstance = value;
            }
        }

        public override int GetCanvasOrder() {

            return this.layouts.GetActive().GetCanvasOrder();

        }

        public override WindowLayoutPreferences GetCurrentLayoutPreferences() {

            return this.layouts.GetActive().layoutPreferences;

        }

        public override Canvas GetCanvas() {

            return this.layouts.GetActive().GetCanvas();

        }

        public bool GetLayoutComponent<T>(out T component, int localTagId) where T : WindowComponent {

            var currentItem = this.layouts.GetActive();
            return currentItem.GetLayoutComponent(out component, localTagId);

        }

        public WindowLayoutElement GetLayoutElement(int localTagId) {

            var currentItem = this.layouts.GetActive();
            return currentItem.GetLayoutElement(localTagId);

        }

        /// <summary>
        /// Returns component instance of type T
        /// </summary>
        /// <param name="component">Component instance</param>
        /// <param name="algorithm">Algorithm which will be used to return component</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>Return true if component found, otherwise false</returns>
        public bool GetLayoutComponent<T>(out T component, Algorithm algorithm = Algorithm.GetNextTypeAny) where T : WindowComponent {

            component = default;
            bool result = false;
            switch (algorithm) {

                case Algorithm.GetNextTypeAny:
                case Algorithm.GetNextTypeStrong: {
                    
                    var key = typeof(T).GetHashCode();
                    var addNew = false;
                    var currentItem = this.layouts.GetActive();
                    if (this.requestedIndexes.TryGetValue(key, out var lastIndex) == false) {

                        addNew = true;
                        lastIndex = -1;

                    }

                    result = currentItem.GetLayoutComponent(out component, ref lastIndex, algorithm);

                    if (addNew == true) {

                        this.requestedIndexes.Add(key, lastIndex);

                    } else {

                        this.requestedIndexes[key] = lastIndex;

                    }

                }
                    break;

                case Algorithm.GetFirstTypeAny:
                case Algorithm.GetFirstTypeStrong: {
                    var idx = 0;
                    var currentItem = this.layouts.GetActive();
                    result = currentItem.GetLayoutComponent(out component, ref idx, algorithm);
                }
                    break;
                
            }
            
            return result;

        }

        public override void LoadAsync(InitialParameters initialParameters, System.Action onComplete) {

            this.layouts.SetActive();

            var currentItem = this.layouts.GetActive();
            currentItem.LoadAsync(initialParameters, this, () => { base.LoadAsync(initialParameters, onComplete); });

        }

        public int GetNextTagId(LayoutItem layoutItem) {

            var tagId = 1;
            while (layoutItem.components.Any(x => x.tag == tagId)) {

                ++tagId;

            }

            return tagId;

        }

        public int GetNextLocalTagId(LayoutItem layoutItem) {

            var tagId = 1;
            while (layoutItem.components.Any(x => x.localTag == tagId)) {

                ++tagId;

            }

            return tagId;

        }

        private void ValidateLayout(DirtyHelper helper, ref LayoutItem layoutItem, WindowLayout windowLayout, HashSet<WindowLayout> used) {

            used.Add(windowLayout);

            // Validate tags
            for (int j = layoutItem.components.Length - 1; j >= 0; --j) {

                var com = layoutItem.components[j];
                if (com.localTag == 0 || layoutItem.components.Count(x => x.localTag == com.localTag) > 1) {

                    helper.Set(ref com.localTag, this.GetNextLocalTagId(layoutItem));

                }

                /*if (layoutItem.components.Count(x => x.tag == com.tag && x.windowLayout == com.windowLayout) > 1) {

                    com.tag = 0;

                }*/

                layoutItem.components[j] = com;

            }

            // Remove unused
            {

                for (int j = 0; j < layoutItem.components.Length; ++j) {

                    var tag = layoutItem.components[j].tag;
                    if (windowLayout.HasLayoutElementByTagId(tag) == false) {

                        layoutItem.Remove(helper, tag, windowLayout);

                    }

                }

            }

            for (int j = 0; j < windowLayout.layoutElements.Length; ++j) {

                var layoutElement = windowLayout.layoutElements[j];
                if (layoutElement == null) {
                    
                    UnityEngine.Debug.LogError($"Layout Element is null at index {j} on window layout {windowLayout}", windowLayout);
                    continue;

                }
                if (layoutElement.hideInScreen == true) {
                
                    layoutItem.Remove(helper, layoutElement.tagId, windowLayout);
                    continue;
                    
                }
                
                if (layoutItem.GetLayoutComponentItemByTagId(layoutElement.tagId, windowLayout, out var layoutComponentItem) == false) {

                    layoutItem.Add(helper, layoutElement.tagId, windowLayout);

                }

                if (layoutElement.innerLayout != null) {

                    if (used.Contains(layoutElement.innerLayout) == false) {

                        this.ValidateLayout(helper, ref layoutItem, layoutElement.innerLayout, used);

                    } else {

                        Debug.LogWarning($"Ignoring inner layout `{layoutElement.innerLayout}` because of a cycle. Remove innerLayout reference from {layoutElement}", layoutElement);

                    }

                }

            }

        }

        public override void ValidateEditor() {

            base.ValidateEditor();

            var items = this.layouts.items;
            if (items == null) return;
            
            var helper = new UnityEngine.UI.Windows.Utilities.DirtyHelper(this);
            for (int i = 0; i < items.Length; ++i) {

                ref var layoutItem = ref items[i];
                if (layoutItem == null) {
                    helper.SetObj(ref layoutItem, new LayoutItem());
                }
                layoutItem.Validate(helper);

                var windowLayout = layoutItem.windowLayout;
                if (windowLayout != null) {

                    windowLayout.ValidateEditor();

                    { // Validate components list

                        for (int c = 0; c < layoutItem.components.Length; ++c) {

                            ref var com = ref layoutItem.components[c];
                            var comLock = com;
                            if ((windowLayout != com.windowLayout || windowLayout.HasLayoutElementByTagId(com.tag) == false) && windowLayout.layoutElements.Any(x => {
                                return x.innerLayout != null && x.innerLayout == comLock.windowLayout && x.innerLayout.HasLayoutElementByTagId(comLock.tag);
                            }) == false) {

                                var list = layoutItem.components.ToList();
                                list.RemoveAt(c);
                                helper.SetObj(ref layoutItem.components, list.ToArray());
                                --c;

                            }

                        }

                    }

                    var used = new HashSet<WindowLayout>();
                    this.ValidateLayout(helper, ref layoutItem, windowLayout, used);

                    used.Clear();
                    this.ValidateLayout(helper, ref layoutItem, windowLayout, used);

                }

            }

            this.layouts.items = items;
            helper.Apply();

        }

    }

}
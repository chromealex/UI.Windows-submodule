using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Components {

    using Utilities;
    
    public enum Side {

        TopLeft = 0,
        Top = 1,
        TopRight = 2,
        
        Left = 3,
        Middle = 4,
        Right = 5,
        
        BottomLeft = 6,
        Bottom = 7,
        BottomRight = 8,

    }

    public class DropdownComponent : GenericComponent, IInteractable, ISearchComponentByTypeEditor, ISearchComponentByTypeSingleEditor {

        private static Vector2[] anchors = new Vector2[] {
            new Vector2(0f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(1f, 1f),
            
            new Vector2(0f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(1f, 0.5f),
            
            new Vector2(0f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(1f, 0f),
        };
        
        System.Type ISearchComponentByTypeEditor.GetSearchType() { return typeof(DropdownComponentModule); }
        IList ISearchComponentByTypeSingleEditor.GetSearchTypeArray() { return this.componentModules.modules;}
        
        [SerializeField]
        [RequiredReference]
        public ButtonComponent label;
        [SerializeField]
        [RequiredReference]
        public ListComponent list;
        [SerializeField]
        [RequiredReference]
        public ScrollRect scrollRect;

        public Side anchor;
        public Side dropSide;
        public Vector2 minMaxSizeX = new Vector2(-1f, -1f);
        public Vector2 minMaxSizeY = new Vector2(-1f, -1f);
        public bool autoCloseOnChoose;
        public bool autoCloseOnOutClick;
        
        private System.Action<int> callback;
        private System.Action<WindowComponent, int> callbackWithInstance;
        private int selectedIndex = -1;

        private DrivenRectTransformTracker drivenRectTransformTracker;
        
        internal override void OnInitInternal() {
            
            base.OnInitInternal();

            this.list.SetOnLayoutChangedCallback(this.OnElementsChanged);
            this.list.SetOnElementsCallback(this.OnElementsChanged);
            this.label.SetCallback(this.DoToggleDropdown);
            WindowSystem.onPointerUp += this.OnPointerUp;
            WindowSystem.onPointerDown += this.OnPointerDown;

        }

        internal override void OnDeInitInternal() {
            
            base.OnDeInitInternal();

            WindowSystem.onPointerUp -= this.OnPointerUp;
            WindowSystem.onPointerDown -= this.OnPointerDown;
            this.RemoveCallbacks();

        }

        private void OnPointerDown() {
            
            if (this.autoCloseOnOutClick == false) return;
            
            var position = WindowSystem.GetPointerPosition();
            if (RectTransformUtility.RectangleContainsScreenPoint(this.label.rectTransform, position, this.GetWindow().workCamera) == false &&
                RectTransformUtility.RectangleContainsScreenPoint(this.list.rectTransform, position, this.GetWindow().workCamera) == false) {

                this.pointerDownInside = false;

            } else {
                
                this.pointerDownInside = true;

            }

        }

        private bool pointerDownInside;
        private void OnPointerUp() {

            if (this.autoCloseOnOutClick == false) return;
            
            if (this.pointerDownInside == false) {
                
                var position = WindowSystem.GetPointerPosition();
                if (RectTransformUtility.RectangleContainsScreenPoint(this.label.rectTransform, position, this.GetWindow().workCamera) == false &&
                    RectTransformUtility.RectangleContainsScreenPoint(this.list.rectTransform, position, this.GetWindow().workCamera) == false) {

                    this.HideDropdown();

                }
                
            }

        }
        
        public override void OnShowBegin() {
            
            base.OnShowBegin();
            
            this.OnElementsChanged();
            
        }

        private void OnElementsChanged() {

            this.CalculateAnchorsAndPivot();
            this.ApplyMinMaxSize();

        }

        private void ApplyMinMaxSize() {

            var rect = this.list.rectTransform;
            var contentRect = this.scrollRect.content;

            var delta = rect.sizeDelta;
            {
                var contentWidth = contentRect.sizeDelta.x;
                if (this.minMaxSizeX.x >= 0f) {

                    if (contentWidth < this.minMaxSizeX.x) {

                        delta.x = this.minMaxSizeX.x;

                    } else {
                        
                        delta.x = contentWidth;
                        
                    }

                }

                if (this.minMaxSizeX.y >= 0f && contentWidth > this.minMaxSizeX.y) {

                    if (contentWidth > this.minMaxSizeX.y) {

                        delta.x = this.minMaxSizeX.y;

                    } else {
                        
                        delta.x = contentWidth;
                        
                    }

                }
            }
            {
                var contentHeight = contentRect.sizeDelta.y;
                if (this.minMaxSizeY.x >= 0f) {

                    if (contentHeight < this.minMaxSizeY.x) {

                        delta.y = this.minMaxSizeY.x;

                    } else {
                        
                        delta.y = contentHeight;
                        
                    }

                }

                if (this.minMaxSizeY.y >= 0f) {

                    if (contentHeight > this.minMaxSizeY.y) {

                        delta.y = this.minMaxSizeY.y;

                    } else {
                        
                        delta.y = contentHeight;
                        
                    }

                }
            }
            rect.sizeDelta = delta;

        }

        private void CalculateAnchorsAndPivot() {
            
            var rect = this.list.rectTransform;
            
            var anchor = DropdownComponent.anchors[(int)this.anchor];
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;

            rect.pivot = DropdownComponent.anchors[(int)this.dropSide];

            rect.anchoredPosition = Vector2.zero;

        }

        public void DoToggleDropdown() {

            if (this.list.IsVisibleSelf() == true) {
                
                this.HideDropdown();
                
            } else {
                
                this.ShowDropdown();
                
            }
            
        }

        public void HideDropdown() {
            
            this.list.Hide();

        }

        public void ShowDropdown() {

            this.list.SetSortingOrderDelta(1);
            this.list.Show();

        }
        
        public void SetInteractable(bool state) {

            this.label.SetInteractable(state);

        }
        
        public bool IsInteractable() {

            return this.label.IsInteractable();

        }

        public void SetCallback(System.Action<int> callback) {

            this.RemoveCallbacks();
            this.AddCallback(callback);

        }

        public void SetCallback(System.Action<WindowComponent, int> callback) {

            this.RemoveCallbacks();
            this.AddCallback(callback);

        }

        public void AddCallback(System.Action<int> callback) {

            this.callback += callback;

        }

        public void AddCallback(System.Action<WindowComponent, int> callback) {

            this.callbackWithInstance += callback;

        }

        public void RemoveCallback(System.Action<int> callback) {

            this.callback -= callback;

        }

        public void RemoveCallback(System.Action<WindowComponent, int> callback) {

            this.callbackWithInstance -= callback;

        }
        
        public virtual void RemoveCallbacks() {
            
            this.callback = null;
            this.callbackWithInstance = null;
            
        }

        private void DoSelect(int index) {

            if (this.autoCloseOnChoose == true) {
                
                this.HideDropdown();

            }

            this.SelectLabel(index);
            this.SetSelectedIndex(index);

        }

        public int GetSelectedIndex() {

            return this.selectedIndex;

        }
        
        private void SetSelectedIndex(int index) {
            
            this.selectedIndex = index;
            if (this.callback != null) this.callback.Invoke(index);
            if (this.callbackWithInstance != null) this.callbackWithInstance.Invoke(this, index);

        }
        
        private void SelectLabel(int index) {
            
            var textComponent = this.list.GetItem<GenericComponent>(index).Get<TextComponent>();
            if (textComponent != null) {

                this.SetLabelText(textComponent.GetText());

            }
            
        }

        public void SetLabelText(string text) {
            
            var targetText = this.label.Get<TextComponent>();
            targetText.SetText(text);

        }

        private void TrySetCallbackToInteractable<T>(T instance, int index, System.Action<T> callback) where T : WindowComponent {
            
            if (instance is IInteractableButton button) {

                button.SetCallback(() => this.DoSelect(index));

            }
            if (callback != null) callback.Invoke(instance);

        }

        public void Select(int index) {

            if (index < 0) return;
            
            this.SelectLabel(index);
            this.SetSelectedIndex(index);
            
        }
        
        public virtual void AddItem(System.Action<WindowComponent> onComplete = null) {
            
            this.list.AddItem((x, p) => this.TrySetCallbackToInteractable(x, p.index, onComplete));

        }

        public virtual void AddItem<T>(System.Action<T> onComplete = null) where T : WindowComponent {
            
            this.list.AddItem<T>((x, p) => this.TrySetCallbackToInteractable(x, p.index, onComplete));

        }

        public virtual void AddItem<T>(Resource source, System.Action<T> onComplete = null) where T : WindowComponent {

            this.list.AddItem<T>(source, (x, p) => this.TrySetCallbackToInteractable(x, p.index, onComplete));
            
        }

        public virtual void RemoveItem(int index) {

            this.list.RemoveAt(index);

        }

        private struct DropdownClosureParameters<T> : IListClosureParameters where T : WindowComponent {

            public int index { get; set; }
            public DropdownComponent component;
            public System.Action<T, int> onItem;

        }
        public virtual void SetItems<T>(int count, System.Action<T, int> onItem, System.Action onComplete = null) where T : WindowComponent {
            
            this.list.SetItems<T, DropdownClosureParameters<T>>(count, (item, c) => {
                
                c.component.TrySetCallbackToInteractable(item, c.index, null);
                if (c.onItem != null) c.onItem.Invoke(item, c.index);
                
            }, new DropdownClosureParameters<T>() {
                component = this,
                onItem = onItem,
            }, onComplete);
            
        }

        public virtual void SetItems<T>(int count, Resource source, System.Action<T, int> onItem, System.Action onComplete = null) where T : WindowComponent {
            
            this.list.SetItems<T, DropdownClosureParameters<T>>(count, source, (item, c) => {
                
                c.component.TrySetCallbackToInteractable(item, c.index, null);
                if (c.onItem != null) c.onItem.Invoke(item, c.index);
                
            }, new DropdownClosureParameters<T>() {
                component = this,
                onItem = onItem,
            }, onComplete);

        }

        public override void ValidateEditor() {
            
            base.ValidateEditor();

            if (this.label == null) this.label = this.GetComponentInChildren<ButtonComponent>(true);
            if (this.list == null) this.list = this.GetComponentInChildren<ListComponent>(true);

            if (this.list != null) {

                this.scrollRect = this.list.GetComponent<ScrollRect>();
                if (this.scrollRect == null) return;

                this.drivenRectTransformTracker = new DrivenRectTransformTracker();
                this.drivenRectTransformTracker.Add(this, this.list.rectTransform, DrivenTransformProperties.Anchors | DrivenTransformProperties.Pivot | DrivenTransformProperties.AnchoredPosition);
                if (this.minMaxSizeX.x >= 0f || this.minMaxSizeX.y >= 0f) this.drivenRectTransformTracker.Add(this, this.list.rectTransform, DrivenTransformProperties.SizeDeltaX);
                if (this.minMaxSizeY.x >= 0f || this.minMaxSizeY.y >= 0f) this.drivenRectTransformTracker.Add(this, this.list.rectTransform, DrivenTransformProperties.SizeDeltaY);

                this.list.hiddenByDefault = true;
                this.list.AddEditorParametersRegistry(new EditorParametersRegistry() {
                    holder = this,
                    hiddenByDefault = true,
                    hiddenByDefaultDescription = "Value is hold by DropdownComponent"
                });
                
                this.CalculateAnchorsAndPivot();
                this.ApplyMinMaxSize();
                
            }
            
        }

    }

}
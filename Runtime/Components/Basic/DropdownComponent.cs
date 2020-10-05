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
        private ButtonComponent label;
        [SerializeField]
        [RequiredReference]
        private ListComponent list;
        [SerializeField]
        [RequiredReference]
        private ScrollRect scrollRect;

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
        
        public override void OnInit() {
            
            base.OnInit();

            this.list.SetOnLayoutChangedCallback(this.OnElementsChanged);
            this.list.SetOnElementsCallback(this.OnElementsChanged);
            this.label.SetCallback(this.DoToggleDropdown);
            WindowSystem.onPointerUp += this.OnPointerUp;

        }

        public override void OnDeInit() {
            
            base.OnDeInit();
            
            WindowSystem.onPointerUp -= this.OnPointerUp;
            this.RemoveCallbacks();
            
        }

        private void OnPointerUp() {

            var position = WindowSystem.GetPointerPosition();
            if (RectTransformUtility.RectangleContainsScreenPoint(this.label.rectTransform, position, this.GetWindow().workCamera) == false) {

                this.HideDropdown();

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
                if (this.minMaxSizeX.x >= 0f && contentWidth < this.minMaxSizeX.x) {

                    delta.x = this.minMaxSizeX.x;

                }

                if (this.minMaxSizeX.y >= 0f && contentWidth > this.minMaxSizeX.y) {

                    delta.x = this.minMaxSizeX.y;

                }
            }
            {
                var contentHeight = contentRect.sizeDelta.y;
                if (this.minMaxSizeY.x >= 0f && contentHeight < this.minMaxSizeY.x) {

                    delta.y = this.minMaxSizeY.x;

                }

                if (this.minMaxSizeY.y >= 0f && contentHeight > this.minMaxSizeY.y) {

                    delta.y = this.minMaxSizeY.y;

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

        private void SetSelectedIndex(int index) {
            
            this.selectedIndex = index;
            if (this.callback != null) this.callback.Invoke(index);
            if (this.callbackWithInstance != null) this.callbackWithInstance.Invoke(this, index);

        }
        
        private void SelectLabel(int index) {
            
            var textComponent = this.list.GetItem<GenericComponent>(index).Get<TextComponent>();
            if (textComponent != null) {

                var targetText = this.label.Get<TextComponent>();
                targetText.SetText(textComponent.GetText());

            }
            
        }

        private void TrySetCallbackToInteractable<T>(T instance, System.Action<T> callback) where T : WindowComponent {
            
            if (instance is IInteractableButton button) {

                var idx = this.list.Count;
                button.SetCallback(() => this.DoSelect(idx));

            }
            if (callback != null) callback.Invoke(instance);

        }

        public void Select(int index) {
            
            this.SelectLabel(index);
            this.SetSelectedIndex(index);
            
        }
        
        public virtual void AddItem(System.Action<WindowComponent> onComplete = null) {
            
            this.list.AddItem(x => this.TrySetCallbackToInteractable(x, onComplete));

        }

        public virtual void AddItem<T>(System.Action<T> onComplete = null) where T : WindowComponent {
            
            this.list.AddItem<T>(x => this.TrySetCallbackToInteractable(x, onComplete));

        }

        public virtual void AddItem<T>(Resource source, System.Action<T> onComplete = null) where T : WindowComponent {

            this.list.AddItem<T>(source, x => this.TrySetCallbackToInteractable(x, onComplete));
            
        }

        public virtual void RemoveItem(int index) {

            this.list.RemoveItem(index);

        }

        public virtual void SetItems<T>(int count, System.Action<T, int> onItem, System.Action onComplete = null) where T : WindowComponent {
            
            this.list.SetItems<T>(count, (item, index) => {
                
                this.TrySetCallbackToInteractable(item, null);
                if (onItem != null) onItem.Invoke(item, index);
                
            }, onComplete);
            
        }

        public virtual void SetItems<T>(int count, Resource source, System.Action<T, int> onItem, System.Action onComplete = null) where T : WindowComponent {
            
            this.list.SetItems<T>(count, source, (item, index) => {
                
                this.TrySetCallbackToInteractable(item, null);
                if (onItem != null) onItem.Invoke(item, index);
                
            }, onComplete);

        }

        public override void ValidateEditor() {
            
            base.ValidateEditor();

            if (this.label == null) this.label = this.GetComponentInChildren<ButtonComponent>(true);
            if (this.list == null) this.list = this.GetComponentInChildren<ListComponent>(true);

            if (this.list != null) {

                this.scrollRect = this.list.GetComponent<ScrollRect>();

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
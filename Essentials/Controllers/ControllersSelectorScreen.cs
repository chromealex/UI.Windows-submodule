using UnityEngine.UI.Windows.WindowTypes;
using UnityEngine.UI.Windows.Components;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Modules.Controllers {

    public class ControllersSelectorScreen : LayoutWindowType, IControllersSelectorScreen, ILateUpdate {

        private readonly Dictionary<int, IInteractableNavigation> selections = new Dictionary<int, IInteractableNavigation>();
        private WindowComponent selection;
        private IInteractableNavigation currentSelection;

        public override void OnInit() {
            base.OnInit();
            this.GetLayoutComponent(out this.selection);
            WindowSystem.GetEvents().Register(WindowEvent.OnFocusTook, this.OnObjectFocusTook);
            WindowSystem.GetEvents().Register(WindowEvent.OnHideEnd, this.OnObjectHideEnd);
        }

        public override void OnDeInit() {
            WindowSystem.GetEvents().UnRegister(WindowEvent.OnFocusTook, this.OnObjectFocusTook);
            WindowSystem.GetEvents().UnRegister(WindowEvent.OnHideEnd, this.OnObjectHideEnd);
            base.OnDeInit();
        }

        private void OnObjectHideEnd(WindowObject component) {
            if (component is WindowBase window) {
                this.selections.Remove(window.GetInstanceID());
            }
        }

        private void OnObjectFocusTook(WindowObject component) {
            if (component is WindowBase window) {
                if (this.selections.TryGetValue(window.GetInstanceID(), out var interactable) == true) {
                    this.currentSelection = interactable;
                } else {
                    if (window is INavigationDefaultScreen navigationScreen) {
                        // set default navigation if not presented for current window
                        this.currentSelection = navigationScreen.GetDefaultNavigation();
                    } else {
                        // if no interface presented - find first interactable
                        this.currentSelection = window.FindComponent<IInteractable>();
                    }
                    if (this.currentSelection != null) this.selections.Add(window.GetInstanceID(), this.currentSelection);
                }
                this.UpdateSelection(immediately: true);
            }
        }

        public void NavigateUp() {
            this.Navigate(Vector2.up);
        }

        public void NavigateDown() {
            this.Navigate(Vector2.down);
        }
        
        public void NavigateLeft() {
            this.Navigate(Vector2.left);
        }
        
        public void NavigateRight() {
            this.Navigate(Vector2.right);
        }
        
        public void Navigate(Vector2 direction) {
            var next = this.currentSelection.GetNext(direction);
            if (next != null) {
                this.currentSelection = next;
                this.UpdateSelection(immediately: false);
            }
        }
        
        public ButtonControl Button(ControllerButton button) {
            return this.currentSelection.DoAction(button);
        }
        
        public IInteractableNavigation GetCurrentSelection() {
            return this.currentSelection;
        }

        private void UpdateSelection(bool immediately) {
            if (this.currentSelection == null) return;
            var component = ((WindowComponent)this.currentSelection);
            var rect = component.rectTransform;
            HUDItem.Reposition(this.selection.rectTransform, rect.position, component.GetWindow().workCamera, this.GetWindow().workCamera);
            this.selection.rectTransform.pivot = rect.pivot;
            this.selection.rectTransform.sizeDelta = new Vector2(rect.rect.width, rect.rect.height);
            this.selection.Show();
        }

        public void OnLateUpdate(float dt) {
            this.UpdateSelection(immediately: true);
        }

    }

}
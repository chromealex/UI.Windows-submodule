using System.Collections;
using UnityEngine;

namespace UnityEngine.UI.Windows.Components {

    public interface ICheckboxGroup {
        void OnChecked(CheckboxComponent checkbox);
        bool CanBeUnchecked(CheckboxComponent checkbox);
    }
    
    public class CheckboxComponent : ButtonComponent {

        public WindowComponent checkedContainer;
        public WindowComponent uncheckedContainer;
        public bool isChecked;
        public bool autoToggle = true;
        public ICheckboxGroup group;

        private System.Action<bool> callback;
        private System.Action<CheckboxComponent, bool> callbackWithInstance;

        public override void ValidateEditor() {
            
            base.ValidateEditor();

            if (this.checkedContainer != null) {
                
                this.checkedContainer.hiddenByDefault = true;
                this.checkedContainer.AddEditorParametersRegistry(new EditorParametersRegistry() {
                    holder = this,
                    hiddenByDefault = true, hiddenByDefaultDescription = "Value is hold by CheckboxButtonComponent"
                });
                
            }
            
            if (this.uncheckedContainer != null) {
                
                this.uncheckedContainer.hiddenByDefault = true;
                this.uncheckedContainer.AddEditorParametersRegistry(new EditorParametersRegistry() {
                    holder = this,
                    hiddenByDefault = true, hiddenByDefaultDescription = "Value is hold by CheckboxButtonComponent"
                });
                
            }
            
            //this.UpdateCheckState();
            
        }

        public override void OnInit() {
            
            base.OnInit();

            if (this.autoToggle == true) {
                
                this.button.onClick.AddListener(this.Toggle);
                
            }
            
        }

        public override void OnDeInit() {
            
            this.button.onClick.RemoveListener(this.Toggle);
            
            base.OnDeInit();
            
        }

        public override void OnShowBegin() {
            
            base.OnShowBegin();
            
            this.SetCheckedState(this.isChecked);

        }

        public void Toggle() {
            
            this.SetCheckedState(!this.isChecked);
            
        }

        public void SetCheckedState(bool state, bool call = true) {

            var stateChanged = this.isChecked != state;
            this.isChecked = state;
            if (group != null) {
                if (state == false && group.CanBeUnchecked(this) == false) {
                    isChecked = true;
                    stateChanged = false;
                }

                if (isChecked) {
                    group.OnChecked(this);
                }
            }
            
            this.UpdateCheckState();
            
            if (call == true && stateChanged == true) {

                if (this.callback != null) this.callback.Invoke(state);
                if (this.callbackWithInstance != null) this.callbackWithInstance.Invoke(this, state);

            }

        }

        public void SetGroup(ICheckboxGroup group) {
            this.group = group;
        }

        private void UpdateCheckState() {

            if (this.checkedContainer != null) this.checkedContainer.ShowHide(this.isChecked == true);
            if (this.uncheckedContainer != null) this.uncheckedContainer.ShowHide(this.isChecked == false);

        }

        public void SetCallback(System.Action<bool> callback) {

            this.RemoveCallbacks();
            this.AddCallback(callback);

        }

        public void SetCallback(System.Action<CheckboxComponent, bool> callback) {

            this.RemoveCallbacks();
            this.AddCallback(callback);

        }

        public void AddCallback(System.Action<bool> callback) {

            this.callback += callback;

        }

        public void AddCallback(System.Action<CheckboxComponent, bool> callback) {

            this.callbackWithInstance += callback;

        }

        public void RemoveCallback(System.Action<bool> callback) {

            this.callback -= callback;

        }

        public void RemoveCallback(System.Action<CheckboxComponent, bool> callback) {

            this.callbackWithInstance -= callback;

        }
        
        public override void RemoveCallbacks() {
            
            base.RemoveCallbacks();
            
            this.callback = null;
            this.callbackWithInstance = null;
            
        }

    }

}
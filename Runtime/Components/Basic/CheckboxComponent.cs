using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Components {

    public class CheckboxComponent : ButtonComponent {

        public WindowComponent checkedContainer;
        public WindowComponent uncheckedContainer;
        public bool isChecked;
        public bool autoToggle = true;

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
            
            this.SetCheckedState(this.isChecked);

            if (this.autoToggle == true) {
                
                this.button.onClick.AddListener(this.Toggle);
                
            }
            
        }

        public override void OnDeInit() {
            
            this.button.onClick.RemoveListener(this.Toggle);
            
            base.OnDeInit();
            
        }

        public void Toggle() {
            
            this.SetCheckedState(!this.isChecked);
            
        }

        public void SetCheckedState(bool state, bool call = true) {

            if (this.isChecked != state) {

                this.isChecked = state;
                this.UpdateCheckState();

                if (call == true) {

                    if (this.callback != null) this.callback.Invoke(state);
                    if (this.callbackWithInstance != null) this.callbackWithInstance.Invoke(this, state);

                }
                
            }

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
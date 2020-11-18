using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Components {

    using Utilities;

    public interface IInteractable {

        bool IsInteractable();
        void SetInteractable(bool state);

    }

    public interface IInteractableButton : IInteractable {

        void SetCallback(System.Action callback);
        void AddCallback(System.Action callback);
        void RemoveCallback(System.Action callback);

        void SetCallback(System.Action<ButtonComponent> callback);
        void AddCallback(System.Action<ButtonComponent> callback);
        void RemoveCallback(System.Action<ButtonComponent> callback);

        void RemoveCallbacks();

    }
    
    public class ButtonComponent : GenericComponent, IInteractableButton, ISearchComponentByTypeEditor, ISearchComponentByTypeSingleEditor {

        System.Type ISearchComponentByTypeEditor.GetSearchType() {

           return typeof(ButtonComponentModule);

        }

        IList ISearchComponentByTypeSingleEditor.GetSearchTypeArray() {

           return this.componentModules.modules;

        }

        [RequiredReference]
        public Button button;
        
        private System.Action callback;
        private System.Action<ButtonComponent> callbackWithInstance;

        public override void OnInit() {
            
            base.OnInit();
            
            this.button.onClick.AddListener(this.DoClick);
            
        }

        public override void OnDeInit() {
            
            base.OnDeInit();
            
            this.button.onClick.RemoveAllListeners();
            this.RemoveCallbacks();
            
        }

        public void SetInteractable(bool state) {

            this.button.interactable = state;

        }
        
        public bool IsInteractable() {

            return this.button.interactable;

        }

        protected virtual void DoClick() {

            if (this.GetWindow().GetState() != ObjectState.Showing &&
                this.GetWindow().GetState() != ObjectState.Shown) {

                Debug.LogWarning("Couldn't send click because window is in `" + this.GetWindow().GetState().ToString() + "` state.", this);
                return;

            }

            if (this.GetState() != ObjectState.Showing &&
                this.GetState() != ObjectState.Shown) {

                Debug.LogWarning("Couldn't send click because component is in `" + this.GetWindow().GetState().ToString() + "` state.", this);
                return;

            }

            if (this.callback != null) this.callback.Invoke();
            if (this.callbackWithInstance != null) this.callbackWithInstance.Invoke(this);
            
        }
        
        public void SetCallback(System.Action callback) {

            this.RemoveCallbacks();
            this.AddCallback(callback);

        }

        public void SetCallback(System.Action<ButtonComponent> callback) {

            this.RemoveCallbacks();
            this.AddCallback(callback);

        }

        public void AddCallback(System.Action callback) {

            this.callback += callback;

        }

        public void AddCallback(System.Action<ButtonComponent> callback) {

            this.callbackWithInstance += callback;

        }

        public void RemoveCallback(System.Action callback) {

            this.callback -= callback;

        }

        public void RemoveCallback(System.Action<ButtonComponent> callback) {

            this.callbackWithInstance -= callback;

        }
        
        public virtual void RemoveCallbacks() {
            
            this.callback = null;
            this.callbackWithInstance = null;
            
        }

        public override void ValidateEditor() {
            
            base.ValidateEditor();

            if (this.button == null) this.button = this.GetComponent<Button>();

        }

    }

}
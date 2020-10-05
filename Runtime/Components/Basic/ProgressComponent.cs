using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Components {
    
    using Utilities;

    public class ProgressComponent : GenericComponent, IInteractable, ISearchComponentByTypeEditor, ISearchComponentByTypeSingleEditor {

        System.Type ISearchComponentByTypeEditor.GetSearchType() { return typeof(ProgressComponentModule); }
        IList ISearchComponentByTypeSingleEditor.GetSearchTypeArray() { return this.componentModules.modules;}

        [RequiredReference]
        public Slider slider;
        
        private System.Action<float> callback;
        private System.Action<ProgressComponent, float> callbackWithInstance;

        public override void OnInit() {
            
            base.OnInit();
            
            this.slider.onValueChanged.AddListener(this.OnValueChanged);
            
        }

        public override void OnDeInit() {
            
            base.OnDeInit();
            
            this.slider.onValueChanged.RemoveAllListeners();
            this.RemoveCallbacks();
            
        }

        public float GetValue() {

            return this.slider.value;

        }

        public float GetNormalizedValue() {

            return this.slider.normalizedValue;

        }

        public void SetNormalizedValue(float value) {

            this.slider.normalizedValue = value;

        }

        public void SetValue(float value) {

            this.slider.value = value;

        }

        public void SetInteractable(bool state) {

            this.slider.interactable = state;

        }

        public bool IsInteractable() {

            return this.slider.interactable;

        }

        private void OnValueChanged(float value) {
            
            if (this.callback != null) this.callback.Invoke(value);
            if (this.callbackWithInstance != null) this.callbackWithInstance.Invoke(this, value);
            
        }
        
        public void SetCallback(System.Action<float> callback) {

            this.RemoveCallbacks();
            this.AddCallback(callback);

        }

        public void SetCallback(System.Action<ProgressComponent, float> callback) {

            this.RemoveCallbacks();
            this.AddCallback(callback);

        }

        public void AddCallback(System.Action<float> callback) {

            this.callback += callback;

        }

        public void AddCallback(System.Action<ProgressComponent, float> callback) {

            this.callbackWithInstance += callback;

        }

        public void RemoveCallback(System.Action<float> callback) {

            this.callback -= callback;

        }

        public void RemoveCallback(System.Action<ProgressComponent, float> callback) {

            this.callbackWithInstance -= callback;

        }
        
        public virtual void RemoveCallbacks() {
            
            this.callback = null;
            this.callbackWithInstance = null;
            
        }

        public override void ValidateEditor() {
            
            base.ValidateEditor();

            this.slider = this.GetComponent<Slider>();

        }

    }

}
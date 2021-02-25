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
        private bool ignoreCallbacks;

        public override void OnInit() {
            
            base.OnInit();
            
            this.slider.onValueChanged.AddListener(this.OnValueChanged);
            
        }

        internal override void OnDeInitInternal() {
            
            base.OnDeInitInternal();
            
            this.ResetInstance();

        }

        public override void OnPoolAdd() {
            
            base.OnPoolAdd();

            this.RemoveCallbacks();

        }

        private void ResetInstance() {
            
            this.slider.onValueChanged.RemoveAllListeners();
            this.RemoveCallbacks();

        }

        public float GetValue() {

            return this.slider.value;

        }

        public float GetNormalizedValue() {

            return this.slider.normalizedValue;

        }

        public float GetMaxValue() {

            return this.slider.maxValue;

        }

        public void SetMaxValue(float value) {

            this.slider.maxValue = value;

        }

        public void SetNormalizedValue(float value, bool ignoreCallbacks = false) {

            var prev = this.ignoreCallbacks;
            this.ignoreCallbacks = ignoreCallbacks;
            if (Mathf.Abs(value - this.slider.normalizedValue) > Mathf.Epsilon) this.slider.normalizedValue = value;
            this.ignoreCallbacks = prev;

        }

        public void SetValue(float value, bool ignoreCallbacks = false) {

            var prev = this.ignoreCallbacks;
            this.ignoreCallbacks = ignoreCallbacks;
            if (Mathf.Abs(value - this.slider.value) > Mathf.Epsilon) this.slider.value = value;
            this.ignoreCallbacks = prev;

        }

        public void SetInteractable(bool state) {

            this.slider.interactable = state;

        }

        public bool IsInteractable() {

            return this.slider.interactable;

        }

        private void OnValueChanged(float value) {
            
            if (this.ignoreCallbacks == true) return;
            
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
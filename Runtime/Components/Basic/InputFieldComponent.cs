using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Components {
    
    using Utilities;

    public class InputFieldComponent : GenericComponent, IInteractable, ISearchComponentByTypeEditor, ISearchComponentByTypeSingleEditor {

        System.Type ISearchComponentByTypeEditor.GetSearchType() { return typeof(InputFieldComponentModule); }
        IList ISearchComponentByTypeSingleEditor.GetSearchTypeArray() { return this.componentModules.modules;}

        [RequiredReference]
        public InputField inputField;
        public TextComponent placeholder;
        
        private System.Action<string> callbackOnEditEnd;
        private System.Action<string> callbackOnChanged;
        private System.Func<string, int, char, char> callbackValidateChar;

        public override void OnInit() {
            
            base.OnInit();
            
            this.inputField.onValueChanged.AddListener(this.OnValueChanged);
            this.inputField.onEndEdit.AddListener(this.OnEndEdit);
            this.inputField.onValidateInput += this.OnValidateChar;
            
        }

        public override void OnDeInit() {
            
            base.OnDeInit();
            
            this.inputField.onValueChanged.RemoveListener(this.OnValueChanged);
            this.inputField.onEndEdit.RemoveListener(this.OnEndEdit);
            this.inputField.onValidateInput -= this.OnValidateChar;
            
            this.RemoveCallbacks();
            
        }

        public InputField GetSource() {

            return this.inputField;

        }

        private void OnValueChanged(string value) {
            
            if (this.callbackOnChanged != null) this.callbackOnChanged.Invoke(value);

        }

        private void OnEndEdit(string value) {
            
            if (this.callbackOnEditEnd != null) this.callbackOnEditEnd.Invoke(value);
            
        }

        private char OnValidateChar(string value, int charIndex, char addedChar) {

            if (this.callbackValidateChar != null) return this.callbackValidateChar.Invoke(value, charIndex, addedChar);

            return addedChar;

        }
        
        public string GetText() {

            return this.inputField.text;

        }

        public void SetText(string text) {

            this.inputField.text = text;

        }

        public string GetPlaceholderText() {

            if (this.placeholder != null) return this.placeholder.GetText();
            
            return null;

        }

        public void SetPlaceholderText(string text) {

            if (this.placeholder != null) this.placeholder.SetText(text);

        }

        public void SetInteractable(bool state) {

            this.inputField.interactable = state;

        }

        public bool IsInteractable() {

            return this.inputField.interactable;

        }
        
        public void SetCallbackValueChanged(System.Action<string> callback) {

            this.callbackOnChanged = null;
            this.callbackOnChanged += callback;

        }

        public void AddCallbackValueChanged(System.Action<string> callback) {

            this.callbackOnChanged += callback;

        }

        public void RemoveCallbackValueChanged(System.Action<string> callback) {

            this.callbackOnChanged -= callback;

        }

        public void SetCallbackEditEnd(System.Action<string> callback) {

            this.callbackOnEditEnd = null;
            this.callbackOnEditEnd += callback;

        }

        public void AddCallbackEditEnd(System.Action<string> callback) {

            this.callbackOnEditEnd += callback;

        }

        public void RemoveCallbackEditEnd(System.Action<string> callback) {

            this.callbackOnEditEnd -= callback;

        }

        public void SetCallbackValidateChar(System.Func<string, int, char, char> callback) {

            this.callbackValidateChar = null;
            this.callbackValidateChar += callback;

        }

        public void AddCallbackEditEnd(System.Func<string, int, char, char> callback) {

            this.callbackValidateChar += callback;

        }

        public void RemoveCallbackEditEnd(System.Func<string, int, char, char> callback) {

            this.callbackValidateChar -= callback;

        }

        public virtual void RemoveCallbacksValueChanged() {
            
            this.callbackOnChanged = null;
            
        }

        public virtual void RemoveCallbacksEditEnd() {
            
            this.callbackOnEditEnd = null;
            
        }

        public virtual void RemoveCallbacksValidateChar() {
            
            this.callbackValidateChar = null;

        }

        public virtual void RemoveCallbacks() {
            
            this.callbackOnChanged = null;
            this.callbackOnEditEnd = null;
            this.callbackValidateChar = null;

        }

        public override void ValidateEditor() {
            
            base.ValidateEditor();

            this.inputField = this.GetComponent<InputField>();

        }

    }

}
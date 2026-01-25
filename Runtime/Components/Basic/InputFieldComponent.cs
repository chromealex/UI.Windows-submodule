using System.Collections;

namespace UnityEngine.UI.Windows.Components {
    
    using Utilities;

    public partial class InputFieldComponent : GenericComponent, IInteractable, ILateUpdate, ISearchComponentByTypeEditor, ISearchComponentByTypeSingleEditor {

        System.Type ISearchComponentByTypeEditor.GetSearchType() { return typeof(InputFieldComponentModule); }
        IList ISearchComponentByTypeSingleEditor.GetSearchTypeArray() { return this.componentModules.modules;}

        [RequiredReference]
        public Selectable inputField;
        [Space]
        public TextComponent placeholder;
        public ButtonComponent clearTextButton;
        public WindowComponent focusedContainer;
        
        private System.Action<string> callbackOnEditEnd;
        private System.Action<string> callbackOnChanged;
        private System.Func<string, int, char, char> callbackValidateChar;

        IInteractableNavigation IInteractableNavigation.GetNext(Vector2 direction) => WindowSystem.GetNavigation(this.inputField, direction);

        ButtonControl IInteractableNavigation.DoAction(ControllerButton button) {
            if (button == ControllerButton.Click) {
                // Call keyboard
                WindowSystem.ShowSystemKeyboard(this);
            }
            return ButtonControl.None;
        }

        private void AddValueChangedListener(UnityEngine.Events.UnityAction<string> action) {

            if (this.inputField is InputField inputField) {
                
                inputField.onValueChanged.AddListener(action);
                
            }
            
            #if TEXTMESHPRO_SUPPORT
            if (this.inputField is TMPro.TMP_InputField tmpInputField) {
                
                tmpInputField.onValueChanged.AddListener(action);
                
            }
            #endif
            
        }

        private void AddEndEditListener(UnityEngine.Events.UnityAction<string> action) {

            if (this.inputField is InputField inputField) {
                
                inputField.onEndEdit.AddListener(action);
                
            }
            
            #if TEXTMESHPRO_SUPPORT
            if (this.inputField is TMPro.TMP_InputField tmpInputField) {
                
                tmpInputField.onEndEdit.AddListener(action);
                
            }
            #endif
            
        }

        private void AddValidateCharListener() {

            if (this.inputField is InputField inputField) {
                
                inputField.onValidateInput += this.OnValidateChar;
                
            }
            
            #if TEXTMESHPRO_SUPPORT
            if (this.inputField is TMPro.TMP_InputField tmpInputField) {
                
                tmpInputField.onValidateInput += this.OnValidateChar;
                
            }
            #endif
            
        }

        private void RemoveValueChangedListener(UnityEngine.Events.UnityAction<string> action) {

            if (this.inputField is InputField inputField) {
                
                inputField.onValueChanged.RemoveListener(action);
                
            }
            
            #if TEXTMESHPRO_SUPPORT
            if (this.inputField is TMPro.TMP_InputField tmpInputField) {
                
                tmpInputField.onValueChanged.RemoveListener(action);
                
            }
            #endif
            
        }

        private void RemoveEndEditListener(UnityEngine.Events.UnityAction<string> action) {

            if (this.inputField is InputField inputField) {
                
                inputField.onEndEdit.RemoveListener(action);
                
            }
            
            #if TEXTMESHPRO_SUPPORT
            if (this.inputField is TMPro.TMP_InputField tmpInputField) {
                
                tmpInputField.onEndEdit.RemoveListener(action);
                
            }
            #endif
            
        }

        private void RemoveValidateCharListener() {

            if (this.inputField is InputField inputField) {
                
                inputField.onValidateInput -= this.OnValidateChar;
                
            }
            
            #if TEXTMESHPRO_SUPPORT
            if (this.inputField is TMPro.TMP_InputField tmpInputField) {
                
                tmpInputField.onValidateInput -= this.OnValidateChar;
                
            }
            #endif
            
        }

        internal override void OnInitInternal() {
            
            base.OnInitInternal();

            this.AddValueChangedListener(this.OnValueChanged);
            this.AddEndEditListener(this.OnEndEdit);
            this.AddValidateCharListener();
            
            if (this.clearTextButton != null) this.clearTextButton.SetCallback(this, static (obj) => obj.Clear());
            
        }

        internal override void OnDeInitInternal() {
            
            base.OnDeInitInternal();

            this.RemoveValueChangedListener(this.OnValueChanged);
            this.RemoveEndEditListener(this.OnEndEdit);
            this.RemoveValidateCharListener();
            
            this.RemoveCallbacks();

        }

        public void Clear() {

            this.SetText(string.Empty);
            
        }

        public bool IsFocused() {

            if (this.inputField is InputField inputField) {
                
                return inputField.isFocused;
                
            }
            
            #if TEXTMESHPRO_SUPPORT
            if (this.inputField is TMPro.TMP_InputField tmpInputField) {

                return tmpInputField.isFocused;

            }
            #endif

            return false;

        }

        public void SetFocus() {
            
            this.inputField.Select();
            if (this.inputField is InputField inputField) {
                
                inputField.ActivateInputField();
                
            }
            
            #if TEXTMESHPRO_SUPPORT
            if (this.inputField is TMPro.TMP_InputField tmpInputField) {

                tmpInputField.ActivateInputField();

            }
            #endif
            
        }

        public T GetSource<T>() where T : Selectable {

            return (T)this.inputField;

        }

        private void OnValueChanged(string value) {
            
            if (this.clearTextButton != null) this.clearTextButton.ShowHide(string.IsNullOrEmpty(value) == false);
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
            
            if (this.inputField is InputField inputField) {
                
                return inputField.text;
                
            }
            
            #if TEXTMESHPRO_SUPPORT
            if (this.inputField is TMPro.TMP_InputField tmpInputField) {

                return tmpInputField.text;

            }
            #endif
            
            return null;

        }

        public void SetText(string text) {

            if (text == null) text = string.Empty;
            
            if (this.inputField is InputField inputField) {
                
                inputField.text = text;
                
            }
            
            #if TEXTMESHPRO_SUPPORT
            if (this.inputField is TMPro.TMP_InputField tmpInputField) {

                tmpInputField.text = text;

            }
            #endif

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

            this.callbackOnChanged = callback;

        }

        public void AddCallbackValueChanged(System.Action<string> callback) {

            this.callbackOnChanged += callback;

        }

        public void RemoveCallbackValueChanged(System.Action<string> callback) {

            this.callbackOnChanged -= callback;

        }

        public void SetCallbackEditEnd(System.Action<string> callback) {

            this.callbackOnEditEnd = callback;

        }

        public void AddCallbackEditEnd(System.Action<string> callback) {

            this.callbackOnEditEnd += callback;

        }

        public void RemoveCallbackEditEnd(System.Action<string> callback) {

            this.callbackOnEditEnd -= callback;

        }

        public void SetCallbackValidateChar(System.Func<string, int, char, char> callback) {

            this.callbackValidateChar = callback;

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

            if (this.inputField == null) this.inputField = this.GetComponent<InputField>();
            #if TEXTMESHPRO_SUPPORT
            if (this.inputField == null) this.inputField = this.GetComponent<TMPro.TMP_InputField>();
            #endif

        }

        public void OnLateUpdate(float dt) {
            if (this.IsFocused() == true) {
                this.focusedContainer?.Show();
            } else {
                this.focusedContainer?.Hide();
            }
        }

    }

}
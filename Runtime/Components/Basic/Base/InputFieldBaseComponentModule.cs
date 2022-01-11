namespace UnityEngine.UI.Windows {

    public abstract class InputFieldComponentModule : WindowComponentModule {

        public UnityEngine.UI.Windows.Components.InputFieldComponent inputFieldComponent;

        public override void ValidateEditor() {
            
            base.ValidateEditor();
            
            this.inputFieldComponent = this.windowComponent as UnityEngine.UI.Windows.Components.InputFieldComponent;
            
        }

    }
    
}

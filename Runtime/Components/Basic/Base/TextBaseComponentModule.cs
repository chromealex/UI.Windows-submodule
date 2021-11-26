namespace UnityEngine.UI.Windows {

    public abstract class TextComponentModule : WindowComponentModule {

        public UnityEngine.UI.Windows.Components.TextComponent textComponent;

        public override void ValidateEditor() {
            
            base.ValidateEditor();
            
            this.textComponent = this.windowComponent as UnityEngine.UI.Windows.Components.TextComponent;
            
        }

        public virtual void OnSetValue(double prevValue, double value, UnityEngine.UI.Windows.Components.SourceValue sourceValue, string strFormat) { }
        
        public virtual void OnSetText(string prevText, string text) { }

    }
    
}

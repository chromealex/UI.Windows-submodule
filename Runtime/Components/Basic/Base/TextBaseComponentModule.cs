namespace UnityEngine.UI.Windows {
    
    using Components;

    public abstract class TextComponentModule : WindowComponentModule {

        public UnityEngine.UI.Windows.Components.TextComponent textComponent;

        public override void ValidateEditor() {
            
            base.ValidateEditor();
            
            this.textComponent = this.windowComponent as UnityEngine.UI.Windows.Components.TextComponent;
            
        }

        public virtual void OnSetValue(double prevValue, double value, UnityEngine.UI.Windows.Components.SourceValue sourceValue, string strFormat) { }

        public virtual void SetValue(double value, SourceValue sourceValue = SourceValue.Digits, TimeResult timeValueResult = TimeResult.None, TimeResult timeShortestVariant = TimeResult.None) { }

        public virtual string SetText(string text) {
            return text;
        }
        
        public virtual void OnSetText(string prevText, string text) { }

    }
    
}

namespace UnityEngine.UI.Windows {

    using Components;
    
    public class SetMultipleTextsModule : TextComponentModule {

        public TextComponent[] others;

        public override void SetValue(double value, SourceValue sourceValue = SourceValue.Digits, TimeResult timeValueResult = TimeResult.None, TimeResult timeShortestVariant = TimeResult.None) {
            
            foreach (var other in this.others) {
                other.SetValue(value, sourceValue, timeValueResult, timeShortestVariant);
            }

        }

        public override void SetText(string text) {
            
            foreach (var other in this.others) {
                other.SetText(text);
            }

        }

    }
    
}

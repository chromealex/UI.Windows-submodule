namespace UnityEngine.UI.Windows {

    using Components;
    
    public class SetMultipleTextsModule : TextComponentModule {

        [System.Flags]
        public enum ApplyFlag {
            SetValue = 1 << 0,
            SetText = 1 << 1,
            SetColor = 1 << 2,
            All = -1,
        }

        public ApplyFlag flags = ApplyFlag.All;
        public TextComponent[] others;

        public override void SetValue(double value, SourceValue sourceValue = SourceValue.Digits, TimeResult timeValueResult = TimeResult.None, TimeResult timeShortestVariant = TimeResult.None) {

            if ((this.flags & ApplyFlag.SetValue) == 0) return;
            
            foreach (var other in this.others) {
                other.SetValue(value, sourceValue, timeValueResult, timeShortestVariant);
            }

        }

        public override void OnSetText(string prevText, string text) {
            
            if ((this.flags & ApplyFlag.SetText) == 0) return;

            foreach (var other in this.others) {
                other.SetText(text);
            }

        }

        public override void OnSetColor(Color color) {
            
            if ((this.flags & ApplyFlag.SetColor) == 0) return;

            foreach (var other in this.others) {
                other.SetColor(color);
            }

        }

    }
    
}

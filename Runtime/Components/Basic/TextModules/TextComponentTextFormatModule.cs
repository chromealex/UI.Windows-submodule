namespace UnityEngine.UI.Windows {

    public class TextComponentTextFormatModule : TextComponentModule {

        public enum TextFormat {
            None,
            ToLowerCase,
            ToUpperCase,
        }
        public TextFormat format;
        
        public override string SetText(string text) {
            switch (this.format) {
                case TextFormat.ToLowerCase: return text.ToLower();
                case TextFormat.ToUpperCase: return text.ToUpper();
            }
            return text;
        }

    }
    
}

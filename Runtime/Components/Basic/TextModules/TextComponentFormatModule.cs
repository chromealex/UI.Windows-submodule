namespace UnityEngine.UI.Windows {

    public class TextComponentFormatModule : TextComponentModule {

        public string format;

        public override void OnInit() {
            base.OnInit();
            this.textComponent.SetValueFormat(this.format);
        }

    }
    
}

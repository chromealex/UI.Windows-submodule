
namespace UnityEngine.UI.Windows {

    public class TextLocalizationComponentModule : TextComponentModule {

        public UnityEngine.Localization.LocalizedString key;

        public override void OnInit() {
            
            base.OnInit();

            this.key.StringChanged += this.OnChanged;

        }

        public override void OnDeInit() {
            
            this.key.StringChanged -= this.OnChanged;
            
            base.OnDeInit();
            
        }

        public override void OnShowBegin() {
            
            base.OnShowBegin();

            this.key.RefreshString();

        }

        private void OnChanged(string val) {
            
            this.textComponent.SetText(val);
            
        }

    }

}

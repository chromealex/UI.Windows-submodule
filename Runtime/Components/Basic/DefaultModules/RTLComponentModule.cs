namespace UnityEngine.UI.Windows {

    public class RTLComponentModule : WindowComponentModule {

        public HorizontalOrVerticalLayoutGroup layoutGroup;
        [HideInInspector]
        public bool defaultAlignment;

        public override void ValidateEditor() {
            base.ValidateEditor();
            if (this.layoutGroup == null) this.layoutGroup = this.GetComponentInChildren<HorizontalOrVerticalLayoutGroup>(true);
            if (this.layoutGroup != null) this.defaultAlignment = this.layoutGroup.reverseArrangement;
        }

        public override void OnShowBegin() {
            base.OnShowBegin();
            this.UpdateRTL();
        }

        public override void OnLayoutChanged() {
            base.OnLayoutChanged();
            this.UpdateRTL();
        }

        private void UpdateRTL() {
            this.layoutGroup.reverseArrangement = WindowSystem.IsRTL() == true ? !this.defaultAlignment : this.defaultAlignment;
        }

    }
    
}

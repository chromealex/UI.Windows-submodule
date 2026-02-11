namespace UnityEngine.UI.Windows {

    public class SafeAreaComponentModule : WindowComponentModule {

        public enum TargetType {
            RectTransformAnchors,
            LayoutGroup,
        }
        
        public TargetType targetType;
        public SafeArea config = SafeArea.Default;
        public RectTransform rectTransform;
        public LayoutGroup layoutGroup;
        
        private WindowLayoutSafeZone.ScreenCache screenCache;

        public override void ValidateEditor() {
            base.ValidateEditor();
            if (this.layoutGroup == null) this.layoutGroup = this.GetComponentInChildren<LayoutGroup>(true);
        }

        public override void OnShowBegin() {
            base.OnShowBegin();
            this.UpdateSafeArea();
        }

        public override void OnLayoutChanged() {
            base.OnLayoutChanged();
            this.UpdateSafeArea();
        }

        private void LateUpdate() {
            if (this.windowComponent.IsVisible() == false) return;
            if (this.screenCache.HasChanged == true) {
                this.UpdateSafeArea();
            }
        }

        private void UpdateSafeArea() {
            
            this.screenCache.Update();

            if (this.targetType == TargetType.RectTransformAnchors) {
                var (anchorMin, anchorMax) = WindowLayoutSafeZone.GetAnchors(this.config.PaddingType, this.config.CustomPaddings);
                this.rectTransform.anchorMin = anchorMin;
                this.rectTransform.anchorMax = anchorMax;
                this.rectTransform.sizeDelta = Vector2.zero;
                this.rectTransform.anchoredPosition = Vector2.zero;
            } else if (this.targetType == TargetType.LayoutGroup) {
                WindowLayoutSafeZone.GetRectOffset(this.GetWindow().GetCanvas().scaleFactor, this.layoutGroup.padding, this.config.PaddingType, this.config.CustomPaddings);
            }

        }

    }
    
}

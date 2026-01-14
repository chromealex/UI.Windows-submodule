namespace UnityEngine.UI.Windows {

    public class SafeAreaComponentModule : WindowComponentModule {

        public enum TargetType {
            RectTransformAnchors,
            LayoutGroup,
        }
        
        public TargetType targetType;
        public WindowLayoutSafeZone.PaddingType paddingType;
        public WindowLayoutSafeZone.CustomPaddings customPaddings;
        public RectTransform rectTransform;
        public LayoutGroup layoutGroup;
        
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

        private void UpdateSafeArea() {

            if (this.targetType == TargetType.RectTransformAnchors) {
                var (anchorMin, anchorMax) = WindowLayoutSafeZone.GetAnchors(this.paddingType, this.customPaddings);
                this.rectTransform.anchorMin = anchorMin;
                this.rectTransform.anchorMax = anchorMax;
                this.rectTransform.sizeDelta = Vector2.zero;
                this.rectTransform.anchoredPosition = Vector2.zero;
            } else if (this.targetType == TargetType.LayoutGroup) {
                WindowLayoutSafeZone.GetRectOffset(this.layoutGroup.padding, this.paddingType, this.customPaddings);
            }

        }

    }
    
}

namespace UnityEngine.UI.Windows {

    public class WindowLayoutSafeZoneExtended : WindowLayoutSafeZone, ILateUpdate {

        [System.Serializable]
        public struct CustomPadding {

            public WindowSystemTargets targets;
            public Rect paddingPixels;
            public Rect paddingPercent;

        }
        
        [System.Flags]
        public enum PaddingType {
            None = 0,
            Top = 1 << 0,
            Bottom = 1 << 2,
            Left = 1 << 3,
            Right = 1 << 4,
            All = -1,
        }

        public PaddingType paddingType = PaddingType.All;
        public CustomPadding[] customPadding;

        private ScreenOrientation savedOrientation;
        private Rect savedSafeArea;

        public override void OnShowBegin() {
            base.OnShowBegin();
            this.SetDirty();
        }

        public void SetDirty() {
            this.UpdateOrientation();
            this.UpdateSafeArea();
            this.Apply();
        }

        private void UpdateSafeArea() {
            this.savedSafeArea = Screen.safeArea;
        }

        private void UpdateOrientation() {
            this.savedOrientation = Screen.orientation;
        }

        public void OnLateUpdate(float dt) {
            if (Screen.orientation != this.savedOrientation ||
                Screen.safeArea != this.savedSafeArea) {
                this.SetDirty();
            }
        }

        public override void Apply() {

            var rect = this.rectTransform;
            rect.localScale = Vector3.one;

            var w = Screen.width;
            var h = Screen.height;
            var safeZone = Screen.safeArea;
            safeZone.xMin /= w;
            safeZone.yMin /= h;
            safeZone.xMax /= w;
            safeZone.yMax /= h;
            var anchorMin = Vector2.zero;
            var anchorMax = Vector2.one;

            if ((this.paddingType & PaddingType.Top) == 0) safeZone.yMax = 1f;
            if ((this.paddingType & PaddingType.Bottom) == 0) safeZone.yMin = 0f;
            if ((this.paddingType & PaddingType.Left) == 0) safeZone.xMin = 0f;
            if ((this.paddingType & PaddingType.Right) == 0) safeZone.xMax = 1f;

            var customPadding = this.GetCustomPadding();
            
            anchorMin.x = safeZone.xMin;
            anchorMax.x = safeZone.xMax;
            anchorMin.y = safeZone.yMin;
            anchorMax.y = safeZone.yMax;

            anchorMin.x += customPadding.paddingPixels.xMin / w + customPadding.paddingPercent.xMin;
            anchorMax.x += customPadding.paddingPixels.xMax / w + customPadding.paddingPercent.xMax;
            anchorMin.y += customPadding.paddingPixels.yMin / h + customPadding.paddingPercent.yMin;
            anchorMax.y += customPadding.paddingPixels.yMax / h + customPadding.paddingPercent.yMax;
            
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

        }

        private CustomPadding GetCustomPadding() {
            var targetData = WindowSystem.GetTargetData();
            foreach (var item in this.customPadding) {
                if (item.targets.IsValid(targetData) == true) return item;
            }
            return default;
        }

    }

}
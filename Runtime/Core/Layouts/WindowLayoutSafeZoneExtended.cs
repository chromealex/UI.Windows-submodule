namespace UnityEngine.UI.Windows {

    public class WindowLayoutSafeZoneExtended : WindowLayoutSafeZone {

        [System.Flags]
        public enum PaddingType {

            None = 0b00,
            DeviceScreenTop = 0b01,
            DeviceScreenBottom = 0b10,
            All = 0b11,

        }

        public PaddingType paddingType = PaddingType.All;
        public bool ignoreVertSafePaddings = false;

        private ScreenOrientation savedOrientation;

        public override void OnShowBegin() {

            base.OnShowBegin();

            this.UpdateOrientation();

        }

        private void UpdateOrientation() {

            this.savedOrientation = Screen.orientation;
            this.Apply();

        }

        private void LateUpdate() {

            if (this.GetState() == ObjectState.Shown && Screen.orientation != this.savedOrientation) {

                this.UpdateOrientation();

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

            this.ApplyHorAnchors(ref anchorMin, ref anchorMax, safeZone);

            if (this.ignoreVertSafePaddings == true) {

                anchorMin.y = 0;
                anchorMax.y = 1;

            } else {

                anchorMin.y = safeZone.yMin;
                anchorMax.y = safeZone.yMax;

            }

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

        }

        private void ApplyHorAnchors(ref Vector2 anchorMin, ref Vector2 anchorMax, Rect safeZone) {

            if ((this.paddingType & PaddingType.DeviceScreenTop) != 0) {

                if (Screen.orientation == ScreenOrientation.LandscapeLeft) {
                    anchorMin.x = safeZone.xMin;
                } else if (Screen.orientation == ScreenOrientation.LandscapeRight) {
                    anchorMax.x = safeZone.xMax;
                }
            }

            if ((this.paddingType & PaddingType.DeviceScreenBottom) != 0) {

                if (Screen.orientation == ScreenOrientation.LandscapeLeft) {
                    anchorMax.x = safeZone.xMax;
                } else if (Screen.orientation == ScreenOrientation.LandscapeRight) {
                    anchorMin.x = safeZone.xMin;
                }

            }

        }

    }

}
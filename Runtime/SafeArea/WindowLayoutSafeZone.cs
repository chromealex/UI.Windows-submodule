namespace UnityEngine.UI.Windows {

    public class WindowLayoutSafeZone : WindowComponent, ILateUpdate {

        [ContextMenu("Apply")]
        public void ContextApply() {
            this.Apply();
        }

        public struct ScreenCache {

            private ScreenOrientation savedOrientation;
            private Rect savedSafeArea;
            private int width;
            private int height;
            private string deviceName;

            public bool HasChanged {
                get {
                    if (this.deviceName != Device.SystemInfo.deviceName) return false;
                    return Screen.width != this.width || Screen.height != this.height || Screen.orientation != this.savedOrientation || Screen.safeArea != this.savedSafeArea;
                }
            }

            public void Update() {
                this.width = Screen.width;
                this.height = Screen.height;
                this.savedOrientation = Screen.orientation;
                this.savedSafeArea = Screen.safeArea;
                this.deviceName = Device.SystemInfo.deviceName;
            }

        }
        
        [System.Serializable]
        public struct CustomPadding {

            public WindowSystemTargets targets;
            public Rect paddingPixels;
            public Rect paddingPercent;

        }

        [System.Serializable]
        public struct CustomPaddings {

            public CustomPadding[] items;
            
            public CustomPadding GetCustomPadding() {
                if (this.items == null) return default;
                var targetData = WindowSystem.GetTargetData();
                foreach (var item in this.items) {
                    if (item.targets.IsValid(targetData) == true) return item;
                }
                return default;
            }

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

        public SafeArea config = SafeArea.None;
        private ScreenCache screenCache;

        public override void OnShowBegin() {
            base.OnShowBegin();
            this.SetDirty();
        }

        public void SetDirty() {
            this.screenCache.Update();
            this.Apply();
        }

        public void OnLateUpdate(float dt) {
            if (this.screenCache.HasChanged == true) {
                this.SetDirty();
            }
        }

        public virtual void Apply() {

            var rect = this.rectTransform;
            rect.localScale = Vector3.one;

            var anchors = GetAnchors(this.config.PaddingType, this.config.CustomPaddings);
            
            rect.anchorMin = anchors.anchorMin;
            rect.anchorMax = anchors.anchorMax;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

        }

        public static (Vector2 anchorMin, Vector2 anchorMax) GetAnchors(PaddingType paddingType, CustomPaddings customPaddings) {
            
            var w = Screen.width;
            var h = Screen.height;
            var safeArea = Screen.safeArea;
            var anchorMin = safeArea.position;
            var anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= w;
            anchorMin.y /= h;
            anchorMax.x /= w;
            anchorMax.y /= h;

            if ((paddingType & PaddingType.Top) == 0) anchorMax.y = 1f;
            if ((paddingType & PaddingType.Bottom) == 0) anchorMin.y = 0f;
            if ((paddingType & PaddingType.Left) == 0) anchorMin.x = 0f;
            if ((paddingType & PaddingType.Right) == 0) anchorMax.x = 1f;

            var customPadding = customPaddings.GetCustomPadding();
            anchorMin.x += customPadding.paddingPixels.xMin / w + customPadding.paddingPercent.xMin;
            anchorMax.x += customPadding.paddingPixels.xMax / w + customPadding.paddingPercent.xMax;
            anchorMin.y += customPadding.paddingPixels.yMin / h + customPadding.paddingPercent.yMin;
            anchorMax.y += customPadding.paddingPixels.yMax / h + customPadding.paddingPercent.yMax;

            return (anchorMin, anchorMax);
            
        }

        public static RectOffset GetRectOffset(RectOffset rectOffset, PaddingType paddingType, CustomPaddings customPaddings) {
            
            var w = Screen.width;
            var h = Screen.height;
            var safeArea = Screen.safeArea;

            var custom = customPaddings.GetCustomPadding();

            if ((paddingType & PaddingType.Left) != 0) {
                rectOffset.left = Mathf.RoundToInt(
                    safeArea.xMin +
                    custom.paddingPixels.xMin +
                    custom.paddingPercent.xMin * w
                );
            }

            if ((paddingType & PaddingType.Right) != 0) {
                rectOffset.right = Mathf.RoundToInt(
                    w - safeArea.xMax +
                    custom.paddingPixels.xMax +
                    custom.paddingPercent.xMax * w
                );
            }

            if ((paddingType & PaddingType.Bottom) != 0) {
                rectOffset.bottom = Mathf.RoundToInt(
                    safeArea.yMin +
                    custom.paddingPixels.yMin +
                    custom.paddingPercent.yMin * h
                );
            }

            if ((paddingType & PaddingType.Top) != 0) {
                rectOffset.top = Mathf.RoundToInt(
                    h - safeArea.yMax +
                    custom.paddingPixels.yMax +
                    custom.paddingPercent.yMax * h
                );
            }

            return rectOffset;
            
        }

    }

}
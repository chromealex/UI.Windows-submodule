using System.Collections;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Windows {

    [ComponentModuleDisplayName("Circle swipe")]
    public class CircleSwipeComponentModule : ListComponentDraggableModule {

        public RectTransform container;

        public float snapDuration = 0.2f;
        public float rotationAngle = 45f;

        private Coroutine smoothLerpCoroutine;
        private int currentItemIndex;

        public System.Action<int> onSelectedPageChanged;

        public override void OnInit() {

            base.OnInit();

            this.UpdatePages();

        }

        public override void OnShowEnd() {
            base.OnShowEnd();

            var sectorCount = 0;

            for (var i = 0; i < this.container.childCount; ++i) {
                if (this.container.GetChild(i).gameObject.activeSelf) {
                    ++sectorCount;
                }
            }

            if (sectorCount == 0) {
                return;
            }

            this.rotationAngle = 360f / sectorCount;

        }

        private void UpdatePages() {

            this.currentItemIndex = 0;
            this.onSelectedPageChanged?.Invoke(this.currentItemIndex);

        }

        private IEnumerator SmoothLerp() {

            var timer = 0f;
            var originRotation = this.container.localRotation;

            var targetRotation = Quaternion.Euler(originRotation.eulerAngles.x, originRotation.eulerAngles.y, this.currentItemIndex * this.rotationAngle);

            while (timer < 1f) {
                timer += Time.deltaTime / this.snapDuration;
                this.container.localRotation = Quaternion.Lerp(originRotation, targetRotation, timer);
                yield return null;
            }

            this.container.localRotation = targetRotation;
            this.onSelectedPageChanged?.Invoke(this.currentItemIndex);

        }

        public override void OnBeginDrag(PointerEventData data) {

            if (this.isActiveAndEnabled == false) {
                return;
            }

            if (this.smoothLerpCoroutine != null) {
                this.StopCoroutine(this.smoothLerpCoroutine);
            }

        }

        public override void OnEndDrag(PointerEventData data) {

            if (this.isActiveAndEnabled == false) {
                return;
            }

            var sector = Mathf.Round(this.container.localRotation.eulerAngles.z / this.rotationAngle);
            if (sector >= 360f / this.rotationAngle) {

                sector = 0f;

            }

            this.currentItemIndex = (int)sector;
            this.smoothLerpCoroutine = this.StartCoroutine(this.SmoothLerp());

        }

        public override void OnDrag(PointerEventData eventData) {

            this.container.Rotate(0f, 0f, -eventData.delta.x / 10f, Space.Self);

        }

    }

}
using System;
using System.Collections;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Windows {

	[ComponentModuleDisplayName("Circle swipe")]
	public class CircleSwipeComponentModule : ListComponentModule {

		public RectTransform container;

		public float snapDuration = 0.2f;
		public float rotationAngle = 45f;

		private Coroutine smoothLerpCoroutine;
		private int currentItemIndex;

		public Action<int> onSelectedPageChanged;

		public override void OnInit() {

			base.OnInit();

			this.UpdatePages();
		}

		private void UpdatePages() {

			this.currentItemIndex = 0;
			this.onSelectedPageChanged?.Invoke(this.currentItemIndex);

		}

		private IEnumerator SmoothLerp() { 

			var timer = 0f;
			var originRotation = this.container.localRotation;

			var targetRotation = Quaternion.Euler(originRotation.eulerAngles.x, originRotation.eulerAngles.y, this.currentItemIndex * this.rotationAngle);

			while (timer < 1f)
			{
				timer += Time.deltaTime / snapDuration;
				this.container.localRotation = Quaternion.Lerp(originRotation, targetRotation, timer);
				yield return null;
			}

			this.container.localRotation = targetRotation;
			this.onSelectedPageChanged?.Invoke(this.currentItemIndex);

		}

		public override void OnDragBegin(PointerEventData data) {

			if (this.isActiveAndEnabled == false) return;

			if (this.smoothLerpCoroutine != null)
				StopCoroutine(this.smoothLerpCoroutine);

		}

		public override void OnDragEnd(PointerEventData data) {

			if(this.isActiveAndEnabled == false) return;

			var sector = Mathf.Round(this.container.localRotation.eulerAngles.z / this.rotationAngle);

			if (sector >= 360 / this.rotationAngle) {

				sector = 0;

			}

			this.currentItemIndex = (int) sector;
			this.smoothLerpCoroutine = StartCoroutine(SmoothLerp());
		}

		public override void OnDragMove(PointerEventData eventData) {

			this.container.Rotate(0, 0, -eventData.delta.x / 10, Space.Self);

		}

	}

}

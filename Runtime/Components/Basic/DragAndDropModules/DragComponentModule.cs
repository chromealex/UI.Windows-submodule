using System;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Windows.Components.DragAndDropModules {

	public class DragComponentModule : WindowComponentModule, IBeginDragHandler, IEndDragHandler, IDragHandler {

		private Action<PointerEventData> onDragCallback;
		private Action<PointerEventData> onBeginDragCallback;
		private Action<PointerEventData> onEndDragCallback;

		public void SetDragCallback(System.Action<PointerEventData> callback) {

			this.onDragCallback = callback;

		}

		public void SetBeginDragCallback(System.Action<PointerEventData> callback) {

			this.onBeginDragCallback = callback;

		}

		public void SetEndDragCallback(System.Action<PointerEventData> callback) {

			this.onEndDragCallback = callback;

		}

		public void AddDragCallback(System.Action<PointerEventData> callback) {

			this.onDragCallback += callback;

		}

		public void AddBeginDragCallback(System.Action<PointerEventData> callback) {

			this.onBeginDragCallback += callback;

		}

		public void AddEndDragCallback(System.Action<PointerEventData> callback) {

			this.onEndDragCallback += callback;

		}

		public void RemoveDragCallback(System.Action<PointerEventData> callback) {

			this.onDragCallback -= callback;

		}

		public void RemoveBeginDragCallback(System.Action<PointerEventData> callback) {

			this.onBeginDragCallback -= callback;

		}

		public void RemoveEndDragCallback(System.Action<PointerEventData> callback) {

			this.onEndDragCallback -= callback;

		}

		public void RemoveAllDragCallbacks() {

			this.onDragCallback = null;

		}

		public void RemoveAllBeginDragCallbacks() {

			this.onBeginDragCallback = null;

		}

		public void RemoveAllEndDragCallbacks() {

			this.onEndDragCallback = null;

		}

		public virtual void OnBeginDrag(PointerEventData eventData) {

			this.onBeginDragCallback?.Invoke(eventData);

		}

		private Transform GetParentCanvasTransform() {

			var currentGameObject = gameObject;

			while (currentGameObject.GetComponent<Canvas>() == null) {

				currentGameObject = currentGameObject.transform.parent.gameObject;

			}

			return currentGameObject.transform;

		}

		public virtual void OnDrag(PointerEventData eventData) {

			onDragCallback?.Invoke(eventData);

		}

		public virtual void OnEndDrag(PointerEventData eventData) {

			onEndDragCallback?.Invoke(eventData);

		}

	}

}

using System;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Windows.Components.DragAndDropModules {

	[RequireComponent(typeof(CanvasGroup))]
	public class DragComponentModule : WindowComponentModule, IBeginDragHandler, IEndDragHandler, IDragHandler {

		private GameObject dragObject;
		private RectTransform rectTransform;
		private CanvasGroup canvasGroup;

		[Tooltip("Prefab for dragging object preview")]
		public GameObject dragObjectPrefab;

		[Tooltip("Dragging object icon")]
		public Image icon;

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

		public void RemoveAllDragCallbacks(System.Action<PointerEventData> callback) {

			this.onDragCallback = null;

		}

		public void RemoveAllBeginDragCallbacks(System.Action<PointerEventData> callback) {

			this.onBeginDragCallback = null;

		}

		public void RemoveAllEndDragCallbacks(System.Action<PointerEventData> callback) {

			this.onEndDragCallback = null;

		}

		private void Awake() {

			this.canvasGroup = GetComponent<CanvasGroup>();

		}

		public void OnBeginDrag(PointerEventData eventData) {

			this.dragObject = Instantiate(dragObjectPrefab, GetParentCanvasTransform());
			this.dragObject.GetComponent<ImageComponent>().SetImage(icon.sprite);

			this.canvasGroup.alpha = .6f;
			this.canvasGroup.blocksRaycasts = false;

			this.rectTransform = dragObject.transform as RectTransform;
			this.rectTransform.position = transform.position;

			onBeginDragCallback?.Invoke(eventData);

		}

		private Transform GetParentCanvasTransform() {

			var currentGameObject = gameObject;

			while (currentGameObject.GetComponent<Canvas>() == null) {

				currentGameObject = currentGameObject.transform.parent.gameObject;

			}

			return currentGameObject.transform;

		}

		public void OnDrag(PointerEventData eventData) {

			this.rectTransform.anchoredPosition += eventData.delta;
			onDragCallback?.Invoke(eventData);

		}

		public void OnEndDrag(PointerEventData eventData) {

			this.canvasGroup.alpha = 1f;
			this.canvasGroup.blocksRaycasts = true;

			Destroy(dragObject);
			onEndDragCallback?.Invoke(eventData);

		}

	}

}

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

		public Action<PointerEventData> OnDragEvent;
		public Action<PointerEventData> OnBeginDragEvent;
		public Action<PointerEventData> OnEndDragEvent;

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

			OnBeginDragEvent?.Invoke(eventData);

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
			OnDragEvent?.Invoke(eventData);

		}

		public void OnEndDrag(PointerEventData eventData) {

			this.canvasGroup.alpha = 1f;
			this.canvasGroup.blocksRaycasts = true;

			Destroy(dragObject);
			OnEndDragEvent?.Invoke(eventData);

		}

	}

}

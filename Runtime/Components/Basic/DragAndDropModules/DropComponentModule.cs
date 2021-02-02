using System;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Windows.Components.DragAndDropModules {

	public class DropComponentModule : WindowComponentModule, IDropHandler {

		public Action<PointerEventData> OnDropEvent;

		public void OnDrop(PointerEventData eventData) {

			if (eventData.pointerDrag != null) {

				OnDropEvent?.Invoke(eventData);

			}

		}

	}

}

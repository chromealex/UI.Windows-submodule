using System;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Windows.Components.DragAndDropModules {

	public class DropComponentModule : WindowComponentModule, IDropHandler {

		private Action<PointerEventData> callback;

		public void SetCallback(System.Action<PointerEventData> callback) {

			this.callback = callback;

		}

		public void AddCallback(System.Action<PointerEventData> callback) {

			this.callback += callback;

		}

		public void RemoveCallback(System.Action<PointerEventData> callback) {

			this.callback -= callback;

		}

		public void RemoveAllCallbacks() {

			this.callback = null;

		}

		public void OnDrop(PointerEventData eventData) {

			if (eventData.pointerDrag != null) {

				callback?.Invoke(eventData);

			}

		}

	}

}

namespace UnityEngine.UI.Windows {

    public class ButtonLongPressComponentModule : ButtonComponentModule, UnityEngine.EventSystems.IPointerDownHandler, UnityEngine.EventSystems.IPointerUpHandler {

        public float pressTime = 2f;
        public UnityEngine.UI.Windows.Components.ProgressComponent progressComponent;

        [Header("Use long press via callback, not by overriding RaiseClick()")]
        public bool callbackMode;

        private float pressTimer;
        private bool isPressed;
        private System.Action callback;

		public override void ValidateEditor() {

            base.ValidateEditor();

            if (this.progressComponent != null) {
                
                this.progressComponent.hiddenByDefault = true;
                
            }

        }

		public override void OnHideBegin() {

			base.OnHideBegin();

			this.isPressed = false;
		}

		public override void OnDeInit() {

			base.OnDeInit();

			this.RemoveAllCallbacks();

		}

		public void SetCallback(System.Action callback) {

			this.callback = callback;

		}

		public void AddCallback(System.Action callback) {

			this.callback += callback;

		}

		public void RemoveCallback(System.Action callback) {

			this.callback -= callback;

		}

		public void RemoveAllCallbacks() {

			this.callback = null;

		}

		public override void OnInit() {
            
            base.OnInit();

            if (this.callbackMode == false) {

	            this.buttonComponent.button.onClick.RemoveAllListeners();

            }

		}

        public void LateUpdate() {

	        if (this.isPressed == false) return;

	        var dt = Time.realtimeSinceStartup - this.pressTimer;

			if (this.progressComponent != null) {

                this.progressComponent.SetNormalizedValue(dt / this.pressTime);
                
            }

            if (this.callbackMode == true && dt > this.pressTime) {

	            this.callback?.Invoke();
	            this.isPressed = false;

            }

		}

		public void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData) {

            if (this.isActiveAndEnabled == false) return;

			this.isPressed = true;
            this.pressTimer = Time.realtimeSinceStartup;

            if (this.progressComponent != null) {
                
                this.progressComponent.Show();
                this.progressComponent.SetNormalizedValue(0f);
                
            }

        }
        
        public void OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData) {

            this.isPressed = false;

            if (this.progressComponent != null) this.progressComponent.Hide();
            if (this.callbackMode == true) return;

            if (Time.realtimeSinceStartup - this.pressTimer >= this.pressTime) {

                this.buttonComponent.RaiseClick();

            }
            
        }

    }

}

namespace UnityEngine.UI.Windows {

    public class ButtonLongPressComponentModule : ButtonComponentModule, UnityEngine.EventSystems.IPointerDownHandler, UnityEngine.EventSystems.IPointerUpHandler {

        public float pressTime = 2f;
        public UnityEngine.UI.Windows.Components.ProgressComponent progressComponent;

        public bool hideShowProgress = true;
        [Header("Use long press via callback, not by overriding RaiseClick()")]
        public bool callbackMode;

        private float pressTimer;
        private bool isPressed;

        private CallbackRegistries callbackRegistries;
        private CallbackRegistries callbackOnBreakRegistries;
        
		public override void ValidateEditor() {

            base.ValidateEditor();

            if (this.progressComponent != null && this.hideShowProgress == true) {
                
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
			this.callbackRegistries.DeInitialize();
			this.callbackOnBreakRegistries.DeInitialize();

		}

		public void SetCallback(System.Action callback) {
			this.callbackRegistries.Clear();
			this.callbackRegistries.Add(callback);
		}

		public void AddCallback(System.Action callback) => this.callbackRegistries.Add(callback);
		public void RemoveCallback(System.Action callback) => this.callbackRegistries.Remove(callback);

		public void SetCallback<T>(T data, System.Action<T> callback) where T : System.IEquatable<T> {
			this.callbackRegistries.Clear();
			this.callbackRegistries.Add(data, callback);
		}
		
		public void AddCallback<T>(T data, System.Action<T> callback) where T : System.IEquatable<T> => this.callbackRegistries.Add(data, callback);
		public void RemoveCallback<T>(T data, System.Action<T> callback) where T : System.IEquatable<T> => this.callbackRegistries.Remove(data, callback);

		public void SetOnBreakCallback(System.Action callback) {
			this.callbackOnBreakRegistries.Clear();
			this.callbackOnBreakRegistries.Add(callback);
		}

		public void AddOnBreakCallback(System.Action callback) => this.callbackOnBreakRegistries.Add(callback);
		public void RemoveOnBreakCallback(System.Action callback) => this.callbackOnBreakRegistries.Remove(callback);

		public void SetOnBreakCallback<T>(T data, System.Action<T> callback) where T : System.IEquatable<T> {
			this.callbackOnBreakRegistries.Clear();
			this.callbackOnBreakRegistries.Add(data, callback);
		}
		
		public void AddOnBreakCallback<T>(T data, System.Action<T> callback) where T : System.IEquatable<T> => this.callbackOnBreakRegistries.Add(data, callback);
		public void RemoveOnBreakCallback<T>(T data, System.Action<T> callback) where T : System.IEquatable<T> => this.callbackOnBreakRegistries.Remove(data, callback);

		public void RemoveAllCallbacks() {

			this.callbackRegistries.Clear();
			this.callbackOnBreakRegistries.Clear();

		}

		public override void OnInit() {
            
            base.OnInit();

            this.callbackRegistries.Initialize();
            this.callbackOnBreakRegistries.Initialize();
            
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

	            this.callbackOnBreakRegistries.Clear();
	            this.callbackRegistries.Invoke();
	            this.isPressed = false;

            }

		}

		public void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData) {

            if (this.isActiveAndEnabled == false) return;

			this.isPressed = true;
            this.pressTimer = Time.realtimeSinceStartup;

            if (this.progressComponent != null) {
                
                if (this.hideShowProgress == true) this.progressComponent.Show();
                this.progressComponent.SetNormalizedValue(0f);
                
            }

        }
        
        public void OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData) {

            this.isPressed = false;

            if (this.progressComponent != null) {
	            if (this.hideShowProgress == true) {
		            this.progressComponent.Hide();
	            } else {
		            this.progressComponent.SetNormalizedValue(0f);
	            }
            }
            this.callbackOnBreakRegistries.Invoke();
            
            if (this.callbackMode == true) return;

            if (Time.realtimeSinceStartup - this.pressTimer >= this.pressTime) {

                this.buttonComponent.RaiseClick();

            }
            
        }

    }

}

using UnityEngine.EventSystems;

namespace UnityEngine.UI.Windows {

    public class DoublePressComponentModule : ButtonComponentModule, IPointerDownHandler {

        public float pressTime = 1f;

        private bool isPressed;
        private float pressTimer;
        private System.Action callback;

        public override void OnDeInit() {
	        
	        base.OnDeInit();

	        this.RemoveAllCallbacks();

        }

        public override void OnPoolAdd() {
            
            base.OnPoolAdd();
            
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

        public void LateUpdate() {

            if (this.isPressed && Time.realtimeSinceStartup - this.pressTimer > this.pressTime) {
                
                this.isPressed = false;
                
            }

        }

        public void OnPointerDown(PointerEventData eventData) {

            if (this.isPressed == false) {
            
                this.isPressed = true;
                this.pressTimer = Time.realtimeSinceStartup;
                
            } else {
            
                if (Time.realtimeSinceStartup - this.pressTimer <= this.pressTime) {
                    
                    if (this.callback != null) this.callback.Invoke();
                    this.isPressed = false;
                    
                }
                
            }

        }

    }

}

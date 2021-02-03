using UnityEngine.EventSystems;

namespace UnityEngine.UI.Windows {

    public class DoublePressComponentModule : ButtonComponentModule, IPointerDownHandler {

        public float pressTime = 1f;

        private bool isPressed;
        private float pressTimer;
        private System.Action onDoublePressed;

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
                    
                    if (this.onDoublePressed != null) this.onDoublePressed.Invoke();
                    this.isPressed = false;
                    
                }
                
            }

        }

    }

}
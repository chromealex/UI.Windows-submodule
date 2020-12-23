using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows {

    public class ButtonLongPressComponentModule : ButtonComponentModule, UnityEngine.EventSystems.IPointerDownHandler, UnityEngine.EventSystems.IPointerUpHandler {

        public float pressTime = 2f;
        public UnityEngine.UI.Windows.Components.ProgressComponent progressComponent;

        private float pressTimer;
        private bool isPressed;

        public override void ValidateEditor() {

            base.ValidateEditor();

            if (this.progressComponent != null) {
                
                this.progressComponent.hiddenByDefault = true;
                
            }

        }

        public override void OnInit() {
            
            base.OnInit();
            
            this.buttonComponent.button.onClick.RemoveAllListeners();
            
        }

        public void LateUpdate() {

            if (this.isPressed == true && this.progressComponent != null) {

                var dt = Time.realtimeSinceStartup - this.pressTimer;
                this.progressComponent.SetNormalizedValue(dt / this.pressTime);
                
            }
            
        }

        public void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData) {

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
            
            if (Time.realtimeSinceStartup - this.pressTimer >= this.pressTime) {

                this.buttonComponent.RaiseClick();

            }
            
        }

    }

}

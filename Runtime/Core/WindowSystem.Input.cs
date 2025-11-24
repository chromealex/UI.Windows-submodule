using UnityEngine.UI.Windows.Utilities;

namespace UnityEngine.UI.Windows {

    public partial class WindowSystem {

        private Vector2 pointerScreenPosition;
        private bool hasPointerUpThisFrame;
        private bool hasPointerDownThisFrame;
        private float lastPointerActionTime;

        public static Vector2 GetPointerPosition() {

            return WindowSystem.instance.pointerScreenPosition;

        }

        public static bool HasPointerUpThisFrame() {

            return WindowSystem.instance.hasPointerUpThisFrame;

        }
        
        public static bool HasPointerDownThisFrame() {

            return WindowSystem.instance.hasPointerDownThisFrame;

        }
        
        public void DoUpdateInput() {

            if (this.modules != null) {

                for (int i = 0; i < this.modules.Count; ++i) {

                    this.modules[i]?.OnUpdate();

                }

            }

            this.hasPointerUpThisFrame = false;
            this.hasPointerDownThisFrame = false;
            WindowSystemInput.hasPointerClickThisFrame.Data = false;
            
            #if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Mouse.current != null &&
                (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame == true ||
                 UnityEngine.InputSystem.Mouse.current.rightButton.wasPressedThisFrame == true ||
                 UnityEngine.InputSystem.Mouse.current.middleButton.wasPressedThisFrame == true)) {
                
                this.pointerScreenPosition = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
                if (WindowSystem.onPointerDown != null) WindowSystem.onPointerDown.Invoke();
                
            }
            if (UnityEngine.InputSystem.Mouse.current != null &&
                (UnityEngine.InputSystem.Mouse.current.leftButton.wasReleasedThisFrame == true ||
                UnityEngine.InputSystem.Mouse.current.rightButton.wasReleasedThisFrame == true ||
                UnityEngine.InputSystem.Mouse.current.middleButton.wasReleasedThisFrame == true)) {
                
                this.pointerScreenPosition = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
                this.hasPointerUpThisFrame = true;
                this.lastPointerActionTime = Time.realtimeSinceStartup;
                if (WindowSystem.onPointerUp != null) WindowSystem.onPointerUp.Invoke();
                
            }

            var touches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
            if (touches.Count > 0) {

                for (int i = 0; i < touches.Count; ++i) {

                    var touch = touches[i];
                    if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began) {
                        
                        this.pointerScreenPosition = touch.screenPosition;
                        this.lastPointerActionTime = Time.realtimeSinceStartup;
                        if (WindowSystem.onPointerDown != null) WindowSystem.onPointerDown.Invoke();
                        
                    } else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended || touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled) {

                        this.pointerScreenPosition = touch.screenPosition;
                        this.hasPointerUpThisFrame = true;
                        WindowSystemInput.hasPointerClickThisFrame.Data = (Time.realtimeSinceStartup - this.lastPointerActionTime) <= CLICK_THRESHOLD;
                        this.lastPointerActionTime = Time.realtimeSinceStartup;
                        if (WindowSystem.onPointerUp != null) WindowSystem.onPointerUp.Invoke();

                    }

                }
                
            }
            #elif ENABLE_LEGACY_INPUT_MANAGER
            if (UnityEngine.Input.GetMouseButtonDown(0) == true ||
                UnityEngine.Input.GetMouseButtonDown(1) == true ||
                UnityEngine.Input.GetMouseButtonDown(2) == true) {
                
                this.pointerScreenPosition = Input.mousePosition;
                this.hasPointerDownThisFrame = true;
                this.lastPointerActionTime = Time.realtimeSinceStartup;
                if (WindowSystem.onPointerDown != null) WindowSystem.onPointerDown.Invoke();
                
            }
            
            if (UnityEngine.Input.GetMouseButtonUp(0) == true ||
                UnityEngine.Input.GetMouseButtonUp(1) == true ||
                UnityEngine.Input.GetMouseButtonUp(2) == true) {
                
                this.pointerScreenPosition = Input.mousePosition;
                this.hasPointerUpThisFrame = true;
                WindowSystemInput.hasPointerClickThisFrame.Data = (Time.realtimeSinceStartup - this.lastPointerActionTime) <= CLICK_THRESHOLD;
                this.lastPointerActionTime = Time.realtimeSinceStartup;
                if (WindowSystem.onPointerUp != null) WindowSystem.onPointerUp.Invoke();
                
            }

            if (UnityEngine.Input.touchCount > 0) {

                for (int i = 0; i < UnityEngine.Input.touches.Length; ++i) {

                    var touch = UnityEngine.Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Began) {
                        
                        this.pointerScreenPosition = touch.position;
                        this.lastPointerActionTime = Time.realtimeSinceStartup;
                        if (WindowSystem.onPointerDown != null) WindowSystem.onPointerDown.Invoke();
                        
                    } else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) {
                        
                        this.pointerScreenPosition = touch.position;
                        this.hasPointerUpThisFrame = true;
                        if (WindowSystem.onPointerUp != null) WindowSystem.onPointerUp.Invoke();

                    }

                }
                
            }
            #endif
            
        }

    }

}
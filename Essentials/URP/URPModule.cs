#if UNITY_URP
using UnityEngine.Rendering.Universal;
#endif

namespace UnityEngine.UI.Windows {
    
    [CreateAssetMenu(menuName = "UI.Windows/URPModule")]
    public class URPModule : WindowSystemModule {

        private readonly System.Collections.Generic.List<Camera> cameras = new System.Collections.Generic.List<Camera>();
        
        public virtual Camera MainCamera => Camera.main;
        
#if UNITY_URP
        private bool awaitForCamera;
        private void AddToStack(WindowObject obj) {

            if (obj is WindowBase window) {
                this.cameras.Add(window.workCamera);
                this.SortCameras();
                
                if (this.MainCamera == null) {
                    this.awaitForCamera = true;
                    return;
                }
                
                WindowSystem.GetEvents().RegisterOnce(window, WindowEvent.OnDeInitialize, this.RemoveFromStack);

                this.UpdateStack();
            }
            
        }

        private void UpdateStack() {
            
            var data = this.MainCamera.GetUniversalAdditionalCameraData();
            foreach (var camera in this.cameras) {
                data.cameraStack.Remove(camera);
            }
            foreach (var camera in this.cameras) {
                data.cameraStack.Add(camera);
            }

        }

        private void SortCameras() {
            this.cameras.Sort((c1, c2) => c1.depth.CompareTo(c2.depth));
        }

        private void RemoveFromStack(WindowObject obj) {
            
            if (obj is WindowBase window) {
                this.cameras.Remove(window.workCamera);
                if (this.MainCamera == null) {
                    return;
                }
                this.MainCamera.GetUniversalAdditionalCameraData().cameraStack.Remove(window.workCamera);
            }
            
        }

        public override void OnUpdate() {

            if (this.awaitForCamera == true && this.MainCamera != null) {
                this.awaitForCamera = false;
                this.UpdateStack();
            }
            
        }

        public override void OnStart() {
            WindowSystem.GetEvents().Register(WindowEvent.OnInitialize, this.AddToStack);
        }

        public override void OnDestroy() {
            WindowSystem.GetEvents().UnRegister(WindowEvent.OnInitialize, this.AddToStack);
        }
#else
        public override void OnStart() {
            
        }

        public override void OnDestroy() {
            
        }
#endif

    }
    
}

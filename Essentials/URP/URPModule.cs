#if UNITY_URP
using UnityEngine.Rendering.Universal;
#endif

namespace UnityEngine.UI.Windows {
    
    [CreateAssetMenu(menuName = "UI.Windows/Modules/URPModule")]
    public class URPModule : WindowSystemModule {

        private readonly System.Collections.Generic.List<Camera> cameras = new System.Collections.Generic.List<Camera>();
        
        public virtual Camera MainCamera => Camera.main;
        
#if UNITY_URP
        private bool awaitForCamera;

        protected virtual System.Collections.Generic.List<Camera> GetCameras() => this.cameras;
        
        protected virtual void AddToStack(WindowObject obj) {

            if (obj is WindowBase window) {
                this.GetCameras().Add(window.workCamera);
                this.SortCameras();
                
                if (this.MainCamera == null) {
                    this.awaitForCamera = true;
                    return;
                }
                
                WindowSystem.GetEvents().RegisterOnce((module: this, window), window, WindowEvent.OnDeInitialized, static (obj, data) => data.module.RemoveFromStack(obj));

                this.UpdateStack();
            }
            
        }

        protected virtual void UpdateStack() {
            
            this.GetCameras().RemoveAll(x => x == null);
            
            var data = this.MainCamera.GetUniversalAdditionalCameraData();
            foreach (var camera in this.GetCameras()) {
                data.cameraStack.Remove(camera);
            }
            foreach (var camera in this.GetCameras()) {
                data.cameraStack.Add(camera);
            }

        }

        protected virtual void SortCameras() {
            this.GetCameras().Sort((c1, c2) => {
                if (c1 == null || c2 == null) return 0;
                return c1.depth.CompareTo(c2.depth);
            });
        }

        protected virtual void RemoveFromStack(WindowObject obj) {
            
            if (obj is WindowBase window) {
                this.GetCameras().Remove(window.workCamera);
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
            WindowSystem.GetEvents().Register(WindowEvent.OnInitializing, this.AddToStack);
        }

        public override void OnDestroy() {
            WindowSystem.GetEvents().UnRegister(WindowEvent.OnInitializing, this.AddToStack);
        }
#else
        public override void OnStart() {
            
        }

        public override void OnDestroy() {
            
        }
#endif

    }
    
}

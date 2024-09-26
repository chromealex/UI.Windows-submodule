#if UNITY_URP
using UnityEngine.Rendering.Universal;
#endif

namespace UnityEngine.UI.Windows {
    
    [CreateAssetMenu(menuName = "UI.Windows/URPModule")]
    public class URPModule : WindowSystemModule {
        
        public virtual Camera MainCamera => Camera.main;
        
#if UNITY_URP

        private void AddToStack(WindowObject obj) {

            if (obj is WindowBase window) {
                this.MainCamera.GetUniversalAdditionalCameraData().cameraStack.Add(window.workCamera);
                WindowSystem.GetEvents().RegisterOnce(window, WindowEvent.OnDeInitialize, this.RemoveFromStack);
            }
            
        }

        private void RemoveFromStack(WindowObject obj) {
            
            if (obj is WindowBase window) {
                this.MainCamera.GetUniversalAdditionalCameraData().cameraStack.Remove(window.workCamera);
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

namespace UnityEngine.UI.Windows {

    public class UIParticleSystem : MonoBehaviour {

        public WindowObject windowObject;
        public int sortingOrder;
        public string sortingLayerName;
        public ParticleSystemRenderer particleSystemRenderer;

        public void OnValidate() {
            
            if (this.particleSystemRenderer == null) this.particleSystemRenderer = this.GetComponent<ParticleSystemRenderer>();
            
        }
        
        public void OnEnable() {

            if (this.windowObject.GetState() >= ObjectState.Showing) {
                
                this.ApplyOrder();
                
            } else {
            
                WindowSystem.GetEvents().RegisterOnce(this.windowObject, WindowEvent.OnShowBegin, this.ApplyOrder);

            }
            
        }

        private void ApplyOrder() {

            this.particleSystemRenderer.sortingOrder = this.windowObject.GetWindow().GetCanvasOrder() + this.sortingOrder;
            if (string.IsNullOrEmpty(this.sortingLayerName) == false) this.particleSystemRenderer.sortingLayerName = this.sortingLayerName;

        }

    }

}
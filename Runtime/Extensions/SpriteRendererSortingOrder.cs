namespace UnityEngine.UI.Windows {

    public class SpriteRendererSortingOrder : MonoBehaviour {

        public WindowObject windowObject;
        public int sortingOrder;
        public string sortingLayerName;
        public SpriteRenderer spriteRenderer;

        public void OnValidate() {

            if (this.spriteRenderer == null) this.spriteRenderer = this.GetComponent<SpriteRenderer>();
            
        }

        public void OnEnable() {

            if (this.windowObject.GetState() >= ObjectState.Showing) {

                this.ApplyOrder(this.windowObject);

            } else {

                WindowSystem.GetEvents().RegisterOnce(this.windowObject, WindowEvent.OnShowBegin, this.ApplyOrder);

            }

        }

        private void ApplyOrder(WindowObject obj) {

            this.spriteRenderer.sortingOrder = this.windowObject.GetWindow().GetCanvasOrder() + this.sortingOrder;
            if (string.IsNullOrEmpty(this.sortingLayerName) == false) this.spriteRenderer.sortingLayerName = this.sortingLayerName;

        }

    }

}
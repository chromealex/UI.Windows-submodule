namespace UnityEngine.UI.Windows {

    public class CanvasRebuilderEvents : MonoBehaviour, ICanvasElement {

        public WindowLayout windowLayout;
        
        public void Rebuild(CanvasUpdate executing) {
            if (this.windowLayout != null) this.windowLayout.GetWindow().DoLayoutEvent(executing);
        }

        public void LayoutComplete() {
            if (this.windowLayout != null) this.windowLayout.GetWindow().DoLayoutReady();
        }

        public void GraphicUpdateComplete() {
            
        }

        public bool IsDestroyed() {
            return false;
        }

    }

}
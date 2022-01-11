namespace UnityEngine.UI.Windows {

    internal class ListRectTransformChangedInternal : MonoBehaviour {

        public UnityEngine.UI.Windows.Components.ListBaseComponent listBaseComponent;

        public void OnRectTransformDimensionsChange() {
            
            if (this.listBaseComponent != null) this.listBaseComponent.ForceLayoutChange();
            
        }

    }

}

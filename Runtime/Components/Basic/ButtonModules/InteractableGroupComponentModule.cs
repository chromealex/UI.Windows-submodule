namespace UnityEngine.UI.Windows {

    public class InteractableGroupComponentModule : ButtonComponentModule {

        public CanvasGroup[] canvasGroups;
        public bool changeAlpha;
        public float alphaNormal;
        public float alphaDisabled;

        public override void OnInteractableChanged(bool state) {
            
            base.OnInteractableChanged(state);

            foreach (var canvasGroup in this.canvasGroups) {

                canvasGroup.interactable = state;
                if (this.changeAlpha == true) {

                    canvasGroup.alpha = (state == true ? this.alphaNormal : this.alphaDisabled);

                }

            }

        }

    }

}

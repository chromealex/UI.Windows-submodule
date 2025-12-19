namespace UnityEngine.UI.Windows {

    public abstract class ImageComponentModule : WindowComponentModule {

        public UnityEngine.UI.Windows.Components.ImageComponent imageComponent;

        public override void ValidateEditor() {
            
            base.ValidateEditor();
            
            this.imageComponent = this.windowComponent as UnityEngine.UI.Windows.Components.ImageComponent;
            
        }

        public virtual void SetImage(Sprite prevSprite, Sprite newSprite) {
            
        }

        public virtual void SetImage(Texture prevTexture, Texture newTexture) {
            
        }

    }
    
}

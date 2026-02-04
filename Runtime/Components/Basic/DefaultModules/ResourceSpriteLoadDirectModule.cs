namespace UnityEngine.UI.Windows {

    public class ResourceSpriteLoadDirectModule : WindowComponentModule {

        public ResourceRef<Sprite> resourceRef;
        public Image image;
        
        public override void OnInit() {
            
            base.OnInit();
            
            this.image.sprite = this.resourceRef.Load(this);
            
        }

        public override void OnDeInit() {
            
            this.resourceRef.Unload(this);
            
            base.OnDeInit();
            
        }

    }
    
}

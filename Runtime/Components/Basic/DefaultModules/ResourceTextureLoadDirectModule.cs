namespace UnityEngine.UI.Windows {

    public class ResourceTextureLoadDirectModule : WindowComponentModule {

        public ResourceRef<Texture> resourceRef;
        public RawImage image;
        
        public override void OnInit() {
            
            base.OnInit();
            
            this.image.texture = this.resourceRef.Load(this);
            
        }

        public override void OnDeInit() {
            
            this.resourceRef.Unload(this);
            
            base.OnDeInit();
            
        }

    }
    
}

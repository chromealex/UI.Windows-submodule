namespace UnityEngine.UI.Windows {

    public class ResourceTextureLoadDirectModule : WindowComponentModule {

        public Resource<Texture> resource;
        public RawImage image;
        
        public override void OnInit() {
            
            base.OnInit();
            
            this.image.texture = this.resource.Load(this);
            
        }

        public override void OnDeInit() {
            
            this.resource.Unload(this);
            
            base.OnDeInit();
            
        }

    }
    
}

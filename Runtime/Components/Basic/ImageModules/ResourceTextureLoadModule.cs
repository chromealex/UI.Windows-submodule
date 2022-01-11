namespace UnityEngine.UI.Windows {

    public class ResourceTextureLoadModule : ImageComponentModule {

        public Resource<Texture> resource;
        
        public override void OnInit() {
            
            base.OnInit();
            
            this.imageComponent.SetImage(this.resource);
            
        }

    }
    
}

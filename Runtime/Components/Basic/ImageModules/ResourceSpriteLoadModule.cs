namespace UnityEngine.UI.Windows {

    public class ResourceSpriteLoadModule : ImageComponentModule {

        public Resource<Sprite> resource;
        
        public override void OnInit() {
            
            base.OnInit();
            
            this.imageComponent.SetImage(this.resource);
            
        }

    }
    
}

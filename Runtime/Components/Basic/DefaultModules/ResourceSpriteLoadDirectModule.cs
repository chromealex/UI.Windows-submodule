namespace UnityEngine.UI.Windows {

    public class ResourceSpriteLoadDirectModule : WindowComponentModule {

        public Resource<Sprite> resource;
        public Image image;
        
        public override void OnInit() {
            
            base.OnInit();
            
            this.image.sprite = this.resource.Load(this);
            
        }

        public override void OnDeInit() {
            
            this.resource.Unload(this);
            
            base.OnDeInit();
            
        }

    }
    
}

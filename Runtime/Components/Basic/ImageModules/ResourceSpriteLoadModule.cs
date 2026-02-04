namespace UnityEngine.UI.Windows {

    using UnityEngine.UI.Windows.Modules;
    
    public class ResourceSpriteLoadModule : ImageComponentModule {

        [ResourceType(typeof(Sprite))]
        public Resource resource;
        
        public override void OnInit() {
            
            base.OnInit();
            
            this.imageComponent.SetImage(this.resource);
            
        }

    }
    
}

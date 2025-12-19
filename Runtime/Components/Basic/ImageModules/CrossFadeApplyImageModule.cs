namespace UnityEngine.UI.Windows {

    public class CrossFadeApplyImageModule : ImageComponentModule {

        public float duration = 0.5f;
        private Graphic graphics;

        public override void OnInit() {
            base.OnInit();
            // Create a copy
            var go = new GameObject($"[InternalCopy] {this.imageComponent.name}");
            this.graphics = (Graphic)go.AddComponent(this.imageComponent.graphics.GetType());
        }

        public override void SetImage(Sprite prevSprite, Sprite newSprite) {

        }

        public override void SetImage(Texture prevTexture, Texture newTexture) {

        }

    }
    
}

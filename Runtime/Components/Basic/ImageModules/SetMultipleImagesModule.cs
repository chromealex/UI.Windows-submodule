namespace UnityEngine.UI.Windows {

    using Components;
    
    public class SetMultipleImagesModule : ImageComponentModule {

        public ImageComponent[] others;

        public override void SetImage(Sprite prevSprite, Sprite newSprite) {

            foreach (var other in this.others) {
                other.SetImage(newSprite);
            }

        }

        public override void SetImage(Texture prevTexture, Texture newTexture) {

            foreach (var other in this.others) {
                other.SetImage(newTexture);
            }

        }

    }
    
}

namespace UnityEngine.UI.Windows {

    using Components;
    
    public class SetMultipleImagesModule : ImageComponentModule {

        public ImageComponent[] others;

        public override void SetImage(Sprite sprite) {

            foreach (var other in this.others) {
                other.SetImage(sprite);
            }

        }

        public override void SetImage(Texture sprite) {

            foreach (var other in this.others) {
                other.SetImage(sprite);
            }

        }

    }
    
}

namespace UnityEngine.UI.Windows {

    using Components;
    
    public class SetMultipleImagesModule : ImageComponentModule {

        [System.Flags]
        public enum ApplyFlag {
            SetSprite = 1 << 0,
            SetTexture = 1 << 1,
            SetColor = 1 << 2,
            SetMaterial = 1 << 3,
            All = -1,
        }

        public ApplyFlag flags = ApplyFlag.All;
        public ImageComponent[] others;

        public override void SetImage(Sprite prevSprite, Sprite newSprite) {

            if ((this.flags & ApplyFlag.SetSprite) == 0) return;

            foreach (var other in this.others) {
                other.SetImage(newSprite);
            }

        }

        public override void SetImage(Texture prevTexture, Texture newTexture) {

            if ((this.flags & ApplyFlag.SetTexture) == 0) return;

            foreach (var other in this.others) {
                other.SetImage(newTexture);
            }

        }

        public override void OnSetColor(Color color) {
            
            if ((this.flags & ApplyFlag.SetColor) == 0) return;

            foreach (var other in this.others) {
                other.SetColor(color);
            }

        }

        public override void OnSetMaterial(Material material) {
            
            if ((this.flags & ApplyFlag.SetMaterial) == 0) return;

            foreach (var other in this.others) {
                other.SetMaterial(material);
            }

        }

    }
    
}

namespace UnityEngine.UI.Windows {

    public class CrossFadeApplyImageModule : ImageComponentModule {

        public float duration = 0.5f;
        
        private Graphic graphics;

        public override void OnInit() {
            base.OnInit();
            // Create a copy
            #if UNITY_EDITOR
            var name = $"[InternalCopy] {this.imageComponent.name}";
            #else
            var name = "[InternalCopy]";
            #endif
            var go = new GameObject(name);
            go.layer = this.imageComponent.gameObject.layer;
            var tr = go.AddComponent<RectTransform>();
            tr.SetParent(this.imageComponent.graphics.transform);
            tr.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.pivot = new Vector2(0.5f, 0.5f);
            tr.anchoredPosition = Vector2.zero;
            tr.sizeDelta = Vector2.zero;
            tr.localScale = Vector3.one;
            this.graphics = (Graphic)go.AddComponent(this.imageComponent.graphics.GetType());
            this.graphics.raycastTarget = false;
            this.HideContainer();
        }

        public override void SetImage<TClosure>(TClosure closure, Sprite prevSprite, Sprite newSprite, System.Action<TClosure, Sprite> onFinished) {
            
            // set prev texture to the original container
            var img = (Image)this.imageComponent.graphics;
            img.sprite = prevSprite;
            
            // set new texture to the cross-fade container
            var curImg = (Image)this.graphics;
            curImg.sprite = newSprite;
            
            // fade out new texture
            var startColor = this.graphics.color;
            var tweener = WindowSystem.GetTweener();
            tweener.Stop(this.graphics);
            tweener.Add((img, startColor, newSprite, module: this, closure, onFinished), this.duration, 0f, 1f)
                   .Tag(this.graphics)
                   .OnUpdate(static (obj, value) => {
                       var targetColor = obj.module.graphics.color;
                       targetColor.a = 1f;
                       obj.module.graphics.color = Color.Lerp(obj.startColor, targetColor, value);
                   }).OnComplete(static (obj) => {
                       // set new texture to the original container
                       obj.img.sprite = obj.newSprite;
                       // reset cross-fade container
                       ((Image)obj.module.graphics).sprite = null;
                       obj.module.HideContainer();
                       obj.onFinished.Invoke(obj.closure, obj.newSprite);
                   }).OnCancel(static (obj) => {
                       // set new texture to the original container
                       obj.img.sprite = obj.newSprite;
                       // reset cross-fade container
                       ((Image)obj.module.graphics).sprite = null;
                       obj.module.HideContainer();
                       obj.onFinished.Invoke(obj.closure, obj.newSprite);
                   });
            
        }

        public override void SetImage<TClosure>(TClosure closure, Texture prevTexture, Texture newTexture, System.Action<TClosure, Texture> onFinished) {

            // set prev texture to the original container
            var img = (RawImage)this.imageComponent.graphics;
            img.texture = prevTexture;
            
            // set new texture to the cross-fade container
            var curImg = (RawImage)this.graphics;
            curImg.texture = newTexture;
            
            // fade out new texture
            var startColor = this.graphics.color;
            var tweener = WindowSystem.GetTweener();
            tweener.Stop(this.graphics);
            tweener.Add((img, startColor, newTexture, module: this, closure, onFinished), this.duration, 0f, 1f)
                   .Tag(this.graphics)
                   .OnUpdate(static (obj, value) => {
                       var targetColor = obj.module.graphics.color;
                       targetColor.a = 1f;
                       obj.module.graphics.color = Color.Lerp(obj.startColor, targetColor, value);
                   }).OnComplete(static (obj) => {
                       // set new texture to the original container
                       obj.img.texture = obj.newTexture;
                       // reset cross-fade container
                       ((RawImage)obj.module.graphics).texture = null;
                       obj.module.HideContainer();
                       obj.onFinished.Invoke(obj.closure, obj.newTexture);
                   }).OnCancel(static (obj) => {
                       // set new texture to the original container
                       obj.img.texture = obj.newTexture;
                       // reset cross-fade container
                       ((RawImage)obj.module.graphics).texture = null;
                       obj.module.HideContainer();
                       obj.onFinished.Invoke(obj.closure, obj.newTexture);
                   });

        }

        private void HideContainer() {
            var color = this.imageComponent.graphics.color;
            color.a = 0f;
            this.graphics.color = color;
        }

    }
    
}

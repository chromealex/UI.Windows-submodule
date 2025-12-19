namespace UnityEngine.UI.Windows {
    
    using UnityEngine.UI.Windows.Utilities;

    public class CrossFadeApplyImageModule : ImageComponentModule {

        private static readonly UnityEngine.Pool.ObjectPool<Image> crossFadeApplyImagePool = new UnityEngine.Pool.ObjectPool<Image>(static () => {
            var name = "[InternalCopy] Image";
            var go = new GameObject(name);
            go.AddComponent<RectTransform>();
            return go.AddComponent<Image>();
        }, actionOnGet: static (img) => {
            img.gameObject.SetActive(true);
        }, actionOnRelease: static (img) => {
            img.sprite = null;
            img.gameObject.SetActive(false);
            var poolRoot = WindowSystem.GetPools().transform;
            img.transform.SetParent(poolRoot);
        });
        
        private static readonly UnityEngine.Pool.ObjectPool<RawImage> crossFadeApplyRawImagePool = new UnityEngine.Pool.ObjectPool<RawImage>(static () => {
            var name = "[InternalCopy] RawImage";
            var go = new GameObject(name);
            go.AddComponent<RectTransform>();
            return go.AddComponent<RawImage>();
        }, actionOnGet: static (img) => {
            img.gameObject.SetActive(true);
        }, actionOnRelease: static (img) => {
            img.texture = null;
            img.gameObject.SetActive(false);
            var poolRoot = WindowSystem.GetPools().transform;
            img.transform.SetParent(poolRoot);
        });
        
        public float duration = 0.5f;
        public Tweener.EaseFunction easeFunction;
        
        private Image GetImageInstance() {
            return this.GetInstance(crossFadeApplyImagePool);
        }

        private RawImage GetRawImageInstance() {
            return this.GetInstance(crossFadeApplyRawImagePool);
        }

        private void ReleaseImageInstance(Image image) {
            crossFadeApplyImagePool.Release(image);
        }

        private void ReleaseRawImageInstance(RawImage image) {
            crossFadeApplyRawImagePool.Release(image);
        }

        private T GetInstance<T>(UnityEngine.Pool.ObjectPool<T> pool) where T : Graphic {
            var instance = pool.Get();
            var tr = instance.rectTransform;
            tr.SetParent(this.imageComponent.graphics.transform);
            tr.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.pivot = new Vector2(0.5f, 0.5f);
            tr.anchoredPosition = Vector2.zero;
            tr.sizeDelta = Vector2.zero;
            tr.localScale = Vector3.one;
            instance.raycastTarget = false;
            return instance;
        }

        public override void SetImage<TClosure>(TClosure closure, Sprite prevSprite, Sprite newSprite, System.Action<TClosure, Sprite> onFinished) {
            
            // set prev texture to the original container
            var img = (Image)this.imageComponent.graphics;
            img.sprite = prevSprite;
            img.enabled = prevSprite != null;
            
            // set new texture to the cross-fade container
            var curImg = this.GetImageInstance();
            curImg.sprite = newSprite;
            curImg.enabled = newSprite != null;
            
            // fade out new texture
            var startColor = curImg.color;
            startColor.a = 0f;
            var tweener = WindowSystem.GetTweener();
            tweener.Add((img, startColor, newSprite, curImg, module: this, closure, onFinished), this.duration, 0f, 1f)
                   .Tag(this.imageComponent)
                   .Ease(this.easeFunction)
                   .OnUpdate(static (obj, value) => {
                       var targetColor = obj.curImg.color;
                       targetColor.a = 1f;
                       obj.curImg.color = Color.Lerp(obj.startColor, targetColor, value);
                   }).OnComplete(static (obj) => {
                       // set new texture to the original container
                       obj.img.sprite = obj.newSprite;
                       obj.img.enabled = true;
                       // reset cross-fade container
                       obj.module.ReleaseImageInstance(obj.curImg);
                       obj.onFinished.Invoke(obj.closure, obj.newSprite);
                   });
            
        }

        public override void SetImage<TClosure>(TClosure closure, Texture prevTexture, Texture newTexture, System.Action<TClosure, Texture> onFinished) {

            // set prev texture to the original container
            var img = (RawImage)this.imageComponent.graphics;
            img.texture = prevTexture;
            img.enabled = prevTexture != null;
            
            // set new texture to the cross-fade container
            var curImg = this.GetRawImageInstance();
            curImg.texture = newTexture;
            curImg.enabled = newTexture != null;
            
            // fade out new texture
            var startColor = curImg.color;
            startColor.a = 0f;
            var tweener = WindowSystem.GetTweener();
            tweener.Add((img, startColor, newTexture, curImg, module: this, closure, onFinished), this.duration, 0f, 1f)
                   .Tag(this.imageComponent)
                   .Ease(this.easeFunction)
                   .OnUpdate(static (obj, value) => {
                       var targetColor = obj.curImg.color;
                       targetColor.a = 1f;
                       obj.curImg.color = Color.Lerp(obj.startColor, targetColor, value);
                   }).OnComplete(static (obj) => {
                       // set new texture to the original container
                       obj.img.texture = obj.newTexture;
                       obj.img.enabled = true;
                       // reset cross-fade container
                       obj.module.ReleaseRawImageInstance(obj.curImg);
                       obj.onFinished.Invoke(obj.closure, obj.newTexture);
                   });

        }

    }
    
}

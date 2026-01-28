using System.Collections;

namespace UnityEngine.UI.Windows.Components {

    using Modules;
    using Utilities;

    public class ImageComponent : GenericComponent, ISearchComponentByTypeEditor, ISearchComponentByTypeSingleEditor {

        System.Type ISearchComponentByTypeEditor.GetSearchType() { return typeof(ImageComponentModule); }
        IList ISearchComponentByTypeSingleEditor.GetSearchTypeArray() { return this.componentModules.modules; }

        [RequiredReference]
        public UnityEngine.UI.Graphic graphics;

        public bool preserveAspect;
        public bool useSpriteMesh;

        public UnloadResourceEventType autoUnloadResourcesOnEvent;

        private Resource prevResourceLoad;

        private readonly struct Texture2DConstructor : IResourceConstructor<Texture2D> {

            private readonly int x;
            private readonly int y;

            public Texture2DConstructor(int x, int y) {

                this.x = x;
                this.y = y;

            }

            public Texture2D Construct() {

                return new Texture2D(this.x, this.y);

            }
            
            public void Deconstruct(ref Texture2D obj) {

                Object.DestroyImmediate(obj);
                obj = null;

            }

        }

        private readonly struct SpriteConstructor : IResourceConstructor<Sprite> {

            private readonly Texture2D tex2d;
            private readonly Rect rect;
            private readonly Vector2 pivot;

            public SpriteConstructor(Texture2D tex2d, Rect rect, Vector2 pivot) {

                this.tex2d = tex2d;
                this.rect = rect;
                this.pivot = pivot;

            }

            public Sprite Construct() {

                return Sprite.Create(this.tex2d, this.rect, this.pivot);

            }

            public void Deconstruct(ref Sprite obj) {

                Object.DestroyImmediate(obj);
                obj = null;

            }

        }

        internal override void OnDeInitInternal() {

            base.OnDeInitInternal();

            if (this.autoUnloadResourcesOnEvent == UnloadResourceEventType.OnDeInit) {

                this.UnloadCurrentResources();

            }

        }

        internal override void OnHideBeginInternal() {
            
            base.OnHideBeginInternal();
            
            if (this.autoUnloadResourcesOnEvent == UnloadResourceEventType.OnHideBegin) {

                this.UnloadCurrentResources();

            }

        }

        internal override void OnHideEndInternal() {
            
            base.OnHideEndInternal();
            
            if (this.autoUnloadResourcesOnEvent == UnloadResourceEventType.OnHideEnd) {

                this.UnloadCurrentResources();

            }

        }

        private void UnloadCurrentResources() {

            this.prevResourceLoad = default;

            var resources = WindowSystem.GetResources();
            if (this.graphics is UnityEngine.UI.Image image) {

                var obj = image.sprite;
                image.sprite = null;
                resources.Delete(this, obj);

            } else if (this.graphics is UnityEngine.UI.RawImage rawImage) {

                var obj = rawImage.texture;
                rawImage.texture = null;
                resources.Delete(this, obj);

            }
            
        }
        
        public void SetImage<T>(T provider, bool async = true) where T : IResourceProvider {

            if (provider == null) return;
            this.SetImage(provider.GetResource(), async);

        }

        private readonly struct LoadClosure {

            public readonly ImageComponent component;
            public readonly Resource resource;
            public readonly System.Action onSetImageComplete;

            public LoadClosure(ImageComponent component, Resource resource, System.Action onSetImageComplete) {
                this.component = component;
                this.resource = resource;
                this.onSetImageComplete = onSetImageComplete;
            }

        }
        
        public void SetImage(Resource resource, bool async = true, System.Action onSetImageComplete = null) {

            if (this.prevResourceLoad.IsEquals(resource) == false) {

                var resources = WindowSystem.GetResources();
                switch (resource.objectType) {

                    case Resource.ObjectType.Sprite: {
                        if (async == true) {
                            resources.LoadAsync<Sprite, LoadClosure>(this, new LoadClosure(this, resource, onSetImageComplete), resource, static (asset, data) => {
                                if (data.component != null) {
                                    data.component.SetImage(asset);
                                    data.component.prevResourceLoad = data.resource;
                                    data.onSetImageComplete?.Invoke();
                                }
                            });
                        } else {
                            this.SetImage(resources.Load<Sprite>(this, resource));
                            onSetImageComplete?.Invoke();
                            this.prevResourceLoad = resource;
                        }
                    } 
                        break;

                    case Resource.ObjectType.Texture: {
                        if (async == true) {
                            resources.LoadAsync<Texture, LoadClosure>(this, new LoadClosure(this, resource, onSetImageComplete), resource, static (asset, data) => {
                                if (data.component != null) {
                                    data.component.SetImage(asset);
                                    data.component.prevResourceLoad = data.resource;
                                    data.onSetImageComplete?.Invoke();
                                }
                            });
                        } else {
                            this.SetImage(resources.Load<Texture>(this, resource));
                            onSetImageComplete?.Invoke();
                            this.prevResourceLoad = resource;
                        }
                    }
                        break;

                }

            } else {
                onSetImageComplete?.Invoke();
            }

        }

        private class Counter {

            public int value;

        }

        public void SetImage(Sprite sprite) {

            if (this.graphics is UnityEngine.UI.Image) {

                this.SetImage_INTERNAL(sprite);
                
            } else if (this.graphics is UnityEngine.UI.RawImage) {

                if (WindowSystem.IsStrictMode() == true) {
                    WindowSystem.StrictWarning(this, sprite);
                    return;
                }
                
                var resources = WindowSystem.GetResources();
                var size = new Vector2Int((int)sprite.rect.width, (int)sprite.rect.height);
                var tex = resources.New<Texture2D, Texture2DConstructor>(this, new Texture2DConstructor(size.x, size.y));
                var block = sprite.texture.GetPixels((int)sprite.rect.x, (int)sprite.rect.y, size.x, size.y);
                tex.SetPixels(block);
                tex.Apply();
                
                this.SetImage_INTERNAL(tex);

            }

        }

        public void SetImage(Texture texture) {

            if (this.graphics is UnityEngine.UI.RawImage) {

                this.SetImage_INTERNAL(texture);
                
            } else if (this.graphics is UnityEngine.UI.Image && texture is Texture2D tex2d) {
                
                if (WindowSystem.IsStrictMode() == true) {
                    WindowSystem.StrictWarning(this, texture);
                    return;
                }

                var resources = WindowSystem.GetResources();
                var sprite = resources.New<Sprite, SpriteConstructor>(this, new SpriteConstructor(tex2d, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f)));
                this.SetImage_INTERNAL(sprite);
                
            }

		}

        private void SetImage_INTERNAL(Texture texture) {
            
            var prevImage = ((RawImage)this.graphics).texture;
            System.Action<ImageComponent, Texture> onComplete = static (img, texture) => {
                img.UnloadCurrentResources();
                ((RawImage)img.graphics).texture = texture;
            };
                
            var results = PoolList<ImageComponentModule>.Spawn();
            this.GetModules(results);
            if (results.Count > 0) {
                var counter = PoolClass<Counter>.Spawn();
                counter.value = results.Count;
                foreach (var item in results) {
                    item.SetImage((counter, onComplete, component: this), prevImage, texture, static (data, texture) => {
                        --data.counter.value;
                        if (data.counter.value == 0) {
                            PoolClass<Counter>.Recycle(data.counter);
                            data.onComplete.Invoke(data.component, texture);
                        }
                    });
                }
                PoolList<ImageComponentModule>.Recycle(results);
            } else {
                onComplete.Invoke(this, texture);
            }

            this.ForEachModule<ImageComponentModule, System.ValueTuple<Texture, Texture>>((prevImage, texture), static (c, texture) => c.SetImage(texture.Item1, texture.Item2));
            
        }

        private void SetImage_INTERNAL(Sprite sprite) {
            
            var prevImage = ((Image)this.graphics).sprite;
            System.Action<ImageComponent, Sprite> onComplete = static (img, sprite) => {
                img.UnloadCurrentResources();
                var graphics = ((Image)img.graphics);
                graphics.preserveAspect = img.preserveAspect;
                graphics.useSpriteMesh = img.useSpriteMesh;
                graphics.sprite = sprite;
            };
                
            var results = PoolList<ImageComponentModule>.Spawn();
            this.GetModules(results);
            if (results.Count > 0) {
                var counter = PoolClass<Counter>.Spawn();
                counter.value = results.Count;
                foreach (var item in results) {
                    item.SetImage((counter, onComplete, component: this), prevImage, sprite, static (data, texture) => {
                        --data.counter.value;
                        if (data.counter.value == 0) {
                            PoolClass<Counter>.Recycle(data.counter);
                            data.onComplete.Invoke(data.component, texture);
                        }
                    });
                }
                PoolList<ImageComponentModule>.Recycle(results);
            } else {
                onComplete.Invoke(this, sprite);
            }

            this.ForEachModule<ImageComponentModule, System.ValueTuple<Sprite, Sprite>>((prevImage, sprite), static (c, texture) => c.SetImage(texture.Item1, texture.Item2));
            
        }

        public override void ValidateEditor() {
            
            base.ValidateEditor();

            if (this.graphics == null) this.graphics = this.GetComponent<Graphic>();

        }
        
        public void SetFillAmount(float value) {

            if (this.graphics is Image image) image.fillAmount = value;

        }

        public void SetMaterial(Material material) {

            if (this.graphics == null) return;
            this.graphics.material = material;
            
            this.ForEachModule<ImageComponentModule, Material>(material, static (c, state) => c.OnSetMaterial(state));

        }

        public Material GetMaterial() {

            if (this.graphics == null) return null;
            return this.graphics.material;
            
        }

        public void SetColor(Color color) {

            if (this.graphics == null) return;
            this.graphics.color = color;

            this.ForEachModule<ImageComponentModule, Color>(color, static (c, state) => c.OnSetColor(state));

        }

        public Color GetColor() {

            if (this.graphics == null) return Color.white;
            return this.graphics.color;

        }

    }

}

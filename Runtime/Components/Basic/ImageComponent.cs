using System.Collections;

namespace UnityEngine.UI.Windows.Components {

    using Modules;
    using Utilities;

    public class ImageComponent : WindowComponent, ISearchComponentByTypeEditor, ISearchComponentByTypeSingleEditor {

        System.Type ISearchComponentByTypeEditor.GetSearchType() { return typeof(ImageComponentModule); }
        IList ISearchComponentByTypeSingleEditor.GetSearchTypeArray() { return this.componentModules.modules;}

        [RequiredReference]
        public UnityEngine.UI.Graphic graphics;

        public bool preserveAspect;
        public bool useSpriteMesh;

        public UnloadResourceEventType autoUnloadResourcesOnEvent;

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
        
        public void SetColor(Color color) {

            if (this.graphics == null) return;
            this.graphics.color = color;

        }

        public Color GetColor() {

            if (this.graphics == null) return Color.white;
            return this.graphics.color;

        }

        private Resource prevResourceLoad;

        public void SetImage<T>(T provider, bool async = true) where T : IResourceProvider {

            if (provider == null) return;
            this.SetImage(provider.GetResource(), async);

        }

        private readonly struct LoadClosure {

            public readonly ImageComponent component;
            public readonly System.Action onSetImageComplete;

            public LoadClosure(ImageComponent component, System.Action onSetImageComplete) {
                this.component = component;
                this.onSetImageComplete = onSetImageComplete;
            }

        }
        
        public void SetImage(Resource resource, bool async = true, System.Action onSetImageComplete = null) {

            if (this.prevResourceLoad.IsEquals(resource) == false) {

                var resources = WindowSystem.GetResources();
                switch (resource.objectType) {

                    case Resource.ObjectType.Sprite: {
                        if (async == true) {
                            UnityEngine.UI.Windows.Utilities.Coroutines.Run(resources.LoadAsync<Sprite, LoadClosure>(this, new LoadClosure(this, onSetImageComplete), resource, static (asset, data) => {
                                if (data.component != null) {
                                    data.component.SetImage(asset);
                                    data.onSetImageComplete?.Invoke();
                                }
                            }));
                        } else {
                            this.SetImage(resources.Load<Sprite>(this, resource));
                            onSetImageComplete?.Invoke();
                        }
                    } 
                        break;

                    case Resource.ObjectType.Texture: {
                        if (async == true) {
                            UnityEngine.UI.Windows.Utilities.Coroutines.Run(resources.LoadAsync<Texture, LoadClosure>(this, new LoadClosure(this, onSetImageComplete), resource, static (asset, data) => {
                                if (data.component != null) {
                                    data.component.SetImage(asset);
                                    data.onSetImageComplete?.Invoke();
                                }
                            }));
                        } else {
                            this.SetImage(resources.Load<Texture>(this, resource));
                            onSetImageComplete?.Invoke();
                        }
                    }
                        break;

                }

                this.prevResourceLoad = resource;

            } else {
                onSetImageComplete?.Invoke();
            }

        }

        public void SetImage(Sprite sprite) {

            this.UnloadCurrentResources();
            
            if (this.graphics is UnityEngine.UI.Image image) {

                image.sprite = sprite;
                image.preserveAspect = this.preserveAspect;
                image.useSpriteMesh = this.useSpriteMesh;

            } else if (this.graphics is UnityEngine.UI.RawImage rawImage) {

                var resources = WindowSystem.GetResources();

                var size = new Vector2Int((int)sprite.rect.width, (int)sprite.rect.height);
                var tex = resources.New<Texture2D, Texture2DConstructor>(this, new Texture2DConstructor(size.x, size.y));
                var block = tex.GetPixels((int)sprite.rect.x, (int)sprite.rect.y, size.x, size.y);
                tex.SetPixels(block);
                tex.Apply();
                rawImage.texture = sprite.texture;
                
            }

        }

        public void SetImage(Texture texture) {

            this.UnloadCurrentResources();

            if (this.graphics is UnityEngine.UI.RawImage rawImage) {

                rawImage.texture = texture;

            } else if (this.graphics is UnityEngine.UI.Image image && texture is Texture2D tex2d) {

                var resources = WindowSystem.GetResources();

                var sprite = resources.New<Sprite, SpriteConstructor>(this, new SpriteConstructor(tex2d, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f)));
                image.sprite = sprite;
                image.preserveAspect = this.preserveAspect;
                image.useSpriteMesh = this.useSpriteMesh;

            }

		}

        public override void ValidateEditor() {
            
            base.ValidateEditor();

            if(this.graphics == null) this.graphics = this.GetComponent<Graphic>();

        }
        
        public void SetFillAmount(float value) {

            if (this.graphics == null) return;
            if (this.graphics is Image image) image.fillAmount = value;

        }

    }

}

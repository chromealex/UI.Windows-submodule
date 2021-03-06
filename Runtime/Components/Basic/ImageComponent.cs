﻿using System.Collections;

namespace UnityEngine.UI.Windows.Components {

    using Modules;
    using Utilities;

    public class ImageComponent : WindowComponent, ISearchComponentByTypeEditor, ISearchComponentByTypeSingleEditor {

        System.Type ISearchComponentByTypeEditor.GetSearchType() { return typeof(ImageComponentModule); }
        IList ISearchComponentByTypeSingleEditor.GetSearchTypeArray() { return this.componentModules.modules;}

        [RequiredReference]
        public UnityEngine.UI.Graphic graphics;
        private Object currentLoaded;

        public bool preserveAspect;
        public bool useSpriteMesh;

        public UnloadResourceEventType autoUnloadResourcesOnEvent;

        private struct Texture2DConstructor : IResourceConstructor<Texture2D> {

            public int x;
            public int y;

            public Texture2DConstructor(int x, int y) {

                this.x = x;
                this.y = y;

            }

            public Texture2D Construct() {

                return new Texture2D(this.x, this.y);

            }

        }

        private struct SpriteConstructor : IResourceConstructor<Sprite> {

            public Texture2D tex2d;
            public Rect rect;
            public Vector2 pivot;

            public SpriteConstructor(Texture2D tex2d, Rect rect, Vector2 pivot) {

                this.tex2d = tex2d;
                this.rect = rect;
                this.pivot = pivot;

            }

            public Sprite Construct() {

                return Sprite.Create(this.tex2d, this.rect, this.pivot);

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

        private struct SetImageClosure {

            public ImageComponent component;

        }

        private bool isLoading;
        private Resource prevResourceLoad;
        public void SetImage(Resource resource) {

            if (this.prevResourceLoad.IsEquals(resource) == false) {

                this.prevResourceLoad = resource;
                
                var resources = WindowSystem.GetResources();
                if (this.isLoading == true) {

                    resources.StopLoadAll(this);
                    resources.Delete(this, ref this.currentLoaded);

                }

                var data = new SetImageClosure() {
                    component = this,
                };
                this.isLoading = true;
                switch (resource.objectType) {

                    case Resource.ObjectType.Sprite:
                        Coroutines.Run(resources.LoadAsync<Sprite, SetImageClosure>(this, data, resource, (asset, closure) => {

                            closure.component.currentLoaded = asset;
                            closure.component.SetImage(asset);
                            closure.component.isLoading = false;

                        }));
                        break;

                    case Resource.ObjectType.Texture:
                        Coroutines.Run(resources.LoadAsync<Texture, SetImageClosure>(this, data, resource, (asset, closure) => {

                            closure.component.currentLoaded = asset;
                            closure.component.SetImage(asset);
                            closure.component.isLoading = false;

                        }));
                        break;

                }

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

    }

}

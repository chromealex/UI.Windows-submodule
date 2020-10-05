using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Components {

    using Modules;
    using Utilities;

    public class ImageComponent : WindowComponent, ISearchComponentByTypeEditor, ISearchComponentByTypeSingleEditor {

        System.Type ISearchComponentByTypeEditor.GetSearchType() { return typeof(ImageComponentModule); }
        IList ISearchComponentByTypeSingleEditor.GetSearchTypeArray() { return this.componentModules.modules;}

        [RequiredReference]
        public UnityEngine.UI.Graphic graphics;
        private Object currentLoaded;

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
        
        public void SetImage(Resource resource) {

            var resources = WindowSystem.GetResources();
            resources.StopLoadAll(this);
            resources.Delete(this, ref this.currentLoaded);
            switch (resource.objectType) {
                
                case Resource.ObjectType.Sprite:
                    Coroutines.Run(resources.LoadAsync<Sprite>(this, resource, (asset) => {
                        
                        this.currentLoaded = asset;
                        this.SetImage(asset);
                        
                    }));
                    break;

                case Resource.ObjectType.Texture:
                    Coroutines.Run(resources.LoadAsync<Texture>(this, resource, (asset) => {
                        
                        this.currentLoaded = asset;
                        this.SetImage(asset);

                    }));
                    break;

            }
            
        }

        public void SetImage(Sprite sprite) {

            this.UnloadCurrentResources();
            
            if (this.graphics is UnityEngine.UI.Image image) {

                image.sprite = sprite;

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

            }

        }

        public override void ValidateEditor() {
            
            base.ValidateEditor();

            this.graphics = this.GetComponent<Graphic>();

        }

    }

}
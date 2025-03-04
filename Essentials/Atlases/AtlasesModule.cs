
using System.Linq;

namespace UnityEngine.UI.Windows {
    
    [CreateAssetMenu(menuName = "UI.Windows/AtlasesModule")]
    public class AtlasesModule : WindowSystemModule {
        
        [System.SerializableAttribute]
        public struct AtlasData {

            public string name;
            public Resource<UnityEngine.U2D.SpriteAtlas> data;

        }

        public AtlasData[] items;
        
        public override void OnStart() {

            UnityEngine.U2D.SpriteAtlasManager.atlasRequested -= this.AtlasRequest;
            UnityEngine.U2D.SpriteAtlasManager.atlasRequested += this.AtlasRequest;

        }

        private async void AtlasRequest(string name, System.Action<UnityEngine.U2D.SpriteAtlas> callback) {

            var item = this.GetByName(name);
            await item.data.LoadAsync(this);
            callback.Invoke(item.data.Get());

        }

        public AtlasData GetByName(string name) {

            foreach (var item in this.items) {
                if (item.name == name) return item;
            }

            return default;
        }

        public override void OnDestroy() {
            
            UnityEngine.U2D.SpriteAtlasManager.atlasRequested -= this.AtlasRequest;
            
        }

        #if UNITY_EDITOR

        [ContextMenu("Collect Atlases")]
        public void Collect() {

            var guids = UnityEditor.AssetDatabase.FindAssets("t:SpriteAtlas");
            this.items = new AtlasData[guids.Length];
            
            for (int i = 0; i < guids.Length; ++i) {

                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.U2D.SpriteAtlas>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]));
                this.items[i] = new AtlasData() {
                    name = asset.name, 
                    data = Resource<UnityEngine.U2D.SpriteAtlas>.Validate(asset),
                };

            }

        }

        #endif

    }
    
}

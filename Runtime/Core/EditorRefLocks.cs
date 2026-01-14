namespace UnityEngine.UI.Windows.Editor {

    #if UNITY_EDITOR
    public class EditorRefLocks : ScriptableObject {

        [System.Serializable]
        public struct Item {

            public string obj;
            public System.Collections.Generic.List<string> directories;

        }

        public Item[] items;

        public Item GetItemOrNull(Object obj) {
            var guid = (obj == null ? "UIWS" : UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(obj)));
            for (var index = 0; index < this.items.Length; ++index) {
                ref var item = ref this.items[index];
                if (item.obj == guid) {
                    return item;
                }
            }
            return default;
        }

        public bool GetItem(Object obj, out int index) {
            var guid = (obj == null ? "UIWS" : UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(obj)));
            for (index = 0; index < this.items.Length; ++index) {
                ref var item = ref this.items[index];
                if (item.obj == guid) {
                    return false;
                }
            }

            if (this.items == null) this.items = System.Array.Empty<Item>();
            System.Array.Resize(ref this.items, this.items.Length + 1);
            this.items[this.items.Length - 1] = new Item() {
                obj = guid,
                directories = new System.Collections.Generic.List<string>(),
            };
            return true;
        }

    }
    #endif

}
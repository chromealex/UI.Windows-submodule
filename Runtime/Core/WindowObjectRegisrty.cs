using System.Linq;
using Unity.Collections;

namespace UnityEngine.UI.Windows.Editor {
    
    using System.Collections.Generic;

    public class WindowObjectRegistry : ScriptableObject {

        public const string EDITOR_WINDOW_OBJECT_REGISTRY = "Assets/EditorResources/UI.Windows/WindowObjectRegistry.asset";

        [System.Serializable]
        public struct Item {

            public WindowObject windowObject;
            public List<EditorParametersRegistry> registry;

        }
        
        private static Item dummyItem;
        public Item[] items;
        
        public static void Add(WindowObject windowObject, EditorParametersRegistry param) {

            #if UNITY_EDITOR
            if (windowObject == null) return;
            {
                var obj = Validate();
                ref var item = ref obj.GetItem(windowObject);
                var found = false;
                foreach (var parametersRegistry in item.registry) {
                    if (parametersRegistry.IsEquals(param) == true) {
                        found = true;
                        break;
                    }
                }
                if (found == false) {
                    item.registry.Add(param);
                    UnityEditor.EditorUtility.SetDirty(obj);
                }
            }
            #endif

        }

        public void PostValidate() {
            var temp = this.items.ToList();
            for (int i = 0; i < temp.Count; ++i) {
                var item = temp[i];
                if (item.windowObject == null || item.registry == null || item.registry.Count == 0) {
                    temp.RemoveAtSwapBack(i);
                }
            }

            if (this.items.Length != temp.Count) {
                this.items = temp.ToArray();
                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
                #endif
            }
        }

        public static WindowObjectRegistry Validate() {

            #if UNITY_EDITOR
            {
                var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<WindowObjectRegistry>(EDITOR_WINDOW_OBJECT_REGISTRY);
                if (obj == null) {
                    var instance = ScriptableObject.CreateInstance<WindowObjectRegistry>();
                    instance.items = System.Array.Empty<Item>();
                    var dir = System.IO.Path.GetDirectoryName(EDITOR_WINDOW_OBJECT_REGISTRY);
                    if (System.IO.Directory.Exists(dir) == false) {
                        System.IO.Directory.CreateDirectory(dir);
                    }
                    UnityEditor.AssetDatabase.CreateAsset(instance, EDITOR_WINDOW_OBJECT_REGISTRY);
                    obj = UnityEditor.AssetDatabase.LoadAssetAtPath<WindowObjectRegistry>(EDITOR_WINDOW_OBJECT_REGISTRY);
                }

                UnityEditor.EditorApplication.delayCall += () => obj.PostValidate();
                return obj;
            }
            #else
            return null;
            #endif

        }

        private ref Item GetItem(WindowObject windowObject) {
            for (int i = 0; i < this.items.Length; ++i) {
                var item = this.items[i];
                if (item.windowObject == windowObject) return ref this.items[i];
            }
            if (this.items == null) this.items = System.Array.Empty<Item>();
            System.Array.Resize(ref this.items, this.items.Length + 1);
            this.items[this.items.Length - 1] = new Item() {
                windowObject = windowObject,
                registry = new List<EditorParametersRegistry>(),
            };
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
            return ref this.items[this.items.Length - 1];
        }

        public static ref List<EditorParametersRegistry> GetRegistry(WindowObject windowObject) {
            var obj = Validate();
            ref var item = ref obj.GetItem(windowObject);
            return ref item.registry;
        }

    }

}

namespace UnityEngine.UI.Windows {

    [System.Serializable]
    public struct EditorParametersRegistry : System.IEquatable<EditorParametersRegistry> {

        [SerializeField]
        private WindowObject holder;
        [SerializeField]
        private WindowComponentModule moduleHolder;
        
        public bool holdHiddenByDefault;
        public bool holdAllowRegisterInRoot;

        public IHolder GetHolder() {

            if (this.holder == null) return this.moduleHolder;
            return this.holder;
            
        }

        public string GetHolderName() {
            if (this.moduleHolder != null) return this.moduleHolder.name;
            return this.holder.name;
        }

        public EditorParametersRegistry(WindowObject holder) {

            this.holder = holder;
            this.moduleHolder = null;
            
            this.holdHiddenByDefault = default;
            this.holdAllowRegisterInRoot = default;

        }

        public EditorParametersRegistry(WindowComponentModule holder) {

            this.holder = holder.windowComponent;
            this.moduleHolder = holder;
            
            this.holdHiddenByDefault = default;
            this.holdAllowRegisterInRoot = default;

        }

        public bool IsEquals(EditorParametersRegistry other) {

            return this.holder == other.holder &&
                   this.holdHiddenByDefault == other.holdHiddenByDefault &&
                   this.holdAllowRegisterInRoot == other.holdAllowRegisterInRoot;

        }

        public bool Equals(EditorParametersRegistry other) {
            return Equals(this.holder, other.holder) && Equals(this.moduleHolder, other.moduleHolder) && this.holdHiddenByDefault == other.holdHiddenByDefault && this.holdAllowRegisterInRoot == other.holdAllowRegisterInRoot;
        }

        public override bool Equals(object obj) {
            return obj is EditorParametersRegistry other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (this.holder != null ? this.holder.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.moduleHolder != null ? this.moduleHolder.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.holdHiddenByDefault.GetHashCode();
                hashCode = (hashCode * 397) ^ this.holdAllowRegisterInRoot.GetHashCode();
                return hashCode;
            }
        }

    }
    
}

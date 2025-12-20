namespace UnityEngine.UI.Windows.Editor {

    public class ComponentsDebugLog : ScriptableObject {

        public const string EDITOR_COMPONENTS_DEBUG_PATH = "Assets/EditorResources/UI.Windows/ComponentsDebugLog.asset";

        #if UNITY_EDITOR
        [System.Serializable]
        public struct Item {

            [System.Serializable]
            public struct Element {
                
                public ObjectState state;
                public string stackTrace;

            }
            
            public WindowObject obj;
            public System.Collections.Generic.List<Element> elements;

        }

        public Item[] items;

        public ref Item GetItem(WindowObject obj) {
            for (var index = 0; index < this.items.Length; ++index) {
                ref var item = ref this.items[index];
                if (item.obj == obj) {
                    return ref item;
                }
            }

            if (this.items == null) this.items = System.Array.Empty<Item>();
            System.Array.Resize(ref this.items, this.items.Length + 1);
            this.items[this.items.Length - 1] = new Item() {
                obj = obj,
                elements = new System.Collections.Generic.List<Item.Element>(),
            };
            return ref this.items[this.items.Length - 1];
        }
        #endif

        public static void Add(WindowObject windowObject, ObjectState state) {
            
            #if UNITY_EDITOR
            var isDebug = WindowSystem.GetSettings().collectDebugInfo;
            if (isDebug == true) {
                var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.UI.Windows.Editor.ComponentsDebugLog>(EDITOR_COMPONENTS_DEBUG_PATH);
                if (obj == null) {
                    var instance = ScriptableObject.CreateInstance<UnityEngine.UI.Windows.Editor.ComponentsDebugLog>();
                    instance.items = System.Array.Empty<UnityEngine.UI.Windows.Editor.ComponentsDebugLog.Item>();
                    var dir = System.IO.Path.GetDirectoryName(EDITOR_COMPONENTS_DEBUG_PATH);
                    if (System.IO.Directory.Exists(dir) == false) {
                        System.IO.Directory.CreateDirectory(dir);
                    }
                    UnityEditor.AssetDatabase.CreateAsset(instance, EDITOR_COMPONENTS_DEBUG_PATH);
                    obj = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.UI.Windows.Editor.ComponentsDebugLog>(EDITOR_COMPONENTS_DEBUG_PATH);
                }
                ref var item = ref obj.GetItem(windowObject);
                if (item.elements == null) {
                    item.elements = new System.Collections.Generic.List<Item.Element>();
                }
                
                var trace = StackTraceUtility.ExtractStackTrace();
                item.elements.Add(new Item.Element() {
                    state = state,
                    stackTrace = trace,
                });
            }
            #endif
        }

    }

}
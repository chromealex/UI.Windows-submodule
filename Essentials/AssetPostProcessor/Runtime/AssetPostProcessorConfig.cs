namespace UnityEngine.UI.Windows.Essentials.AssetPostProcessor.Runtime {

    public class LabelAttribute : PropertyAttribute {}
    
    public class AssetPostProcessorConfig : ScriptableObject {

        [System.Serializable]
        public struct Item {

            [LabelAttribute]
            public string label;
            #if UNITY_EDITOR
            [Space]
            public UnityEditor.Presets.Preset preset;
            #endif
            
            public bool IsValid => string.IsNullOrEmpty(this.label) == false;

        }

        public Item[] items = System.Array.Empty<Item>();

        public Item GetItemByLabels(System.Collections.Generic.List<string> labels) {
            foreach (var item in this.items) {
                foreach (var label in labels) {
                    if (item.label == label) return item;
                }
            }

            return default;
        }

        public string[] labels;

        #if UNITY_EDITOR
        [ContextMenu("Update Labels")]
        public void UpdateLabels() {
            UnityEditor.AssetDatabase.SetLabels(this, this.labels);
        }
        #endif

    }

}
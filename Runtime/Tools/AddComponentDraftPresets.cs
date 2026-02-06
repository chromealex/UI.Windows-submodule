using System.Linq;

namespace UnityEngine.UI.Windows.Utilities {
    
    using UnityEngine.ResourceManagement.Util;
    
    [System.Serializable]
    public struct Item {

        public string fieldName;
        [SearchComponentsByTypePopup(typeof(WindowComponent), menuName: "Type", singleOnly: true)]
        public SerializedType type;

    }

    [System.Serializable]
    public struct PresetsData {

        [System.Serializable]
        public struct Preset {

            public Item[] items;

            public override string ToString() {
                return string.Join("\n", this.items.Select(x => $"<color=#4FB5FF>{x.type.Value.Name}</color> {x.fieldName}").ToArray());
            }

        }
            
        public System.Collections.Generic.List<Preset> presets;

    }

    public class AddComponentDraftPresets : UnityEngine.ScriptableObject {

        public PresetsData data;


    }

}
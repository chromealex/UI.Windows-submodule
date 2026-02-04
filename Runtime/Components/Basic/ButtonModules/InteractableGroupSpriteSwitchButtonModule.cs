using UnityEngine;
using UnityEngine.UI.Windows.Components;
using UnityEngine.UI.Windows.Modules;

namespace UnityEngine.UI.Windows {

    public class InteractableGroupSpriteSwitchButtonModule : ButtonComponentModule {
        
        [System.SerializableAttribute]
        public struct Item {
            
            public ImageComponent source;
            [ResourceType(typeof(Sprite))]
            public Resource disabled;
            [ResourceType(typeof(Sprite))]
            public Resource normal;
            
        }
        
        public Item[] groupItems;

        public override void OnInteractableChanged(bool state) {

            base.OnInteractableChanged(state);

            foreach (var item in this.groupItems) {
                item.source.SetImage(state == true ? item.normal : item.disabled);
            }

        }

    }

}

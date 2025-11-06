using UnityEngine;
using UnityEngine.UI.Windows.Components;

namespace UnityEngine.UI.Windows {

    public class InteractableGroupColorSwitchButtonModule : ButtonComponentModule {

        [System.SerializableAttribute]
        public struct Item {

            public WindowComponent source;
            public Color disabled;
            public Color normal;

        }

        public Item[] groupItems;

        public override void OnInteractableChanged(bool state) {

            base.OnInteractableChanged(state);

            foreach (var item in this.groupItems) {
                if (item.source is ImageComponent image) {
                    image.SetColor(state == true ? item.normal : item.disabled);
                }
                if (item.source is TextComponent text) {
                    text.SetColor(state == true ? item.normal : item.disabled);
                }
            }

        }

    }

}

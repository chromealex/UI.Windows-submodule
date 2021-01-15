using UnityEngine.UI.Windows.Components;
using UnityEngine.UI.Windows.WindowTypes;

namespace UnityEngine.UI.Windows {

    public class CloseComponentModule: WindowComponentModule {

        public ButtonComponent closeButton;
        public LayoutWindowType screen; 

        public override void OnInit() {
            base.OnInit();

            closeButton.SetCallback(() => {

            });
        }

    }

}
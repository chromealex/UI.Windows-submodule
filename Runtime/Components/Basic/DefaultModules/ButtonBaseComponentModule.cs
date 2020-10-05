using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows {

    public abstract class ButtonComponentModule : WindowComponentModule {

        public UnityEngine.UI.Windows.Components.ButtonComponent buttonComponent;

        public override void ValidateEditor() {
            
            base.ValidateEditor();
            
            this.buttonComponent = this.windowComponent as UnityEngine.UI.Windows.Components.ButtonComponent;
            
        }

    }
    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows {

    public abstract class ProgressComponentModule : WindowComponentModule {

        public UnityEngine.UI.Windows.Components.ProgressComponent progressComponent;

        public override void ValidateEditor() {
            
            base.ValidateEditor();
            
            this.progressComponent = this.windowComponent as UnityEngine.UI.Windows.Components.ProgressComponent;
            
        }

    }
    
}

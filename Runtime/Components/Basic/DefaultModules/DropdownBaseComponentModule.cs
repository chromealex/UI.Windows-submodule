using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows {

    public abstract class DropdownComponentModule : WindowComponentModule {

        public UnityEngine.UI.Windows.Components.DropdownComponent dropdownComponent;

        public override void ValidateEditor() {
            
            base.ValidateEditor();
            
            this.dropdownComponent = this.windowComponent as UnityEngine.UI.Windows.Components.DropdownComponent;
            
        }

    }
    
}

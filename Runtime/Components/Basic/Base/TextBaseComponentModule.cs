using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows {

    public abstract class TextComponentModule : WindowComponentModule {

        public UnityEngine.UI.Windows.Components.TextComponent textComponent;

        public override void ValidateEditor() {
            
            base.ValidateEditor();
            
            this.textComponent = this.windowComponent as UnityEngine.UI.Windows.Components.TextComponent;
            
        }

    }
    
}

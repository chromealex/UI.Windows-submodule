﻿namespace UnityEngine.UI.Windows {

    public abstract class ProgressComponentModule : WindowComponentModule {

        public UnityEngine.UI.Windows.Components.ProgressComponent progressComponent;

        public override void ValidateEditor() {
            
            base.ValidateEditor();
            
            this.progressComponent = this.windowComponent as UnityEngine.UI.Windows.Components.ProgressComponent;
            
        }

        public virtual void OnValueChanged(float f) {
            
        }

    }
    
}

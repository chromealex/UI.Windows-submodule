using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows {

    public abstract class ListComponentModule : WindowComponentModule {

        public UnityEngine.UI.Windows.Components.ListBaseComponent listComponent;

        public override void ValidateEditor() {
            
            base.ValidateEditor();
            
            this.listComponent = this.windowComponent as UnityEngine.UI.Windows.Components.ListBaseComponent;
            
        }

        public virtual void OnElementsChanged() {}

        public virtual void OnDragBegin(UnityEngine.EventSystems.PointerEventData data) { }
        public virtual void OnDragMove(UnityEngine.EventSystems.PointerEventData data) { }
        public virtual void OnDragEnd(UnityEngine.EventSystems.PointerEventData data) { }

    }
    
}

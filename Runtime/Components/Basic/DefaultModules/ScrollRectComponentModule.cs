using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows {

    public class ScrollRectComponentModule : ButtonComponentModule, UnityEngine.EventSystems.IInitializePotentialDragHandler, UnityEngine.EventSystems.IBeginDragHandler, UnityEngine.EventSystems.IDragHandler, UnityEngine.EventSystems.IEndDragHandler {

        public ScrollRect scrollRect;

        public override void ValidateEditor() {
            
            base.ValidateEditor();
            
            this.scrollRect = this.GetComponentInParent<ScrollRect>();
            
        }

        public void OnInitializePotentialDrag(UnityEngine.EventSystems.PointerEventData eventData) {
            
            if (this.scrollRect != null) this.scrollRect.OnInitializePotentialDrag(eventData);

        }
        
        public void OnBeginDrag(UnityEngine.EventSystems.PointerEventData eventData) {
            
            if (this.scrollRect != null) this.scrollRect.OnBeginDrag(eventData);
            
        }

        public void OnDrag(UnityEngine.EventSystems.PointerEventData eventData) {
            
            if (this.scrollRect != null) this.scrollRect.OnDrag(eventData);
            
        }

        public void OnEndDrag(UnityEngine.EventSystems.PointerEventData eventData) {
            
            if (this.scrollRect != null) this.scrollRect.OnEndDrag(eventData);
            
        }

    }
    
}

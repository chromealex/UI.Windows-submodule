namespace UnityEngine.UI.Windows {

    public abstract class ListComponentDraggableModule : ListComponentModule, UnityEngine.EventSystems.IBeginDragHandler, UnityEngine.EventSystems.IDragHandler, UnityEngine.EventSystems.IEndDragHandler {

        public override void OnInit() {
            
            base.OnInit();
            
            WindowSystem.onPointerUp += this.OnPointerUp;
            
        }

        public override void OnDeInit() {
            
            base.OnDeInit();
            
            WindowSystem.onPointerUp -= this.OnPointerUp;
            
        }

        public abstract void OnBeginDrag(UnityEngine.EventSystems.PointerEventData data);
        public abstract void OnDrag(UnityEngine.EventSystems.PointerEventData data);
        public abstract void OnEndDrag(UnityEngine.EventSystems.PointerEventData data);

        private void OnPointerUp() {
            
            var eventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
            this.OnEndDrag(eventData);

        }

    }
    
    public abstract class ListComponentModule : WindowComponentModule {

        public UnityEngine.UI.Windows.Components.ListBaseComponent listComponent;

        public override void ValidateEditor() {
            
            base.ValidateEditor();
            
            this.listComponent = this.windowComponent as UnityEngine.UI.Windows.Components.ListBaseComponent;
            
        }

        public virtual void OnComponentsChanged() { }
        public virtual void OnComponentAdded(WindowComponent windowComponent) { }
        public virtual void OnComponentRemoved(WindowComponent windowComponent) { }
        public virtual void OnSetItems() { }
        
        public virtual bool HasCustomAdd() {
            
            return false;
            
        }

        public virtual void SetDataSource(IDataSource dataSource) { }

        public virtual void AddItem<T, TClosure>(Resource source, TClosure closure, System.Action<T, TClosure> onComplete) where T : WindowComponent where TClosure : UnityEngine.UI.Windows.Components.IListClosureParameters {}
        
        public virtual void SetItems<T, TClosure>(int count, Resource source, System.Action<T, TClosure> onItem, TClosure closure, System.Action<TClosure> onComplete) where T : WindowComponent where TClosure : UnityEngine.UI.Windows.Components.IListClosureParameters {}
        
    }
    
}

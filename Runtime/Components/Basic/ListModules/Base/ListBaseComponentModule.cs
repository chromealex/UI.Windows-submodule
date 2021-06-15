namespace UnityEngine.UI.Windows {

    public abstract class ListComponentModule : WindowComponentModule {

        public UnityEngine.UI.Windows.Components.ListBaseComponent listComponent;

        public override void ValidateEditor() {
            
            base.ValidateEditor();
            
            this.listComponent = this.windowComponent as UnityEngine.UI.Windows.Components.ListBaseComponent;
            
        }

        public virtual void OnComponentsChanged() {}
        public virtual void OnComponentAdded(WindowComponent windowComponent) { }
        public virtual void OnComponentRemoved(WindowComponent windowComponent) { }
        
        public virtual void OnDragBegin(UnityEngine.EventSystems.PointerEventData data) { }
        public virtual void OnDragMove(UnityEngine.EventSystems.PointerEventData data) { }
        public virtual void OnDragEnd(UnityEngine.EventSystems.PointerEventData data) { }

        public virtual bool HasCustomAdd() {
            
            return false;
            
        }

        public virtual void SetDataSource(IDataSource dataSource) { }

        public virtual void AddItem<T, TClosure>(Resource source, TClosure closure, System.Action<T, TClosure> onComplete) where T : WindowComponent where TClosure : UnityEngine.UI.Windows.Components.IListClosureParameters {}
        
        public virtual void SetItems<T, TClosure>(int count, Resource source, System.Action<T, TClosure> onItem, TClosure closure, System.Action onComplete) where T : WindowComponent where TClosure : UnityEngine.UI.Windows.Components.IListClosureParameters {}
        
    }
    
}

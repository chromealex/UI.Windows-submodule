using System.Collections;

namespace UnityEngine.UI.Windows.Components {

    using Utilities;

    public interface IInteractable {

        bool IsInteractable();
        void SetInteractable(bool state);

    }

    public interface IInteractableButton : IInteractable {

        void SetCallback(System.Action callback);
        void AddCallback(System.Action callback);
        void RemoveCallback(System.Action callback);

        void SetCallback(System.Action<ButtonComponent> callback);
        void AddCallback(System.Action<ButtonComponent> callback);
        void RemoveCallback(System.Action<ButtonComponent> callback);

        void RemoveCallbacks();

    }
    
    public class ButtonComponent : GenericComponent, IInteractableButton, ISearchComponentByTypeEditor, ISearchComponentByTypeSingleEditor {

        System.Type ISearchComponentByTypeEditor.GetSearchType() {

           return typeof(ButtonComponentModule);

        }

        IList ISearchComponentByTypeSingleEditor.GetSearchTypeArray() {

           return this.componentModules.modules;

        }
        
        [RequiredReference]
        public Button button;

        private CallbackRegistries callbackRegistries;
        
        internal override void OnInitInternal() {
            
            this.button.onClick.AddListener(this.DoClickInternal);
            this.callbackRegistries.Initialize();
            
            base.OnInitInternal();
            
        }

        internal override void OnDeInitInternal() {
            
            base.OnDeInitInternal();
            
            this.ResetInstance();
            this.callbackRegistries.DeInitialize();
            
        }

        private void ResetInstance() {
            
            this.button.onClick.RemoveAllListeners();
            this.RemoveCallbacks();
            
        }
        
        public void SetInteractable(bool state) {

            this.button.interactable = state;
            this.componentModules.OnInteractableChanged(state);

        }
        
        public bool IsInteractable() {

            return this.button.interactable;

        }

        public void RaiseClick() {
            
            this.DoClickInternal();
            
        }

        public bool CanClick() {

            if (this.GetWindow().GetState() != ObjectState.Showing &&
                this.GetWindow().GetState() != ObjectState.Shown) {

                Debug.LogWarning($"Couldn't send click because window is in `{this.GetWindow().GetState()}` state.", this);
                return false;

            }

            if (this.GetState() != ObjectState.Showing &&
                this.GetState() != ObjectState.Shown) {

                Debug.LogWarning($"Couldn't send click because component is in `{this.GetState()}` state.", this);
                return false;

            }

            foreach (var module in WindowSystem.instance.modules) {
                if (module is ICanClickCheckModule moduleClick) {
                    if (moduleClick.CanClick(this) == false) return false;
                }
            }
            
            return WindowSystem.CanInteractWith(this);

        }
        
        internal void DoClickInternal() {
            
            if (this.callbackRegistries.Count == 0) {
                
                return;
                
            }

            if (this.CanClick() == true) {

                WindowSystem.InteractWith(this);
                
                this.DoClick();

            }

        }

        protected virtual void DoClick() {

            this.callbackRegistries.Invoke();

        }
        
        private struct WithInstance : System.IEquatable<WithInstance> {

            public ButtonComponent component;
            public System.Action<ButtonComponent> action;

            public bool Equals(WithInstance other) {
                return this.component == other.component && this.action == other.action;
            }

        }

        private struct WithInstance<T> : System.IEquatable<WithInstance<T>> where T : WindowObject {

            public WindowObject component;
            public System.Action<T> action;

            public bool Equals(WithInstance<T> other) {
                return this.component == other.component && this.action == other.action;
            }

        }

        private struct SetCallbackData<T> : System.IEquatable<SetCallbackData<T>> {

            public T data;
            public System.Action<T> callback;

            public bool Equals(SetCallbackData<T> other) {
                return System.Collections.Generic.EqualityComparer<T>.Default.Equals(this.data, other.data);
            }

            public override bool Equals(object obj) {
                return obj is SetCallbackData<T> other && Equals(other);
            }

            public override int GetHashCode() {
                return System.Collections.Generic.EqualityComparer<T>.Default.GetHashCode(this.data);
            }
        }
        
        public void SetCallback<TState>(TState state, System.Action<TState> callback) {

            this.RemoveCallbacks();
            this.AddCallback(new SetCallbackData<TState> { data = state, callback = callback }, static (s) => s.callback.Invoke(s.data));

        }

        public void SetCallback(Callback callback) {

            this.SetCallback(callback, static x => x.Invoke());

        }

        public void SetCallback(System.Action callback) {

            this.RemoveCallbacks();
            this.AddCallback(callback);

        }

        public void SetCallback(System.Action<ButtonComponent> callback) {

            this.RemoveCallbacks();
            this.AddCallback(callback);

        }

        public void SetCallback<T>(System.Action<T> callback) where T : ButtonComponent {

            this.RemoveCallbacks();
            this.AddCallback(callback);

        }

        public void AddCallback(System.Action callback) {

            this.callbackRegistries.Add(callback);

        }

        public void AddCallback<TState>(TState state, System.Action<TState> callback) where TState : System.IEquatable<TState> {

            this.callbackRegistries.Add(state, callback);

        }

        public void AddCallback(System.Action<ButtonComponent> callback) {

            this.AddCallback(new WithInstance() { component = this, action = callback, }, cb => cb.action.Invoke(cb.component));

        }

        public void AddCallback<T>(System.Action<T> callback) where T : ButtonComponent {

            this.AddCallback(new WithInstance<T>() { component = this, action = callback, }, cb => cb.action.Invoke((T)cb.component));

        }

        public void RemoveCallback(System.Action callback) {

            this.callbackRegistries.Remove(callback);

        }

        public void RemoveCallback(System.Action<ButtonComponent> callback) {

            this.callbackRegistries.Remove(new WithInstance() { component = this, action = callback, }, null);

        }

        public void RemoveCallback<TState>(TState state) where TState : System.IEquatable<TState> {

            this.callbackRegistries.Remove(state, null);

        }

        public void RemoveCallback<TState>(System.Action<TState> callback) where TState : System.IEquatable<TState> {

            this.callbackRegistries.Remove(default, callback);

        }

        public virtual void RemoveCallbacks() {
            
            this.callbackRegistries.Clear();
            
        }

        public override void ValidateEditor() {
            
            base.ValidateEditor();

            if (this.button == null) this.button = this.GetComponent<Button>();

        }

    }

}
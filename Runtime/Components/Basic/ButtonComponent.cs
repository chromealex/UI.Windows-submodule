using System.Collections;

namespace UnityEngine.UI.Windows.Components {

    using Utilities;

    public interface IInteractable : IInteractableNavigation {

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

    public interface IButtonExtended {

        public void AddListener(System.Action callback);
        public void RemoveListener(System.Action callback);
        public void AddListener<T>(T data, System.Action<T> callback) where T : System.IEquatable<T>;
        public void RemoveListener<T>(T data, System.Action<T> callback) where T : System.IEquatable<T>;
        public void RemoveAllListeners();

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
        
        IInteractableNavigation IInteractableNavigation.GetNext(Vector2 direction) => WindowSystem.GetNavigation(this.button, direction);

        ButtonControl IInteractableNavigation.DoAction(ControllerButton button) {
            if (button == ControllerButton.Click) {
                this.DoClick();
            }
            return ButtonControl.None;
        }
        
        internal override void OnInitInternal() {

            this.RegisterClick();
            this.callbackRegistries.Initialize();
            
            base.OnInitInternal();
            
        }

        protected virtual void RegisterClick() {
            
            if (this.button is IButtonExtended buttonExtended) {
                buttonExtended.AddListener((button: this, _: 0), static x => x.button.DoClickInternal());
            } else {
                this.button.onClick.AddListener(this.DoClickInternal);
            }

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
        
        protected virtual int GetRegistryCount() => this.callbackRegistries.Count;
        
        internal void DoClickInternal() {

            if (this.GetRegistryCount() == 0) return;
            
            if (this.CanClick() == true) {

                WindowSystem.InteractWith(this);
                
                this.DoClick();

            }

        }

        protected virtual void DoClick() {

            this.callbackRegistries.Invoke();

        }
        
        public void SetCallback<TState>(TState state, System.Action<TState> callback) {

            this.RemoveCallbacks();
            this.AddCallback((state, callback), static (s) => s.callback.Invoke(s.state));

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

            this.AddCallback((comp: this, callback), static cb => cb.callback.Invoke(cb.comp));

        }

        public void AddCallback<T>(System.Action<T> callback) where T : ButtonComponent {

            this.AddCallback((comp: (T)this, callback), static cb => cb.callback.Invoke(cb.comp));

        }

        public void RemoveCallback(System.Action callback) {

            this.callbackRegistries.Remove(callback);

        }

        public void RemoveCallback(System.Action<ButtonComponent> callback) {

            this.callbackRegistries.Remove((comp: this, callback), null);

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
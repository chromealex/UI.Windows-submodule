namespace UnityEngine.UI.Windows.Components {

    public interface ICheckboxGroup {

        void OnChecked(CheckboxComponent checkbox);
        bool CanBeUnchecked(CheckboxComponent checkbox);

    }

    public class CheckboxComponent : ButtonComponent {

        public WindowComponent checkedContainer;
        public WindowComponent uncheckedContainer;
        public bool isChecked;
        public bool autoToggle = true;
        public ICheckboxGroup group;

        private CallbackRegistries<bool> callbackRegistries;

        public override void ValidateEditor() {

            base.ValidateEditor();

            if (this.checkedContainer != null) {

                this.checkedContainer.hiddenByDefault = true;
                this.checkedContainer.AddEditorParametersRegistry(new EditorParametersRegistry(this) {
                    holdHiddenByDefault = true,
                });

            }

            if (this.uncheckedContainer != null) {

                this.uncheckedContainer.hiddenByDefault = true;
                this.uncheckedContainer.AddEditorParametersRegistry(new EditorParametersRegistry(this) {
                    holdHiddenByDefault = true,
                });

            }

            //this.UpdateCheckState();

        }

        protected override int GetRegistryCount() => this.callbackRegistries.Count;

        protected override void RegisterClick() {
            
            if (this.autoToggle == true) {

                base.RegisterClick();
                
            }

        }

        internal override void OnDeInitInternal() {
            
            this.button.onClick.RemoveAllListeners();

            base.OnDeInitInternal();

        }

        internal override void OnShowBeginInternal() {

            base.OnShowBeginInternal();

            this.SetCheckedState(this.isChecked);

        }

        public void Toggle() {

            this.SetCheckedState(!this.isChecked);

        }

        protected override void DoClick() {

            WindowSystem.InteractWith(this);
            this.SetCheckedStateInternal(!this.isChecked);
            
        }

        internal void SetCheckedStateInternal(bool state, bool call = true, bool checkGroup = true) {

            if (this.CanClick() == false) return;

            this.SetCheckedState(state, call, checkGroup);
            
        }

        public void SetCheckedState(bool state, bool call = true, bool checkGroup = true) {

            var stateChanged = this.isChecked != state;
            this.isChecked = state;
            if (checkGroup == true && this.group != null) {
                if (state == false) {
                    if (this.group.CanBeUnchecked(this) == false) {
                        this.isChecked = true;
                        state = true;
                        if (stateChanged == true) {
                            stateChanged = false;
                        }
                    }
                } else {
                    this.group.OnChecked(this);
                }
            }

            this.UpdateCheckState();

            if (call == true && stateChanged == true) {

                this.callbackRegistries.Invoke(state);

            }

        }

        public void SetGroup(ICheckboxGroup group) {
            
            this.group = group;
            
        }

        private void UpdateCheckState() {

            if (this.checkedContainer != null) {
                
                this.checkedContainer.ShowHide(this.isChecked == true);
                
            }

            if (this.uncheckedContainer != null) {
                
                this.uncheckedContainer.ShowHide(this.isChecked == false);
                
            }

        }

        public CallbackHandler SetCallback<TState>(TState state, System.Action<TState, bool> callback) {

            this.RemoveCallbacks();
            return this.AddCallback((state, callback), static (s, state) => s.callback.Invoke(s.state, state));

        }

        public CallbackHandler SetCallback(System.Action<bool> callback) {

            this.RemoveCallbacks();
            return this.AddCallback(callback);

        }

        public CallbackHandler SetCallback(System.Action<CheckboxComponent, bool> callback) {

            this.RemoveCallbacks();
            return this.AddCallback(callback);

        }

        public CallbackHandler SetCallback<T>(System.Action<T, bool> callback) where T : CheckboxComponent {

            this.RemoveCallbacks();
            return this.AddCallback(callback);

        }

        public CallbackHandler AddCallback(System.Action<bool> callback) {

            return this.callbackRegistries.Add(callback);

        }

        public CallbackHandler AddCallback<TState>(TState state, System.Action<TState, bool> callback) where TState : System.IEquatable<TState> {

            return this.callbackRegistries.Add(state, callback);

        }

        public CallbackHandler AddCallback(System.Action<CheckboxComponent, bool> callback) {

            return this.AddCallback((comp: this, callback), static (cb, state) => cb.callback.Invoke(cb.comp, state));

        }

        public CallbackHandler AddCallback<T>(System.Action<T, bool> callback) where T : CheckboxComponent {

            return this.AddCallback((comp: (T)this, callback), static (cb, state) => cb.callback.Invoke(cb.comp, state));

        }

        public override void RemoveCallback(CallbackHandler callback) {

            base.RemoveCallback(callback);
            this.callbackRegistries.Remove(callback);

        }

        public override void RemoveCallbacks() {
            
            base.RemoveCallbacks();
            this.callbackRegistries.Clear();
            
        }

    }

}
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

        internal override void OnInitInternal() {

            base.OnInitInternal();

            if (this.autoToggle == true) {

                if (this.button is IButtonExtended buttonExtended) {
                    buttonExtended.AddListener((button: this, _: 0), static x => x.button.ToggleInternal());
                } else {
                    this.button.onClick.AddListener(this.ToggleInternal);
                }

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

        internal void ToggleInternal() {

            this.SetCheckedStateInternal(!this.isChecked);

        }

        internal void SetCheckedStateInternal(bool state, bool call = true, bool checkGroup = true) {

            if (this.CanClick() == false) return;

            WindowSystem.InteractWith(this);
            
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

        public void SetCallback<TState>(TState state, System.Action<TState, bool> callback) {

            this.RemoveCallbacks();
            this.AddCallback((state, callback), static (s, state) => s.callback.Invoke(s.state, state));

        }

        public void SetCallback(System.Action<bool> callback) {

            this.RemoveCallbacks();
            this.AddCallback(callback);

        }

        public void SetCallback(System.Action<CheckboxComponent, bool> callback) {

            this.RemoveCallbacks();
            this.AddCallback(callback);

        }

        public void SetCallback<T>(System.Action<T, bool> callback) where T : CheckboxComponent {

            this.RemoveCallbacks();
            this.AddCallback(callback);

        }

        public void AddCallback(System.Action<bool> callback) {

            this.callbackRegistries.Add(callback);

        }

        public void AddCallback<TState>(TState state, System.Action<TState, bool> callback) where TState : System.IEquatable<TState> {

            this.callbackRegistries.Add(state, callback);

        }

        public void AddCallback(System.Action<CheckboxComponent, bool> callback) {

            this.AddCallback((comp: this, callback), static (cb, state) => cb.callback.Invoke(cb.comp, state));

        }

        public void AddCallback<T>(System.Action<T, bool> callback) where T : CheckboxComponent {

            this.AddCallback((comp: (T)this, callback), static (cb, state) => cb.callback.Invoke(cb.comp, state));

        }

        public void RemoveCallback(System.Action<bool> callback) {

            this.callbackRegistries.Remove(callback);

        }

        public void RemoveCallback(System.Action<CheckboxComponent, bool> callback) {

            this.callbackRegistries.Remove((comp: this, callback), null);

        }

        new public void RemoveCallback<TState>(TState state) where TState : System.IEquatable<TState> {

            this.callbackRegistries.Remove(state, null);

        }

        public void RemoveCallback<TState>(System.Action<TState, bool> callback) where TState : System.IEquatable<TState> {

            this.callbackRegistries.Remove(default, callback);

        }

        public override void RemoveCallbacks() {
            
            base.RemoveCallbacks();
            this.callbackRegistries.Clear();
            
        }

    }

}
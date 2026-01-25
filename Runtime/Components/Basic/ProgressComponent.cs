using System.Collections;

namespace UnityEngine.UI.Windows.Components {
    
    using Utilities;

    public class ProgressComponent : GenericComponent, IInteractable, ISearchComponentByTypeEditor, ISearchComponentByTypeSingleEditor {

        System.Type ISearchComponentByTypeEditor.GetSearchType() { return typeof(ProgressComponentModule); }
        IList ISearchComponentByTypeSingleEditor.GetSearchTypeArray() { return this.componentModules.modules;}

        [RequiredReference]
        public Slider slider;
        
        private CallbackRegistries<float> callbackRegistries;
        private bool ignoreCallbacks;

        IInteractableNavigation IInteractableNavigation.GetNext(Vector2 direction) => WindowSystem.GetNavigation(this.slider, direction);

        ButtonControl IInteractableNavigation.DoAction(ControllerButton button) {
            var horizontal = (this.slider.direction == Slider.Direction.LeftToRight || this.slider.direction == Slider.Direction.RightToLeft);
            if (horizontal == true) {
                if (button == ControllerButton.Left) {
                    this.SetValue(this.GetValue() - WindowSystem.GetSettings().controllers.sliderStep);
                    return ButtonControl.Used;
                } else if (button == ControllerButton.Right) {
                    this.SetValue(this.GetValue() + WindowSystem.GetSettings().controllers.sliderStep);
                    return ButtonControl.Used;
                }
            } else {
                if (button == ControllerButton.Up) {
                    this.SetValue(this.GetValue() + WindowSystem.GetSettings().controllers.sliderStep);
                    return ButtonControl.Used;
                } else if (button == ControllerButton.Down) {
                    this.SetValue(this.GetValue() - WindowSystem.GetSettings().controllers.sliderStep);
                    return ButtonControl.Used;
                }
            }
            return ButtonControl.None;
        }

        internal override void OnInitInternal() {
            
            base.OnInitInternal();
            
            this.slider.onValueChanged.AddListener(this.OnValueChanged);
            
        }

        internal override void OnDeInitInternal() {
            
            base.OnDeInitInternal();
            
            this.ResetInstance();

        }

        private void ResetInstance() {
            
            this.slider.onValueChanged.RemoveAllListeners();
            this.RemoveCallbacks();

        }

        public float GetValue() {

            return this.slider.value;

        }

        public float GetNormalizedValue() {

            return this.slider.normalizedValue;

        }

        public float GetMaxValue() {

            return this.slider.maxValue;

        }

        public void SetWholeNumbers(bool state) {

            this.slider.wholeNumbers = state;

        }

        public void SetMaxValue(float value) {

            this.slider.maxValue = value;

        }

        public void SetNormalizedValue(float value, bool ignoreCallbacks = false) {

            var prev = this.ignoreCallbacks;
            this.ignoreCallbacks = ignoreCallbacks;
            if (Mathf.Abs(value - this.slider.normalizedValue) > Mathf.Epsilon) this.slider.normalizedValue = value;
            this.ignoreCallbacks = prev;

        }

        public void SetValue(float value, bool ignoreCallbacks = false) {

            var prev = this.ignoreCallbacks;
            this.ignoreCallbacks = ignoreCallbacks;
            if (Mathf.Abs(value - this.slider.value) > Mathf.Epsilon) this.slider.value = value;
            this.ignoreCallbacks = prev;

        }

        public void SetInteractable(bool state) {

            if (this.slider.interactable != state) this.slider.interactable = state;

        }

        public bool IsInteractable() {

            return this.slider.interactable;

        }

        private void OnValueChanged(float value) {
            
            if (this.ignoreCallbacks == true) return;
            
            this.callbackRegistries.Invoke(value);
            
            this.ForEachModule<ProgressComponentModule, float>(value, (p, v) => p.OnValueChanged(v));
            
        }
        
        public override void ValidateEditor() {
            
            base.ValidateEditor();

			if (this.slider == null) this.slider = this.GetComponent<Slider>();

        }

        public void SetCallback<TState>(TState state, System.Action<TState, float> callback) {

            this.RemoveCallbacks();
            this.AddCallback((state, callback), static (s, state) => s.callback.Invoke(s.state, state));

        }

        public void SetCallback(System.Action<float> callback) {

            this.RemoveCallbacks();
            this.AddCallback(callback);

        }

        public void SetCallback(System.Action<ProgressComponent, float> callback) {

            this.RemoveCallbacks();
            this.AddCallback(callback);

        }

        public void SetCallback<T>(System.Action<T, float> callback) where T : ProgressComponent {

            this.RemoveCallbacks();
            this.AddCallback(callback);

        }

        public void AddCallback(System.Action<float> callback) {

            this.callbackRegistries.Add(callback);

        }

        public void AddCallback<TState>(TState state, System.Action<TState, float> callback) where TState : System.IEquatable<TState> {

            this.callbackRegistries.Add(state, callback);

        }

        public void AddCallback(System.Action<ProgressComponent, float> callback) {

            this.AddCallback((comp: this, callback), static (cb, state) => cb.callback.Invoke(cb.comp, state));

        }

        public void AddCallback<T>(System.Action<T, float> callback) where T : ProgressComponent {

            this.AddCallback((comp: (T)this, callback), static (cb, state) => cb.callback.Invoke(cb.comp, state));

        }

        public void RemoveCallback(System.Action<float> callback) {

            this.callbackRegistries.Remove(callback);

        }

        public void RemoveCallback(System.Action<ProgressComponent, float> callback) {

            this.callbackRegistries.Remove((comp: this, callback), null);

        }

        new public void RemoveCallback<TState>(TState state) where TState : System.IEquatable<TState> {

            this.callbackRegistries.Remove(state, null);

        }

        public void RemoveCallback<TState>(System.Action<TState, float> callback) where TState : System.IEquatable<TState> {

            this.callbackRegistries.Remove(default, callback);

        }

        public void RemoveCallbacks() {
            
            this.callbackRegistries.Clear();
            
        }

    }

}

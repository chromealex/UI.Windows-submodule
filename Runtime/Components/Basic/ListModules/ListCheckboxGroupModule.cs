using UnityEngine.UI.Windows.Components;

namespace UnityEngine.UI.Windows {

    [ComponentModuleDisplayName("Checkbox Group")]
    public class ListCheckboxGroupModule : ListComponentModule, ICheckboxGroup {

        public bool allowSwitchOff;

        private CheckboxComponent currentSelected;

        public override void OnComponentAdded(WindowComponent windowComponent) {
            base.OnComponentAdded(windowComponent);
            if (windowComponent is CheckboxComponent checkbox) {
                checkbox.SetGroup(this);
            }
        }

        public override void OnComponentRemoved(WindowComponent windowComponent) {
            base.OnComponentRemoved(windowComponent);
            if (windowComponent is CheckboxComponent checkbox) {
                checkbox.SetGroup(null);
            }
        }

        public void OnChecked(CheckboxComponent checkbox) {
            if (this.currentSelected != null && this.currentSelected != checkbox) {
                this.currentSelected.SetCheckedState(false, true, false);
            }

            this.currentSelected = checkbox;
        }

        public bool CanBeUnchecked(CheckboxComponent checkbox) {
            return this.currentSelected == null || this.allowSwitchOff || checkbox != this.currentSelected;
        }

    }

}
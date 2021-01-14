using System.Collections.Generic;
using UnityEngine.UI.Windows.Components;

namespace UnityEngine.UI.Windows {
    
    [ComponentModuleDisplayName("Checkbox group")]
    public class ListCheckboxGroupModule : ListComponentModule, ICheckboxGroup {
        
        public bool allowSwitchOff;
        
        private List<CheckboxComponent> checkboxes = new List<CheckboxComponent>();
        
        public override void OnElementAdded(WindowComponent windowComponent) {
            base.OnElementAdded(windowComponent);
            if (windowComponent is CheckboxComponent checkbox && checkboxes.Contains(checkbox) == false) {
                checkbox.SetGroup(this);
                this.checkboxes.Add(checkbox);
            }
        }
        
        public override void OnElementRemoved(WindowComponent windowComponent) {
            base.OnElementAdded(windowComponent);
            if (windowComponent is CheckboxComponent checkbox && this.checkboxes.Contains(checkbox) == true) {
                checkbox.SetGroup(null);
                this.checkboxes.Remove(checkbox);
            }
        }
        
        public void OnChecked(CheckboxComponent checkbox) {
            foreach (var current in this.checkboxes) {
                if (current.isChecked && current != checkbox) {
                    current.SetCheckedState(false);
                }
            }
        }

        public bool CanBeUnchecked(CheckboxComponent checkbox) {
            if (this.allowSwitchOff == true)
                return true;
            
            foreach (var current in this.checkboxes) {
                if (current.isChecked && current != checkbox)
                    return true;
            }

            return false;
        }

    }

}

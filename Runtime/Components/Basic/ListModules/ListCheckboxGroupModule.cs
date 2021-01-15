using System.Collections.Generic;
using UnityEngine.UI.Windows.Components;

namespace UnityEngine.UI.Windows {
    
    [ComponentModuleDisplayName("Checkbox group")]
    public class ListCheckboxGroupModule : ListComponentModule, ICheckboxGroup {
        
        public bool allowSwitchOff;
        
        private HashSet<CheckboxComponent> checkboxes = new HashSet<CheckboxComponent>();
        
        public override void OnElementAdded(WindowComponent windowComponent) {
            base.OnElementAdded(windowComponent);
            if (windowComponent is CheckboxComponent checkbox && this.checkboxes.Add(checkbox) == true) {
                checkbox.SetGroup(this);
            }
        }
        
        public override void OnElementRemoved(WindowComponent windowComponent) {
            base.OnElementAdded(windowComponent);
            if (windowComponent is CheckboxComponent checkbox && this.checkboxes.Remove(checkbox) == true) {
                checkbox.SetGroup(null);
            }
        }
        
        public void OnChecked(CheckboxComponent checkbox) {
            foreach (var current in this.checkboxes) {
                if (current.isChecked == true && current != checkbox) {
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

﻿using System.Collections.Generic;
using UnityEngine.UI.Windows.Components;

namespace UnityEngine.UI.Windows {
    
    [ComponentModuleDisplayName("Checkbox Group")]
    public class ListCheckboxGroupModule : ListComponentModule, ICheckboxGroup {
        
        public bool allowSwitchOff;
        
        private HashSet<CheckboxComponent> checkboxes = new HashSet<CheckboxComponent>();
        
        public override void OnComponentAdded(WindowComponent windowComponent) {
            base.OnComponentAdded(windowComponent);
            if (windowComponent is CheckboxComponent checkbox && this.checkboxes.Add(checkbox) == true) {
                checkbox.SetGroup(this);
            }
        }
        
        public override void OnComponentRemoved(WindowComponent windowComponent) {
            base.OnComponentAdded(windowComponent);
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
        
            if (this.allowSwitchOff == true) return true;
            
            foreach (var current in this.checkboxes) {
                if (current.isChecked == true && current != checkbox) {
                    return true;
                }
            }

            return false;
            
        }

    }

}

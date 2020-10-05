using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows {

    internal class ListRectTransformChangedInternal : MonoBehaviour {

        public UnityEngine.UI.Windows.Components.ListBaseComponent listBaseComponent;

        public void OnRectTransformDimensionsChange() {
            
            this.listBaseComponent.ForceLayoutChange();
            
        }

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Components {
    
    using Utilities;
    
    public class ListComponent : ListBaseComponent, ISearchComponentByTypeEditor, ISearchComponentByTypeSingleEditor {

        System.Type ISearchComponentByTypeEditor.GetSearchType() {

            return typeof(ListComponentModule);

        }

        IList ISearchComponentByTypeSingleEditor.GetSearchTypeArray() {

            return this.componentModules.modules;

        }
        
    }

}
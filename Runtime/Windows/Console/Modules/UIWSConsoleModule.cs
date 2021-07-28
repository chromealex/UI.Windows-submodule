using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Runtime.Windows {

    [Help("UI.Windows Console Module")]
    [Alias("uiws")]
    public class UIWSConsoleModule : ConsoleModule {

        public void HideAll() {
            
            WindowSystem.HideAll();
            
        }

        public void Root() {
            
            WindowSystem.ShowRoot();
            
        }

    }

}
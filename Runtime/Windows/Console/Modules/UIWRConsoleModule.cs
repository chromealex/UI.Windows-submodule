using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Runtime.Windows {

    [Help("UI.Windows Resources Console Module")]
    [Alias("uiwr")]
    public class UIWRConsoleModule : ConsoleModule {

        [Help("Prints all currently used resources")]
        public void Stat() {

            var res = WindowSystem.GetResources();
            Debug.Log("Allocated Objects Count: " + res.GetAllocatedCount());

        }

    }

}
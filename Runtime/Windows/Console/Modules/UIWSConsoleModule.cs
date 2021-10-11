using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Runtime.Windows {

    [Help("UI.Windows Console Module")]
    [Alias("uiws")]
    public class UIWSConsoleModule : ConsoleModule {

        [Help("Destroy UIWS with all submodules")]
        public void Destroy() {

            var instance = WindowSystem.instance;
            GameObject.Destroy(instance.gameObject);

        }

        [Help("Prints all currently opened windows")]
        public void List() {

            var opened = WindowSystem.GetCurrentOpened();
            foreach (var item in opened) {

                Debug.Log($"Source: {item.prefab.name}, Window: {item.instance.name}");

            }

        }

        [Help("Hides all windows")]
        public void HideAll() {
            
            WindowSystem.HideAll();
            
        }

        [Help("Show root window")]
        public void Root() {
            
            WindowSystem.ShowRoot();
            
        }

    }

}
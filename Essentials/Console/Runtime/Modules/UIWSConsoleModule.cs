using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Runtime.Windows {

    [UnityEngine.Scripting.PreserveAttribute]
    [Help("UI.Windows Console Module")]
    [Alias("uiws")]
    public class UIWSConsoleModule : ConsoleModule {

        #if UNITY_LOCALIZATION_SUPPORT
        [UnityEngine.Scripting.PreserveAttribute]
        [Help("Set localization test mode on/off")]
        public void SetLocalizationTestMode(bool state) {

            UnityEngine.UI.Windows.Components.TextComponent.localizationTestMode = state;

        }
        #endif
        
        /*[FastLink("Open Test")]
        public ConsolePopup OpenTest() {

            var popup = new ConsolePopup();
            popup.AddButton("Test", () => { Debug.Log("test"); });
            return popup;

        }*/
        
        [UnityEngine.Scripting.PreserveAttribute]
        [Help("Prints all currently opened windows")]
        public void List() {

            var opened = WindowSystem.GetCurrentOpened();
            foreach (var item in opened) {

                Debug.Log($"Source: {item.prefab.name}, Window: {item.instance.name}");

            }

        }

        [UnityEngine.Scripting.PreserveAttribute]
        [Help("Hides all windows")]
        public void HideAll() {
            
            WindowSystem.HideAll();
            
        }

        [UnityEngine.Scripting.PreserveAttribute]
        [Help("Show root window")]
        public void Root() {
            
            WindowSystem.ShowRoot();
            
        }

    }

}
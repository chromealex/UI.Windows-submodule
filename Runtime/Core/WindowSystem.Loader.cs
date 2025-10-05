using UnityEngine.UI.Windows.Utilities;

namespace UnityEngine.UI.Windows {

    public partial class WindowSystem {

        public static void ShowLoader(TransitionParameters transitionParameters = default, bool showSync = false) {

            if (WindowSystem.instance.loaderScreen != null && WindowSystem.instance.loaderInstance == null && WindowSystem.instance.loaderShowBegin == false) {

                WindowSystem.instance.loaderShowBegin = true;
                var w = WindowSystem.ShowSync(WindowSystem.instance.loaderScreen, new InitialParameters() { showSync = showSync },
                                              onInitialized: static (w) => {
                                                  WindowSystem.instance.loaderInstance = w;
                                                  WindowSystem.instance.loaderShowBegin = false;
                                              }, transitionParameters);
                WindowSystem.GetEvents().RegisterOnce(w, WindowEvent.OnHideBegin, static (_) => {
                    WindowSystem.instance.loaderInstance = null;
                    WindowSystem.instance.loaderShowBegin = false;
                });

            }

        }

        public static void HideLoader() {

            if (WindowSystem.instance.loaderShowBegin == true && WindowSystem.instance.loaderInstance == null) {
                Coroutines.Wait(static () => WindowSystem.instance.loaderInstance != null, static () => WindowSystem.HideLoader());
            } else {
                if (WindowSystem.instance.loaderInstance != null) {
                    WindowSystem.instance.loaderInstance.Hide();
                    WindowSystem.instance.loaderInstance = null;
                    WindowSystem.instance.loaderShowBegin = false;
                }
            }
            
        }

    }

}
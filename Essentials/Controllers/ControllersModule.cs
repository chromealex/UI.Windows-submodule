using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Modules;
using UnityEngine.UI.Windows.Components;

namespace UnityEngine.UI.Windows.Modules.Controllers {

    public interface INavigationDefaultScreen {

        IInteractable GetDefaultNavigation();

    }

    public interface IControllersSelectorScreen {
        void NavigateUp();
        void NavigateDown();
        void NavigateLeft();
        void NavigateRight();
        void Navigate(Vector2 direction);
        void Button(ControllerButton button);
        IInteractableNavigation GetCurrentSelection();
    }

    [CreateAssetMenu(menuName = "UI.Windows/Modules/Controllers Module")]
    public class ControllersModule : WindowSystemModule {

        public WindowBase selectorScreen;
        public bool autoStart = true;
        private WindowHandler<WindowBase> selectorInstance;

        private static ControllersModule instance;

        public override void OnStart() {

            instance = this;

            if (this.autoStart == true) {
                Start();
            }
            
        }

        public static void Start() {
            
            instance.selectorInstance = WindowSystem.Show(instance.selectorScreen);
            WindowSystem.RegisterSystemKeyboard(static (component) => {
                Debug.Log("[UIWS] Use WindowSystem.RegisterSystemKeyboard() to register platform-dependent keyboards.");
            });
            
        }
        
        public override void OnDestroy() {

            this.selectorInstance.Hide();

        }

        public IInteractableNavigation GetCurrentSelection() {
            var screen = (IControllersSelectorScreen)this.selectorInstance.screen;
            return screen.GetCurrentSelection();
        }

        public void NavigateUp() {
            var screen = (IControllersSelectorScreen)this.selectorInstance.screen;
            screen.NavigateUp();
        }
        
        public void NavigateDown() {
            var screen = (IControllersSelectorScreen)this.selectorInstance.screen;
            screen.NavigateDown();
        }
        
        public void NavigateLeft() {
            var screen = (IControllersSelectorScreen)this.selectorInstance.screen;
            screen.NavigateLeft();
        }
        
        public void NavigateRight() {
            var screen = (IControllersSelectorScreen)this.selectorInstance.screen;
            screen.NavigateRight();
        }

        public void Navigate(Vector2 direction) {
            var screen = (IControllersSelectorScreen)this.selectorInstance.screen;
            screen.Navigate(direction);
        }

        public void Navigate(ControllerButton button) {
            var screen = (IControllersSelectorScreen)this.selectorInstance.screen;
            screen.Button(button);
        }

    }

}
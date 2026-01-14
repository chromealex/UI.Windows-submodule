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
        ButtonControl Button(ControllerButton button);
        IInteractableNavigation GetCurrentSelection();
    }

    [CreateAssetMenu(menuName = "UI.Windows/Modules/Controllers Module")]
    public class ControllersModule : WindowSystemModule {

        public WindowBase selectorScreen;
        public bool autoStart = true;
        public bool emulateController = false;
        
        private WindowBase selectorInstance;
        private static ControllersModule instance;

        public override void OnUpdate() {
            base.OnUpdate();
            if (this.emulateController == true) {
                if (Input.GetKeyDown(KeyCode.LeftArrow) == true) {
                    this.Navigate(ControllerButton.Left);
                }
                if (Input.GetKeyDown(KeyCode.RightArrow) == true) {
                    this.Navigate(ControllerButton.Right);
                }
                if (Input.GetKeyDown(KeyCode.UpArrow) == true) {
                    this.Navigate(ControllerButton.Up);
                }
                if (Input.GetKeyDown(KeyCode.DownArrow) == true) {
                    this.Navigate(ControllerButton.Down);
                }
                if (Input.GetKeyDown(KeyCode.KeypadEnter) == true || Input.GetKeyDown(KeyCode.Return) == true) {
                    this.Navigate(ControllerButton.Click);
                }
            }
        }

        public override void OnStart() {

            instance = this;

            if (this.autoStart == true) {
                Start();
            }
            
        }

        public static void Start() {
            
            instance.selectorInstance = WindowSystem.ShowSync(instance.selectorScreen, new InitialParameters() { showSync = true, });
            WindowSystem.RegisterSystemKeyboard(static (component) => {
                Debug.Log("[UIWS] Use WindowSystem.RegisterSystemKeyboard() to register platform-dependent keyboards.");
            });
            
        }
        
        public override void OnDestroy() {

            this.selectorInstance.Hide();

        }

        public IInteractableNavigation GetCurrentSelection() {
            var screen = (IControllersSelectorScreen)this.selectorInstance;
            return screen.GetCurrentSelection();
        }

        public void NavigateUp() {
            var screen = (IControllersSelectorScreen)this.selectorInstance;
            screen.NavigateUp();
        }
        
        public void NavigateDown() {
            var screen = (IControllersSelectorScreen)this.selectorInstance;
            screen.NavigateDown();
        }
        
        public void NavigateLeft() {
            var screen = (IControllersSelectorScreen)this.selectorInstance;
            screen.NavigateLeft();
        }
        
        public void NavigateRight() {
            var screen = (IControllersSelectorScreen)this.selectorInstance;
            screen.NavigateRight();
        }

        public void Navigate(Vector2 direction) {
            var screen = (IControllersSelectorScreen)this.selectorInstance;
            screen.Navigate(direction);
        }

        public void Navigate(ControllerButton button) {
            var screen = (IControllersSelectorScreen)this.selectorInstance;
            var controlType = screen.Button(button);
            if (controlType == ButtonControl.Used) {
                return;
            }

            switch (button) {
                case ControllerButton.Left:
                    this.NavigateLeft();
                    break;
                case ControllerButton.Right:
                    this.NavigateRight();
                    break;
                case ControllerButton.Up:
                    this.NavigateUp();
                    break;
                case ControllerButton.Down:
                    this.NavigateDown();
                    break;
            }
        }

    }

}
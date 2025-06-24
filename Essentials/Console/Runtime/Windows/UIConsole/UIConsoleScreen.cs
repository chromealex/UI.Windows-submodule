using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Components;
using UnityEngine.UI.Windows.WindowTypes;

namespace UnityEngine.UI.Windows.Runtime.Windows {

    public class UIConsoleScreen : LayoutWindowType, IConsoleScreen {

        public UnityEngine.UIElements.UIDocument document;
        private UnityEngine.UI.Windows.Runtime.Windows.Components.UIConsoleComponent consoleComponent;

        private WindowSystemConsoleModule consoleModule;
        private WindowSystemConsole GetConsole() {

            if (this.consoleModule is null == false) return this.consoleModule.console;
            
            var module = WindowSystem.GetWindowSystemModule<WindowSystemConsoleModule>();
            if (module != null) {

                this.consoleModule = module;

            }
            return module.console;

        }
        
        public override void OnInit() {
            
            base.OnInit();

            this.GetLayoutComponent(out this.consoleComponent);

        }

        public void ApplyCommand(string command, bool autoComplete = false) {
            
            this.consoleComponent.ApplyCommand(command, autoComplete);
            
        }
        
        public void AddLine(string text, LogType logType = LogType.Log, bool isCommand = false, bool canCopy = false) {

            var console = this.GetConsole();
            console.AddLine(text, logType, isCommand, canCopy);
            
        }
        
        public override void OnShowBegin() {
            
            base.OnShowBegin();

            this.consoleComponent.SetInfo(this.document);

        }

        public void CloseCustomPopup() {
            
            this.consoleComponent.customPopupComponent.Hide();
            
        }

    }
    
}
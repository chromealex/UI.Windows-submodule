using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Components;
using UnityEngine.UI.Windows.WindowTypes;

namespace UnityEngine.UI.Windows.Runtime.Windows {

    public class UIConsoleScreen : LayoutWindowType, IConsoleScreen {

        public UnityEngine.UIElements.UIDocument document;
        private UnityEngine.UI.Windows.Runtime.Windows.Components.UIConsoleComponent consoleComponent;

        public override void OnInit() {
            
            base.OnInit();

            this.GetLayoutComponent(out this.consoleComponent);

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
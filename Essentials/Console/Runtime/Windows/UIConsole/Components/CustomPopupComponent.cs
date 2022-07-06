using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEngine.UI.Windows.Runtime.Windows.Components {

    using Button = UnityEngine.UIElements.Button;

    public class CustomPopupComponent : WindowComponent {

        public UnityEngine.UIElements.UIDocument document;

        private LineInfo lineInfo;
        private UIConsoleComponent uiConsole;
        public VisualElement content;
        public Button close;

        public void SetInfo(UIConsoleComponent component, VisualElement root) {

            this.uiConsole = component;

            this.content = this.document.rootVisualElement.Q<VisualElement>("Content");
            this.close = this.document.rootVisualElement.Q<Button>("CloseButton");
            
            this.close.UnregisterCallback<ClickEvent>(this.Close);
            this.close.RegisterCallback<ClickEvent>(this.Close);
            
            this.content.Add(root);
            
        }

        private void Close(ClickEvent evt) {
            
            this.Hide();
            
        }

    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEngine.UI.Windows.Runtime.Windows.Components {

    using Button = UnityEngine.UIElements.Button;

    public struct LineInfo {

        public int localIndex;
        public List<int> filteredItems;

    }
    
    public class LinePopupComponent : WindowComponent {

        public UnityEngine.UIElements.UIDocument document;

        private LineInfo lineInfo;
        private UIConsoleComponent uiConsole;
        public TextField text;
        public Button prev;
        public Button copy;
        public Button next;
        public Button close;

        public void SetInfo(UIConsoleComponent component, LineInfo lineInfo) {

            this.uiConsole = component;

            this.text = this.document.rootVisualElement.Q<TextField>();
            this.prev = this.document.rootVisualElement.Q<Button>("Prev");
            this.copy = this.document.rootVisualElement.Q<Button>("Copy");
            this.next = this.document.rootVisualElement.Q<Button>("Next");
            this.close = this.document.rootVisualElement.Q<Button>("CloseButton");
            
            this.close.UnregisterCallback<ClickEvent>(this.Close);
            this.close.RegisterCallback<ClickEvent>(this.Close);
            
            this.SetInfo(lineInfo);

        }

        public void SetInfo(LineInfo lineInfo) {

            this.lineInfo = lineInfo;
            var allItems = this.uiConsole.console.GetItems();
            var item = allItems[lineInfo.filteredItems[lineInfo.localIndex]];

            this.text.value = System.Text.RegularExpressions.Regex.Replace(item.line, "<.*?>", System.String.Empty);
            this.text.style.color = new StyleColor(UIConsoleComponent.GetColorByLogType(item.logType));
            this.copy.UnregisterCallback<ClickEvent>(this.Copy);
            this.copy.RegisterCallback<ClickEvent>(this.Copy);
            this.UpdateButtons();
            
        }

        private void Close(ClickEvent evt) {
            
            this.Hide();
            
        }

        private void Copy(ClickEvent evt) {

            var allItems = this.uiConsole.console.GetItems();
            var item = allItems[this.lineInfo.filteredItems[this.lineInfo.localIndex]];
            this.uiConsole.CopyLine(evt, item.line);

        }
        
        private void UpdateButtons() {

            this.prev.UnregisterCallback<ClickEvent>(this.MovePrev);
            this.next.UnregisterCallback<ClickEvent>(this.MoveNext);
            
            this.prev.RegisterCallback<ClickEvent>(this.MovePrev);
            this.next.RegisterCallback<ClickEvent>(this.MoveNext);
            this.prev.SetEnabled(this.lineInfo.localIndex > 0);
            this.next.SetEnabled(this.lineInfo.localIndex < this.lineInfo.filteredItems.Count - 1);

        }

        private void MoveNext(ClickEvent evt) {

            if (this.lineInfo.localIndex >= this.lineInfo.filteredItems.Count - 1) return;
            this.SetInfo(new LineInfo() {
                localIndex = this.lineInfo.localIndex + 1,
                filteredItems = this.lineInfo.filteredItems,
            });

        }

        private void MovePrev(ClickEvent evt) {

            if (this.lineInfo.localIndex <= 0) return;
            this.SetInfo(new LineInfo() {
                localIndex = this.lineInfo.localIndex - 1,
                filteredItems = this.lineInfo.filteredItems,
            });

        }
        
    }

}
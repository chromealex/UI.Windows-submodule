using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules {

    using UnityEngine.UI.Windows;

    public interface IHighlightWindowSource {

        void OnParametersPass(WindowComponent handler, UnityEngine.Vector2 offset, UnityEngine.Vector2 size);

    }

    [ComponentModuleDisplayName("Essentials.Tutorial/Button Highlight")]
    public class TutorialButtonHighlightComponentModule : ButtonComponentModule {

        public WindowComponent highlight;
        public WindowBase highlightWindowSource;
        public Vector2 customOffset;
        public Vector2 customSize;
        
        private WindowBase highlightWindowInstance;
        
        public override void ValidateEditor() {
            
            base.ValidateEditor();

            if (this.highlight != null) {

                this.highlight.hiddenByDefault = true;
                this.highlight.AddEditorParametersRegistry(new EditorParametersRegistry(this) {
                    holdHiddenByDefault = true,
                });

            }

        }

        public void Do(bool state, WindowComponent handler = null) {
            this.highlight?.ShowHide(state);
            if (this.highlightWindowSource != null) {
                if (state == true) {
                    this.highlightWindowInstance = WindowSystem.ShowSync(this.highlightWindowSource, default, x => ((IHighlightWindowSource)x).OnParametersPass(handler, this.customOffset, this.customSize));
                } else {
                    this.highlightWindowInstance?.Hide();
                    this.highlightWindowInstance = null;
                }
            }
        }

    }

}

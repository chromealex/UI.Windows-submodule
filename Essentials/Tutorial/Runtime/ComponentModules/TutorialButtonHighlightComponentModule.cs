﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules {

    using UnityEngine.UI.Windows;

    [ComponentModuleDisplayName("Essentials.Tutorial/Button Highlight")]
    public class TutorialButtonHighlightComponentModule : ButtonComponentModule {

        public WindowComponent highlight;

        public override void ValidateEditor() {
            
            base.ValidateEditor();

            if (this.highlight != null) {

                this.highlight.hiddenByDefault = true;
                this.highlight.AddEditorParametersRegistry(new WindowObject.EditorParametersRegistry(this) {
                    holdHiddenByDefault = true,
                });

            }

        }

    }

}

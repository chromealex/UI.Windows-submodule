namespace UnityEngine.UI.Windows.Essentials.Tutorial {

    [ComponentModuleDisplayName("UI/Set Interactable Button")]
    public struct SetInteractableButton : IAction {

        public string text => $"Set interactable button with tag `#{this.tag}`";

        public TagComponent tag;
        public bool state;

        public ActionResult Execute(in Context context) {

            this.Do(in context);

            return ActionResult.MoveNext;

        }

        private struct Closure {

            public SetInteractableButton obj;
            public Context context;

        }
        
        public void Do(in Context context) {
            
            var obj = this;

            var state = new Closure() {
                obj = obj,
                context = context,
            };
            if (obj.tag.isList == true) {
                context.window.FindComponent<UnityEngine.UI.Windows.Components.ListComponent, Closure>(state, static (state, x) => {

                    foreach (var moduleBase in x.componentModules.modules) {
                        if (moduleBase is UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules.TutorialListComponentModule module) {
                            if (module.uiTag == state.obj.tag.uiTag) {
                                var button = module.listComponent.GetItem<WindowComponent>(state.obj.tag.listIndex);
                                if (button is UnityEngine.UI.Windows.Components.IInteractable c) {
                                    c.SetInteractable(state.obj.state);
                                }
                            }
                        }
                    }
                    var tagModule = x.GetModule<UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules.TutorialWindowComponentTagComponentModule>();
                    if (tagModule == null) return false;

                    if (tagModule.uiTag == state.obj.tag.uiTag) {
                        var button = x.GetItem<WindowComponent>(state.obj.tag.listIndex);
                        if (button is UnityEngine.UI.Windows.Components.IInteractable c) {
                            c.SetInteractable(state.obj.state);
                        }
                        return true;
                    }

                    return false;

                });
            } else {
                context.window.FindComponent<UnityEngine.UI.Windows.Components.ButtonComponent, Closure>(state, static (state, x) => {

                    var tagModule = x.GetModule<UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules.TutorialWindowComponentTagComponentModule>();
                    if (tagModule == null) return false;

                    if (tagModule.uiTag == state.obj.tag.uiTag) {

                        x.SetInteractable(state.obj.state);
                        return true;

                    }

                    return false;

                });
            }

        }

    }

}
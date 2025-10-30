namespace UnityEngine.UI.Windows.Essentials.Tutorial {

    [ComponentModuleDisplayName("UI/Lock Button")]
    public struct LockButton : IAction {

        public string text => $"Lock button with tag `#{this.tag}`";

        public TagComponent tag;
        public TutorialData nextOnClick;

        public ActionResult Execute(in Context context) {

            this.Do(in context);

            return ActionResult.MoveNext;

        }

        private void OnComplete(in Context context) {
            
            WindowSystem.CancelWaitInteractables();
            
            if (this.nextOnClick != null) {
                context.system.TryToStart(context.window, this.nextOnClick, context.windowEvent);
            }

        }

        private struct Closure {

            public LockButton obj;
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
                                if (module.listComponent.GetItem<WindowComponent>(state.obj.tag.listIndex) is UnityEngine.UI.Windows.Components.IInteractable c) {
                                    WindowSystem.AddWaitInteractable(() => state.obj.OnComplete(state.context), c);
                                }
                            }
                        }
                    }
                    var tagModule = x.GetModule<UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules.TutorialWindowComponentTagComponentModule>();
                    if (tagModule == null) return false;

                    return tagModule.uiTag == state.obj.tag.uiTag;

                });
            } else {
                context.window.FindComponent<UnityEngine.UI.Windows.Components.ButtonComponent, Closure>(state, static (state, x) => {

                    var tagModule = x.GetModule<UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules.TutorialWindowComponentTagComponentModule>();
                    if (tagModule == null) return false;

                    if (tagModule.uiTag == state.obj.tag.uiTag) {

                        WindowSystem.AddWaitInteractable(() => state.obj.OnComplete(state.context), x);
                        return true;

                    }

                    return false;

                });
            }

        }

    }

}
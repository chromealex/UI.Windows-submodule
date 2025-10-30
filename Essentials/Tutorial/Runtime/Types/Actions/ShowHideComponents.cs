namespace UnityEngine.UI.Windows.Essentials.Tutorial {
    
    [UnityEngine.UI.Windows.ComponentModuleDisplayNameAttribute("UI/Show-Hide Component")]
    public struct ShowHideComponents : IAction {

        public enum State {
            Show,
            Hide,
        }

        public string text => $"`{this.state.ToString()}` component with tag `{this.tag.uiTag}`";

        public TagComponent tag;
        public State state;

        public ActionResult Execute(in Context context) {

            this.Do(in context);

            return ActionResult.MoveNext;

        }

        private struct Closure {

            public ShowHideComponents obj;

        }

        public void Do(in Context context) {

            var obj = this;

            var state = new Closure() {
                obj = obj,
            };

            if (this.tag.isList == false) {
                context.window.FindComponent<UnityEngine.UI.Windows.WindowComponent, Closure>(state, static (state, x) => {

                    foreach (var moduleBase in x.componentModules.modules) {
                        if (moduleBase is UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules.TutorialWindowComponentTagComponentModule module) {
                            if (module.uiTag == state.obj.tag.uiTag) {
                                module.windowComponent.ShowHide(state.obj.state == State.Show ? true : false);
                            }
                        }
                    }

                    return false;

                });
            } else {
                context.window.FindComponent<UnityEngine.UI.Windows.Components.ListComponent, Closure>(state, static (state, x) => {

                    foreach (var moduleBase in x.componentModules.modules) {
                        if (moduleBase is UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules.TutorialListComponentModule module) {
                            if (module.uiTag == state.obj.tag.uiTag) {
                                var c = module.listComponent.GetItem<WindowComponent>(state.obj.tag.listIndex);
                                c.ShowHide(state.obj.state == State.Show ? true : false);
                            }
                        }
                    }

                    return false;

                });
            }

        }

    }

}
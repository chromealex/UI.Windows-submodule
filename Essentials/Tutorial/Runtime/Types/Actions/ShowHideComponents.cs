namespace UnityEngine.UI.Windows.Essentials.Tutorial {
    
    [UnityEngine.UI.Windows.ComponentModuleDisplayNameAttribute("UI/Show-Hide Component")]
    public struct ShowHideComponents : IAction {

        public enum State {
            Show,
            Hide,
        }

        public string text => $"`{this.state.ToString()}` component with tag `{this.tag}`";

        public State state;
        public string tag;
        public int listIndex;

        public ActionResult Execute(in Context context) {

            this.Do(in context);

            return ActionResult.MoveNext;

        }

        public void Do(in Context context) {

            var obj = this;
            var tag = this.tag;
            var listIndex = this.listIndex;
            
            var component = context.window.FindComponent<UnityEngine.UI.Windows.WindowComponent>(x => {

                foreach (var moduleBase in x.componentModules.modules) {

                    if (moduleBase is UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules.TutorialWindowComponentTagComponentModule module) {

                        if (module.uiTag == tag) {

                            module.windowComponent.ShowHide(obj.state == State.Show ? true : false);

                        }

                    }

                }

                return false;

            });
            if (component != null) {

                return;

            }

            context.window.FindComponent<UnityEngine.UI.Windows.Components.ListComponent>(x => {

                foreach (var moduleBase in x.componentModules.modules) {

                    if (moduleBase is UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules.TutorialListComponentModule module) {

                        if (module.uiTag == tag) {

                            var c = module.listComponent.GetItem<WindowComponent>(listIndex);
                            c.ShowHide(obj.state == State.Show ? true : false);

                        }

                    }
                    
                }

                return false;

            });

        }

    }

}
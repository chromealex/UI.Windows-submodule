namespace UnityEngine.UI.Windows.Essentials.Tutorial {

    [ComponentModuleDisplayName("UI/Wait List Fill")]
    public struct WaitListFill : IAction {

        public string text => $"Wait while list index will be exists in list with tag `#{this.tag}`";

        public TagComponent tag;

        private struct Closure {

            public WaitListFill obj;
            public Context context;

        }

        public ActionResult Execute(in Context context) {

            var list = context.window as UnityEngine.UI.Windows.Components.ListComponent;
            if (this.tag.isList == true) {
                // wait for required index
                var index = this.tag.listIndex;
                if (this.tag.ignoreSearch == true) {
                    if (list != null) {
                        foreach (var moduleBase in list.componentModules.modules) {
                            if (moduleBase is UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules.TutorialListComponentModule module) {
                                if (module.uiTag == this.tag.uiTag) {
                                    list = module.listComponent as UnityEngine.UI.Windows.Components.ListComponent;
                                }
                            }
                        }
                    }
                    if (list != null) {
                        var tagModule = list.GetModule<UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules.TutorialWindowComponentTagComponentModule>();
                        if (tagModule != null) {
                            if (tagModule.uiTag != this.tag.uiTag) {
                                list = null;
                            }
                        }
                    }
                } else {
                    var state = new Closure() {
                        obj = this,
                        context = context,
                    };
                    list = context.window.FindComponent<UnityEngine.UI.Windows.Components.ListComponent, Closure>(state, static (state, x) => {

                        foreach (var moduleBase in x.componentModules.modules) {
                            if (moduleBase is UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules.TutorialListComponentModule module) {
                                if (module.uiTag == state.obj.tag.uiTag) {
                                    return true;
                                }
                            }
                        }

                        var tagModule = x.GetModule<UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules.TutorialWindowComponentTagComponentModule>();
                        if (tagModule != null) {
                            if (tagModule.uiTag == state.obj.tag.uiTag) {
                                return true;
                            }
                        }

                        return false;

                    });
                }
                
                if (list != null) {
                    if (index >= list.Count) {
                        var contextData = context;
                        UnityEngine.UI.Windows.Utilities.Coroutines.NextFrame(contextData, static (contextData) => { contextData.data.RunActions(contextData, contextData.index); });
                        return ActionResult.Break;
                    }
                }
            }

            return ActionResult.MoveNext;

        }

    }

}
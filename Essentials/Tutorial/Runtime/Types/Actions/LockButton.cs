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

        private void OnComplete(in Context context, UnityEngine.UI.Windows.WindowComponent buttonComponent) {
            
            var buttonHighlight = buttonComponent?.GetModule<UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules.TutorialButtonHighlightComponentModule>();
            if (buttonHighlight != null) {
                buttonHighlight.Do(false);
            }
            
            WindowSystem.CancelWaitInteractables();
            
            if (this.nextOnClick != null) {
                context.system.TryToStart(context.window, this.nextOnClick, TutorialWindowEvent.OnAny, false);
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
                if (obj.tag.ignoreSearch == true) {
                    var x = context.window as UnityEngine.UI.Windows.Components.ListComponent;
                    var hasAny = false;
                    foreach (var moduleBase in x.componentModules.modules) {
                        if (moduleBase is UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules.TutorialListComponentModule module) {
                            if (module.uiTag == state.obj.tag.uiTag) {
                                var button = module.listComponent.GetItem<WindowComponent>(state.obj.tag.listIndex);
                                if (button is UnityEngine.UI.Windows.Components.IInteractable c) {
                                    WindowSystem.AddWaitInteractable(() => {
                                        state.obj.OnComplete(state.context, button);
                                        if (x.scrollRect != null) {
                                            x.scrollRect.enabled = true;
                                        }
                                    }, c);
                                    if (x.scrollRect != null) {
                                        x.scrollRect.enabled = false;
                                    }
                                }
                                var buttonHighlight = button.GetModule<UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules.TutorialButtonHighlightComponentModule>();
                                if (buttonHighlight != null) {
                                    buttonHighlight.Do(true, button);
                                }
                                hasAny = true;
                            }
                        }
                    }
                    var tagModule = x.GetModule<UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules.TutorialWindowComponentTagComponentModule>();
                    if (tagModule != null) {
                        if (tagModule.uiTag == state.obj.tag.uiTag) {
                            var button = x.GetItem<WindowComponent>(state.obj.tag.listIndex);
                            WindowSystem.AddWaitInteractable(() => {
                                state.obj.OnComplete(state.context, button);
                                if (x.scrollRect != null) {
                                    x.scrollRect.enabled = true;
                                }
                            }, button as UnityEngine.UI.Windows.Components.IInteractable);
                            var buttonHighlight = button.GetModule<UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules.TutorialButtonHighlightComponentModule>();
                            if (buttonHighlight != null) {
                                buttonHighlight.Do(true, button);
                            }
                            hasAny = true;
                        }
                    }
                    if (hasAny == true) {
                        if (x.scrollRect != null) {
                            x.scrollRect.enabled = false;
                        }
                    }
                } else {
                    context.window.FindComponent<UnityEngine.UI.Windows.Components.ListComponent, Closure>(state, static (state, x) => {

                        var hasAny = false;
                        foreach (var moduleBase in x.componentModules.modules) {
                            if (moduleBase is UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules.TutorialListComponentModule module) {
                                if (module.uiTag == state.obj.tag.uiTag) {
                                    var button = module.listComponent.GetItem<WindowComponent>(state.obj.tag.listIndex);
                                    if (button is UnityEngine.UI.Windows.Components.IInteractable c) {
                                        WindowSystem.AddWaitInteractable(() => {
                                            state.obj.OnComplete(state.context, button);
                                            if (x.scrollRect != null) {
                                                x.scrollRect.enabled = true;
                                            }
                                        }, c);
                                    }
                                    var buttonHighlight = button.GetModule<UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules.TutorialButtonHighlightComponentModule>();
                                    if (buttonHighlight != null) {
                                        buttonHighlight.Do(true, button);
                                    }
                                    hasAny = true;
                                }
                            }
                        }

                        var tagModule = x.GetModule<UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules.TutorialWindowComponentTagComponentModule>();
                        if (tagModule != null) {
                            if (tagModule.uiTag == state.obj.tag.uiTag) {
                                var button = x.GetItem<WindowComponent>(state.obj.tag.listIndex);
                                WindowSystem.AddWaitInteractable(() => {
                                    state.obj.OnComplete(state.context, button);
                                    if (x.scrollRect != null) {
                                        x.scrollRect.enabled = true;
                                    }
                                }, button as UnityEngine.UI.Windows.Components.IInteractable);
                                var buttonHighlight = button.GetModule<UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules.TutorialButtonHighlightComponentModule>();
                                if (buttonHighlight != null) {
                                    buttonHighlight.Do(true, button);
                                }
                                hasAny = true;
                            }
                        }

                        if (hasAny == true) {
                            if (x.scrollRect != null) {
                                x.scrollRect.enabled = false;
                            }
                        }

                        return hasAny;

                    });
                }
            } else {
                if (obj.tag.ignoreSearch == true) {
                    var x = context.window as UnityEngine.UI.Windows.Components.ButtonComponent;
                    var tagModule = x.GetModule<UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules.TutorialWindowComponentTagComponentModule>();
                    if (tagModule == null) return;

                    if (tagModule.uiTag == state.obj.tag.uiTag) {

                        WindowSystem.AddWaitInteractable(() => state.obj.OnComplete(state.context, x), x);
                        var buttonHighlight = x.GetModule<UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules.TutorialButtonHighlightComponentModule>();
                        if (buttonHighlight != null) {
                            buttonHighlight.Do(true, x);
                        }

                    }
                } else {
                    context.window.FindComponent<UnityEngine.UI.Windows.Components.ButtonComponent, Closure>(state, static (state, x) => {

                        var tagModule = x.GetModule<UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules.TutorialWindowComponentTagComponentModule>();
                        if (tagModule == null) return false;

                        if (tagModule.uiTag == state.obj.tag.uiTag) {

                            WindowSystem.AddWaitInteractable(() => state.obj.OnComplete(state.context, x), x);
                            var buttonHighlight = x.GetModule<UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules.TutorialButtonHighlightComponentModule>();
                            if (buttonHighlight != null) {
                                buttonHighlight.Do(true, x);
                            }

                            return true;

                        }

                        return false;

                    });
                }
            }

        }

    }

}
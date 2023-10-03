
namespace UnityEngine.UI.Windows.Essentials.Tutorial {

    [UnityEngine.UI.Windows.ComponentModuleDisplayNameAttribute("UI/Lock Element")]
    public struct LockElement : IAction {

        public string text => $"Lock element by type `{(this.tag.isList == true ? $"List({this.tag.listIndex})" : "Button")}` with tag `#{this.tag.id}`";

        public Tag tag;
        public TutorialData nextOnClick;

        public ActionResult Execute(in Context context) {

            this.Do(in context);

            return ActionResult.MoveNext;

        }

        private void OnComplete(in Context context) {
            
            WindowSystem.CancelWaitInteractables();
            if (this.nextOnClick != null) context.system.TryToStart(context.window, this.nextOnClick, context.windowEvent);

        }

        public void Do(in Context context) {

            var window = context.window.GetWindow() as UnityEngine.UI.Windows.WindowTypes.LayoutWindowType;
            WindowLayoutElement element = null;
            if (window != null) {
                element = window.GetLayoutElement(this.tag.id);
            }

            if (element != null) {

                var obj = this;
                var contextData = context;
                if (this.tag.isList == true) {

                    var list = element.FindComponent<UnityEngine.UI.Windows.Components.ListComponent>();
                    if (list != null) {

                        WindowSystem.AddWaitInteractable(() => obj.OnComplete(contextData), (UnityEngine.UI.Windows.Components.IInteractable)list.GetItem<WindowComponent>(this.tag.listIndex));

                    }

                } else {
                    
                    var button = element.FindComponent<UnityEngine.UI.Windows.Components.ButtonComponent>();
                    if (button != null) {

                        WindowSystem.AddWaitInteractable(() => obj.OnComplete(contextData), button);

                    }

                }

            }
            
        }

    }

}
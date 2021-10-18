
namespace UnityEngine.UI.Windows.Essentials.Tutorial {

    [UnityEngine.UI.Windows.ComponentModuleDisplayNameAttribute("UI/Lock Element")]
    public struct LockElement : IAction {

        public string text => $"Lock element by type `{(this.tag.isList == true ? $"List({this.tag.listIndex})" : "Button")}` with tag `#{this.tag.id}`";

        public Tag tag;
        public TutorialData nextOnClick;

        public void Execute(in Context context) {

            this.Do(in context);

        }

        private void OnComplete(in Context context) {
            
            if (this.nextOnClick != null) context.system.TryToStart(context.window, this.nextOnClick, context.windowEvent);

        }

        public void Do(in Context context) {

            var element = context.window.GetLayoutElement(this.tag.id);
            if (element != null) {

                var obj = this;
                var contextData = context;
                if (this.tag.isList == true) {

                    var list = element.FindComponent<UnityEngine.UI.Windows.Components.ListComponent>();
                    if (list != null) {

                        WindowSystem.AddWaitIntractable(() => obj.OnComplete(contextData), (UnityEngine.UI.Windows.Components.IInteractable)list.GetItem<WindowComponent>(this.tag.listIndex));

                    }

                } else {
                    
                    var button = element.FindComponent<UnityEngine.UI.Windows.Components.ButtonComponent>();
                    if (button != null) {

                        WindowSystem.AddWaitIntractable(() => obj.OnComplete(contextData), button);

                    }

                }

            }
            
        }

    }

}

namespace UnityEngine.UI.Windows.Essentials.Tutorial {

    [System.Serializable]
    public struct LockElement : IAction {

        public string caption => "Lock Element";
        public string text => $"Lock element by type `{(this.tag.isList == true ? $"List({this.tag.listIndex})" : "Button")}` with tag `#{this.tag.id}`";

        public Tag tag;

        public void Execute(in Context context) {

            this.Do(in context);

        }

        public void Do(in Context context) {

            var element = context.window.GetLayoutElement(this.tag.id);
            if (element != null) {

                var contextData = context;
                if (this.tag.isList == true) {

                    var list = element.FindComponent<UnityEngine.UI.Windows.Components.ListComponent>();
                    if (list != null) {

                        WindowSystem.AddWaitIntractable(() => contextData.system.Complete(contextData), (UnityEngine.UI.Windows.Components.IInteractable)list.GetItem<WindowComponent>(this.tag.listIndex));

                    }

                } else {
                    
                    var button = element.FindComponent<UnityEngine.UI.Windows.Components.ButtonComponent>();
                    if (button != null) {

                        WindowSystem.AddWaitIntractable(() => contextData.system.Complete(contextData), button);

                    }

                }

            }
            
        }

    }

}
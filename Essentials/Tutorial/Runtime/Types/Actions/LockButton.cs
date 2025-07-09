namespace UnityEngine.UI.Windows.Essentials.Tutorial {

    [ComponentModuleDisplayName("UI/Lock Button")]
    public struct LockButton : IAction {

        public string text => $"Lock button with tag `#{this.tag}`";

        public string tag;
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

        public void Do(in Context context) {
            
            var obj = this;
            
            var component = context.window.FindComponent<UnityEngine.UI.Windows.Components.ButtonComponent>(x => {

                var tagModule = x.GetModule<UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules.TutorialWindowComponentTagComponentModule>();
                if (tagModule == null) return false;
                
                return tagModule.uiTag == obj.tag;

            });
            
            if (component == null) {

                return;

            }
            
            var contextData = context;
            WindowSystem.AddWaitInteractable(() => obj.OnComplete(contextData), component);
            
            
        }

    }

}
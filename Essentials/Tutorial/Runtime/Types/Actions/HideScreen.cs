namespace UnityEngine.UI.Windows.Essentials.Tutorial {

    [UnityEngine.UI.Windows.ComponentModuleDisplayNameAttribute("UI/Hide Screen")]
    public struct HideScreen : IAction {

        public string text => $"Hide all screens with source `{this.source}`";

        public WindowBase source;
        public TutorialData onHidden;

        public ActionResult Execute(in Context context) {

            this.Do(in context);
            
            return ActionResult.MoveNext;

        }
        
        private void Do(in Context context) {

            var obj = this;
            var contextData = context;
            WindowSystem.HideAll(x => {

                if (x.windowSourceId == obj.source.GetType().GetHashCode()) {
                    
                    return true;
                    
                }

                return false;

            }, TransitionParameters.Default.ReplaceCallback(() => { contextData.system.TryToStart(contextData.window, obj.onHidden, contextData.windowEvent); }));

        }

    }

}
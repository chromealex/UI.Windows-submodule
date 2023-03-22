namespace UnityEngine.UI.Windows.Essentials.Tutorial {

    [UnityEngine.UI.Windows.ComponentModuleDisplayNameAttribute("UI/Show Screen")]
    public struct ShowScreen : IAction {

        public string text => $"Show screen `{this.source}`";

        public WindowBase source;
        public TutorialData onShown;

        public ActionResult Execute(in Context context) {

            var obj = this;
            var contextData = context;
            WindowSystem.Show(this.source,
                                  default,
                              (x) => x.OnEmptyPass(),
                              TransitionParameters.Default.ReplaceCallback(() => { contextData.system.TryToStart(contextData.window, obj.onShown, contextData.windowEvent); }));

            return ActionResult.MoveNext;

        }

    }

}
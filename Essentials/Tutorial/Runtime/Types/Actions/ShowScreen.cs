namespace UnityEngine.UI.Windows.Essentials.Tutorial {

    [UnityEngine.UI.Windows.ComponentModuleDisplayNameAttribute("UI/Show Screen")]
    public struct ShowScreen : IAction {

        public string text => $"Show screen `{this.source}`";

        public WindowBase source;
        public TutorialData onShown;

        public void Execute(in Context context) {

            var obj = this;
            var contextData = context;
            WindowSystem.ShowSync(this.source,
                                  default,
                              (x) => x.OnEmptyPass(),
                              TransitionParameters.Default.ReplaceCallback(() => { contextData.system.TryToStart(contextData.window, obj.onShown, contextData.windowEvent); }));

        }

    }

}
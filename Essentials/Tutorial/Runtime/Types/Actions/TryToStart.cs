namespace UnityEngine.UI.Windows.Essentials.Tutorial {

    [ComponentModuleDisplayName("Try to start")]
    public struct TryToStart : IAction {

        public TutorialData tutorialData;
        public string text => $"Try to start tutorial data";

        public ActionResult Execute(in Context context) {

            if (this.tutorialData != null) {
                context.system.TryToStart(null, this.tutorialData, TutorialWindowEvent.OnAny);
            }
            
            return ActionResult.MoveNext;

        }

    }

}
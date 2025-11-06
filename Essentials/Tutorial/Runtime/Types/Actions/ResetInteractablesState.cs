namespace UnityEngine.UI.Windows.Essentials.Tutorial {

    [ComponentModuleDisplayName("UI/Reset Interactables state")]
    public struct ResetInteractablesState : IAction {

        public string text => $"Unlock all interactables";

        public ActionResult Execute(in Context context) {

            WindowSystem.CancelWaitInteractables();
                
            return ActionResult.MoveNext;

        }

    }

}
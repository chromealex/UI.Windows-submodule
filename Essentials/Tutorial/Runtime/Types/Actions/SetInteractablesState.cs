namespace UnityEngine.UI.Windows.Essentials.Tutorial {

    [ComponentModuleDisplayName("UI/Set Interactables state")]
    public struct SetInteractablesState : IAction {

        public string text => $"{(this.isLocked == true ? "Lock" : "Unlock")} all interactables";
        public bool isLocked;

        public ActionResult Execute(in Context context) {

            if (this.isLocked == true) WindowSystem.LockAllInteractables();
            else WindowSystem.UnlockAllInteractables();
                
            return ActionResult.MoveNext;

        }

    }

}
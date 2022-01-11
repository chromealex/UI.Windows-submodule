namespace UnityEngine.UI.Windows.Essentials.Tutorial {

    [UnityEngine.UI.Windows.ComponentModuleDisplayNameAttribute("Wait")]
    public struct Wait : IAction {

        public string text => $"Wait for `{this.seconds}` seconds before next action";

        public float seconds;

        public ActionResult Execute(in Context context) {

            var contextData = context;
            UnityEngine.UI.Windows.Utilities.Coroutines.WaitTime(this.seconds, () => {
            
                contextData.data.RunActions(contextData, contextData.index + 1);
    
            });
            
            return ActionResult.Break;

        }

    }

}
 namespace UnityEngine.UI.Windows.Essentials.Tutorial {

    [UnityEngine.UI.Windows.ComponentModuleDisplayNameAttribute("Wait for click")]
    public struct WaitForClick : IAction {

        public string text => $"Wait for click before next action";

        public ActionResult Execute(in Context context) {

            var contextData = context;
            System.Action onPointerUp = null;
            onPointerUp = () => {
                
                WindowSystem.onPointerUp -= onPointerUp;

                contextData.data.RunActions(contextData, contextData.index + 1);

            };
            WindowSystem.onPointerUp += onPointerUp;
            
            return ActionResult.Break;

        }

    }

}
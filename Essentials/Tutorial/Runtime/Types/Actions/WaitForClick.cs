 namespace UnityEngine.UI.Windows.Essentials.Tutorial {

    [UnityEngine.UI.Windows.ComponentModuleDisplayNameAttribute("Wait for click")]
    public struct WaitForClick : IAction {

        public string text => $"Wait for click before next action";

        public ActionResult Execute(in Context context) {

            WindowSystem.RegisterOnPointerUp(context, static (contextData, cbk) => {
            
                WindowSystem.UnRegisterOnPointerUp(cbk);
                contextData.data.RunActions(contextData, contextData.index + 1);

            });
            
            return ActionResult.Break;

        }

    }

}
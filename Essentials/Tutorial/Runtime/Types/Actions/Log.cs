namespace UnityEngine.UI.Windows.Essentials.Tutorial {

    [UnityEngine.UI.Windows.ComponentModuleDisplayNameAttribute("Print Log")]
    public struct Log : IAction {

        public string text => $"Print log with string `{this.str}`";

        public string str;

        public ActionResult Execute(in Context context) {

            Debug.Log($"Process action `{context.data.name}` for window `{context.window}` with string `{this.str}`");

            return ActionResult.MoveNext;

        }

    }

}
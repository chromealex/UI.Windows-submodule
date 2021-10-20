
namespace UnityEngine.UI.Windows.Essentials.Tutorial {

    [UnityEngine.UI.Windows.ComponentModuleDisplayNameAttribute("Storage/Set Key")]
    public struct SetKey : IAction {

        public string text => $"Set key `{this.key}` value to `{this.value}`";

        public string key;
        public int value;

        public ActionResult Execute(in Context context) {

            PlayerPrefs.SetInt(this.key, this.value);
            PlayerPrefs.Save();

            return ActionResult.MoveNext;

        }

    }

}
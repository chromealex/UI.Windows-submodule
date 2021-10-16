
namespace UnityEngine.UI.Windows.Essentials.Tutorial {

    [System.Serializable]
    public struct SetKey : IAction {

        public string caption => "Set Key";
        public string text => $"Set key `{this.key}` value to `{this.value}`";

        public string key;
        public int value;

        public void Execute(in Context context) {

            PlayerPrefs.SetInt(this.key, this.value);
            PlayerPrefs.Save();

        }

    }

}
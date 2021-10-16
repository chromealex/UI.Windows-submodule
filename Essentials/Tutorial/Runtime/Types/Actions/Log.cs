namespace UnityEngine.UI.Windows.Essentials.Tutorial {

    [System.Serializable]
    public struct Log : IAction {

        public string caption => "Log";
        public string text => $"Print log with string `{this.str}`";

        public string str;

        public void Execute(in Context context) {

            Debug.Log($"Process action `{context.data.name}` for window `{context.window}` with string `{this.str}`");

        }

    }

}
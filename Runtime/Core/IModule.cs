namespace UnityEngine.UI.Windows {

    public abstract class WindowSystemModule : ScriptableObject {

        public abstract void OnStart();
        public abstract void OnDestroy();

    }

}